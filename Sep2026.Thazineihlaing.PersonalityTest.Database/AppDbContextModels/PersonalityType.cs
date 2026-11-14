using System;
using System.Collections.Generic;

namespace Sep2026.Thazineihlaing.PersonalityTest.Database.AppDbContextModels;

public partial class PersonalityType
{
    public int PersonalityTypeId { get; set; }

    public string TypeName { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<AnswerOption> AnswerOptions { get; set; } = new List<AnswerOption>();

    public virtual ICollection<Test> Tests { get; set; } = new List<Test>();
}
