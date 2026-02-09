namespace GestionPlazasVacantes.DTOs
{
    public class LoginResponseDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
    }

}
