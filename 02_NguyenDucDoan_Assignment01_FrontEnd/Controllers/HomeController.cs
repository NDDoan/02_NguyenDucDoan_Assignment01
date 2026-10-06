using _02_NguyenDucDoan_Assignment01_FrontEnd.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;

namespace _02_NguyenDucDoan_Assignment01_FrontEnd.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory _clientFactory;

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory clientFactory)
        {
            _logger = logger;
            _clientFactory = clientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var client = _clientFactory.CreateClient("BackendAPI");
            var response = await client.GetAsync("api/NewsArticles/active");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var articles = JsonSerializer.Deserialize<List<NewsArticleDTO>>(
                    content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
                return View(articles);
            }
            return View(new List<NewsArticleDTO>());
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var client = _clientFactory.CreateClient("BackendAPI");
            var response = await client.GetAsync($"api/NewsArticles/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var article = JsonSerializer.Deserialize<NewsArticleDTO>(
                    content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
                return View(article);
            }
            return NotFound();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
