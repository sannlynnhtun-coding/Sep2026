using System;
using System.Collections.Generic;

namespace Sep2026.HnitThitMayKyaw.SupermarketPos.Database.EfAppDbContextModels;

public partial class SaleItem
{
    public long SaleItemId { get; set; }

    public long SaleId { get; set; }

    public int ProductId { get; set; }

    public decimal Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal? LineTotal { get; set; }

    public bool DeleteFlag { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual Sale Sale { get; set; } = null!;
}
