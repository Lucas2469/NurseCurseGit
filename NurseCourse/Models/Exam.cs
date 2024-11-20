using System;
using System.Collections.Generic;

namespace NurseCourse.Models;

public partial class Exam
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public DateTime CreationDate { get; set; }

    public int? ModuleId { get; set; }

    public int? IsMakeup { get; set; }

    public int? OriginalExamId { get; set; }

    public virtual ICollection<ExamAttempt> ExamAttempts { get; set; } = new List<ExamAttempt>();

    public virtual ICollection<Exam> InverseOriginalExam { get; set; } = new List<Exam>();

    public virtual Module? Module { get; set; }

    public virtual Exam? OriginalExam { get; set; }

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
}
