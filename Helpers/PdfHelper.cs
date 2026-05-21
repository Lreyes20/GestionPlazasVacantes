using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;

namespace GestionPlazasVacantes.Helpers
{
    /// <summary>
    /// Helper profesional para generación de PDFs con branding institucional
    /// de la Municipalidad de Curridabat.
    /// Colores: Naranja #F58220 (primario), Azul #1565C0 (secundario)
    /// </summary>
    public static class PdfHelper
    {
        // ═══════════════════════════════════════════════════════════
        // COLORES INSTITUCIONALES
        // ═══════════════════════════════════════════════════════════
        public static readonly string Orange        = "#F58220";
        public static readonly string OrangeDark    = "#E65100";
        public static readonly string OrangeLight   = "#FFF3E0";
        public static readonly string Blue          = "#1565C0";
        public static readonly string BlueLight     = "#E3F2FD";
        public static readonly string Green         = "#2E7D32";
        public static readonly string Red           = "#C62828";
        public static readonly string DarkGrey      = "#37474F";
        public static readonly string MediumGrey    = "#757575";
        public static readonly string LightGrey     = "#E0E0E0";
        public static readonly string VeryLightGrey = "#F5F5F5";

        // ═══════════════════════════════════════════════════════════
        // DATOS INSTITUCIONALES
        // ═══════════════════════════════════════════════════════════
        public const string InstitutionName = "Municipalidad de Curridabat";
        public const string Tagline         = "Curridabat Ciudad Dulce";
        public const string SystemName      = "Sistema de Gestión de Plazas Vacantes";

        // ═══════════════════════════════════════════════════════════
        // HEADER CON LOGO + TÍTULO (para CV y documentos informativos)
        // ═══════════════════════════════════════════════════════════
        public static void DocumentHeader(this IContainer container, string logoPath, string title)
        {
            container.Column(col =>
            {
                col.Item().Row(row =>
                {
                    if (File.Exists(logoPath))
                        row.ConstantItem(70).Height(70).Image(logoPath);
                    else
                        row.ConstantItem(70).Height(70)
                            .Background(OrangeLight).AlignMiddle().AlignCenter()
                            .Text("MC").Bold().FontSize(24).FontColor(Orange);

                    row.RelativeItem().PaddingLeft(15).Column(c =>
                    {
                        c.Item().AlignRight().Text(title)
                            .FontSize(22).Bold().FontColor(Blue);
                        c.Item().AlignRight().Text(InstitutionName)
                            .FontSize(12).FontColor(MediumGrey);
                        c.Item().AlignRight().Text(Tagline)
                            .FontSize(9).FontColor(MediumGrey);
                    });
                });

                // Línea de acento naranja
                col.Item().PaddingTop(8).LineHorizontal(2).LineColor(Orange);
            });
        }

        // ═══════════════════════════════════════════════════════════
        // BANNER NARANJA CON LOGO (para comprobantes y docs oficiales)
        // ═══════════════════════════════════════════════════════════
        public static void OrangeBannerHeader(this IContainer container, string logoPath)
        {
            container.Background(Orange).PaddingHorizontal(20).PaddingVertical(14).Row(row =>
            {
                if (File.Exists(logoPath))
                    row.ConstantItem(55).Height(55).Image(logoPath);
                else
                    row.ConstantItem(55).Height(55)
                        .Background(OrangeDark).AlignMiddle().AlignCenter()
                        .Text("MC").Bold().FontSize(18).FontColor(Colors.White);

                row.RelativeItem().PaddingLeft(15).Column(c =>
                {
                    c.Item().Text(InstitutionName)
                        .FontColor(Colors.White).Bold().FontSize(16);
                    c.Item().Text(Tagline)
                        .FontColor("#FFCC80").FontSize(11);
                    c.Item().PaddingTop(2).Text(SystemName)
                        .FontColor("#FFCC80").FontSize(9);
                });
            });
        }

        // ═══════════════════════════════════════════════════════════
        // FOOTER CON PAGINACIÓN Y FECHA
        // ═══════════════════════════════════════════════════════════
        public static void DocumentFooter(this IContainer container)
        {
            container.Column(col =>
            {
                col.Item().LineHorizontal(1).LineColor(Orange);

                col.Item().PaddingTop(4).Row(row =>
                {
                    row.RelativeItem().Text(SystemName)
                        .FontSize(8).FontColor(MediumGrey);

                    row.RelativeItem().AlignCenter().Text(text =>
                    {
                        text.Span("Página ").FontSize(8).FontColor(MediumGrey);
                        text.CurrentPageNumber().FontSize(8).FontColor(MediumGrey);
                        text.Span(" de ").FontSize(8).FontColor(MediumGrey);
                        text.TotalPages().FontSize(8).FontColor(MediumGrey);
                    });

                    row.RelativeItem().AlignRight()
                        .Text(DateTime.Now.ToString("dd/MM/yyyy HH:mm"))
                        .FontSize(8).FontColor(MediumGrey);
                });
            });
        }

