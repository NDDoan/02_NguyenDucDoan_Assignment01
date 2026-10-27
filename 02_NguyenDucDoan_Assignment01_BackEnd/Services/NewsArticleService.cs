using _02_NguyenDucDoan_Assignment01_BackEnd.DTOs;
using _02_NguyenDucDoan_Assignment01_BackEnd.Models;
using _02_NguyenDucDoan_Assignment01_BackEnd.Repositories;

namespace _02_NguyenDucDoan_Assignment01_BackEnd.Services;

public class NewsArticleService : INewsArticleService
{
    private readonly INewsArticleRepository _newsRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ISystemAccountRepository _accountRepository;

    public NewsArticleService(
        INewsArticleRepository newsRepository,
        ICategoryRepository categoryRepository,
        ISystemAccountRepository accountRepository)
    {
        _newsRepository = newsRepository;
        _categoryRepository = categoryRepository;
        _accountRepository = accountRepository;
    }

    private static NewsArticleDTO MapToDTO(NewsArticle article) => new()
    {
        NewsArticleId = article.NewsArticleId,
        NewsTitle = article.NewsTitle,
        Headline = article.Headline,
        CreatedDate = article.CreatedDate,
        NewsContent = article.NewsContent,
        NewsSource = article.NewsSource,
        CategoryId = article.CategoryId,
        CategoryName = article.Category?.CategoryName,
        NewsStatus = article.NewsStatus,
        CreatedById = article.CreatedById,
        CreatedByName = article.CreatedBy?.AccountName,
        UpdatedById = article.UpdatedById,
        ModifiedDate = article.ModifiedDate,
        Tags = article.Tags.Select(t => new TagDTO
        {
            TagId = t.TagId,
            TagName = t.TagName,
            Note = t.Note
        }).ToList()
    };

    public async Task<IEnumerable<NewsArticleDTO>> GetAllAsync()
    {
        var articles = await _newsRepository.GetAllAsync();
        return articles
            .OrderBy(a => int.TryParse(a.NewsArticleId, out int id) ? id : int.MaxValue)
            .ThenBy(a => a.NewsArticleId)
            .Select(MapToDTO);
    }

    public async Task<IEnumerable<NewsArticleDTO>> GetActiveAsync()
    {
        var articles = await _newsRepository.GetActiveAsync();
        return articles
            .OrderBy(a => int.TryParse(a.NewsArticleId, out int id) ? id : int.MaxValue)
            .ThenBy(a => a.NewsArticleId)
            .Select(MapToDTO);
    }

    public async Task<NewsArticleDTO?> GetByIdAsync(string id)
    {
        var article = await _newsRepository.GetByIdAsync(id);
        return article == null ? null : MapToDTO(article);
    }

    public async Task<NewsArticleDTO> CreateAsync(NewsArticleCreateDTO dto)
    {
        if (await _newsRepository.ExistsAsync(dto.NewsArticleId))
            throw new InvalidOperationException($"NewsArticleId '{dto.NewsArticleId}' already exists.");

        if (dto.CategoryId.HasValue)
        {
            if (!await _categoryRepository.ExistsAsync(dto.CategoryId.Value))
                throw new InvalidOperationException($"Category with ID '{dto.CategoryId.Value}' does not exist.");
        }

        if (dto.CreatedById.HasValue)
        {
            var account = await _accountRepository.GetByIdAsync(dto.CreatedById.Value);
            if (account == null)
                throw new InvalidOperationException($"Account with ID '{dto.CreatedById.Value}' does not exist.");
        }

        var article = new NewsArticle
        {
            NewsArticleId = dto.NewsArticleId,
            NewsTitle = dto.NewsTitle,
            Headline = dto.Headline,
            CreatedDate = DateTime.UtcNow,
            NewsContent = dto.NewsContent,
            NewsSource = dto.NewsSource,
            CategoryId = dto.CategoryId,
            NewsStatus = dto.NewsStatus ?? true,
            CreatedById = dto.CreatedById,
            ModifiedDate = DateTime.UtcNow
        };

        var created = await _newsRepository.CreateAsync(article, dto.TagIds);
        return MapToDTO(created);
    }

    public async Task<NewsArticleDTO?> UpdateAsync(string id, NewsArticleUpdateDTO dto)
    {
        var article = await _newsRepository.GetByIdAsync(id);
        if (article == null) return null;

        if (dto.CategoryId.HasValue)
        {
            if (!await _categoryRepository.ExistsAsync(dto.CategoryId.Value))
                throw new InvalidOperationException($"Category with ID '{dto.CategoryId.Value}' does not exist.");
        }

        if (dto.UpdatedById.HasValue)
        {
            var account = await _accountRepository.GetByIdAsync(dto.UpdatedById.Value);
            if (account == null)
                throw new InvalidOperationException($"Account with ID '{dto.UpdatedById.Value}' does not exist.");
        }

        var updatedEntity = new NewsArticle
        {
            NewsTitle = dto.NewsTitle,
            Headline = dto.Headline,
            NewsContent = dto.NewsContent,
            NewsSource = dto.NewsSource,
            CategoryId = dto.CategoryId,
            NewsStatus = dto.NewsStatus,
            UpdatedById = dto.UpdatedById
        };

        var updated = await _newsRepository.UpdateAsync(id, updatedEntity, dto.TagIds);
        return updated == null ? null : MapToDTO(updated);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await _newsRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<NewsArticleDTO>> SearchAsync(string keyword)
    {
        var articles = await _newsRepository.SearchAsync(keyword);
        return articles
            .OrderBy(a => int.TryParse(a.NewsArticleId, out int id) ? id : int.MaxValue)
            .ThenBy(a => a.NewsArticleId)
            .Select(MapToDTO);
    }

    public async Task<IEnumerable<NewsArticleDTO>> GetByCreatedByAsync(short createdById)
    {
        var articles = await _newsRepository.GetByCreatedByAsync(createdById);
        return articles
            .OrderBy(a => int.TryParse(a.NewsArticleId, out int id) ? id : int.MaxValue)
            .ThenBy(a => a.NewsArticleId)
            .Select(MapToDTO);
    }

    public async Task<IEnumerable<NewsStatisticDTO>> GetStatisticsAsync(DateTime startDate, DateTime endDate)
    {
        var articles = await _newsRepository.GetStatisticsAsync(startDate, endDate);

        return articles.Select(a => new NewsStatisticDTO
        {
            NewsArticleId = a.NewsArticleId,
            NewsTitle = a.NewsTitle,
            CreatedDate = a.CreatedDate,
            CategoryName = a.Category?.CategoryName,
            CreatedByName = a.CreatedBy?.AccountName,
            NewsStatus = a.NewsStatus
        });
    }
}
