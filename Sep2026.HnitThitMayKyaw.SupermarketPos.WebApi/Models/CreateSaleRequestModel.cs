using System.ComponentModel.DataAnnotations;

namespace Sep2026.HnitThitMayKyaw.SupermarketPos.WebApi.Models;

public class CreateSaleRequestModel
{
    [Required]
    [StringLength(30)]
    public string SaleNumber { get; set; } = null!;

    [Range(1, int.MaxValue)]
    public int? CustomerId { get; set; }

    [Range(1, int.MaxValue)]
    public int EmployeeId { get; set; }

    [Range(typeof(decimal), "0", "9999999999999999.99")]
    public decimal Subtotal { get; set; }

    [Range(typeof(decimal), "0", "9999999999999999.99")]
    public decimal DiscountAmount { get; set; }

    [Range(typeof(decimal), "0", "9999999999999999.99")]
    public decimal TaxAmount { get; set; }

    [Range(typeof(decimal), "0", "9999999999999999.99")]
    public decimal TotalAmount { get; set; }
}

