using System.ComponentModel.DataAnnotations;

namespace _02_NguyenDucDoan_Assignment01_BackEnd.DTOs;

public class NewsArticleDTO
{
    public string NewsArticleId { get; set; } = null!;
    public string? NewsTitle { get; set; }
    public string Headline { get; set; } = null!;
    public DateTime? CreatedDate { get; set; }
    public string? NewsContent { get; set; }
    public string? NewsSource { get; set; }
    public short? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public bool? NewsStatus { get; set; }
    public short? CreatedById { get; set; }
    public string? CreatedByName { get; set; }
    public short? UpdatedById { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public List<TagDTO> Tags { get; set; } = new();
}

public class NewsArticleCreateDTO
{
    [Required(ErrorMessage = "News article ID is required.")]
    [MaxLength(20, ErrorMessage = "News article ID must not exceed 20 characters.")]
    public string NewsArticleId { get; set; } = null!;

    [MaxLength(400, ErrorMessage = "News title must not exceed 400 characters.")]
    public string? NewsTitle { get; set; }

    [Required(ErrorMessage = "Headline is required.")]
    [MaxLength(150, ErrorMessage = "Headline must not exceed 150 characters.")]
    public string Headline { get; set; } = null!;

    [MaxLength(4000, ErrorMessage = "News content must not exceed 4000 characters.")]
    public string? NewsContent { get; set; }

    [MaxLength(400, ErrorMessage = "News source must not exceed 400 characters.")]
    public string? NewsSource { get; set; }

    public short? CategoryId { get; set; }

    public bool? NewsStatus { get; set; } = true;

    public short? CreatedById { get; set; }

    public List<int>? TagIds { get; set; }
}

public class NewsArticleUpdateDTO
{
    [MaxLength(400, ErrorMessage = "News title must not exceed 400 characters.")]
    public string? NewsTitle { get; set; }

    [Required(ErrorMessage = "Headline is required.")]
    [MaxLength(150, ErrorMessage = "Headline must not exceed 150 characters.")]
    public string Headline { get; set; } = null!;

    [MaxLength(4000, ErrorMessage = "News content must not exceed 4000 characters.")]
    public string? NewsContent { get; set; }

    [MaxLength(400, ErrorMessage = "News source must not exceed 400 characters.")]
    public string? NewsSource { get; set; }

    public short? CategoryId { get; set; }

    public bool? NewsStatus { get; set; }

    public short? UpdatedById { get; set; }

    public List<int>? TagIds { get; set; }
}

public class TagDTO
{
    public int TagId { get; set; }
    public string? TagName { get; set; }
    public string? Note { get; set; }
}

public class NewsStatisticDTO
{
    public string NewsArticleId { get; set; } = null!;
    public string? NewsTitle { get; set; }
    public DateTime? CreatedDate { get; set; }
    public string? CategoryName { get; set; }
    public string? CreatedByName { get; set; }
    public bool? NewsStatus { get; set; }
}
