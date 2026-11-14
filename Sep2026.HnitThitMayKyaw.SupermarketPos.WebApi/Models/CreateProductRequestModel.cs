using System.ComponentModel.DataAnnotations;

namespace Sep2026.HnitThitMayKyaw.SupermarketPos.WebApi.Models;

public class CreateProductRequestModel
{
    [StringLength(50)]
    public string? Barcode { get; set; }

    [Required]
    [StringLength(200)]
    public string ProductName { get; set; } = null!;

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    [Range(typeof(decimal), "0", "9999999999999999.99")]
    public decimal PurchasePrice { get; set; }

    [Range(typeof(decimal), "0", "9999999999999999.99")]
    public decimal SellingPrice { get; set; }

    [Range(typeof(decimal), "0", "999999999999999.999")]
    public decimal QuantityInStock { get; set; }
}

public class UpsertProductRequestModel
{
    [StringLength(50)]
    public string? Barcode { get; set; }

    [Required]
    [StringLength(200)]
    public string ProductName { get; set; } = null!;

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    [Range(typeof(decimal), "0", "9999999999999999.99")]
    public decimal PurchasePrice { get; set; }

    [Range(typeof(decimal), "0", "9999999999999999.99")]
    public decimal SellingPrice { get; set; }

    [Range(typeof(decimal), "0", "999999999999999.999")]
    public decimal QuantityInStock { get; set; }
}

public class UpdateProductRequestModel
{
    [StringLength(50)]
    public string? Barcode { get; set; }

    [Required]
    [StringLength(200)]
    public string ProductName { get; set; } = null!;

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    [Range(typeof(decimal), "0", "9999999999999999.99")]
    public decimal PurchasePrice { get; set; }

    [Range(typeof(decimal), "0", "9999999999999999.99")]
    public decimal SellingPrice { get; set; }

    [Range(typeof(decimal), "0", "999999999999999.999")]
    public decimal QuantityInStock { get; set; }
}