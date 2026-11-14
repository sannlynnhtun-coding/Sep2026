using System;
using System.Collections.Generic;

namespace Sep2026.HnitThitMayKyaw.SupermarketPos.Database.EfAppDbContextModels;

public partial class Employee
{
    public int EmployeeId { get; set; }

    public string FirstName { get; set; } = null!;

    public string? LastName { get; set; }

    public string? Phone { get; set; }

    public bool DeleteFlag { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();
}
