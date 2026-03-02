using GestionPlazasVacantes.DTOs;
using GestionPlazasVacantes.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class PlazasInternasController : Controller
{
    private readonly HttpClient _api;

    public PlazasInternasController(IHttpClientFactory factory)
    {
        _api = factory.CreateClient("Api");
    }

    public async Task<IActionResult> Index()
    {
        var plazas = await _api.GetFromJsonAsync<List<PlazaDto>>(
            "api/plazas-internas");

        return View(plazas ?? []);
    }

    public async Task<IActionResult> Aplicar(int id)
    {
        var plaza = await _api.GetFromJsonAsync<PlazaDto>(
            $"api/plazas-internas/{id}");

        //ViewBag.Plaza = plaza;
        //return View(new Postulante { PlazaVacanteId = id });
        var vm = new Postulacion
        {
            Plaza = plaza,
            Postulante = new PostulanteDto { PlazaVacanteId = id }
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Aplicar(
        Postulacion vm,
        IFormFile curriculum,
        IFormFile? fotoTitulo,
        IFormFile? fotoColegiatura,
        IFormFile? fotoLicencia,
        IFormFile? fotoPermisoArmas)
    {
        var form = new MultipartFormDataContent();

        form.Add(new StringContent(vm.Postulante.PlazaVacanteId.ToString()), "PlazaVacanteId");
        form.Add(new StringContent(vm.Postulante.NombreCompleto), "NombreCompleto");
        form.Add(new StringContent(vm.Postulante.Cedula), "Cedula");
        form.Add(new StringContent(vm.Postulante.Correo), "Correo");

        form.Add(new StreamContent(curriculum.OpenReadStream()), "curriculum", curriculum.FileName);

        if (fotoTitulo != null)
            form.Add(new StreamContent(fotoTitulo.OpenReadStream()), "fotoTitulo", fotoTitulo.FileName);

        if (fotoColegiatura != null)
            form.Add(new StreamContent(fotoColegiatura.OpenReadStream()), "fotoColegiatura", fotoColegiatura.FileName);

        if (fotoLicencia != null)
            form.Add(new StreamContent(fotoLicencia.OpenReadStream()), "fotoLicencia", fotoLicencia.FileName);

        if (fotoPermisoArmas != null)
            form.Add(new StreamContent(fotoPermisoArmas.OpenReadStream()), "fotoPermisoArmas", fotoPermisoArmas.FileName);

        var response = await _api.PostAsync("api/plazas-internas/aplicar", form);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Error al enviar la postulación.");
            return View(vm);
        }

        var result = await response.Content.ReadFromJsonAsync<dynamic>();
        return RedirectToAction("Confirmacion", new { id = (int)result!.Id });
    }

    public async Task<IActionResult> Confirmacion(int id)
    {
        var postulante = await _api.GetFromJsonAsync<ConfirmacionPostulacionDto>(
            $"api/plazas-internas/confirmacion/{id}");

        if (postulante == null)
            return NotFound();

        return View(postulante);
    }
}