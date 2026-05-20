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
            try
            {
                var response = await _api.GetAsync(
                    $"api/dashboard/counts?groupBy={groupBy}");

                if (!response.IsSuccessStatusCode)
                    return Json(new List<DashboardCountDto>());

                var data = await response.Content
                    .ReadFromJsonAsync<List<DashboardCountDto>>();

                return Json(data ?? new());
            }
            catch (Exception)
            {
                return Json(new List<DashboardCountDto>());
            }
        }
    }
}