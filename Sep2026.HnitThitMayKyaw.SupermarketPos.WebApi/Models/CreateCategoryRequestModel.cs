using System.ComponentModel.DataAnnotations;

namespace Sep2026.HnitThitMayKyaw.SupermarketPos.WebApi.Models;

public class CreateCategoryRequestModel
{
    [Required]
    [StringLength(100)]
    public string CategoryName { get; set; } = null!;

    [StringLength(500)]
    public string? Description { get; set; }
}

public class UpsertCategoryRequestModel
{
    [Required]
    [StringLength(100)]
    public string CategoryName { get; set; } = null!;

    [StringLength(500)]
    public string? Description { get; set; }
}

public class UpdateCategoryRequestModel
{
    [Required]
    [StringLength(100)]
    public string CategoryName { get; set; } = null!;

    [StringLength(500)]
    public string? Description { get; set; }
}