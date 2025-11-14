using Aplicacion;
using Aplicacion.DTOs;
using Aplicacion.Servicios;
using Dominio.Entidades; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Persistencia.Interfaces; 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json; 
using System.Threading.Tasks;

using ClosedXML.Excel;
using System.IO;
using System.Data;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Presentacion.Nominas.Reportes; 


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

        // ==================================================================
        //       Acción 1: Dashboard (Tu 'Index')
        // ==================================================================
        public IActionResult Index()
        {
            return View();
        }

        // ==================================================================
        //       Acción 2: Página de Procesar Nómina [GET]
        // ==================================================================
        public async Task<IActionResult> ProcesarNomina()
        {
            var periodos = await _nominaService.ObtenerPeriodosDisponibles();

            // Pasar TempData a ViewBag para que la vista los muestre
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

        // ==================================================================
        //       Acción 3: Lógica de Procesamiento [POST]
        // ==================================================================
        [HttpPost]
        [ValidateAntiForgeryToken] // Buena práctica de seguridad
        public async Task<IActionResult> Procesar(string periodoCodigo)
        {
            if (string.IsNullOrEmpty(periodoCodigo))
            {
                TempData["Error"] = "Debe seleccionar un periodo";
                return RedirectToAction(nameof(ProcesarNomina));
            }

            var resultado = await _nominaService.ProcesarNominaPorPeriodo(periodoCodigo);

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

        // ==================================================================
        //       Acción 4: Reporte de Nóminas (Listar Nóminas) [GET]
        // ==================================================================
        [HttpGet]
        public async Task<IActionResult> ListarNominas(string periodoCodigo = null, string areaCodigo = null, string tipoContrato = null)
        {
            // 1. Obtener los datos (ya filtrados por el servicio) (RN-01, RN-06)
            var nominasDTO = await _nominaService.ObtenerNominasProcesadas(
                periodoCodigo,
                areaCodigo,
                tipoContrato);

            // 2. Calcular Totales (RN-02)
            var totales = new ReporteTotalesDTO
            {
                TotalSueldoBase = nominasDTO.Sum(n => decimal.TryParse(n.SueldoBaseStr, out var val) ? val : 0),
                TotalAsignacionFamiliar = nominasDTO.Sum(n => decimal.TryParse(n.AsignacionFamiliarStr, out var val) ? val : 0),
                TotalSalarioBruto = nominasDTO.Sum(n => decimal.TryParse(n.SueldoBrutoStr, out var val) ? val : 0),
                TotalDescuentos = nominasDTO.Sum(n => decimal.TryParse(n.TotalDescuentosStr, out var val) ? val : 0),
                TotalNetoPagar = nominasDTO.Sum(n => decimal.TryParse(n.SueldoNetoStr, out var val) ? val : 0),
                TotalESSALUD = nominasDTO.Sum(n => decimal.TryParse(n.AporteESSALUDStr, out var val) ? val : 0)
            };
            ViewBag.Totales = totales; // Enviar totales a la vista
            ViewBag.Error = TempData["Error"]; 

            // 3. Cargar los Dropdowns para los filtros (RN-06)
            await CargarFiltrosReporte(periodoCodigo, areaCodigo, tipoContrato);

            // 4. Pasar el modelo a la Vista
            return View(nominasDTO);
        }

        // ==================================================================
        //         Acciones de Exportación (RN-03)
        // ==================================================================

        // ==================================================================
        //         Acción de Exportación a Excel (RN-03) - COMPLETA
        // ==================================================================
        [HttpGet]
        public async Task<IActionResult> ExportarExcel(string periodoCodigo = null, string areaCodigo = null, string tipoContrato = null)
        {
            // 1. Obtener los datos (exactamente igual que en ListarNominas)
            var nominas = await _nominaService.ObtenerNominasProcesadas(
                periodoCodigo,
                areaCodigo,
                tipoContrato);

            // 2. Crear el libro de Excel en memoria
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Nóminas");
                var currentRow = 1;

                // 3. Crear la Fila de Encabezado (RN-01)
                // (Coincide con el prototipo [cite: 34-43])
                worksheet.Cell(currentRow, 1).Value = "DNI";
                worksheet.Cell(currentRow, 2).Value = "Empleado";
                worksheet.Cell(currentRow, 3).Value = "Cargo";
                worksheet.Cell(currentRow, 4).Value = "Área";
                worksheet.Cell(currentRow, 5).Value = "Sueldo Básico";
                worksheet.Cell(currentRow, 6).Value = "Asig. Familiar";
                worksheet.Cell(currentRow, 7).Value = "H. Extras (monto)";
                worksheet.Cell(currentRow, 8).Value = "ONP";
                worksheet.Cell(currentRow, 9).Value = "AFP";
                worksheet.Cell(currentRow, 10).Value = "Imp. 5ta";
                worksheet.Cell(currentRow, 11).Value = "Total Descuentos";
                worksheet.Cell(currentRow, 12).Value = "Sueldo Neto";
                worksheet.Cell(currentRow, 13).Value = "ESSALUD (9%)";
                worksheet.Cell(currentRow, 14).Value = "Período";

                // Poner el encabezado en negrita
                worksheet.Row(currentRow).Style.Font.Bold = true;

                // 4. Llenar las filas con los datos del DTO
                foreach (var nomina in nominas)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = nomina.DNI;
                    worksheet.Cell(currentRow, 2).Value = nomina.EmpleadoNombre;
                    worksheet.Cell(currentRow, 3).Value = nomina.Cargo;
                    worksheet.Cell(currentRow, 4).Value = nomina.Area;

                    // Convertimos los strings de vuelta a decimal para que Excel los trate como números
                    worksheet.Cell(currentRow, 5).Value = decimal.TryParse(nomina.SueldoBaseStr, out var sb) ? sb : 0;
                    worksheet.Cell(currentRow, 6).Value = decimal.TryParse(nomina.AsignacionFamiliarStr, out var af) ? af : 0;
                    worksheet.Cell(currentRow, 7).Value = decimal.TryParse(nomina.HorasExtrasStr, out var he) ? he : 0;
                    worksheet.Cell(currentRow, 8).Value = decimal.TryParse(nomina.DescuentoONPStr, out var onp) ? onp : 0;
                    worksheet.Cell(currentRow, 9).Value = decimal.TryParse(nomina.DescuentoAFPStr, out var afp) ? afp : 0;
                    worksheet.Cell(currentRow, 10).Value = decimal.TryParse(nomina.Renta5taStr, out var r5) ? r5 : 0;
                    worksheet.Cell(currentRow, 11).Value = decimal.TryParse(nomina.TotalDescuentosStr, out var td) ? td : 0;
                    worksheet.Cell(currentRow, 12).Value = decimal.TryParse(nomina.SueldoNetoStr, out var sn) ? sn : 0;
                    worksheet.Cell(currentRow, 13).Value = decimal.TryParse(nomina.AporteESSALUDStr, out var es) ? es : 0;

                    worksheet.Cell(currentRow, 14).Value = nomina.Periodo;
                }

                // 5. Añadir Fila de Totales (RN-02)
                currentRow++;
                worksheet.Row(currentRow).Style.Font.Bold = true;
                worksheet.Cell(currentRow, 10).Value = "TOTALES:";

                // Usamos fórmulas de Excel para sumar las columnas
                worksheet.Cell(currentRow, 11).FormulaA1 = $"=SUM(K2:K{currentRow - 1})"; // Total Descuentos
                worksheet.Cell(currentRow, 12).FormulaA1 = $"=SUM(L2:L{currentRow - 1})"; // Total Neto
                worksheet.Cell(currentRow, 13).FormulaA1 = $"=SUM(M2:M{currentRow - 1})"; // Total ESSALUD

                // Ajustar columnas al contenido
                worksheet.Columns().AdjustToContents();

                // 6. Guardar el archivo en un stream de memoria
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    // 7. Devolver el archivo al navegador
                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"ReporteNomina_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
                }
            }
        }

        // ==================================================================
        //         Acción de Exportación a PDF (RN-03) - con QuestPDF
        // ==================================================================
        [HttpGet]
        public async Task<IActionResult> ExportarPDF(string periodoCodigo = null, string areaCodigo = null, string tipoContrato = null)
        {
            // 1. Obtener los datos (igual que en ListarNominas)
            var nominasDTO = await _nominaService.ObtenerNominasProcesadas(
                periodoCodigo,
                areaCodigo,
                tipoContrato);

            // 2. Calcular Totales (RN-02)
            var totales = new ReporteTotalesDTO
            {
                TotalSalarioBruto = nominasDTO.Sum(n => decimal.TryParse(n.SueldoBrutoStr, out var val) ? val : 0),
                TotalDescuentos = nominasDTO.Sum(n => decimal.TryParse(n.TotalDescuentosStr, out var val) ? val : 0),
                TotalNetoPagar = nominasDTO.Sum(n => decimal.TryParse(n.SueldoNetoStr, out var val) ? val : 0),
                TotalESSALUD = nominasDTO.Sum(n => decimal.TryParse(n.AporteESSALUDStr, out var val) ? val : 0)
            };

            // 3. Obtener Usuario (RN-04)
            string usuario = User.Identity.IsAuthenticated ? User.Identity.Name : "Sistema";

            // 4. Crear la instancia del documento
            var documento = new ReporteNominaDocument(nominasDTO, totales, usuario);

            // 5. Generar el PDF en memoria
            byte[] pdfBytes = documento.GeneratePdf();

            // 6. Devolver el archivo
            return File(pdfBytes, "application/pdf", $"ReporteNomina_{DateTime.Now:yyyyMMdd}.pdf");
        }

        // ==================================================================
        //         Método Helper para cargar filtros
        // ==================================================================
        private async Task CargarFiltrosReporte(string periodoSel, string areaSel, string tipoContratoSel)
        {
         
            var periodos = await _nominaService.ObtenerTodosLosPeriodos();
            ViewBag.Periodos = periodos.Select(p => new SelectListItem
            {
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
        //         DTO Helper para los Totales (RN-02)
        // ==================================================================
        public class ReporteTotalesDTO
        {
            public decimal TotalSueldoBase { get; set; }
            public decimal TotalAsignacionFamiliar { get; set; }
            public decimal TotalSalarioBruto { get; set; }
            public decimal TotalDescuentos { get; set; }
            public decimal TotalNetoPagar { get; set; }
            public decimal TotalESSALUD { get; set; }
        }
    }
}