namespace Sep2026.HnitThitMayKyaw.SupermarketPos.WebApi.Models;

public class CreateProductRequestModel
{
    public string? Barcode { get; set; }

    public string ProductName { get; set; } = null!;

    public int CategoryId { get; set; }

    public decimal PurchasePrice { get; set; }

    public decimal SellingPrice { get; set; }

    public decimal QuantityInStock { get; set; }
}

public class UpsertProductRequestModel
{
    public string? Barcode { get; set; }

    public string ProductName { get; set; } = null!;

    public int CategoryId { get; set; }

    public decimal PurchasePrice { get; set; }

    public decimal SellingPrice { get; set; }

    public decimal QuantityInStock { get; set; }
}

public class UpdateProductRequestModel
{
    public string? Barcode { get; set; }

    public string ProductName { get; set; } = null!;

    public int CategoryId { get; set; }

    public decimal PurchasePrice { get; set; }

    public decimal SellingPrice { get; set; }

    public decimal QuantityInStock { get; set; }
}