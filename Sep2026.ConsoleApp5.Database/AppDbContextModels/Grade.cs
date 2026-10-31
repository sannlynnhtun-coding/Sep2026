using System;
using System.Collections.Generic;

namespace Sep2026.ConsoleApp5.Database.AppDbContextModels;

public partial class Grade
{
    public int GradeId { get; set; }

    public int StudentId { get; set; }

    public string Subject { get; set; } = null!;

    public int Mark { get; set; }

    public virtual Student Student { get; set; } = null!;
}
