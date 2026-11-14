using System;
using System.Collections.Generic;

namespace Sep2026.Thazineihlaing.PersonalityTest.Database.AppDbContextModels;

public partial class UserAnswer
{
    public int UserAnswerId { get; set; }

    public int TestId { get; set; }

    public int QuestionId { get; set; }

    public int AnswerOptionId { get; set; }

    public virtual AnswerOption AnswerOption { get; set; } = null!;

    public virtual Question Question { get; set; } = null!;

    public virtual Test Test { get; set; } = null!;
}
