namespace GestionPlazasVacantes.DTOs
{
    public class PostulacionDto
    {
        public int PlazaVacanteId { get; set; }

        public List<string> DocumentosEnviados { get; set; } = new();
    }
}
