using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace NurseCourse.Models;

public partial class ModularCourseDbContext : DbContext
{
    public ModularCourseDbContext()
    {
    }

    public ModularCourseDbContext(DbContextOptions<ModularCourseDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Certificate> Certificates { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<Exam> Exams { get; set; }

    public virtual DbSet<ExamAttempt> ExamAttempts { get; set; }

    public virtual DbSet<Module> Modules { get; set; }

    public virtual DbSet<Option> Options { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost;Database=ModularCourseDB;User Id=sa;Password=Dor75707; Trusted_Connection=True; Encrypt=False;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Certificate>(entity =>
        {
            entity.HasKey(e => e.CertificateId).HasName("PK__Certific__BBF8A7E19C63247C");

            entity.Property(e => e.CertificateId).HasColumnName("CertificateID");
            entity.Property(e => e.CertificatePath).HasMaxLength(255);
            entity.Property(e => e.CourseId).HasColumnName("CourseID");
            entity.Property(e => e.IssueDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Course).WithMany(p => p.Certificates)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("FK__Certifica__Cours__5812160E");

            entity.HasOne(d => d.User).WithMany(p => p.Certificates)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Certifica__UserI__571DF1D5");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Course__3213E83F8AE322D7");

            entity.ToTable("Course");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Exam>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Exam__3213E83F8A3021C1");

            entity.ToTable("Exam");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreationDate)
                .HasColumnType("datetime")
                .HasColumnName("creation_date");
            entity.Property(e => e.IsMakeup)
                .HasDefaultValue(0)
                .HasColumnName("is_makeup");
            entity.Property(e => e.ModuleId).HasColumnName("module_id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.OriginalExamId).HasColumnName("original_exam_id");

            entity.HasOne(d => d.Module).WithMany(p => p.Exams)
                .HasForeignKey(d => d.ModuleId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Exam__module_id__4316F928");

            entity.HasOne(d => d.OriginalExam).WithMany(p => p.InverseOriginalExam)
                .HasForeignKey(d => d.OriginalExamId)
                .HasConstraintName("FK__Exam__original_e__440B1D61");
        });

        modelBuilder.Entity<ExamAttempt>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ExamAtte__3213E83F06DEEADA");

            entity.ToTable("ExamAttempt");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AttemptDate)
                .HasColumnType("datetime")
                .HasColumnName("attempt_date");
            entity.Property(e => e.ExamId).HasColumnName("exam_id");
            entity.Property(e => e.IsMakeup)
                .HasDefaultValue(0)
                .HasColumnName("is_makeup");
            entity.Property(e => e.Score).HasColumnName("score");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Exam).WithMany(p => p.ExamAttempts)
                .HasForeignKey(d => d.ExamId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__ExamAttem__exam___534D60F1");

            entity.HasOne(d => d.User).WithMany(p => p.ExamAttempts)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__ExamAttem__user___52593CB8");
        });

        modelBuilder.Entity<Module>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Module__3213E83F3324D992");

            entity.ToTable("Module");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CourseId).HasColumnName("course_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.PdfPath)
                .HasMaxLength(255)
                .HasColumnName("pdf_path");
            entity.Property(e => e.VideoPath)
                .IsUnicode(false)
                .HasColumnName("video_path");

            entity.HasOne(d => d.Course).WithMany(p => p.Modules)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("FK__Module__course_i__3F466844");
        });

        modelBuilder.Entity<Option>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Option__3213E83FE6D5E3AE");

            entity.ToTable("Option");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IsCorrect)
                .HasDefaultValue(false)
                .HasColumnName("is_correct");
            entity.Property(e => e.OptionText)
                .HasMaxLength(255)
                .HasColumnName("option_text");
            entity.Property(e => e.QuestionId).HasColumnName("question_id");

            entity.HasOne(d => d.Question).WithMany(p => p.Options)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Option__question__4AB81AF0");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Question__3213E83F060C57E4");

            entity.ToTable("Question");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CorrectAnswer).HasColumnName("correct_answer");
            entity.Property(e => e.ExamId).HasColumnName("exam_id");
            entity.Property(e => e.QuestionText).HasColumnName("question_text");
            entity.Property(e => e.QuestionType).HasColumnName("question_type");

            entity.HasOne(d => d.Exam).WithMany(p => p.Questions)
                .HasForeignKey(d => d.ExamId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Question_Exam");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__User__3213E83FD9CFFDFA");

            entity.ToTable("User");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Age).HasColumnName("age");
            entity.Property(e => e.BirthDate).HasColumnName("birth_date");
            entity.Property(e => e.CityOfOrigin)
                .HasMaxLength(255)
                .HasColumnName("city_of_origin");
            entity.Property(e => e.CountryOfOrigin)
                .HasMaxLength(255)
                .HasColumnName("country_of_origin");
            entity.Property(e => e.EducationLevel)
                .HasMaxLength(50)
                .HasColumnName("education_level");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(255)
                .HasColumnName("first_name");
            entity.Property(e => e.Gender)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("gender");
            entity.Property(e => e.HealthProfession)
                .HasMaxLength(50)
                .HasColumnName("health_profession");
            entity.Property(e => e.IdNumber)
                .HasMaxLength(255)
                .HasColumnName("id_number");
            entity.Property(e => e.Institution)
                .HasMaxLength(255)
                .HasColumnName("institution");
            entity.Property(e => e.LastName)
                .HasMaxLength(255)
                .HasColumnName("last_name");
            entity.Property(e => e.Occupation)
                .HasMaxLength(50)
                .HasColumnName("occupation");
            entity.Property(e => e.SpecifyOccupation)
                .HasMaxLength(255)
                .HasColumnName("specify_occupation");
            entity.Property(e => e.StateOfOrigin)
                .HasMaxLength(255)
                .HasColumnName("state_of_origin");
            entity.Property(e => e.Workplace)
                .HasMaxLength(255)
                .HasColumnName("workplace");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
