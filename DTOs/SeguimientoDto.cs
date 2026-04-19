namespace GestionPlazasVacantes.DTOs
{
    public class SeguimientoDto
    {
        public int Id { get; set; }

        public int PostulanteId { get; set; }

        public int PlazaVacanteId { get; set; }

        public string EtapaActual { get; set; } = string.Empty;

        public bool CumpleRequisitos { get; set; }

        public decimal? NotaPruebaTecnica { get; set; }

        public decimal? NotaPsicometrica { get; set; }

        public string? Observaciones { get; set; }

        public bool Aprobado { get; set; }

        public string? MotivoDescarte { get; set; }

        public bool Activo { get; set; }

        public DateTime FechaActualizacion { get; set; }
    }
}