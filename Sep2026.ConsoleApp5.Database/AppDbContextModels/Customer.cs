using System;
using System.Collections.Generic;

namespace Sep2026.ConsoleApp5.Database.AppDbContextModels;

public partial class Customer
{
    public int CustomerId { get; set; }

    public string CustomerName { get; set; } = null!;

    public string? ContactName { get; set; }

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? PostalCode { get; set; }

    public string? Country { get; set; }
}
