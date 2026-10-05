using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using _02_NguyenDucDoan_Assignment01_FrontEnd.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace _02_NguyenDucDoan_Assignment01_FrontEnd.Controllers
{
    [Authorize(Roles = "Staff, Lecturer")]
    public class NewsArticlesController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly JsonSerializerOptions _jsonOptions;

        public NewsArticlesController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public async Task<IActionResult> Index()
        {
            var client = _clientFactory.CreateClient("BackendAPI");
            var response = await client.GetAsync("api/NewsArticles");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var articles = JsonSerializer.Deserialize<List<NewsArticleDTO>>(content, _jsonOptions);
                return View(articles);
            }

            return View(new List<NewsArticleDTO>());
        }

        [HttpPost]
        public async Task<IActionResult> Create(NewsArticleCreateDTO dto)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (short.TryParse(userIdStr, out short userId))
            {
                dto.CreatedById = userId;
            }

            var client = _clientFactory.CreateClient("BackendAPI");
            var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("api/NewsArticles", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "News article created successfully.";
            }
            else
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"Failed to create article: {errorMsg}";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Update(string id, NewsArticleUpdateDTO dto)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (short.TryParse(userIdStr, out short userId))
            {
                dto.UpdatedById = userId;
            }

            var client = _clientFactory.CreateClient("BackendAPI");
            var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"api/NewsArticles/{id}", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "News article updated successfully.";
            }
            else
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"Failed to update article: {errorMsg}";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var client = _clientFactory.CreateClient("BackendAPI");
            var response = await client.DeleteAsync($"api/NewsArticles/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "News article deleted successfully.";
            }
            else
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"Failed to delete article: {errorMsg}";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
