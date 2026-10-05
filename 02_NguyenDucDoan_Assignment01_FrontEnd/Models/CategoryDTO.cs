using System.ComponentModel.DataAnnotations;

namespace _02_NguyenDucDoan_Assignment01_FrontEnd.Models;

public class CategoryDTO
{
    public short CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public string CategoryDesciption { get; set; } = null!;
    public short? ParentCategoryId { get; set; }
    public bool? IsActive { get; set; }
}

public class CategoryCreateDTO
{
    [Required(ErrorMessage = "Category name is required.")]
    [MaxLength(100, ErrorMessage = "Category name must not exceed 100 characters.")]
    public string CategoryName { get; set; } = null!;

    [Required(ErrorMessage = "Category description is required.")]
    [MaxLength(250, ErrorMessage = "Category description must not exceed 250 characters.")]
    public string CategoryDesciption { get; set; } = null!;

    public short? ParentCategoryId { get; set; }

    public bool? IsActive { get; set; } = true;
}

public class CategoryUpdateDTO
{
    [Required(ErrorMessage = "Category name is required.")]
    [MaxLength(100, ErrorMessage = "Category name must not exceed 100 characters.")]
    public string CategoryName { get; set; } = null!;

    [Required(ErrorMessage = "Category description is required.")]
    [MaxLength(250, ErrorMessage = "Category description must not exceed 250 characters.")]
    public string CategoryDesciption { get; set; } = null!;

    public short? ParentCategoryId { get; set; }

    public bool? IsActive { get; set; }
}
