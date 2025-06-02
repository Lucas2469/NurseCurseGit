using System;
using System.Collections.Generic;

namespace NurseCourse.Models;

public partial class Option
{
    public int Id { get; set; }

    public string OptionText { get; set; } = null!;

    public bool? IsCorrect { get; set; }

    public int? QuestionId { get; set; }

    public virtual Question? Question { get; set; }
}
