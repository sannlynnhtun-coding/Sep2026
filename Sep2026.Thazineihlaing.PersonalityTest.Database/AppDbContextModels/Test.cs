using System;
using System.Collections.Generic;

namespace Sep2026.Thazineihlaing.PersonalityTest.Database.AppDbContextModels;

public partial class Test
{
    public int TestId { get; set; }

    public int? ResultPersonalityTypeId { get; set; }

    public virtual PersonalityType? ResultPersonalityType { get; set; }

    public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
}
