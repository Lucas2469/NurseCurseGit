using System;
using System.Collections.Generic;

namespace NurseCourse.Models;

public partial class Module
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int? CourseId { get; set; }

    public string? PdfPath { get; set; }

    public string? VideoPath { get; set; }

    public virtual Course? Course { get; set; }

    public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();
}
