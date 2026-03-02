namespace GestionPlazasVacantes.DTOs
{
    public class ConfirmacionPostulacionDto
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;
        public string PlazaTitulo { get; set; } = string.Empty;
        public DateTime FechaActualizacion { get; set; }
        public string EstadoProceso { get; set; } = string.Empty;
    }
}
