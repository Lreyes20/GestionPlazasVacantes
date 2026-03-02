namespace GestionPlazasVacantes.DTOs
{
    public class DashboardCountDto
    {
        public string Key { get; set; } = "";
        public string? SubKey { get; set; }
        public int TotalPostulantes { get; set; }
        public int PlazasActivas { get; set; }
        public int PlazaId { get; set; }
    }
}
