using System.Text.Json;
using System.Threading.Tasks;
using Mango.Presentation.Web.Interfaces;
using Mango.Presentation.Web.Models.Dtos;
using Mango.Presentation.Models;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Presentation.Web.Controllers
{
    public class LoginController : Controller
    {
        private readonly IAuthService _authService;

        public LoginController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            // Mensagens e valores previamente armazenados em TempData
            ViewBag.ErrorMessage = TempData["ErrorMessage"] as string;
            ViewBag.SuccessMessage = TempData["SuccessMessage"] as string;
            ViewBag.UserName = TempData["UserName"] as string ?? string.Empty;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignIn([FromForm] LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please fill in all required fields.";
                TempData["UserName"] = model.UserName;
                return RedirectToAction(nameof(Index));
            }

            var loginRequest = new LoginRequestDto
            {
                UserName = model.UserName,
                Password = model.Password
            };

            ResponseDto? response;
            try
            {
                response = await _authService.LoginAsync(loginRequest);
            }
            catch
            {
                TempData["ErrorMessage"] = "Error communicating with auth service.";
                TempData["UserName"] = model.UserName;
                return RedirectToAction(nameof(Index));
            }

            if (response == null || !response.IsSuccess)
            {
                TempData["ErrorMessage"] = response?.Message ?? "Login failed.";
                TempData["UserName"] = model.UserName;
                return RedirectToAction(nameof(Index));
            }

            var token = ExtractToken(response);

            if (string.IsNullOrEmpty(token))
            {
                TempData["SuccessMessage"] = "Login successful (no token received).";
                return RedirectToAction(nameof(Index));
            }

            TempData["JWToken"] = token;
            return RedirectToAction("Index", "Home");
        }

        private string ExtractToken(ResponseDto response)
        {
            try
            {
                if (response.Result == null) return string.Empty;

                if (response.Result is JsonElement je)
                {
                    if (je.ValueKind == JsonValueKind.Object)
                    {
                        // try top-level "token" / "Token"
                        if (je.TryGetProperty("token", out var tokenProp) || je.TryGetProperty("Token", out tokenProp))
                        {   
                            return tokenProp.GetString() ?? string.Empty;
                        }

                        // try nested "response" / "Response"
                        if (je.TryGetProperty("response", out var inner) || je.TryGetProperty("Response", out inner))
                        {
                            if (inner.ValueKind == JsonValueKind.Object)
                            {
                                // declare once and reuse for both TryGetProperty calls to avoid CS0128
                                JsonElement t2;
                                if (inner.TryGetProperty("token", out t2) || inner.TryGetProperty("Token", out t2))
                                {
                                    return t2.GetString() ?? string.Empty;
                                }
                            }

                            if (inner.ValueKind == JsonValueKind.String)
                            {
                                var s = inner.GetString();
                                if (!string.IsNullOrEmpty(s))
                                {
                                    var lr = JsonSerializer.Deserialize<LoginResponseDto>(s, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                                    if (lr != null) return lr.Token ?? string.Empty;
                                }
                            }
                        }
                    }

                    if (je.ValueKind == JsonValueKind.String)
                    {
                        var str = je.GetString();
                        if (!string.IsNullOrEmpty(str))
                        {
                            var lr = JsonSerializer.Deserialize<LoginResponseDto>(str, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                            if (lr != null) return lr.Token ?? string.Empty;
                        }
                    }
                }

                var resultString = response.Result.ToString();
                if (!string.IsNullOrEmpty(resultString))
                {
                    var lr = JsonSerializer.Deserialize<LoginResponseDto>(resultString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (lr != null && !string.IsNullOrEmpty(lr.Token)) return lr.Token;

                    using var doc = JsonDocument.Parse(resultString);
                    if (doc.RootElement.TryGetProperty("token", out var tok)) return tok.GetString() ?? string.Empty;
                    if (doc.RootElement.TryGetProperty("response", out var resp) && resp.TryGetProperty("token", out var tok2)) return tok2.GetString() ?? string.Empty;
                }
            }
            catch
            {
                // ignore parsing errors
            }

            return string.Empty;
        }
    }
}