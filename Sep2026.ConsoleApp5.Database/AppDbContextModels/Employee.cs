using System;
using System.Collections.Generic;

namespace Sep2026.ConsoleApp5.Database.AppDbContextModels;

public partial class Employee
{
    public int EmployeeId { get; set; }

    public string? LastName { get; set; }

    public string FirstName { get; set; } = null!;

    public DateTime BirthDate { get; set; }

    public string? Photo { get; set; }

    public string? Notes { get; set; }
}
