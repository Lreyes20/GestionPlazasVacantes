using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace GestionPlazasVacantes.Models
{
    [Table("Postulantes", Schema = "gestion")]
    public class Postulante
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, StringLength(300)]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required, StringLength(40)]
        public string Cedula { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Correo { get; set; }

        [StringLength(60)]
        public string? Telefono { get; set; }

        [StringLength(250)]
        public string? Direccion { get; set; }

        // ===== Información Curricular =====
        public string? PerfilProfesional { get; set; }
        public string? ExperienciaLaboral { get; set; }
        public string? FormacionAcademica { get; set; }
        public string? Habilidades { get; set; }
        public string? Idiomas { get; set; }
        public string? FormacionComplementaria { get; set; }
        public string? OtrosDatos { get; set; }

        // ===== Datos adicionales =====
        public string? NumeroColegiatura { get; set; }
        public string? TipoLicencia { get; set; }
        public string? NumeroPermisoArmas { get; set; }

        // ===== Archivos adjuntos =====
        [StringLength(500)]
        public string? CurriculumPath { get; set; }
        [StringLength(500)]
        public string? FotoColegiaturaPath { get; set; }
        [StringLength(500)]
        public string? FotoLicenciaPath { get; set; }
        [StringLength(500)]
        public string? FotoPermisoArmasPath { get; set; }
        [StringLength(500)]
        public string? ArchivoTitulosPath { get; set; }
        public string? FotoTituloPath { get; set; }


        // ===== Estado =====
        [StringLength(100)]
        public string? EstadoProceso { get; set; } = "En revisión";
        public DateTime FechaActualizacion { get; set; } = DateTime.Now;

        // ===== Relación con PlazaVacante =====
        [ForeignKey("PlazaVacante")]
        public int PlazaVacanteId { get; set; }

        [ValidateNever]
        public PlazaVacante? PlazaVacante { get; set; }
    }
}
