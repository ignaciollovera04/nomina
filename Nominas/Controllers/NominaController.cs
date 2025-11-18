using Aplicacion;
using Aplicacion.DTOs;
using Aplicacion.Servicios;

using ClosedXML.Excel;
using Dominio.Entidades;
using Dominio.Resultados; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Persistencia.Interfaces;
using Presentacion.Nominas.Reportes; 
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;


namespace Presentacion.Nominas.Controllers
{
    public class NominaController : Controller
    {
        private readonly INominaService _nominaService;
        private readonly IAreaRepository _areaRepo;

        public NominaController(INominaService nominaService, IAreaRepository areaRepo)
        {
            _nominaService = nominaService;
            _areaRepo = areaRepo;
        }

        // --- Acción 1: Dashboard (Página de Inicio) ---
        public IActionResult Index()
        {
            return View();
        }

        // --- Acción 2: Página de Procesar Nómina [GET] ---
        public async Task<IActionResult> ProcesarNomina()
        {
            var periodos = await _nominaService.ObtenerPeriodosDisponibles();

            // Pasa los resultados del POST (si los hay) a la vista
            ViewBag.Success = TempData["Success"];
            ViewBag.Error = TempData["Error"];

            if (TempData["Detalles"] != null)
            {
                ViewBag.Detalles = JsonSerializer.Deserialize<List<DetalleNominaProcesadaDTO>>(TempData["Detalles"].ToString());
            }
            if (TempData["Errores"] != null)
            {
                ViewBag.Errores = JsonSerializer.Deserialize<List<string>>(TempData["Errores"].ToString());
            }

            return View(periodos);
        }

        // --- Acción 3: Lógica de Procesamiento [POST] ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Procesar(string periodoCodigo)
        {
            if (string.IsNullOrEmpty(periodoCodigo))
            {
                TempData["Error"] = "Debe seleccionar un periodo";
                return RedirectToAction(nameof(ProcesarNomina));
            }

            var resultado = await _nominaService.ProcesarNominaPorPeriodo(periodoCodigo);

            // Pasa los resultados de vuelta a la vista 'ProcesarNomina'
            if (resultado.Exito)
            {
                TempData["Success"] = resultado.Mensaje;
                TempData["Detalles"] = JsonSerializer.Serialize(resultado.Detalles);
            }
            else
            {
                TempData["Error"] = resultado.Mensaje;
                if (resultado.Errores.Any())
                {
                    TempData["Errores"] = JsonSerializer.Serialize(resultado.Errores);
                }
            }
            return RedirectToAction(nameof(ProcesarNomina));
        }

        // --- Acción 4: Reporte de Nóminas (Listar Nóminas) [GET] ---
        [HttpGet]
        public async Task<IActionResult> ListarNominas(string periodoCodigo = null, string areaCodigo = null, string tipoContrato = null)
        {
            // 1. Inicializa la lista de nóminas como VACÍA
            List<NominaDetalleDTO> nominasDTO = new List<NominaDetalleDTO>();

            // 2. Prepara un mensaje de bienvenida/instrucción
            ViewBag.MensajeInstruccion = "Por favor, seleccione un período y haga clic en 'Generar Reporte'.";

            // 3. ¡LÓGICA CLAVE! Solo busca en la BD si se ha enviado un filtro de período
            if (!string.IsNullOrEmpty(periodoCodigo))
            {
                // 4. Obtener el DTO "wrapper" (que contiene la lista Y los totales)
                var reporteCompleto = await _nominaService.ObtenerNominasProcesadas(
                    periodoCodigo,
                    areaCodigo,
                    tipoContrato);

                // 5. Desempaquetar los resultados
                nominasDTO = reporteCompleto.Nominas; // Asigna la lista de nóminas
                ViewBag.Totales = reporteCompleto.Totales; // Pasa los totales

                // 6. Oculta el mensaje de instrucción
                ViewBag.MensajeInstruccion = null;

                // 7. (Opcional) Mostrar mensaje si el filtro no arrojó resultados
                if (!nominasDTO.Any())
                {
                    ViewBag.MensajeInstruccion = "No se encontraron nóminas para los filtros seleccionados.";
                }
            }
            else
            {
                // Si no hay filtro, crea un objeto de totales vacío para que la vista no falle
                ViewBag.Totales = new Dominio.Resultados.ReporteTotales();
            }

            ViewBag.Error = TempData["Error"];

            // 8. Cargar los Dropdowns para los filtros
            await CargarFiltrosReporte(periodoCodigo, areaCodigo, tipoContrato);

            // 9. Pasar el modelo (lista vacía o lista filtrada) a la Vista
            return View(nominasDTO);
        }

