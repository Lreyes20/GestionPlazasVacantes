using GestionPlazasVacantes.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

[Authorize(Roles = "Jefe")]
public class DepartamentosController : Controller
{
    private readonly HttpClient _api;

    public DepartamentosController(IHttpClientFactory factory)
    {
        _api = factory.CreateClient("Api");
    }

    // =========================================================
    // 🔥 INDEX
    // =========================================================

    public async Task<IActionResult> Index()
    {
        var departamentos = await _api
            .GetFromJsonAsync<List<CatalogoDto>>("api/catalogos/departamentos")
            ?? new List<CatalogoDto>();

        return View(departamentos);
    }

    // =========================================================
    // 🔥 CREAR DEPARTAMENTO (AJAX)
    // =========================================================

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CatalogoDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return BadRequest("Nombre requerido");

        var response = await _api.PostAsJsonAsync("api/catalogos/departamentos", dto);

        if (!response.IsSuccessStatusCode)
            return StatusCode(500);

        return Ok();
    }

    // =========================================================
    // 🔥 EDITAR DEPARTAMENTO (AJAX)
    // =========================================================

    [HttpPost]
    public async Task<IActionResult> Editar(int id, [FromBody] CatalogoDto dto)
    {
        var response = await _api.PutAsJsonAsync($"api/catalogos/departamentos/{id}", dto);

        if (!response.IsSuccessStatusCode)
            return StatusCode(500);

        return Ok();
    }

    // =========================================================
    // 🔥 TOGGLE DEPARTAMENTO (AJAX)
    // =========================================================

    [HttpPost]
    public async Task<IActionResult> Toggle(int id)
    {
        var response = await _api.PutAsync($"api/catalogos/departamentos/toggle/{id}", null);

        if (!response.IsSuccessStatusCode)
            return StatusCode(500);

        return Ok();
    }
}
