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

        private string ExtractErrorMessage(string rawContent, string defaultMessage)
        {
            try
            {
                using var doc = JsonDocument.Parse(rawContent);
                if (doc.RootElement.TryGetProperty("message", out var msgProp))
                {
                    return msgProp.GetString() ?? defaultMessage;
                }
            }
            catch { }
            return string.IsNullOrWhiteSpace(rawContent) ? defaultMessage : rawContent;
        }

        public async Task<IActionResult> Index(string? keyword)
        {
            var client = _clientFactory.CreateClient("BackendAPI");
            
            // Fetch categories for dropdowns
            var categoriesResponse = await client.GetAsync("api/Categories");
            if (categoriesResponse.IsSuccessStatusCode)
            {
                var catContent = await categoriesResponse.Content.ReadAsStringAsync();
                var categories = JsonSerializer.Deserialize<List<CategoryDTO>>(catContent, _jsonOptions);
                ViewBag.Categories = categories ?? new List<CategoryDTO>();
            }
            else
            {
                ViewBag.Categories = new List<CategoryDTO>();
            }

            // Fetch tags for checkbox list
            var tagsResponse = await client.GetAsync("api/Tags");
            if (tagsResponse.IsSuccessStatusCode)
            {
                var tagContent = await tagsResponse.Content.ReadAsStringAsync();
                var tags = JsonSerializer.Deserialize<List<TagDTO>>(tagContent, _jsonOptions);
                ViewBag.Tags = tags ?? new List<TagDTO>();
            }
            else
            {
                ViewBag.Tags = new List<TagDTO>();
            }

            var url = string.IsNullOrWhiteSpace(keyword)
                ? "api/NewsArticles"
                : $"api/NewsArticles/search?keyword={Uri.EscapeDataString(keyword.Trim())}";

            ViewBag.Keyword = keyword;
            var response = await client.GetAsync(url);

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
                TempData["Error"] = $"Failed to create article: {ExtractErrorMessage(errorMsg, "Unknown error")}";
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
                TempData["Error"] = $"Failed to update article: {ExtractErrorMessage(errorMsg, "Unknown error")}";
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
                TempData["Error"] = $"Failed to delete article: {ExtractErrorMessage(errorMsg, "Article cannot be deleted.")}";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
