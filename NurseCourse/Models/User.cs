using System;
using System.Collections.Generic;

namespace NurseCourse.Models;

public partial class User
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public int Age { get; set; }

    public string Gender { get; set; } = null!;

    public string IdNumber { get; set; } = null!;

    public string CountryOfOrigin { get; set; } = null!;

    public string? StateOfOrigin { get; set; }

    public string? CityOfOrigin { get; set; }

    public DateOnly BirthDate { get; set; }

    public string Occupation { get; set; } = null!;

    public string? SpecifyOccupation { get; set; }

    public string? HealthProfession { get; set; }

    public string? EducationLevel { get; set; }

    public string? Institution { get; set; }

    public string? Workplace { get; set; }

    public string FullName => $"{FirstName} {LastName}";

    public virtual ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();

    public virtual ICollection<ExamAttempt> ExamAttempts { get; set; } = new List<ExamAttempt>();
}
