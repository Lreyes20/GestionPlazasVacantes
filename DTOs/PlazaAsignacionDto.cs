namespace GestionPlazasVacantes.DTOs
{
    public class PlazaAsignacionDto
    {
        public int Id { get; set; }

        public string Titulo { get; set; } = string.Empty;
        public string TipoConcurso { get; set; } = string.Empty;
        public string NumeroConcurso { get; set; } = string.Empty;
        public string? Departamento { get; set; }
        public DateTime FechaLimite { get; set; }

        public int CantidadPostulantes { get; set; }

        public int? UsuarioAsignadoId { get; set; }
        public string? UsuarioAsignadoNombre { get; set; }
    }
}
