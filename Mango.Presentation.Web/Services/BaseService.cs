    using Mango.Presentation.Web.Interfaces;
    using Mango.Presentation.Web.Models.Dtos;
    using Mango.Presentation.Web.Enuns;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Net;

    namespace Mango.Presentation.Web.Services
    {
        public class BaseService : IBaseService
        {
            private readonly IHttpClientFactory _httpClientFactory;
            private readonly ILogger<BaseService> _logger;

            public BaseService(IHttpClientFactory httpClientFactory, ILogger<BaseService> logger)
            {
                _httpClientFactory = httpClientFactory;
                _logger = logger;
            }

            // Preserve existing simple API for compatibility.
            public Task<ResponseDto?> SendAsync(RequestDto requestDto)
                => SendAsync(requestDto, CancellationToken.None);

            // Robust, reusable send method with retries, cancellation, headers and custom JSON options.
            public async Task<ResponseDto?> SendAsync(
                RequestDto requestDto,
                CancellationToken cancellationToken = default,
                JsonSerializerOptions? jsonOptions = null,
                int maxRetries = 3,
                IDictionary<string, string>? additionalHeaders = null,
                Func<HttpResponseMessage, Task<bool>>? shouldRetry = null)
            {
                if (requestDto == null) throw new ArgumentNullException(nameof(requestDto));
                if (maxRetries < 1) maxRetries = 1;

                jsonOptions ??= new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                try
                {
                    var client = _httpClientFactory.CreateClient("MangoAPI");

                    // Resolve request URI
                    Uri requestUri;
                    if (!string.IsNullOrWhiteSpace(requestDto.Url) && Uri.TryCreate(requestDto.Url, UriKind.Absolute, out var absolute))
                    {
                        requestUri = absolute;
                    }
                    else if (client.BaseAddress != null)
                    {
                        // If Url is null/empty or relative, combine with BaseAddress
                        var relative = string.IsNullOrWhiteSpace(requestDto.Url) ? string.Empty : requestDto.Url!;
                        requestUri = new Uri(client.BaseAddress, relative);
                    }
                    else if (!string.IsNullOrWhiteSpace(requestDto.Url) && Uri.TryCreate(requestDto.Url, UriKind.RelativeOrAbsolute, out var relativeOnly) && relativeOnly.IsAbsoluteUri)
                    {
                        requestUri = relativeOnly;
                    }
                    else
                    {
                        throw new InvalidOperationException("No request URL provided and client BaseAddress is not configured.");
                    }

                    // Factory to produce a fresh HttpRequestMessage each attempt (content can't be reused)
                    HttpRequestMessage CreateRequestMessage()
                    {
                        var method = requestDto.ApiType switch
                        {
                            ApiType.GET => HttpMethod.Get,
                            ApiType.POST => HttpMethod.Post,
                            ApiType.PUT => HttpMethod.Put,
                            ApiType.DELETE => HttpMethod.Delete,
                            _ => throw new NotImplementedException($"API type {requestDto.ApiType} not supported.")
                        };

                        var msg = new HttpRequestMessage
                        {
                            RequestUri = requestUri,
                            Method = method
                        };

                        // Add optional headers
                        if (additionalHeaders != null)
                        {
                            foreach (var kvp in additionalHeaders)
                            {
                                // Use TryAddWithoutValidation to avoid header format exceptions for custom headers
                                msg.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value);
                            }
                        }

                        // Serialize Body for non-GET methods
                        if (method != HttpMethod.Get && requestDto.Data != null)
                        {
                            var json = JsonSerializer.Serialize(requestDto.Data, jsonOptions);
                            msg.Content = new StringContent(json, Encoding.UTF8, "application/json");
                        }

                        return msg;
                    }

                    // Default shouldRetry: treat 5xx and 429 as transient
                    shouldRetry ??= (response) =>
                    {
                        var code = (int)response.StatusCode;
                        return Task.FromResult((code >= 500 && code < 600) || response.StatusCode == HttpStatusCode.TooManyRequests);
                    };

                    var attempt = 0;
                    var rnd = new Random();
                    while (true)
                    {
                        attempt++;
                        using var request = CreateRequestMessage();

                        _logger.LogDebug("Attempt {Attempt} - Sending {Method} to {Url}", attempt, request.Method, request.RequestUri);

                        HttpResponseMessage? apiResponse = null;
                        try
                        {
                            apiResponse = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
                        }
                        catch (OperationCanceledException oce) when (cancellationToken.IsCancellationRequested)
                        {
                            _logger.LogWarning(oce, "Request cancelled by caller.");
                            return new ResponseDto { IsSuccess = false, Message = "Request cancelled." };
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error sending HTTP request on attempt {Attempt}", attempt);
                            if (attempt >= maxRetries)
                            {
                                return new ResponseDto { IsSuccess = false, Message = ex.Message };
                            }

                            // backoff and retry on transient exceptions
                            var delayMs = (int)(Math.Pow(2, attempt) * 100) + rnd.Next(0, 100);
                            _logger.LogDebug("Transient exception, waiting {Delay}ms before retry", delayMs);
                            await Task.Delay(delayMs, cancellationToken).ConfigureAwait(false);
                            continue;
                        }

                        if (apiResponse == null)
                        {
                            if (attempt >= maxRetries)
                            {
                                return new ResponseDto { IsSuccess = false, Message = "No response received from server." };
                            }

                            var delayMs = (int)(Math.Pow(2, attempt) * 100) + rnd.Next(0, 100);
                            await Task.Delay(delayMs, cancellationToken).ConfigureAwait(false);
                            continue;
                        }

                        _logger.LogDebug("Received response {StatusCode} on attempt {Attempt}", apiResponse.StatusCode, attempt);

                        // Handle well-known status codes first
                        if (apiResponse.StatusCode == HttpStatusCode.NotFound)
                        {
                            return new ResponseDto { IsSuccess = false, Message = "Resource not found." };
                        }

                        if (apiResponse.StatusCode == HttpStatusCode.Forbidden)
                        {
                            return new ResponseDto { IsSuccess = false, Message = "Access forbidden." };
                        }

                        if (apiResponse.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            return new ResponseDto { IsSuccess = false, Message = "Unauthorized access." };
                        }

                        if (apiResponse.StatusCode == HttpStatusCode.InternalServerError)
                        {
                            // Optionally retry on 500 depending on shouldRetry and attempts left
                            if (attempt < maxRetries && await shouldRetry(apiResponse).ConfigureAwait(false))
                            {
                                var delayMs = (int)(Math.Pow(2, attempt) * 100) + rnd.Next(0, 100);
                                _logger.LogWarning("Server error {StatusCode}. Retrying after {Delay}ms (attempt {Attempt} of {MaxAttempts})", apiResponse.StatusCode, delayMs, attempt, maxRetries);
                                await Task.Delay(delayMs, cancellationToken).ConfigureAwait(false);
                                continue;
                            }

                            return new ResponseDto { IsSuccess = false, Message = "Internal server error." };
                        }

                        // If success, deserialize to ResponseDto
                        if (apiResponse.IsSuccessStatusCode)
                        {
                            var content = await apiResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                            if (string.IsNullOrWhiteSpace(content))
                            {
                                return new ResponseDto { IsSuccess = true, Message = "Success", Result = null };
                            }

                            try
                            {
                                var deserialized = JsonSerializer.Deserialize<ResponseDto>(content, jsonOptions);
                                if (deserialized != null) return deserialized;

                                // fallback: return success with raw content
                                return new ResponseDto { IsSuccess = true, Message = "Success", Result = content };
                            }
                            catch (JsonException jex)
                            {
                                _logger.LogWarning(jex, "Failed to deserialize response to ResponseDto; returning raw content.");
                                return new ResponseDto { IsSuccess = true, Message = "Success (raw)", Result = await apiResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false) };
                            }
                        }

                        // Non-success, non-handled status codes: maybe retry
                        var shouldAttemptRetry = await shouldRetry(apiResponse).ConfigureAwait(false);
                        var responseContent = await apiResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

                        if (shouldAttemptRetry && attempt < maxRetries)
                        {
                            var delayMs = (int)(Math.Pow(2, attempt) * 100) + rnd.Next(0, 100);
                            _logger.LogWarning("Transient response {StatusCode}. Retrying after {Delay}ms (attempt {Attempt} of {MaxAttempts}). Response content: {Content}", apiResponse.StatusCode, delayMs, attempt, maxRetries, TruncateForLog(responseContent));
                            await Task.Delay(delayMs, cancellationToken).ConfigureAwait(false);
                            continue;
                        }

                        // Final failure response: include server message if available
                        var message = !string.IsNullOrWhiteSpace(responseContent) ? responseContent : $"Request failed with status code {(int)apiResponse.StatusCode} ({apiResponse.StatusCode}).";
                        return new ResponseDto { IsSuccess = false, Message = message };
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error in SendAsync");
                    return new ResponseDto { IsSuccess = false, Message = ex.Message };
                }
            }

            private static string TruncateForLog(string? input, int max = 512)
            {
                if (string.IsNullOrEmpty(input)) return string.Empty;
                if (input.Length <= max) return input;
                return input.Substring(0, max) + "...(truncated)";
            }
        }
    }