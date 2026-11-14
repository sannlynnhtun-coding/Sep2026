using System;
using System.Collections.Generic;

namespace Sep2026.HnitThitMayKyaw.SupermarketPos.Database.EfAppDbContextModels;

public partial class Category
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public string? Description { get; set; }

    public bool DeleteFlag { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
