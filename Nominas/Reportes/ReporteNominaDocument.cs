using Aplicacion.DTOs;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using Dominio.Resultados; // <<== 1. AÑADE ESTE USING

// <<== 2. ELIMINA EL 'using static Presentacion.Nominas.Controllers.NominaController'

namespace Presentacion.Nominas.Reportes
{
    public class ReporteNominaDocument : IDocument
    {
        private readonly List<NominaDetalleDTO> _nominas;
        private readonly ReporteTotales _totales; // <<== 3. TIPO CORREGIDO
        private readonly string _usuario;

        // <<== 3. TIPO CORREGIDO EN EL CONSTRUCTOR
        public ReporteNominaDocument(List<NominaDetalleDTO> nominas, ReporteTotales totales, string usuario)
        {
            _nominas = nominas;
            _totales = totales;
            _usuario = usuario;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        // (El resto de tu clase 'Compose', 'ComposeTable', 'ComposeFooter', etc.
        // ya usa la variable '_totales' correctamente, así que no necesita cambios.)

        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape()); // Hoja horizontal
                    page.Margin(1, Unit.Centimetre);
                    // <<== TAMAÑO DE FUENTE REDUCIDO PARA QUE QUEPA ==>>
                    page.DefaultTextStyle(style => style.FontSize(8).FontFamily(Fonts.Arial));

                    page.Header().AlignCenter().Text("Reporte de Nómina por Período")
                        .SemiBold().FontSize(14).FontColor(Colors.Grey.Darken4);

