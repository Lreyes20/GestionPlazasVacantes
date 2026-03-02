using GestionPlazasVacantes.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace GestionPlazasVacantes.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly HttpClient _api;

        public DashboardController(IHttpClientFactory factory)
        {
            _api = factory.CreateClient("Api");
        }

        public IActionResult Index() => View();

        [HttpGet]
        public async Task<IActionResult> Counts(string groupBy = "plaza")
        {
            var data = await _api.GetFromJsonAsync<List<DashboardCountDto>>(
                $"api/dashboard/counts?groupBy={groupBy}");

            return Json(data ?? new());
        }
    }
}