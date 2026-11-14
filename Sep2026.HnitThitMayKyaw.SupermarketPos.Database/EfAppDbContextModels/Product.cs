using System;
using System.Collections.Generic;

namespace Sep2026.HnitThitMayKyaw.SupermarketPos.Database.EfAppDbContextModels;

public partial class Product
{
    public int ProductId { get; set; }

    public string? Barcode { get; set; }

    public string ProductName { get; set; } = null!;

    public int CategoryId { get; set; }

    public decimal PurchasePrice { get; set; }

    public decimal SellingPrice { get; set; }

    public decimal QuantityInStock { get; set; }

    public bool DeleteFlag { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
}
