using System.ComponentModel.DataAnnotations;

namespace GestionPlazasVacantes.Models
{
    public class LoginVM
    {
        [Required, Display(Name = "Usuario")]
        public string Username { get; set; } = null!;

        [Required, DataType(DataType.Password), Display(Name = "Contraseña")]
        public string Password { get; set; } = null!;

        [Display(Name = "Recordarme")]
        public bool RememberMe { get; set; }
    }
}

