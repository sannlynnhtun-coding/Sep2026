using System;
using System.Collections.Generic;

namespace Sep2026.HnitThitMayKyaw.SupermarketPos.Database.EfAppDbContextModels;

public partial class Sale
{
    public long SaleId { get; set; }

    public string SaleNumber { get; set; } = null!;

    public DateTime SaleDateUtc { get; set; }

    public int? CustomerId { get; set; }

    public int EmployeeId { get; set; }

    public decimal Subtotal { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public bool DeleteFlag { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
}
