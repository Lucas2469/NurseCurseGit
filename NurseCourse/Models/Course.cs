using System;
using System.Collections.Generic;

namespace NurseCourse.Models;

public partial class Course
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();

    public virtual ICollection<Module> Modules { get; set; } = new List<Module>();
}
