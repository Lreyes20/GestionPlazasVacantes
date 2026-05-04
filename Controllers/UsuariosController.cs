using GestionPlazasVacantes.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

[Authorize(Roles = "Jefe")]
public class UsuariosController : Controller
{
    private readonly HttpClient _api;

    public UsuariosController(IHttpClientFactory factory)
    {
        _api = factory.CreateClient("Api");
    }

    // LISTADO
    public async Task<IActionResult> Index()
    {
        var data = await _api.GetFromJsonAsync<List<UsuarioDto>>("api/usuarios");
        return View(data ?? new List<UsuarioDto>());
    }

    // CREAR GET
    public IActionResult Crear()
    {
        return View(new CrearUsuarioDto());
    }

    // CREAR POST
    [HttpPost]
    public async Task<IActionResult> Crear(CrearUsuarioDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var response = await _api.PostAsJsonAsync("api/usuarios", dto);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();

            TempData["Error"] = error; // 👈 muestra mensaje del API
            return View(dto);
        }

        TempData["Success"] = "Usuario creado correctamente";
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Editar(int id)
    {
        var user = await _api.GetFromJsonAsync<UsuarioDto>($"api/usuarios/{id}");

        if (user == null)
            return NotFound();

        var vm = new CrearUsuarioDto
        {
            Username = user.Username,
            FullName = user.Nombre,
            Rol = "" // si lo agregas en DTO luego
        };

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Editar(int id, CrearUsuarioDto dto)
    {
        var response = await _api.PutAsJsonAsync($"api/usuarios/{id}", dto);

        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] = "Error al editar usuario";
            return View(dto);
        }

        TempData["Success"] = "Usuario actualizado";
        return RedirectToAction("Index");
    }

    //[HttpPost]
    //public async Task<IActionResult> Toggle(int id)
    //{
    //    await _api.PutAsync($"api/usuarios/toggle/{id}", null);
    //    return RedirectToAction("Index");
    //}

    [HttpPost]
    public async Task<IActionResult> Toggle(int id)
    {
        var response = await _api.PutAsync($"api/usuarios/toggle/{id}", null);

        if (!response.IsSuccessStatusCode)
            return StatusCode(500);

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(int id, string nuevaPassword)
    {
        if (string.IsNullOrWhiteSpace(nuevaPassword) || nuevaPassword.Length < 6)
            return BadRequest("Password inválido");

        var response = await _api.PutAsJsonAsync(
            $"api/usuarios/reset-password/{id}",
            new { NuevaPassword = nuevaPassword });

        if (!response.IsSuccessStatusCode)
            return StatusCode(500);

        return Ok();
    }
}