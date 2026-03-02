using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GestionPlazasVacantes.Models
{
    public class PlazaVacante
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public TipoConcursoEnum TipoConcurso { get; set; }

        [Required, StringLength(50)]
        public string NumeroConcurso { get; set; } = string.Empty;

        [Required, StringLength(150)]
        public string Titulo { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Departamento { get; set; }

        [Required, Range(typeof(decimal), "0", "9999999999", ErrorMessage = "Ingrese un valor válido para el salario.")]
        public decimal SalarioCompuesto { get; set; }

        [Required, Range(typeof(decimal), "0", "9999999999", ErrorMessage = "Ingrese un valor válido para el salario.")]
        public decimal SalarioGlobal { get; set; }

        [StringLength(100)]
        public string? Horario { get; set; }

        [Required]
        public DateTime FechaLimite { get; set; }

        [Required, StringLength(1000)]
        public string Requisitos { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Observaciones { get; set; }
        public bool Activa { get; set; } = true;


        public bool SolicitarColegiatura { get; set; }
        public bool ColegiaturaObligatoria { get; set; }

        public bool SolicitarLicencia { get; set; }
        public bool LicenciaObligatoria { get; set; }

        public bool SolicitarPermisoArmas { get; set; }
        public bool ArmasObligatorio { get; set; }

        public bool SolicitarTitulos { get; set; }
        public bool TitulosObligatorios { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [Required, StringLength(20)]
        public string Estado { get; set; } = "Abierta";
        [StringLength(20)]
        public string EstadoFinal { get; set; } = "En proceso";

        public DateTime? FechaCierre { get; set; }

        // Asignación a colaborador de RRHH
        public int? UsuarioAsignadoId { get; set; }
        public Usuario? UsuarioAsignado { get; set; }

        public ICollection<Postulante> Postulantes { get; set; } = new List<Postulante>();
    }
}
