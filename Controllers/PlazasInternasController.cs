using DocumentFormat.OpenXml.Drawing.Diagrams;
using GestionPlazasVacantes.DTOs;
using GestionPlazasVacantes.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text.Json;

[Authorize]
public class PlazasInternasController : Controller
{
    private readonly HttpClient _api;
    private readonly IWebHostEnvironment _env;

    public PlazasInternasController(IHttpClientFactory factory, IWebHostEnvironment env)
    {
        _api = factory.CreateClient("Api");
        _env = env;
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
        // Reconstruir si el model binding falló (común con multipart)
        if (vm?.Postulante == null || vm.Postulante.PlazaVacanteId == 0)
        {
            vm = new Postulacion
            {
                Postulante = new PostulanteDto
                {
                    PlazaVacanteId = int.Parse(Request.Form["Postulante.PlazaVacanteId"].FirstOrDefault() ?? "0"),
                    NombreCompleto = Request.Form["Postulante.NombreCompleto"].FirstOrDefault() ?? "",
                    Cedula = Request.Form["Postulante.Cedula"].FirstOrDefault() ?? "",
                    Correo = Request.Form["Postulante.Correo"].FirstOrDefault() ?? "",
                    Telefono = Request.Form["Postulante.Telefono"].FirstOrDefault(),
                    Direccion = Request.Form["Postulante.Direccion"].FirstOrDefault(),
                    PerfilProfesional = Request.Form["Postulante.PerfilProfesional"].FirstOrDefault(),
                    ExperienciaLaboral = Request.Form["Postulante.ExperienciaLaboral"].FirstOrDefault(),
                    FormacionAcademica = Request.Form["Postulante.FormacionAcademica"].FirstOrDefault(),
                    Habilidades = Request.Form["Postulante.Habilidades"].FirstOrDefault(),
                    Idiomas = Request.Form["Postulante.Idiomas"].FirstOrDefault(),
                    FormacionComplementaria = Request.Form["Postulante.FormacionComplementaria"].FirstOrDefault(),
                    OtrosDatos = Request.Form["Postulante.OtrosDatos"].FirstOrDefault()
                }
            };
        }

        // Recargar Plaza
        if (vm.Plaza == null && vm.Postulante.PlazaVacanteId > 0)
        {
            vm.Plaza = await _api.GetFromJsonAsync<PlazaDto>(
                $"api/plazas-internas/{vm.Postulante.PlazaVacanteId}");
        }

        if (curriculum == null || curriculum.Length == 0)
        {
            ModelState.AddModelError("", "Debe adjuntar el currículum.");
            return View(vm);
        }

        var form = new MultipartFormDataContent();

        form.Add(new StringContent(vm.Postulante.PlazaVacanteId.ToString()), "PlazaVacanteId");
        form.Add(new StringContent(vm.Plaza?.TipoConcurso ?? "Interno"), "TipoConcurso");

        form.Add(new StringContent(vm.Postulante.NombreCompleto), "NombreCompleto");
        form.Add(new StringContent(vm.Postulante.Cedula), "Cedula");
        form.Add(new StringContent(vm.Postulante.Correo), "Correo");
        form.Add(new StringContent(vm.Postulante.Telefono ?? ""), "Telefono");
        form.Add(new StringContent(vm.Postulante.Direccion ?? ""), "Direccion");

        form.Add(new StringContent(vm.Postulante.PerfilProfesional ?? ""), "PerfilProfesional");
        form.Add(new StringContent(vm.Postulante.ExperienciaLaboral ?? ""), "ExperienciaLaboral");
        form.Add(new StringContent(vm.Postulante.FormacionAcademica ?? ""), "FormacionAcademica");
        form.Add(new StringContent(vm.Postulante.Habilidades ?? ""), "Habilidades");
        form.Add(new StringContent(vm.Postulante.Idiomas ?? ""), "Idiomas");
        form.Add(new StringContent(vm.Postulante.FormacionComplementaria ?? ""), "FormacionComplementaria");
        form.Add(new StringContent(vm.Postulante.OtrosDatos ?? ""), "OtrosDatos");

        // Archivos
        form.Add(new StreamContent(curriculum.OpenReadStream()), "curriculum", curriculum.FileName);

        if (fotoTitulo != null && fotoTitulo.Length > 0)
            form.Add(new StreamContent(fotoTitulo.OpenReadStream()), "fotoTitulo", fotoTitulo.FileName);

        if (fotoColegiatura != null && fotoColegiatura.Length > 0)
            form.Add(new StreamContent(fotoColegiatura.OpenReadStream()), "fotoColegiatura", fotoColegiatura.FileName);

        if (fotoLicencia != null && fotoLicencia.Length > 0)
            form.Add(new StreamContent(fotoLicencia.OpenReadStream()), "fotoLicencia", fotoLicencia.FileName);

        if (fotoPermisoArmas != null && fotoPermisoArmas.Length > 0)
            form.Add(new StreamContent(fotoPermisoArmas.OpenReadStream()), "fotoPermisoArmas", fotoPermisoArmas.FileName);

        foreach (var archivo in Request.Form.Files.Where(f => f.Name == "ArchivoTitulos"))
        {
            if (archivo.Length > 0)
                form.Add(new StreamContent(archivo.OpenReadStream()), "archivoTitulos", archivo.FileName);
        }

        var response = await _api.PostAsync("api/plazas-internas/aplicar", form);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", error);
            return View(vm);
        }

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        int id = json.TryGetProperty("Id", out var idElement) ? idElement.GetInt32() : 0;

