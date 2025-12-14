using Mango.Presentation.Interfaces;
using Mango.Presentation.Models.Dtos;
using System.Security.AccessControl;
using System.Text.Json;
using System.Text.Json.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Mango.Presentation.Services
{
    public class BaseService : IBaseService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public BaseService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<ResponseDto?> SendAsync(RequestDto requestDto)
        {
            try
            {
                HttpClient client = _httpClientFactory.CreateClient("MangoAPI");

                HttpRequestMessage message = new();

                message.RequestUri = new Uri(requestDto.Url);

                if (message.RequestUri != null)
                {

                    byte[] jsonUtf8Bytes = JsonSerializer.SerializeToUtf8Bytes(requestDto);

                    var content = new ByteArrayContent(jsonUtf8Bytes);
                    content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                    message.Content = content;
                }

                HttpResponseMessage? apiResponse = null;

                message.Method = requestDto.ApiType switch
                {
                    RequestType.GET => HttpMethod.Get,
                    RequestType.POST => HttpMethod.Post,
                    RequestType.PUT => HttpMethod.Put,
                    RequestType.DELETE => HttpMethod.Delete,
                    _ => throw new NotImplementedException()
                };

                apiResponse = await client.SendAsync(message);

                switch (apiResponse.StatusCode)
                {
                    case System.Net.HttpStatusCode.NotFound:
                        return new() { IsSuccess = false, Message = "Resource not found." };
                    case System.Net.HttpStatusCode.Forbidden:
                        return new() { IsSuccess = false, Message = "Access forbidden." };
                    case System.Net.HttpStatusCode.Unauthorized:
                        return new() { IsSuccess = false, Message = "Unauthorized access." };
                    case System.Net.HttpStatusCode.InternalServerError:
                        return new() { IsSuccess = false, Message = "Internal server error." };
                    default:
                        return apiResponse != null ? JsonSerializer.Deserialize<ResponseDto>(
                                                                                    await apiResponse.Content.ReadAsStringAsync(),
                                                                                                                new JsonSerializerOptions
                                                                                                                {
                                                                                                                    PropertyNameCaseInsensitive = true
                                                                                                                }) : null;
                }
            }
            catch (Exception ex)
            {
                var dto = new ResponseDto
                {
                    IsSuccess = false,
                    Message = ex.Message.ToString()
                };

                return dto;
            }
        }
    }
}
