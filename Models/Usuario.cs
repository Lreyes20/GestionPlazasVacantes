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

        // 🔹 Contraseña en texto plano (simple por ahora)
        [Required, StringLength(100)]
        public string Password { get; set; } = null!;

        [Required]
        public RolUsuario Rol { get; set; } = RolUsuario.Colaborador;

        public bool Activo { get; set; } = true;

        public DateTime? UltimoAccesoUtc { get; set; }
        public DateTime CreadoUtc { get; set; } = DateTime.UtcNow;

        // Relación con plazas asignadas
        public ICollection<PlazaVacante> PlazasAsignadas { get; set; } = new List<PlazaVacante>();
    }
}
