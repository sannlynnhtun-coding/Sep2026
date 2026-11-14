using System;
using System.Collections.Generic;

namespace Sep2026.Thazineihlaing.PersonalityTest.Database.AppDbContextModels;

public partial class AnswerOption
{
    public int AnswerOptionId { get; set; }

    public int QuestionId { get; set; }

    public string OptionText { get; set; } = null!;

    public int PersonalityTypeId { get; set; }

    public virtual PersonalityType PersonalityType { get; set; } = null!;

    public virtual Question Question { get; set; } = null!;

    public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
}