        // ==================================================================
        //         Acción de Exportación a Excel (ACTUALIZADA)
        // ==================================================================
        [HttpGet]
        public async Task<IActionResult> ExportarExcel(string periodoCodigo = null, string areaCodigo = null, string tipoContrato = null)
        {
            // 1. Obtener el DTO "wrapper" (lista y totales)
            var reporteCompleto = await _nominaService.ObtenerNominasProcesadas(
                periodoCodigo,
                areaCodigo,
                tipoContrato);

            var nominas = reporteCompleto.Nominas;
            var totales = reporteCompleto.Totales;

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Nóminas");
                var currentRow = 1;

                // 3. Crear la Fila de Encabezado (18 columnas)
                // (Coincide con la vista ListarNominas.cshtml)
                worksheet.Cell(currentRow, 1).Value = "Cód. Nómina";
                worksheet.Cell(currentRow, 2).Value = "Cód. Contrato";
                worksheet.Cell(currentRow, 3).Value = "DNI";
                worksheet.Cell(currentRow, 4).Value = "Empleado";
                worksheet.Cell(currentRow, 5).Value = "Cargo";
                worksheet.Cell(currentRow, 6).Value = "S. Básico";
                worksheet.Cell(currentRow, 7).Value = "Asig. Familiar";
                worksheet.Cell(currentRow, 8).Value = "H. Extras (Cant)";
                worksheet.Cell(currentRow, 9).Value = "H. Extras (Monto)";
                worksheet.Cell(currentRow, 10).Value = "Gratificación";
                worksheet.Cell(currentRow, 11).Value = "CTS";
                worksheet.Cell(currentRow, 12).Value = "Total Ingresos";
                worksheet.Cell(currentRow, 13).Value = "ONP";
                worksheet.Cell(currentRow, 14).Value = "AFP";
                worksheet.Cell(currentRow, 15).Value = "Imp. 5ta";
                worksheet.Cell(currentRow, 16).Value = "Total Descuentos";
                worksheet.Cell(currentRow, 17).Value = "ESSALUD (9%)";
                worksheet.Cell(currentRow, 18).Value = "Sueldo Neto";

                // Aplicar estilo al encabezado
                var headerRange = worksheet.Range(currentRow, 1, currentRow, 18);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(0x34, 0x3A, 0x40); // Color table-dark
                headerRange.Style.Font.FontColor = XLColor.White;

                // 4. Llenar las filas con los datos del DTO
                foreach (var nomina in nominas)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = nomina.NominaCodigo;
                    worksheet.Cell(currentRow, 2).Value = nomina.ContratoCodigo;
                    worksheet.Cell(currentRow, 3).Value = nomina.DNI;
                    worksheet.Cell(currentRow, 4).Value = nomina.EmpleadoNombre;
                    worksheet.Cell(currentRow, 5).Value = nomina.Cargo;

                    // Convertir strings a decimal para que Excel los trate como números
                    worksheet.Cell(currentRow, 6).SetValue(decimal.TryParse(nomina.SueldoBaseStr, out var v) ? v : 0).Style.NumberFormat.Format = "S/ #,##0.00";
                    worksheet.Cell(currentRow, 7).SetValue(decimal.TryParse(nomina.AsignacionFamiliarStr, out v) ? v : 0).Style.NumberFormat.Format = "S/ #,##0.00";
                    worksheet.Cell(currentRow, 8).Value = nomina.HorasExtrasCantStr; // Cantidad (string)
                    worksheet.Cell(currentRow, 9).SetValue(decimal.TryParse(nomina.HorasExtrasMontoStr, out v) ? v : 0).Style.NumberFormat.Format = "S/ #,##0.00";
                    worksheet.Cell(currentRow, 10).SetValue(decimal.TryParse(nomina.GratificacionStr, out v) ? v : 0).Style.NumberFormat.Format = "S/ #,##0.00";
                    worksheet.Cell(currentRow, 11).SetValue(decimal.TryParse(nomina.CTSStr, out v) ? v : 0).Style.NumberFormat.Format = "S/ #,##0.00";
                    worksheet.Cell(currentRow, 12).SetValue(decimal.TryParse(nomina.SueldoBrutoStr, out v) ? v : 0).Style.NumberFormat.Format = "S/ #,##0.00";
                    worksheet.Cell(currentRow, 13).SetValue(decimal.TryParse(nomina.DescuentoONPStr, out v) ? v : 0).Style.NumberFormat.Format = "S/ #,##0.00";
                    worksheet.Cell(currentRow, 14).SetValue(decimal.TryParse(nomina.DescuentoAFPStr, out v) ? v : 0).Style.NumberFormat.Format = "S/ #,##0.00";
                    worksheet.Cell(currentRow, 15).SetValue(decimal.TryParse(nomina.Renta5taStr, out v) ? v : 0).Style.NumberFormat.Format = "S/ #,##0.00";
                    worksheet.Cell(currentRow, 16).SetValue(decimal.TryParse(nomina.TotalDescuentosStr, out v) ? v : 0).Style.NumberFormat.Format = "S/ #,##0.00";
                    worksheet.Cell(currentRow, 17).SetValue(decimal.TryParse(nomina.AporteESSALUDStr, out v) ? v : 0).Style.NumberFormat.Format = "S/ #,##0.00";
                    worksheet.Cell(currentRow, 18).SetValue(decimal.TryParse(nomina.SueldoNetoStr, out v) ? v : 0).Style.NumberFormat.Format = "S/ #,##0.00";
                }

