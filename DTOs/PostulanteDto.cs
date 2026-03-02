using System;
using System.ComponentModel.DataAnnotations;

namespace GestionPlazasVacantes.DTOs
{
    public class PostulanteDto
    {
        public int Id { get; set; }

        public int PlazaVacanteId { get; set; }

        [Required]
        [StringLength(150)]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Cedula { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Correo { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Telefono { get; set; }

        [StringLength(250)]
        public string? Direccion { get; set; }

        public string? PerfilProfesional { get; set; }
        public string? ExperienciaLaboral { get; set; }
        public string? FormacionAcademica { get; set; }
        public string? Habilidades { get; set; }
        public string? Idiomas { get; set; }
        public string? FormacionComplementaria { get; set; }
        public string? OtrosDatos { get; set; }

        public string? NumeroColegiatura { get; set; }
        public string? TipoLicencia { get; set; }
        public string? NumeroPermisoArmas { get; set; }

        public string EstadoProceso { get; set; } = "En revisión";
        public DateTime FechaActualizacion { get; set; } = DateTime.Now;
    }
}