                    page.Content().Element(ComposeTable);
                    page.Footer().Element(ComposeFooter);
                });
        }

        // Método que dibuja la tabla principal (ACTUALIZADO)
        void ComposeTable(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Table(table =>
                {
                    // Columnas (ACTUALIZADO para 18 columnas)
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(40); // Cód. Nómina
                        columns.ConstantColumn(40); // Cód. Contrato
                        columns.ConstantColumn(50); // DNI
                        columns.RelativeColumn(1.5f); // Empleado
                        columns.RelativeColumn(1f); // Cargo
                        columns.ConstantColumn(45); // S. Básico
                        columns.ConstantColumn(45); // Asig. Fam
                        columns.ConstantColumn(35); // H.E. (Cant)
                        columns.ConstantColumn(45); // H.E. (Monto)
                        columns.ConstantColumn(40); // Grati
                        columns.ConstantColumn(40); // CTS
                        columns.ConstantColumn(50); // Total Ing.
                        columns.ConstantColumn(40); // ONP
                        columns.ConstantColumn(40); // AFP
                        columns.ConstantColumn(40); // Imp. 5ta
                        columns.ConstantColumn(50); // Total Desc.
                        columns.ConstantColumn(45); // ESSALUD
                        columns.ConstantColumn(55); // Total Neto
                    });

                    // Encabezado de la Tabla (ACTUALIZADO)
                    table.Header(header =>
                    {
                        header.Cell().Element(CellEstiloHeader).Text("Cód. Nómina");
                        header.Cell().Element(CellEstiloHeader).Text("Cód. Contrato");
                        header.Cell().Element(CellEstiloHeader).Text("DNI");
                        header.Cell().Element(CellEstiloHeader).Text("Empleado");
                        header.Cell().Element(CellEstiloHeader).Text("Cargo");
                        header.Cell().Element(CellEstiloHeader).AlignRight().Text("S. Básico");
                        header.Cell().Element(CellEstiloHeader).AlignRight().Text("Asig. Fam.");
                        header.Cell().Element(CellEstiloHeader).AlignRight().Text("H.E (Cant)");
                        header.Cell().Element(CellEstiloHeader).AlignRight().Text("H.E (Monto)");
                        header.Cell().Element(CellEstiloHeader).AlignRight().Text("Grati.");
                        header.Cell().Element(CellEstiloHeader).AlignRight().Text("CTS");
                        header.Cell().Element(CellEstiloHeader).AlignRight().Text("Total Ing.");
                        header.Cell().Element(CellEstiloHeader).AlignRight().Text("ONP");
                        header.Cell().Element(CellEstiloHeader).AlignRight().Text("AFP");
                        header.Cell().Element(CellEstiloHeader).AlignRight().Text("Imp. 5ta");
                        header.Cell().Element(CellEstiloHeader).AlignRight().Text("Total Desc.");
                        header.Cell().Element(CellEstiloHeader).AlignRight().Text("ESSALUD");
                        header.Cell().Element(CellEstiloHeader).AlignRight().Text("Total Neto");
                    });

                    // Filas de Datos (ACTUALIZADO)
                    foreach (var nomina in _nominas)
                    {
                        table.Cell().Element(CellEstiloContenido).Text(nomina.NominaCodigo);
                        table.Cell().Element(CellEstiloContenido).Text(nomina.ContratoCodigo);
                        table.Cell().Element(CellEstiloContenido).Text(nomina.DNI);
                        table.Cell().Element(CellEstiloContenido).Text(nomina.EmpleadoNombre);
                        table.Cell().Element(CellEstiloContenido).Text(nomina.Cargo);
                        table.Cell().Element(CellEstiloContenido).AlignRight().Text(nomina.SueldoBaseStr);
                        table.Cell().Element(CellEstiloContenido).AlignRight().Text(nomina.AsignacionFamiliarStr);
                        table.Cell().Element(CellEstiloContenido).AlignRight().Text(nomina.HorasExtrasCantStr);
                        table.Cell().Element(CellEstiloContenido).AlignRight().Text(nomina.HorasExtrasMontoStr);
                        table.Cell().Element(CellEstiloContenido).AlignRight().Text(nomina.GratificacionStr);
                        table.Cell().Element(CellEstiloContenido).AlignRight().Text(nomina.CTSStr);
                        table.Cell().Element(CellEstiloContenido).AlignRight().Text(nomina.SueldoBrutoStr);
                        table.Cell().Element(CellEstiloContenido).AlignRight().Text(nomina.DescuentoONPStr);
                        table.Cell().Element(CellEstiloContenido).AlignRight().Text(nomina.DescuentoAFPStr);
                        table.Cell().Element(CellEstiloContenido).AlignRight().Text(nomina.Renta5taStr);
                        table.Cell().Element(CellEstiloContenido).AlignRight().Text(nomina.TotalDescuentosStr);
                        table.Cell().Element(CellEstiloContenido).AlignRight().Text(nomina.AporteESSALUDStr);
                        table.Cell().Element(CellEstiloContenido).AlignRight().Text(text =>
                        {
                            text.Span($"S/ {nomina.SueldoNetoStr}").SemiBold();
                        });
                    }
                });

                column.Item().PaddingVertical(10);

                // Esta parte ya usa '_totales', que ahora es del tipo correcto
                column.Item().AlignRight().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn();
                        columns.ConstantColumn(80);
                    });

                    table.Cell().Element(CellEstiloTotales).Text("Total Salario Bruto:");
                    table.Cell().Element(CellEstiloTotales).AlignRight().Text($"S/ {_totales.TotalSalarioBruto:N2}");

                    table.Cell().Element(CellEstiloTotales).Text("Total Descuentos:");
                    table.Cell().Element(CellEstiloTotales).AlignRight().Text($"S/ {_totales.TotalDescuentos:N2}");

                    table.Cell().Element(CellEstiloTotales).Text(text => text.Span("Total Neto Pagado:").SemiBold());
                    table.Cell().Element(CellEstiloTotales).AlignRight().Text(text => text.Span($"S/ {_totales.TotalNetoPagar:N2}").SemiBold());

                    table.Cell().Element(CellEstiloTotales).Text("Total Aporte ESSALUD (9%):");
                    table.Cell().Element(CellEstiloTotales).AlignRight().Text($"S/ {_totales.TotalESSALUD:N2}");
                });
            });
        }

        void ComposeFooter(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Text(text =>
                {
                    text.Span($"Generado por: {_usuario} el {DateTime.Now:dd/MM/yyyy HH:mm:ss}")
                        .FontSize(8).FontColor(Colors.Grey.Medium);
                });

                row.ConstantItem(100).AlignRight().Text(text =>
                {
                    text.Span("Página ").FontSize(8).FontColor(Colors.Grey.Medium);
                    text.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
                    text.Span(" de ").FontSize(8).FontColor(Colors.Grey.Medium);
                    text.TotalPages().FontSize(8).FontColor(Colors.Grey.Medium);
                });
            });
        }

        // --- Métodos Helper para estilos de celda ---

        static IContainer CellEstiloHeader(IContainer container)
        {
            return container.DefaultTextStyle(x => x.SemiBold())
                .BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                .Padding(4);
        }

        static IContainer CellEstiloContenido(IContainer container)
        {
            return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten3)
                .PaddingVertical(2).PaddingHorizontal(4);
        }

        static IContainer CellEstiloTotales(IContainer container)
        {
            return container.Border(1).BorderColor(Colors.Grey.Lighten2)
                .Padding(4);
        }
    }
}