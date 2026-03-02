using GestionPlazasVacantes.Models;

namespace GestionPlazasVacantes.DTOs
{
    public class PlazaDto
    {
        public int Id { get; set; }
        public TipoConcursoEnum TipoConcurso { get; set; }
        public string NumeroConcurso { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string? Departamento { get; set; }
        public decimal SalarioCompuesto { get; set; }
        public decimal SalarioGlobal { get; set; }
        public string? Horario { get; set; }
        public DateTime FechaLimite { get; set; }
        public string Requisitos { get; set; } = string.Empty;
        public string? Observaciones { get; set; }
        public bool Activa { get; set; }
        public DateTime FechaCreacion { get; set; }

        public bool SolicitarColegiatura { get; set; }
        public bool ColegiaturaObligatoria { get; set; }
        public bool SolicitarLicencia { get; set; }
        public bool LicenciaObligatoria { get; set; }
        public bool SolicitarPermisoArmas { get; set; }
        public bool ArmasObligatorio { get; set; }
        public bool SolicitarTitulos { get; set; }
        public bool TitulosObligatorios { get; set; }
    }
}