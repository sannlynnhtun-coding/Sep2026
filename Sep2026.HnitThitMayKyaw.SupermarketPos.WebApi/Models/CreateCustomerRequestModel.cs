using System.ComponentModel.DataAnnotations;

namespace Sep2026.HnitThitMayKyaw.SupermarketPos.WebApi.Models;

public class CreateCustomerRequestModel
{
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = null!;

    [StringLength(100)]
    public string? LastName { get; set; }

    [StringLength(30)]
    public string? Phone { get; set; }

    [StringLength(254)]
    public string? Email { get; set; }
}

public class UpsertCustomerRequestModel
{
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = null!;

    [StringLength(100)]
    public string? LastName { get; set; }

    [StringLength(30)]
    public string? Phone { get; set; }

    [StringLength(254)]
    public string? Email { get; set; }
}

public class UpdateCustomerRequestModel
{
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = null!;

    [StringLength(100)]
    public string? LastName { get; set; }

    [StringLength(30)]
    public string? Phone { get; set; }

    [StringLength(254)]
    public string? Email { get; set; }
}