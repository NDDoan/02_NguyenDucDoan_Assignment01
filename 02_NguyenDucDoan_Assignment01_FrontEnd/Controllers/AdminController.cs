using _02_NguyenDucDoan_Assignment01_FrontEnd.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace _02_NguyenDucDoan_Assignment01_FrontEnd.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly JsonSerializerOptions _jsonOptions;

        public AdminController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Accounts()
        {
            var client = _clientFactory.CreateClient("BackendAPI");
            var response = await client.GetAsync("api/SystemAccounts");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var accounts = JsonSerializer.Deserialize<List<SystemAccountDTO>>(content, _jsonOptions);
                return View(accounts);
            }
            return View(new List<SystemAccountDTO>());
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccount(SystemAccountCreateDTO dto)
        {
            var client = _clientFactory.CreateClient("BackendAPI");
            var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("api/SystemAccounts", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Account created successfully.";
            }
            else
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"Failed to create account: {errorMsg}";
            }
            return RedirectToAction(nameof(Accounts));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAccount(short id, SystemAccountUpdateDTO dto)
        {
            var client = _clientFactory.CreateClient("BackendAPI");
            var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"api/SystemAccounts/{id}", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Account updated successfully.";
            }
            else
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"Failed to update account: {errorMsg}";
            }
            return RedirectToAction(nameof(Accounts));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAccount(short id)
        {
            var client = _clientFactory.CreateClient("BackendAPI");
            var response = await client.DeleteAsync($"api/SystemAccounts/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Account deleted successfully.";
            }
            else
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"Failed to delete account: {errorMsg}";
            }
            return RedirectToAction(nameof(Accounts));
        }

        public async Task<IActionResult> Report(DateTime? startDate, DateTime? endDate)
        {
            if (!startDate.HasValue || !endDate.HasValue)
            {
                return View(new List<NewsStatisticDTO>());
            }

            var client = _clientFactory.CreateClient("BackendAPI");
            var url = $"api/NewsArticles/statistics?startDate={startDate.Value:yyyy-MM-dd}&endDate={endDate.Value:yyyy-MM-dd}";
            var response = await client.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var stats = JsonSerializer.Deserialize<List<NewsStatisticDTO>>(content, _jsonOptions);
                return View(stats);
            }

            ViewBag.Error = "Failed to load report.";
            return View(new List<NewsStatisticDTO>());
        }
    }
}
