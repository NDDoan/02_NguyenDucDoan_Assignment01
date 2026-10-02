using _02_NguyenDucDoan_Assignment01_BackEnd.DTOs;

namespace _02_NguyenDucDoan_Assignment01_BackEnd.Services;

public interface INewsArticleService
{
    Task<IEnumerable<NewsArticleDTO>> GetAllAsync();
    Task<IEnumerable<NewsArticleDTO>> GetActiveAsync();
    Task<NewsArticleDTO?> GetByIdAsync(string id);
    Task<NewsArticleDTO> CreateAsync(NewsArticleCreateDTO dto);
    Task<NewsArticleDTO?> UpdateAsync(string id, NewsArticleUpdateDTO dto);
    Task<bool> DeleteAsync(string id);
    Task<IEnumerable<NewsArticleDTO>> SearchAsync(string keyword);
    Task<IEnumerable<NewsArticleDTO>> GetByCreatedByAsync(short createdById);
    Task<IEnumerable<NewsStatisticDTO>> GetStatisticsAsync(DateTime startDate, DateTime endDate);
}
