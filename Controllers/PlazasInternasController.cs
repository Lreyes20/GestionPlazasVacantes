using DocumentFormat.OpenXml.Drawing.Diagrams;
using GestionPlazasVacantes.DTOs;
using GestionPlazasVacantes.Helpers;
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
        // =========================================================
        // 🔐 AGREGAR TOKEN (SI USAS AUTH)
        // =========================================================
        var token = HttpContext.Session.GetString("Token");

        if (!string.IsNullOrEmpty(token))
        {
            _api.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        // =========================================================
        // 🔥 CARGAR PLAZA DESDE API (CON DOCUMENTOS)
        // =========================================================
        var plaza = await _api.GetFromJsonAsync<PlazaDto>($"api/plazas/{id}");

        if (plaza == null)
            return NotFound();

        // =========================================================
        // CREAR VIEWMODEL
        // =========================================================
        var vm = new Postulacion
        {
            Plaza = plaza,
            Postulante = new PostulanteDto
            {
                PlazaVacanteId = id
            }
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Aplicar(
    Postulacion vm,
    IFormFile curriculum)
    {
        // =========================================================
        // RECONSTRUIR MODELO
        // =========================================================
        if (vm?.Postulante == null || vm.Postulante.PlazaVacanteId == 0)
        {
            vm = new Postulacion
            {
                Postulante = new PostulanteDto
                {
                    PlazaVacanteId = int.Parse(Request.Form["Postulante.PlazaVacanteId"].FirstOrDefault() ?? "0"),
                    NombreCompleto = Request.Form["Postulante.NombreCompleto"].FirstOrDefault(),
                    Cedula = Request.Form["Postulante.Cedula"].FirstOrDefault(),
                    Correo = Request.Form["Postulante.Correo"].FirstOrDefault(),
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

        // =========================================================
        // 🔥 SIEMPRE RECARGAR PLAZA
        // =========================================================
        vm.Plaza = await _api.GetFromJsonAsync<PlazaDto>(
            $"api/plazas/{vm.Postulante.PlazaVacanteId}");

        // =========================================================
        // VALIDAR CURRICULUM
        // =========================================================
        if (curriculum == null || curriculum.Length == 0)
        {
            ModelState.AddModelError("", "Debe adjuntar el currículum.");
            return View(vm);
        }

        // =========================================================
        // CREAR FORM DATA
        // =========================================================
        var form = new MultipartFormDataContent();

        // 🔥 AGREGAR CURRICULUM COMO CAMPO SEPARADO
        if (curriculum != null && curriculum.Length > 0)
        {
            form.Add(new StreamContent(curriculum.OpenReadStream()), "curriculum", curriculum.FileName);
        }

        // 🔥 CAMPOS SEGUROS (NULL SAFE)
        form.Add(new StringContent(vm.Postulante.PlazaVacanteId.ToString()), "PlazaVacanteId");

        form.Add(new StringContent(vm.Postulante.NombreCompleto ?? "N/A"), "NombreCompleto");
        form.Add(new StringContent(vm.Postulante.Cedula ?? "000"), "Cedula");
        form.Add(new StringContent(vm.Postulante.Correo ?? ""), "Correo");
        form.Add(new StringContent(vm.Postulante.Telefono ?? ""), "Telefono");
        form.Add(new StringContent(vm.Postulante.Direccion ?? ""), "Direccion");

        form.Add(new StringContent(vm.Postulante.PerfilProfesional ?? ""), "PerfilProfesional");
        form.Add(new StringContent(vm.Postulante.ExperienciaLaboral ?? ""), "ExperienciaLaboral");
        form.Add(new StringContent(vm.Postulante.FormacionAcademica ?? ""), "FormacionAcademica");
        form.Add(new StringContent(vm.Postulante.Habilidades ?? ""), "Habilidades");
        form.Add(new StringContent(vm.Postulante.Idiomas ?? ""), "Idiomas");
        form.Add(new StringContent(vm.Postulante.FormacionComplementaria ?? ""), "FormacionComplementaria");
        form.Add(new StringContent(vm.Postulante.OtrosDatos ?? ""), "OtrosDatos");

        // =========================================================
        // 🔥 DOCUMENTOS DINÁMICOS
        // =========================================================
        var archivos = Request.Form.Files.Where(f => f.Name == "Archivos").ToList();
        var tipos = Request.Form["Tipos"];

        for (int i = 0; i < archivos.Count; i++)
        {
            var archivo = archivos[i];
            var tipo = tipos[i];

            if (archivo.Length > 0)
            {
                form.Add(new StreamContent(archivo.OpenReadStream()), "Archivos", archivo.FileName);
                form.Add(new StringContent(tipo), "Tipos");
            }
        }

        // =========================================================
        // ENVIAR A API
        // =========================================================
        var response = await _api.PostAsync("api/plazas-internas/aplicar", form);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();

            // 🔥 LIMPIAR ERROR (para no mostrar stacktrace gigante)
            ModelState.AddModelError("", "Error al enviar la postulación. Verifique los datos.");

            return View(vm);
        }

        var result = await response.Content.ReadFromJsonAsync<PostulacionResponseDto>();

        return RedirectToAction("Confirmacion", "PlazasInternas", new { id = result.Id });
    }

    public async Task<IActionResult> Confirmacion(int id)
    {
        var postulante = await _api.GetFromJsonAsync<ConfirmacionPostulacionDto>(
            $"api/plazas-internas/confirmacion/{id}");

        if (postulante == null)
            return NotFound();

        return View(postulante);
    }

    public async Task<IActionResult> DescargarComprobantePdf(int id)
    {
        var response = await _api.GetAsync($"api/plazas-internas/comprobante/{id}");

        if (!response.IsSuccessStatusCode)
            return NotFound();

        var bytes = await response.Content.ReadAsByteArrayAsync();

        return File(bytes, "application/pdf", $"Comprobante_{id}.pdf");
    }

    // 🔥 PDF
    public async Task<IActionResult> DescargarCVPdf(int id)
    {
        var modelo = await _api.GetFromJsonAsync<Postulante>(
            $"api/postulacion/{id}");

        if (modelo == null)
            return NotFound();

        QuestPDF.Settings.License = LicenseType.Community;

        var logoPath = Path.Combine(_env.WebRootPath, "images", "logo.png");

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.MarginTop(40);
                page.MarginBottom(40);
                page.MarginLeft(30);
                page.MarginRight(30);

                // 🔶 HEADER INSTITUCIONAL
                page.Header().Element(h => h.DocumentHeader(logoPath, "CURRÍCULUM VITAE"));

                page.Content().Column(col =>
                {
                    col.Spacing(10);

                    // ═══ NOMBRE DESTACADO ═══
                    col.Item().Background(PdfHelper.BlueLight)
                        .Padding(15)
                        .Column(nameCol =>
                        {
                            nameCol.Item().Text(modelo.NombreCompleto)
                                .FontSize(20).Bold().FontColor(PdfHelper.Blue);
                            nameCol.Item().Text($"Cédula: {modelo.Cedula}")
                                .FontSize(11).FontColor(PdfHelper.DarkGrey);
                        });

                    // ═══ DATOS DE CONTACTO ═══
                    col.Item().DataCard().Column(contact =>
                    {
                        contact.Spacing(6);
                        contact.Item().InfoRow("Correo", modelo.Correo);
                        contact.Item().InfoRow("Teléfono", modelo.Telefono);
                        contact.Item().InfoRow("Dirección", modelo.Direccion);
                    });

                    // ═══ SECCIONES CV ═══
                    void CvSection(string titulo, string? contenido)
                    {
                        if (string.IsNullOrWhiteSpace(contenido))
                            return;

                        col.Item().PaddingTop(8).SectionTitle(titulo);
                        col.Item().PaddingTop(4).Text(contenido)
                            .FontSize(10).FontColor(PdfHelper.DarkGrey);
                    }

                    CvSection("Perfil Profesional", modelo.PerfilProfesional);
                    CvSection("Experiencia Laboral", modelo.ExperienciaLaboral);
                    CvSection("Formación Académica", modelo.FormacionAcademica);

                    // ═══ HABILIDADES E IDIOMAS EN DOS COLUMNAS ═══
                    if (!string.IsNullOrWhiteSpace(modelo.Habilidades) ||
                        !string.IsNullOrWhiteSpace(modelo.Idiomas))
                    {
                        col.Item().PaddingTop(8).Row(row =>
                        {
                            if (!string.IsNullOrWhiteSpace(modelo.Habilidades))
                            {
                                row.RelativeItem().PaddingRight(5).Column(c =>
                                {
                                    c.Item().SectionTitle("Habilidades");
                                    c.Item().PaddingTop(4).Text(modelo.Habilidades)
                                        .FontSize(10).FontColor(PdfHelper.DarkGrey);
                                });
                            }

                            if (!string.IsNullOrWhiteSpace(modelo.Idiomas))
                            {
                                row.RelativeItem().PaddingLeft(5).Column(c =>
                                {
                                    c.Item().SectionTitle("Idiomas");
                                    c.Item().PaddingTop(4).Text(modelo.Idiomas)
                                        .FontSize(10).FontColor(PdfHelper.DarkGrey);
                                });
                            }
                        });
                    }

                    CvSection("Formación Complementaria", modelo.FormacionComplementaria);
                    CvSection("Otros Datos", modelo.OtrosDatos);

                    // ═══ DISCLAIMER ═══
                    col.Item().LegalText(
                        "Documento generado automáticamente por el sistema institucional. " +
                        "La información contenida es responsabilidad del postulante.");
                });

                // 🔻 FOOTER PROFESIONAL
                page.Footer().PaddingTop(10).Element(f => f.DocumentFooter());
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


    [HttpPost]
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
        QuestPDF.Settings.License = LicenseType.Community;

        var logoPath = Path.Combine(_env.WebRootPath, "images", "logo.png");

        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.MarginTop(40);
                page.MarginBottom(40);
                page.MarginLeft(30);
                page.MarginRight(30);

                // 🔶 HEADER INSTITUCIONAL
                page.Header().Element(h => h.DocumentHeader(logoPath, "CURRÍCULUM VITAE"));

                page.Content().Column(col =>
                {
                    col.Spacing(10);

                    // ═══ NOMBRE DESTACADO ═══
                    col.Item().Background(PdfHelper.BlueLight)
                        .Padding(15)
                        .Column(nameCol =>
                        {
                            nameCol.Item().Text(NombreCompleto)
                                .FontSize(20).Bold().FontColor(PdfHelper.Blue);
                            nameCol.Item().Text($"Cédula: {Cedula}")
                                .FontSize(11).FontColor(PdfHelper.DarkGrey);
                        });

                    // ═══ DATOS DE CONTACTO ═══
                    col.Item().DataCard().Column(contact =>
                    {
                        contact.Spacing(6);
                        contact.Item().InfoRow("Correo", Correo);
                        contact.Item().InfoRow("Teléfono", Telefono);
                        contact.Item().InfoRow("Dirección", Direccion);
                    });

                    // ═══ SECCIONES CV ═══
                    void CvSection(string titulo, string? contenido)
                    {
                        if (string.IsNullOrWhiteSpace(contenido))
                            return;

                        col.Item().PaddingTop(8).SectionTitle(titulo);
                        col.Item().PaddingTop(4).Text(contenido)
                            .FontSize(10).FontColor(PdfHelper.DarkGrey);
                    }

                    CvSection("Perfil Profesional", PerfilProfesional);
                    CvSection("Experiencia Laboral", ExperienciaLaboral);
                    CvSection("Formación Académica", FormacionAcademica);

                    // ═══ HABILIDADES E IDIOMAS EN DOS COLUMNAS ═══
                    if (!string.IsNullOrWhiteSpace(Habilidades) ||
                        !string.IsNullOrWhiteSpace(Idiomas))
                    {
                        col.Item().PaddingTop(8).Row(row =>
                        {
                            if (!string.IsNullOrWhiteSpace(Habilidades))
                            {
                                row.RelativeItem().PaddingRight(5).Column(c =>
                                {
                                    c.Item().SectionTitle("Habilidades");
                                    c.Item().PaddingTop(4).Text(Habilidades)
                                        .FontSize(10).FontColor(PdfHelper.DarkGrey);
                                });
                            }

                            if (!string.IsNullOrWhiteSpace(Idiomas))
                            {
                                row.RelativeItem().PaddingLeft(5).Column(c =>
                                {
                                    c.Item().SectionTitle("Idiomas");
                                    c.Item().PaddingTop(4).Text(Idiomas)
                                        .FontSize(10).FontColor(PdfHelper.DarkGrey);
                                });
                            }
                        });
                    }

                    CvSection("Formación Complementaria", FormacionComplementaria);
                    CvSection("Otros Datos", OtrosDatos);

                    // ═══ DISCLAIMER ═══
                    col.Item().LegalText(
                        "Documento generado automáticamente por el sistema institucional. " +
                        "La información contenida es responsabilidad del postulante.");
                });

                // 🔻 FOOTER PROFESIONAL
                page.Footer().PaddingTop(10).Element(f => f.DocumentFooter());
            });
        });

        var stream = new MemoryStream();
        pdf.GeneratePdf(stream);

        return File(stream.ToArray(), "application/pdf");
    }
}