        if (id > 0)
            return RedirectToAction("Confirmacion", new { id });

        ModelState.AddModelError("", "Postulación registrada pero no se obtuvo el ID.");
        return View(vm);
    }

    public async Task<IActionResult> Confirmacion(int id)
    {
        var postulante = await _api.GetFromJsonAsync<ConfirmacionPostulacionDto>(
            $"api/plazas-internas/confirmacion/{id}");

        if (postulante == null)
            return NotFound();

        return View(postulante);
    }

    // 🔥 PDF
    public async Task<IActionResult> DescargarCVPdf(int id)
    {
        var modelo = await _api.GetFromJsonAsync<Postulante>(
            $"api/postulacion/{id}");

        if (modelo == null)
            return NotFound();

        QuestPDF.Settings.License = LicenseType.Community;

        // 🔥 FIX LOGO (CORRECTO EN MVC)
        var logoPath = Path.Combine(_env.WebRootPath, "images", "logo.png");

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(40);

                page.Content().Column(col =>
                {
                    col.Spacing(15);

                    // 🔷 HEADER INSTITUCIONAL
                    col.Item().Row(row =>
                    {
                        if (System.IO.File.Exists(logoPath))
                            row.ConstantItem(80).Height(80).Image(logoPath);

                        row.RelativeItem().AlignRight().Column(c =>
                        {
                            c.Item().Text("CURRÍCULUM VITAE")
                                .FontSize(20)
                                .Bold()
                                .FontColor(Colors.Blue.Darken2);

                            c.Item().Text("Municipalidad de Curridabat")
                                .FontSize(12)
                                .FontColor(Colors.Grey.Darken1);
                        });
                    });

                    // 🔥 LÍNEA CORPORATIVA (TE FALTABA)
                    col.Item().LineHorizontal(2)
                        .LineColor(Colors.Blue.Darken2);

                    // 🔷 CAJA PRINCIPAL
                    col.Item().Border(1)
                        .BorderColor(Colors.Grey.Lighten2)
                        .Background(Colors.Grey.Lighten5)
                        .Padding(20)
                        .Column(main =>
                        {
                            main.Spacing(12);

                            // 👤 NOMBRE DESTACADO
                            main.Item().Text(modelo.NombreCompleto)
                                .FontSize(16)
                                .Bold();

                            // 📌 DATOS
                            main.Item().Text($"Cédula: {modelo.Cedula}");
                            main.Item().Text($"Correo: {modelo.Correo}");
                            main.Item().Text($"Teléfono: {modelo.Telefono}");
                            main.Item().Text($"Dirección: {modelo.Direccion}");

                            main.Item().PaddingVertical(5)
                                .LineHorizontal(1);

                            // 🔷 FUNCIÓN SECCIONES
                            void Sec(string titulo, string? contenido)
                            {
                                if (string.IsNullOrWhiteSpace(contenido))
                                    return;

                                main.Item().PaddingTop(10).Column(sec =>
                                {
                                    sec.Item().Text(titulo)
                                        .FontSize(12)
                                        .Bold()
                                        .FontColor(Colors.Blue.Darken2);

                                    sec.Item().LineHorizontal(1)
                                        .LineColor(Colors.Grey.Lighten2);

                                    sec.Item().PaddingTop(5)
                                        .Text(contenido)
                                        .FontSize(11);
                                });
                            }

                            // 🔥 SECCIONES CV
                            Sec("Perfil Profesional", modelo.PerfilProfesional);
                            Sec("Experiencia Laboral", modelo.ExperienciaLaboral);
                            Sec("Formación Académica", modelo.FormacionAcademica);
                            Sec("Habilidades", modelo.Habilidades);
                            Sec("Idiomas", modelo.Idiomas);
                            Sec("Formación Complementaria", modelo.FormacionComplementaria);
                            Sec("Otros Datos", modelo.OtrosDatos);
                        });

                    // 🔻 TEXTO FINAL
                    col.Item().PaddingTop(10).Text(
                        "Documento generado automáticamente por el sistema institucional.")
                        .FontSize(9)
                        .FontColor(Colors.Grey.Darken1);
                });

                // 🔻 FOOTER
                page.Footer().AlignCenter().Text(txt =>
                {
                    txt.Span("Generado automáticamente - ")
                        .FontSize(9)
                        .FontColor(Colors.Grey.Darken1);

                    txt.Span(DateTime.Now.ToString("dd/MM/yyyy"))
                        .Bold()
                        .FontSize(9)
                        .FontColor(Colors.Grey.Darken1);
                });
            });
        });

        var stream = new MemoryStream();
        document.GeneratePdf(stream);

        return File(
            stream.ToArray(),
            "application/pdf",
            $"CV_{modelo.NombreCompleto.Replace(" ", "_")}.pdf"
        );
    }


    [HttpPost("GenerarCVPrevia")]
    public IActionResult GenerarCVPrevia(
        string NombreCompleto,
        string Cedula,
        string? Correo,
        string? Telefono,
        string? Direccion,
        string? PerfilProfesional,
        string? ExperienciaLaboral,
        string? FormacionAcademica,
        string? Habilidades,
        string? Idiomas,
        string? FormacionComplementaria,
        string? OtrosDatos
    )
    {
        Console.WriteLine("LLEGÓ AL API");
        Console.WriteLine(NombreCompleto);
        Console.WriteLine(Cedula);
        var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "logo.png");

        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);

                page.Content().Column(col =>
                {
                    // 🔷 HEADER CON LOGO
                    col.Item().Row(row =>
                    {
                        row.ConstantItem(80).Height(60).Image(logoPath);

                        row.RelativeItem().Column(c =>
                        {
                            c.Item().AlignRight().Text(NombreCompleto)
                                .FontSize(20).Bold();

                            c.Item().AlignRight().Text($"Cédula: {Cedula}")
                                .FontSize(10).FontColor(Colors.Grey.Darken1);
                        });
                    });

                    col.Item().PaddingVertical(5).LineHorizontal(1);

                    // 🔷 CONTACTO
                    col.Item().PaddingTop(5).Text(text =>
                    {
                        text.Span("Correo: ").SemiBold();
                        text.Span(Correo ?? "");
                    });

                    col.Item().Text(text =>
                    {
                        text.Span("Teléfono: ").SemiBold();
                        text.Span(Telefono ?? "");
                    });

                    col.Item().Text(text =>
                    {
                        text.Span("Dirección: ").SemiBold();
                        text.Span(Direccion ?? "");
                    });

                    // 🔷 SECCIONES
                    void Seccion(string titulo, string? contenido)
                    {
                        col.Item().PaddingTop(10).Text(titulo)
                            .FontSize(14).Bold().FontColor(Colors.Blue.Darken2);

                        col.Item().LineHorizontal(1);

                        col.Item().Text(contenido ?? "").FontSize(10);
                    }

                    Seccion("Perfil Profesional", PerfilProfesional);
                    Seccion("Experiencia Laboral", ExperienciaLaboral);
                    Seccion("Formación Académica", FormacionAcademica);

                    // 🔷 DOS COLUMNAS
                    col.Item().PaddingTop(10).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Habilidades").Bold();
                            c.Item().LineHorizontal(1);
                            c.Item().Text(Habilidades ?? "").FontSize(10);
                        });

                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Idiomas").Bold();
                            c.Item().LineHorizontal(1);
                            c.Item().Text(Idiomas ?? "").FontSize(10);
                        });
                    });

                    Seccion("Formación Complementaria", FormacionComplementaria);
                    Seccion("Otros Datos", OtrosDatos);
                });
            });
        });

        var stream = new MemoryStream();
        pdf.GeneratePdf(stream);

        return File(stream.ToArray(), "application/pdf");
    }
}