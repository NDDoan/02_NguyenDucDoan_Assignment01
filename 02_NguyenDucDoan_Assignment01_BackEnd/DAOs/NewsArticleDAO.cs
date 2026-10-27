using _02_NguyenDucDoan_Assignment01_BackEnd.Models;
using Microsoft.EntityFrameworkCore;

namespace _02_NguyenDucDoan_Assignment01_BackEnd.DAOs;

public class NewsArticleDAO
{
    private static NewsArticleDAO? _instance;
    private static readonly object _lock = new();

    private NewsArticleDAO() { }

    public static NewsArticleDAO Instance
    {
        get
        {
            lock (_lock)
            {
                return _instance ??= new NewsArticleDAO();
            }
        }
    }

    public async Task<List<NewsArticle>> GetAllAsync()
    {
        using var context = new FunewsManagementContext();
        return await context.NewsArticles
            .Include(a => a.Category)
            .Include(a => a.CreatedBy)
            .Include(a => a.Tags)
            .ToListAsync();
    }

    public async Task<List<NewsArticle>> GetActiveAsync()
    {
        using var context = new FunewsManagementContext();
        return await context.NewsArticles
            .Include(a => a.Category)
            .Include(a => a.CreatedBy)
            .Include(a => a.Tags)
            .Where(a => a.NewsStatus == true)
            .ToListAsync();
    }

    public async Task<NewsArticle?> GetByIdAsync(string id)
    {
        using var context = new FunewsManagementContext();
        return await context.NewsArticles
            .Include(a => a.Category)
            .Include(a => a.CreatedBy)
            .Include(a => a.Tags)
            .FirstOrDefaultAsync(a => a.NewsArticleId == id);
    }

    public async Task<bool> ExistsAsync(string id)
    {
        using var context = new FunewsManagementContext();
        return await context.NewsArticles.AnyAsync(a => a.NewsArticleId == id);
    }

    public async Task<NewsArticle> CreateAsync(NewsArticle article, List<int>? tagIds)
    {
        using var context = new FunewsManagementContext();

        if (tagIds != null && tagIds.Any())
        {
            var tags = await context.Tags
                .Where(t => tagIds.Contains(t.TagId))
                .ToListAsync();
            foreach (var tag in tags)
            {
                article.Tags.Add(tag);
            }
        }

        context.NewsArticles.Add(article);
        await context.SaveChangesAsync();

        return await GetByIdAsync(article.NewsArticleId) ?? article;
    }

    public async Task<NewsArticle?> UpdateAsync(string id, NewsArticle updatedData, List<int>? tagIds)
    {
        using var context = new FunewsManagementContext();
        var article = await context.NewsArticles
            .Include(a => a.Tags)
            .FirstOrDefaultAsync(a => a.NewsArticleId == id);

        if (article == null) return null;

        article.NewsTitle = updatedData.NewsTitle;
        article.Headline = updatedData.Headline;
        article.NewsContent = updatedData.NewsContent;
        article.NewsSource = updatedData.NewsSource;
        article.CategoryId = updatedData.CategoryId;
        article.NewsStatus = updatedData.NewsStatus;
        article.UpdatedById = updatedData.UpdatedById;
        article.ModifiedDate = DateTime.UtcNow;

        article.Tags.Clear();
        await context.SaveChangesAsync();

        if (tagIds != null && tagIds.Any())
        {
            var tags = await context.Tags
                .Where(t => tagIds.Contains(t.TagId))
                .ToListAsync();
            foreach (var tag in tags)
            {
                article.Tags.Add(tag);
            }
            await context.SaveChangesAsync();
        }

        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        using var context = new FunewsManagementContext();
        var article = await context.NewsArticles
            .Include(a => a.Tags)
            .FirstOrDefaultAsync(a => a.NewsArticleId == id);

        if (article == null) return false;

        article.Tags.Clear();
        await context.SaveChangesAsync();

        context.NewsArticles.Remove(article);
        await context.SaveChangesAsync();

        return true;
    }

    public async Task<List<NewsArticle>> SearchAsync(string keyword)
    {
        using var context = new FunewsManagementContext();
        return await context.NewsArticles
            .Include(a => a.Category)
            .Include(a => a.CreatedBy)
            .Include(a => a.Tags)
            .Where(a => (a.NewsTitle != null && a.NewsTitle.Contains(keyword)) ||
                        a.Headline.Contains(keyword) ||
                        (a.NewsContent != null && a.NewsContent.Contains(keyword)))
            .ToListAsync();
    }

    public async Task<List<NewsArticle>> GetByCreatedByAsync(short createdById)
    {
        using var context = new FunewsManagementContext();
        return await context.NewsArticles
            .Include(a => a.Category)
            .Include(a => a.CreatedBy)
            .Include(a => a.Tags)
            .Where(a => a.CreatedById == createdById)
            .ToListAsync();
    }

    public async Task<List<NewsArticle>> GetStatisticsAsync(DateTime startDate, DateTime endDate)
    {
        using var context = new FunewsManagementContext();
        return await context.NewsArticles
            .Include(a => a.Category)
            .Include(a => a.CreatedBy)
            .Where(a => a.CreatedDate >= startDate && a.CreatedDate <= endDate)
            .OrderByDescending(a => a.CreatedDate)
            .ToListAsync();
    }
}
