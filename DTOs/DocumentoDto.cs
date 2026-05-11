namespace GestionPlazasVacantes.DTOs
{
    public class DocumentoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public bool Obligatorio { get; set; }
    }
}
