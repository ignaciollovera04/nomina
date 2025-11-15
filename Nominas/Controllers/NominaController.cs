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

        // --- Acción 5: Exportación a Excel [GET] (RN-03) ---
        [HttpGet]
        public async Task<IActionResult> ExportarExcel(string periodoCodigo = null, string areaCodigo = null, string tipoContrato = null)
        {
            var reporteCompleto = await _nominaService.ObtenerNominasProcesadas(
                periodoCodigo,
                areaCodigo,
                tipoContrato);

            var nominas = reporteCompleto.Nominas;

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Nóminas");
                var currentRow = 1;

                // 3. Crear la Fila de Encabezado (Coincide con la vista)
                worksheet.Cell(currentRow, 1).Value = "Cód. Nómina";
                worksheet.Cell(currentRow, 2).Value = "Cód. Contrato"; // <<== AÑADIDO
                worksheet.Cell(currentRow, 3).Value = "DNI";
                worksheet.Cell(currentRow, 4).Value = "Empleado";
                worksheet.Cell(currentRow, 5).Value = "Cargo";
                worksheet.Cell(currentRow, 6).Value = "S. Básico";
                worksheet.Cell(currentRow, 7).Value = "Asig. Familiar";
                worksheet.Cell(currentRow, 8).Value = "H. Extras (monto)";
                worksheet.Cell(currentRow, 9).Value = "ONP";
                worksheet.Cell(currentRow, 10).Value = "AFP";
                worksheet.Cell(currentRow, 11).Value = "ESSALUD (9%)";
                worksheet.Cell(currentRow, 12).Value = "Imp. 5ta";
                worksheet.Cell(currentRow, 13).Value = "Total Neto";
                worksheet.Row(currentRow).Style.Font.Bold = true;

                // 4. Llenar las filas con los datos del DTO
                foreach (var nomina in nominas)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = nomina.NominaCodigo;
                    worksheet.Cell(currentRow, 2).Value = nomina.ContratoCodigo; // <<== AÑADIDO
                    worksheet.Cell(currentRow, 3).Value = nomina.DNI;
                    worksheet.Cell(currentRow, 4).Value = nomina.EmpleadoNombre;
                    worksheet.Cell(currentRow, 5).Value = nomina.Cargo;
                    worksheet.Cell(currentRow, 6).Value = decimal.TryParse(nomina.SueldoBaseStr, out var v) ? v : 0;
                    worksheet.Cell(currentRow, 7).Value = decimal.TryParse(nomina.AsignacionFamiliarStr, out v) ? v : 0;
                    worksheet.Cell(currentRow, 8).Value = decimal.TryParse(nomina.HorasExtrasStr, out v) ? v : 0;
                    worksheet.Cell(currentRow, 9).Value = decimal.TryParse(nomina.DescuentoONPStr, out v) ? v : 0;
                    worksheet.Cell(currentRow, 10).Value = decimal.TryParse(nomina.DescuentoAFPStr, out v) ? v : 0;
                    worksheet.Cell(currentRow, 11).Value = decimal.TryParse(nomina.AporteESSALUDStr, out v) ? v : 0;
                    worksheet.Cell(currentRow, 12).Value = decimal.TryParse(nomina.Renta5taStr, out v) ? v : 0;
                    worksheet.Cell(currentRow, 13).Value = decimal.TryParse(nomina.SueldoNetoStr, out v) ? v : 0;
                }

                // (Los totales ahora están después de la tabla)
                currentRow++;
                worksheet.Row(currentRow).Style.Font.Bold = true;
                worksheet.Cell(currentRow, 12).Value = "TOTAL NETO:";
                worksheet.Cell(currentRow, 13).FormulaA1 = $"=SUM(M2:M{currentRow - 1})";

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

        
    }
}