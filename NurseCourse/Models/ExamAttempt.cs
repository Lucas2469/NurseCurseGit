using System;
using System.Collections.Generic;

namespace NurseCourse.Models;

public partial class ExamAttempt
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public int? ExamId { get; set; }

    public DateTime AttemptDate { get; set; }

    public double Score { get; set; }

    public int? IsMakeup { get; set; }

    public virtual Exam? Exam { get; set; }

    public virtual User? User { get; set; }
}
