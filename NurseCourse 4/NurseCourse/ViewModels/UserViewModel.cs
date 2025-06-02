using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace NurseCourse.Models
{
    public class UserViewModel
    {
        
        public string UserId { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico es inválido.")]
        public string Email { get; set; }

        public bool Blocked { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "El apodo es obligatorio.")]
        [StringLength(50, ErrorMessage = "El apodo no puede superar los 50 caracteres.")]
        public string Nickname { get; set; }

        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        [StringLength(50, ErrorMessage = "El apodo no puede superar los 50 caracteres.")]
        public string UserName { get; set; }
        public string Connection { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [MinLength(10, ErrorMessage = "La contraseña debe tener al menos 10 caracteres.")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$", ErrorMessage = "La contraseña debe contener al menos una letra mayúscula, una letra minúscula, un número y un carácter especial.")]
        public string Password { get; set; }
    }
}
