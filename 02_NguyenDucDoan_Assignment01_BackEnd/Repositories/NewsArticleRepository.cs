using _02_NguyenDucDoan_Assignment01_BackEnd.DAOs;
using _02_NguyenDucDoan_Assignment01_BackEnd.Models;

namespace _02_NguyenDucDoan_Assignment01_BackEnd.Repositories;

public interface INewsArticleRepository
{
    Task<List<NewsArticle>> GetAllAsync();
    Task<List<NewsArticle>> GetActiveAsync();
    Task<NewsArticle?> GetByIdAsync(string id);
    Task<bool> ExistsAsync(string id);
    Task<NewsArticle> CreateAsync(NewsArticle article, List<int>? tagIds);
    Task<NewsArticle?> UpdateAsync(string id, NewsArticle updatedData, List<int>? tagIds);
    Task<bool> DeleteAsync(string id);
    Task<List<NewsArticle>> SearchAsync(string keyword);
    Task<List<NewsArticle>> GetByCreatedByAsync(short createdById);
    Task<List<NewsArticle>> GetStatisticsAsync(DateTime startDate, DateTime endDate);
}

public class NewsArticleRepository : INewsArticleRepository
{
    public Task<List<NewsArticle>> GetAllAsync() => NewsArticleDAO.Instance.GetAllAsync();
    public Task<List<NewsArticle>> GetActiveAsync() => NewsArticleDAO.Instance.GetActiveAsync();
    public Task<NewsArticle?> GetByIdAsync(string id) => NewsArticleDAO.Instance.GetByIdAsync(id);
    public Task<bool> ExistsAsync(string id) => NewsArticleDAO.Instance.ExistsAsync(id);
    public Task<NewsArticle> CreateAsync(NewsArticle article, List<int>? tagIds) => NewsArticleDAO.Instance.CreateAsync(article, tagIds);
    public Task<NewsArticle?> UpdateAsync(string id, NewsArticle updatedData, List<int>? tagIds) => NewsArticleDAO.Instance.UpdateAsync(id, updatedData, tagIds);
    public Task<bool> DeleteAsync(string id) => NewsArticleDAO.Instance.DeleteAsync(id);
    public Task<List<NewsArticle>> SearchAsync(string keyword) => NewsArticleDAO.Instance.SearchAsync(keyword);
    public Task<List<NewsArticle>> GetByCreatedByAsync(short createdById) => NewsArticleDAO.Instance.GetByCreatedByAsync(createdById);
    public Task<List<NewsArticle>> GetStatisticsAsync(DateTime startDate, DateTime endDate) => NewsArticleDAO.Instance.GetStatisticsAsync(startDate, endDate);
}
