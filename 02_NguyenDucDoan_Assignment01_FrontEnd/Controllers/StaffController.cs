using _02_NguyenDucDoan_Assignment01_FrontEnd.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace _02_NguyenDucDoan_Assignment01_FrontEnd.Controllers
{
    [Authorize(Roles = "Staff, Lecturer")]
    public class StaffController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly JsonSerializerOptions _jsonOptions;

        public StaffController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var client = _clientFactory.CreateClient("BackendAPI");
            var response = await client.GetAsync($"api/SystemAccounts/({userId})");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var account = JsonSerializer.Deserialize<SystemAccountDTO>(content, _jsonOptions);
                return View(account);
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(SystemAccountUpdateDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var client = _clientFactory.CreateClient("BackendAPI");

            // Fetch current role to avoid overriding it with 0 if not passed
            var roleClaim = User.FindFirstValue(ClaimTypes.Role);
            dto.AccountRole = roleClaim == "Staff" ? 1 : (roleClaim == "Lecturer" ? 2 : 1);

            var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"api/SystemAccounts/{userId}", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Profile updated successfully.";
            }
            else
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"Failed to update profile: {errorMsg}";
            }
            return RedirectToAction(nameof(Profile));
        }

        public async Task<IActionResult> History()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var client = _clientFactory.CreateClient("BackendAPI");
            var response = await client.GetAsync($"api/NewsArticles/by-creator/{userId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var articles = JsonSerializer.Deserialize<List<NewsArticleDTO>>(content, _jsonOptions);
                return View(articles);
            }
            return View(new List<NewsArticleDTO>());
        }
    }
}
