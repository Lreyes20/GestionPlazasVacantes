using GestionPlazasVacantes.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace GestionPlazasVacantes.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;
        public DashboardController(AppDbContext context) => _context = context;

        // Vista del dashboard
        public IActionResult Index() => View();

        // API: devuelve conteos por plaza o departamento
        // /Dashboard/Counts?groupBy=plaza   (default)
        // /Dashboard/Counts?groupBy=departamento
        [HttpGet]
        public async Task<IActionResult> Counts(string groupBy = "plaza")
        {
            var ahora = DateTime.Now;

            // Solo plazas vigentes y abiertas (tu lógica original)
            var query = _context.PlazasVacantes
                .Where(p => p.FechaLimite > ahora && (p.Estado == "Abierta" || p.Estado == null));

            if (groupBy.Equals("departamento", StringComparison.OrdinalIgnoreCase))
            {
                var data = await query
                    .GroupJoin(_context.Postulantes,
                        plaza => plaza.Id,
                        post => post.PlazaVacanteId,
                        (plaza, posts) => new { plaza.Departamento, Count = posts.Count() })
                    .GroupBy(x => x.Departamento)
                    .Select(g => new CountItem
                    {
                        Key = g.Key ?? "Sin departamento",
                        SubKey = null,
                        TotalPostulantes = g.Sum(x => x.Count),
                        PlazasActivas = g.Count(),
                        PlazaId = 0
                    })
                    .OrderByDescending(x => x.TotalPostulantes)
                    .ToListAsync();

                return Json(data);
            }
            else
            {
                var data = await query
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
                    .ToListAsync();

                return Json(data);
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
