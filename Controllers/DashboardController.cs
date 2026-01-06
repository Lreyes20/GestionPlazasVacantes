using GestionPlazasVacantes.Data;
using GestionPlazasVacantes.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace GestionPlazasVacantes.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IPlazaVacanteRepository _plazaRepository;
        private readonly AppDbContext _context;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            IPlazaVacanteRepository plazaRepository,
            AppDbContext context,
            ILogger<DashboardController> logger)
        {
            _plazaRepository = plazaRepository;
            _context = context;
            _logger = logger;
        }

        // Vista del dashboard
        public IActionResult Index() => View();

        // Vista de detalle de plaza con postulantes
        public async Task<IActionResult> Plaza(int id)
        {
            try
            {
                var plaza = await _plazaRepository.GetPlazaConPostulantesAsync(id);

                if (plaza == null)
                {
                    _logger.LogWarning("Plaza no encontrada: {PlazaId}", id);
                    return NotFound();
                }

                ViewBag.Postulantes = plaza.Postulantes.ToList();
                return View(plaza);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar plaza: {PlazaId}", id);
                throw;
            }
        }

        // API: devuelve conteos por plaza o departamento
        // /Dashboard/Counts?groupBy=plaza   (default)
        // /Dashboard/Counts?groupBy=departamento
        [HttpGet]
        public async Task<IActionResult> Counts(string groupBy = "plaza")
        {
            try
            {
                var plazasActivas = await _plazaRepository.GetPlazasActivasAsync();

                if (groupBy.Equals("departamento", StringComparison.OrdinalIgnoreCase))
                {
                    var data = plazasActivas
                        .GroupBy(p => p.Departamento)
                        .Select(g => new CountItem
                        {
                            Key = g.Key ?? "Sin departamento",
                            SubKey = null,
                            TotalPostulantes = _context.Postulantes
                                .Count(post => g.Select(p => p.Id).Contains(post.PlazaVacanteId)),
                            PlazasActivas = g.Count(),
                            PlazaId = 0
                        })
                        .OrderByDescending(x => x.TotalPostulantes)
                        .ToList();

                    return Json(data);
                }
                else
                {
                    var data = plazasActivas
                        .Select(plaza => new CountItem
                        {
                            Key = plaza.Titulo,
                            SubKey = plaza.Departamento,
                            TotalPostulantes = _context.Postulantes
                                .Count(p => p.PlazaVacanteId == plaza.Id),
                            PlazasActivas = 1,
                            PlazaId = plaza.Id
                        })
                        .OrderByDescending(x => x.TotalPostulantes)
                        .ToList();

                    return Json(data);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener conteos del dashboard");
                return StatusCode(500, new { error = "Error al cargar datos" });
            }
        }

        public class CountItem
        {
            public string Key { get; set; } = "";
            public string? SubKey { get; set; }
            public int TotalPostulantes { get; set; }
            public int PlazasActivas { get; set; }
            public int PlazaId { get; set; }
        }
    }
}
