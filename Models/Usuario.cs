using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionPlazasVacantes.Models
{
    public enum RolUsuario
    {
        Jefe,
        Colaborador
    }

    [Table("Usuarios", Schema = "auth")]
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Username { get; set; } = null!;

        [Required, StringLength(150)]
        public string FullName { get; set; } = null!;

        [Required, StringLength(150)]
        public string Email { get; set; } = null!;

        // 🔒 Hash de contraseña (BCrypt) - NUNCA almacenar en texto plano
        [Required, StringLength(250)]
        public string PasswordHash { get; set; } = null!;

        /// <summary>
        /// Establece la contraseña hasheándola con BCrypt
        /// </summary>
        public void SetPassword(string password)
        {
            PasswordHash = Services.PasswordHasher.HashPassword(password);
        }

        /// <summary>
        /// Verifica si la contraseña proporcionada es correcta
        /// </summary>
        public bool VerifyPassword(string password)
        {
            return Services.PasswordHasher.VerifyPassword(password, PasswordHash);
        }

        [Required]
        public RolUsuario Rol { get; set; } = RolUsuario.Colaborador;

        public bool Activo { get; set; } = true;

        public DateTime? UltimoAccesoUtc { get; set; }
        public DateTime CreadoUtc { get; set; } = DateTime.UtcNow;

        // Relación con plazas asignadas
        public ICollection<PlazaVacante> PlazasAsignadas { get; set; } = new List<PlazaVacante>();
    }
}
