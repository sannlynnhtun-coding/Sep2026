using System;
using System.Collections.Generic;

namespace Sep2026.HnitThitMayKyaw.SupermarketPos.Database.EfAppDbContextModels;

public partial class Customer
{
    public int CustomerId { get; set; }

    public string FirstName { get; set; } = null!;

    public string? LastName { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public bool DeleteFlag { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();
}