                // 5. Añadir Fila de Totales (RN-02)
                currentRow++;
                worksheet.Row(currentRow).Style.Font.Bold = true;
                worksheet.Cell(currentRow, 11).Value = "TOTALES:";
                worksheet.Cell(currentRow, 12).FormulaA1 = $"=SUM(L2:L{currentRow - 1})"; // Total Ingresos
                worksheet.Cell(currentRow, 16).FormulaA1 = $"=SUM(P2:P{currentRow - 1})"; // Total Descuentos
                worksheet.Cell(currentRow, 17).FormulaA1 = $"=SUM(Q2:Q{currentRow - 1})"; // Total ESSALUD
                worksheet.Cell(currentRow, 18).FormulaA1 = $"=SUM(R2:R{currentRow - 1})"; // Total Neto

                // Aplicar formato a los totales
                worksheet.Cell(currentRow, 12).Style.NumberFormat.Format = "S/ #,##0.00";
                worksheet.Cell(currentRow, 16).Style.NumberFormat.Format = "S/ #,##0.00";
                worksheet.Cell(currentRow, 17).Style.NumberFormat.Format = "S/ #,##0.00";
                worksheet.Cell(currentRow, 18).Style.NumberFormat.Format = "S/ #,##0.00";

                worksheet.Columns().AdjustToContents();

