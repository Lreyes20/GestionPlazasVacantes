using GestionPlazasVacantes.Data;
using GestionPlazasVacantes.DTOs;
using GestionPlazasVacantes.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionPlazasVacantes.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == dto.Username && u.Activo);

            if (user == null || !user.VerifyPassword(dto.Password))
            {
                return Unauthorized(new { message = "Usuario o contraseña inválidos" });
            }

            // Mapear a DTO de respuesta (mismo que espera AccountController)
            var response = new LoginResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Rol = user.Rol.ToString()
            };

            return Ok(response);
        }
    }
}
