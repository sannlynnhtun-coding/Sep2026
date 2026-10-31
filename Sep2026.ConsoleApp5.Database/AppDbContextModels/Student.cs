using System;
using System.Collections.Generic;

namespace Sep2026.ConsoleApp5.Database.AppDbContextModels;

public partial class Student
{
    public int Id { get; set; }

    public string No { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? ClassNo { get; set; }

    public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();
}