                // 6. Guardar y devolver
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"ReporteNomina_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
                }
            }
        } 

        // --- Acción 6: Exportación a PDF [GET] (RN-03) ---
        [HttpGet]
        public async Task<IActionResult> ExportarPDF(string periodoCodigo = null, string areaCodigo = null, string tipoContrato = null)
        {
            // 1. Obtener el DTO "wrapper"
            var reporteCompleto = await _nominaService.ObtenerNominasProcesadas(
                periodoCodigo,
                areaCodigo,
                tipoContrato);

            // 2. Desempaquetar
            var nominasDTO = reporteCompleto.Nominas;
            var totales = reporteCompleto.Totales; // <-- Totales ya calculados por el Dominio

            // 3. Obtener Usuario (RN-04)
            string usuario = User.Identity.IsAuthenticated ? User.Identity.Name : "Sistema";

            // 4. Crear instancia del documento QuestPDF
            var documento = new ReporteNominaDocument(nominasDTO, totales, usuario);

            // 5. Generar PDF
            byte[] pdfBytes = documento.GeneratePdf();

            // 6. Devolver archivo
            return File(pdfBytes, "application/pdf", $"ReporteNomina_{DateTime.Now:yyyyMMdd}.pdf");
        }

        // --- Helper: Cargar Filtros (Dropdowns) ---
        private async Task CargarFiltrosReporte(string periodoSel, string areaSel, string tipoContratoSel)
        {
            // 1. Cargar Períodos (RN-06)
            var periodos = await _nominaService.ObtenerTodosLosPeriodos();
            ViewBag.Periodos = periodos.Select(p => new SelectListItem
            {
                // Esta línea añade el texto (ej. "(Activo)", "(Cerrado)")
                Text = $"{p.PeriodoInicioStr} - {p.PeriodoFinStr} ({p.EstadoDescripcion})",
                Value = p.PeriodoCodigo,
                Selected = (p.PeriodoCodigo == periodoSel)
            }).ToList(); 

            // 2. Cargar Áreas (RN-06)
            var areas = await _areaRepo.ListarActivas();
            ViewBag.Areas = areas.Select(a => new SelectListItem
            {
                Text = a.AreaDescripcion,
                Value = a.AreaCodigo,
                Selected = (a.AreaCodigo == areaSel)
            }).ToList();

            // 3. Cargar Tipos de Contrato (RN-06)
            var tiposContrato = new List<SelectListItem>
            {
                new SelectListItem { Text = "Indefinido", Value = "INDEFINIDO", Selected = ("INDEFINIDO" == tipoContratoSel) },
                new SelectListItem { Text = "Plazo Fijo", Value = "PLAZO_FIJO", Selected = ("PLAZO_FIJO" == tipoContratoSel) }
            };
            ViewBag.TiposContrato = tiposContrato;
        }

        // ==================================================================
        //       Acción NUEVA: Generar Períodos [POST]
        // ==================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerarPeriodos()
        {
            var resultado = await _nominaService.GenerarNuevosPeriodos();

            if (resultado.Exito)
            {
                TempData["Success"] = resultado.Mensaje;
            }
            else
            {
                TempData["Error"] = resultado.Mensaje;
            }

            return RedirectToAction(nameof(ProcesarNomina));
        }

        // ==================================================================
        //       Acción NUEVA: Anular Nómina [POST]
        // ==================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Anular(string periodoCodigoAnular) // Usamos un nombre único
        {
            if (string.IsNullOrEmpty(periodoCodigoAnular))
            {
                TempData["Error"] = "No se especificó un período para anular.";
                return RedirectToAction(nameof(ProcesarNomina));
            }

            var resultado = await _nominaService.AnularNomina(periodoCodigoAnular);

            if (resultado.Exito)
            {
                TempData["Success"] = resultado.Mensaje;
            }
            else
            {
                TempData["Error"] = resultado.Mensaje;
            }

            // Redirige de vuelta a la página 'ProcesarNomina'
            return RedirectToAction(nameof(ProcesarNomina));
        }









    }
}