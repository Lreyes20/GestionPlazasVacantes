namespace GestionPlazasVacantes.DTOs
{
    public class PostulanteArchivoDto
    {
        public int Id { get; set; }

        public int PostulanteId { get; set; }

        public string Ruta { get; set; } = string.Empty;

        public string Tipo { get; set; } = string.Empty;
    }
}
