using _02_NguyenDucDoan_Assignment01_FrontEnd.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace _02_NguyenDucDoan_Assignment01_FrontEnd.Controllers
{
    public class AuthController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;

        public AuthController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDTO dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var client = _clientFactory.CreateClient("BackendAPI");
            var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("api/SystemAccounts/login", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<LoginResponseDTO>(
                    responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (result != null && result.Success && !string.IsNullOrEmpty(result.Token))
                {
                    var claims = ExtractClaimsFromToken(result.Token);
                    claims.Add(new Claim("Token", result.Token));

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    var roleClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "role")?.Value;
                    var isAdminClaim = claims.FirstOrDefault(c => c.Type == "IsAdmin")?.Value;
                    var isAdmin = string.Equals(isAdminClaim, "true", StringComparison.OrdinalIgnoreCase) || roleClaim == "Admin";

                    if (isAdmin)
                        return RedirectToAction("Index", "Admin");
                    else if (roleClaim == "Staff")
                        return RedirectToAction("Index", "Staff");
                    else
                        return RedirectToAction("Index", "Home");
                }
            }

            ViewBag.Error = "Invalid email or password.";
            return View(dto);
        }

        private static List<Claim> ExtractClaimsFromToken(string token)
        {
            var claims = new List<Claim>();
            try
            {
                var parts = token.Split('.');
                if (parts.Length > 1)
                {
                    var payload = parts[1];
                    switch (payload.Length % 4)
                    {
                        case 2: payload += "=="; break;
                        case 3: payload += "="; break;
                    }
                    var jsonBytes = Convert.FromBase64String(payload.Replace('-', '+').Replace('_', '/'));
                    using var doc = JsonDocument.Parse(jsonBytes);
                    foreach (var prop in doc.RootElement.EnumerateObject())
                    {
                        var claimType = prop.Name switch
                        {
                            "role" or "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" => ClaimTypes.Role,
                            "nameid" or "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier" => ClaimTypes.NameIdentifier,
                            "email" or "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress" => ClaimTypes.Email,
                            "unique_name" or "name" or "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name" => ClaimTypes.Name,
                            _ => prop.Name
                        };
                        claims.Add(new Claim(claimType, prop.Value.GetString() ?? prop.Value.ToString()));
                    }
                }
            }
            catch
            {
                // Fallback if parsing fails
            }
            return claims;
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
