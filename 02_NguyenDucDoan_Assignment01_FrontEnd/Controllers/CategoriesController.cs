using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using _02_NguyenDucDoan_Assignment01_FrontEnd.Models;
using Microsoft.AspNetCore.Authorization;

namespace _02_NguyenDucDoan_Assignment01_FrontEnd.Controllers
{
    [Authorize(Roles = "Staff, Lecturer")]
    public class CategoriesController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly JsonSerializerOptions _jsonOptions;

        public CategoriesController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public async Task<IActionResult> Index()
        {
            var client = _clientFactory.CreateClient("BackendAPI");
            var response = await client.GetAsync("api/Categories");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var categories = JsonSerializer.Deserialize<List<CategoryDTO>>(content, _jsonOptions);
                return View(categories);
            }

            return View(new List<CategoryDTO>());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CategoryCreateDTO dto)
        {
            var client = _clientFactory.CreateClient("BackendAPI");
            var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("api/Categories", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Category created successfully.";
            }
            else
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"Failed to create category: {errorMsg}";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Update(short id, CategoryUpdateDTO dto)
        {
            var client = _clientFactory.CreateClient("BackendAPI");
            var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"api/Categories/{id}", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Category updated successfully.";
            }
            else
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"Failed to update category: {errorMsg}";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(short id)
        {
            var client = _clientFactory.CreateClient("BackendAPI");
            var response = await client.DeleteAsync($"api/Categories/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Category deleted successfully.";
            }
            else
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"Failed to delete category: {errorMsg}";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}