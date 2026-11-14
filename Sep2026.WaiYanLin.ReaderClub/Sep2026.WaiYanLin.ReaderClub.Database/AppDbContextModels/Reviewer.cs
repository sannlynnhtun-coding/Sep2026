using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Sep2026.ReaderClub.Database.AppDbContextModels;

public partial class Reviewer
{
    public int ReviewerId { get; set; }

    public string? ReviewerName { get; set; }

    public string? ProfileDescription { get; set; }
    [JsonIgnore]
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
