using System.ComponentModel.DataAnnotations;

namespace Sep2026.HnitThitMayKyaw.SupermarketPos.WebApi.Models;

public class CreateEmployeesRequestModel
{
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = null!;

    [StringLength(100)]
    public string? LastName { get; set; }

    [StringLength(30)]
    public string? Phone { get; set; }
}

public class UpsertEmployeesRequestModel
{
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = null!;

    [StringLength(100)]
    public string? LastName { get; set; }

    [StringLength(30)]
    public string? Phone { get; set; }
}

public class UpdateEmployeesRequestModel
{
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = null!;

    [StringLength(100)]
    public string? LastName { get; set; }

    [StringLength(30)]
    public string? Phone { get; set; }
}