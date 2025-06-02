using NurseCourse.Models;

namespace NurseCourse.ViewModels
{
    public class UserDetailsViewModel
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Nickname { get; set; }
        public string Picture { get; set; }
        public List<Role> Roles { get; set; } = new List<Role>();
        public ICollection<string> AssignedRoleIds { get; set; }

        // Datos adicionales de la base de datos
        public int? Age { get; set; }
        public string Gender { get; set; }
        public string IdNumber { get; set; }
        public string CountryOfOrigin { get; set; }
        public string StateOfOrigin { get; set; }
        public string CityOfOrigin { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Occupation { get; set; }
        public string SpecifyOccupation { get; set; }
        public string HealthProfession { get; set; }
        public string EducationLevel { get; set; }
        public string Institution { get; set; }
        public string Workplace { get; set; }

        public List<string> EnrolledCourses { get; set; } = new List<string>();
    }
}
