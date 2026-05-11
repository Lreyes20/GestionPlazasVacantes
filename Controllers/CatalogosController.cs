using GestionPlazasVacantes.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

[Authorize(Roles = "Jefe")]
public class CatalogosController : Controller
{
    private readonly HttpClient _api;

    public CatalogosController(IHttpClientFactory factory)
    {
        _api = factory.CreateClient("Api");
    }

    // =========================================================
    // 🔥 INDEX
    // =========================================================

    public async Task<IActionResult> Index()
    {
        var requisitos = await _api
            .GetFromJsonAsync<List<CatalogoDto>>("api/catalogos/requisitos")
            ?? new List<CatalogoDto>();

        var documentos = await _api
            .GetFromJsonAsync<List<CatalogoDto>>("api/catalogos/documentos")
            ?? new List<CatalogoDto>();

        ViewBag.Documentos = documentos;

        return View(requisitos);
    }

    // =========================================================
    // 🔥 CREAR REQUISITO (AJAX)
    // =========================================================

    [HttpPost]
    public async Task<IActionResult> CrearRequisito([FromBody] CatalogoDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return BadRequest("Nombre requerido");

        var response = await _api.PostAsJsonAsync("api/catalogos/requisitos", dto);

        if (!response.IsSuccessStatusCode)
            return StatusCode(500);

        return Ok();
    }

    // =========================================================
    // 🔥 CREAR DOCUMENTO (AJAX)
    // =========================================================

    [HttpPost]
    public async Task<IActionResult> CrearDocumento([FromBody] CatalogoDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return BadRequest("Nombre requerido");

        var response = await _api.PostAsJsonAsync("api/catalogos/documentos", dto);

        if (!response.IsSuccessStatusCode)
            return StatusCode(500);

        return Ok();
    }

    // =========================================================
    // 🔥 TOGGLE REQUISITO (AJAX)
    // =========================================================

    [HttpPost]
    public async Task<IActionResult> ToggleRequisito(int id)
    {
        var response = await _api.PutAsync($"api/catalogos/requisitos/toggle/{id}", null);

        if (!response.IsSuccessStatusCode)
            return StatusCode(500);

        return Ok();
    }

    // =========================================================
    // 🔥 TOGGLE DOCUMENTO (AJAX)
    // =========================================================

    [HttpPost]
    public async Task<IActionResult> ToggleDocumento(int id)
    {
        var response = await _api.PutAsync($"api/catalogos/documentos/toggle/{id}", null);

        if (!response.IsSuccessStatusCode)
            return StatusCode(500);

        return Ok();
    }

    // 🔥 EDITAR REQUISITO
    [HttpPost]
    public async Task<IActionResult> EditarRequisito(int id, [FromBody] CatalogoDto dto)
    {
        var response = await _api.PutAsJsonAsync($"api/catalogos/requisitos/{id}", dto);

        if (!response.IsSuccessStatusCode)
            return StatusCode(500);

        return Ok();
    }

    // 🔥 EDITAR DOCUMENTO
    [HttpPost]
    public async Task<IActionResult> EditarDocumento(int id, [FromBody] CatalogoDto dto)
    {
        var response = await _api.PutAsJsonAsync($"api/catalogos/documentos/{id}", dto);

        if (!response.IsSuccessStatusCode)
            return StatusCode(500);

        return Ok();
    }
}