        // ═══════════════════════════════════════════════════════════
        // TÍTULO DE SECCIÓN CON LÍNEA NARANJA
        // ═══════════════════════════════════════════════════════════
        public static void SectionTitle(this IContainer container, string title)
        {
            container.Column(col =>
            {
                col.Item().Text(title)
                    .FontSize(13).Bold().FontColor(Blue);
                col.Item().LineHorizontal(1).LineColor(Orange);
            });
        }

        // ═══════════════════════════════════════════════════════════
        // FILA DE INFORMACIÓN: Etiqueta: Valor
        // ═══════════════════════════════════════════════════════════
        public static void InfoRow(this IContainer container, string label, string? value)
        {
            container.Text(text =>
            {
                text.Span(label + ": ").SemiBold().FontColor(DarkGrey).FontSize(10);
                text.Span(value ?? "N/A").FontColor(DarkGrey).FontSize(10);
            });
        }

        // ═══════════════════════════════════════════════════════════
        // FILA DE INFORMACIÓN DESTACADA
        // ═══════════════════════════════════════════════════════════
        public static void InfoRowBold(this IContainer container, string label, string? value)
        {
            container.Text(text =>
            {
                text.Span(label + ": ").SemiBold().FontColor(Blue).FontSize(10);
                text.Span(value ?? "N/A").Bold().FontColor(DarkGrey).FontSize(10);
            });
        }

        // ═══════════════════════════════════════════════════════════
        // MARCA DE AGUA
        // ═══════════════════════════════════════════════════════════
        public static void Watermark(this IContainer container)
        {
            container.AlignCenter().AlignMiddle().Rotate(-45)
                .Text(InstitutionName.ToUpper())
                .FontSize(80).FontColor("#E8E8E8");
        }

        // ═══════════════════════════════════════════════════════════
        // BADGE DE ESTADO CON COLOR
        // ═══════════════════════════════════════════════════════════
        public static void StatusBadge(this IContainer container, string status)
        {
            var lower = status.ToLowerInvariant();
            var bgColor = Blue;

            if (lower.Contains("seleccionado") || lower.Contains("aprobado") || lower.Contains("finalizado"))
                bgColor = Green;
            else if (lower.Contains("rechazado") || lower.Contains("descartado"))
                bgColor = Red;
            else if (lower.Contains("revisión") || lower.Contains("pendiente"))
                bgColor = Orange;

            container.Background(bgColor)
                .PaddingHorizontal(10).PaddingVertical(4)
                .Text(status).FontColor(Colors.White).Bold().FontSize(10);
        }

        // ═══════════════════════════════════════════════════════════
        // TARJETA DE DATOS (borde + fondo claro)
        // ═══════════════════════════════════════════════════════════
        public static IContainer DataCard(this IContainer container)
        {
            return container.Border(1).BorderColor(LightGrey)
                .Background(VeryLightGrey).Padding(15);
        }

        // ═══════════════════════════════════════════════════════════
        // TEXTO LEGAL/DISCLAIMER
        // ═══════════════════════════════════════════════════════════
        public static void LegalText(this IContainer container, string text)
        {
            container.PaddingTop(12).Text(text)
                .FontSize(8).FontColor(MediumGrey).Italic();
        }

        // ═══════════════════════════════════════════════════════════
        // ÁREA DE FIRMA
        // ═══════════════════════════════════════════════════════════
        public static void SignatureArea(this IContainer container)
        {
            container.PaddingTop(30).AlignRight().Column(firma =>
            {
                firma.Item().LineHorizontal(1).LineColor(DarkGrey);
                firma.Item().PaddingTop(4).Text(SystemName)
                    .FontSize(9).Bold().FontColor(DarkGrey);
                firma.Item().Text(InstitutionName)
                    .FontSize(8).FontColor(MediumGrey);
            });
        }

        // ═══════════════════════════════════════════════════════════
        // TARJETA DE ESTADÍSTICA (para reportes)
        // ═══════════════════════════════════════════════════════════
        public static void StatCard(this IContainer container, string label, string value, string color)
        {
            container.Border(1).BorderColor(LightGrey).Column(col =>
            {
                col.Item().Background(color).PaddingVertical(10).AlignCenter()
                    .Text(value).FontSize(24).Bold().FontColor(Colors.White);

                col.Item().Background(VeryLightGrey).PaddingVertical(6).AlignCenter()
                    .Text(label).FontSize(9).FontColor(DarkGrey).SemiBold();
            });
        }
    }
}
