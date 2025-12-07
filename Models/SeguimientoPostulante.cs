using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionPlazasVacantes.Models
{
    [Table("SeguimientosPostulantes", Schema = "gestion")]
    public class SeguimientoPostulante
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Postulante")]
        public int PostulanteId { get; set; }
        public Postulante Postulante { get; set; }

        [StringLength(50)]
        public string? EtapaActual { get; set; }

        [StringLength(200)]
        public string? Observaciones { get; set; }

        public decimal? NotaPruebaTecnica { get; set; }
        public decimal? NotaPsicometrica { get; set; }

        public bool CumpleRequisitos { get; set; } = true;
        public bool Aprobado { get; set; } = false;
        public DateTime FechaActualizacion { get; set; } = DateTime.Now;

        //  Campos nuevos (solo una vez)
        [StringLength(300)]
        public string? MotivoDescarte { get; set; } // razón por la cual fue descartado

        [Column(TypeName = "bit")]
        public bool Activo { get; set; } = true; // sirve para filtrar los descartados

        [ForeignKey("PlazaVacante")]
        public int PlazaVacanteId { get; set; }
        public PlazaVacante PlazaVacante { get; set; }
    }
}
