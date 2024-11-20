using System;
using System.Collections.Generic;

namespace NurseCourse.Models;

public partial class Question
{
    public int Id { get; set; }

    public string QuestionText { get; set; } = null!;

    public int QuestionType { get; set; }

    public string? CorrectAnswer { get; set; }

    public int ExamId { get; set; }

    public virtual Exam Exam { get; set; } = null!;

    public virtual ICollection<Option> Options { get; set; } = new List<Option>();
}
