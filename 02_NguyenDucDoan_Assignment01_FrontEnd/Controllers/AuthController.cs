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

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, result.AccountId.ToString()),
                    new Claim(ClaimTypes.Email, result.AccountEmail),
                    new Claim(ClaimTypes.Name, result.AccountName ?? ""),
                    new Claim("IsAdmin", result.IsAdmin.ToString())
                };

                if (result.IsAdmin)
                {
                    claims.Add(new Claim(ClaimTypes.Role, "Admin"));
                }
                else
                {
                    if (result.AccountRole == 1)
                        claims.Add(new Claim(ClaimTypes.Role, "Staff"));
                    else if (result.AccountRole == 2)
                        claims.Add(new Claim(ClaimTypes.Role, "Lecturer"));
                }

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                if (result.IsAdmin)
                    return RedirectToAction("Index", "Admin");
                else
                    return RedirectToAction("Index", "Staff");
            }

            ViewBag.Error = "Invalid email or password.";
            return View(dto);
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
