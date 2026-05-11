namespace GestionPlazasVacantes.DTOs
{
    public class CatalogoDto
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public bool Activo { get; set; }
    }
}