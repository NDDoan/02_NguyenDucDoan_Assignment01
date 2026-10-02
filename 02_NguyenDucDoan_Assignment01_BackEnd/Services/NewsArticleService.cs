using _02_NguyenDucDoan_Assignment01_BackEnd.DTOs;
using _02_NguyenDucDoan_Assignment01_BackEnd.Models;
using Microsoft.EntityFrameworkCore;

namespace _02_NguyenDucDoan_Assignment01_BackEnd.Services;

public class NewsArticleService : INewsArticleService
{
    private readonly FunewsManagementContext _context;

    public NewsArticleService(FunewsManagementContext context)
    {
        _context = context;
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
        var articles = await _context.NewsArticles
            .Include(a => a.Category)
            .Include(a => a.CreatedBy)
            .Include(a => a.Tags)
            .ToListAsync();
        return articles.Select(MapToDTO);
    }

    public async Task<IEnumerable<NewsArticleDTO>> GetActiveAsync()
    {
        var articles = await _context.NewsArticles
            .Include(a => a.Category)
            .Include(a => a.CreatedBy)
            .Include(a => a.Tags)
            .Where(a => a.NewsStatus == true)
            .ToListAsync();
        return articles.Select(MapToDTO);
    }

    public async Task<NewsArticleDTO?> GetByIdAsync(string id)
    {
        var article = await _context.NewsArticles
            .Include(a => a.Category)
            .Include(a => a.CreatedBy)
            .Include(a => a.Tags)
            .FirstOrDefaultAsync(a => a.NewsArticleId == id);
        return article == null ? null : MapToDTO(article);
    }

    public async Task<NewsArticleDTO> CreateAsync(NewsArticleCreateDTO dto)
    {
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

        // Attach tags
        if (dto.TagIds != null && dto.TagIds.Any())
        {
            var tags = await _context.Tags
                .Where(t => dto.TagIds.Contains(t.TagId))
                .ToListAsync();
            foreach (var tag in tags)
                article.Tags.Add(tag);
        }

        _context.NewsArticles.Add(article);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(article.NewsArticleId) ?? MapToDTO(article);
    }

    public async Task<NewsArticleDTO?> UpdateAsync(string id, NewsArticleUpdateDTO dto)
    {
        var article = await _context.NewsArticles
            .Include(a => a.Tags)
            .FirstOrDefaultAsync(a => a.NewsArticleId == id);

        if (article == null) return null;

        article.NewsTitle = dto.NewsTitle;
        article.Headline = dto.Headline;
        article.NewsContent = dto.NewsContent;
        article.NewsSource = dto.NewsSource;
        article.CategoryId = dto.CategoryId;
        article.NewsStatus = dto.NewsStatus;
        article.UpdatedById = dto.UpdatedById;
        article.ModifiedDate = DateTime.UtcNow;

        // Update tags
        article.Tags.Clear();
        if (dto.TagIds != null && dto.TagIds.Any())
        {
            var tags = await _context.Tags
                .Where(t => dto.TagIds.Contains(t.TagId))
                .ToListAsync();
            foreach (var tag in tags)
                article.Tags.Add(tag);
        }

        await _context.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var article = await _context.NewsArticles
            .Include(a => a.Tags)
            .FirstOrDefaultAsync(a => a.NewsArticleId == id);

        if (article == null) return false;

        article.Tags.Clear();
        _context.NewsArticles.Remove(article);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<NewsArticleDTO>> SearchAsync(string keyword)
    {
        var articles = await _context.NewsArticles
            .Include(a => a.Category)
            .Include(a => a.CreatedBy)
            .Include(a => a.Tags)
            .Where(a => (a.NewsTitle != null && a.NewsTitle.Contains(keyword))
                     || a.Headline.Contains(keyword)
                     || (a.NewsContent != null && a.NewsContent.Contains(keyword)))
            .ToListAsync();
        return articles.Select(MapToDTO);
    }

    public async Task<IEnumerable<NewsArticleDTO>> GetByCreatedByAsync(short createdById)
    {
        var articles = await _context.NewsArticles
            .Include(a => a.Category)
            .Include(a => a.CreatedBy)
            .Include(a => a.Tags)
            .Where(a => a.CreatedById == createdById)
            .ToListAsync();
        return articles.Select(MapToDTO);
    }

    public async Task<IEnumerable<NewsStatisticDTO>> GetStatisticsAsync(DateTime startDate, DateTime endDate)
    {
        var articles = await _context.NewsArticles
            .Include(a => a.Category)
            .Include(a => a.CreatedBy)
            .Where(a => a.CreatedDate >= startDate && a.CreatedDate <= endDate)
            .OrderByDescending(a => a.CreatedDate)
            .ToListAsync();

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
