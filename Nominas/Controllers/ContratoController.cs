using Aplicacion.DTOs;
using Aplicacion.Servicios;
using Dominio.Entidades;
using Dominio.Resultados;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Text.Json; 
using System.Threading.Tasks;

namespace Presentacion.Nominas.Controllers
{
    public class ContratoController : Controller
    {
        private readonly IContratoService _contratoService;

        public ContratoController(IContratoService contratoService)
        {
            _contratoService = contratoService;
        }

        // ==================================================================
        //       Acción 1: Dashboard "Gestión de Contratos" (HU1-Dashboard)
        // ==================================================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Llama al servicio para obtener la lista de DTOs
            var contratos = await _contratoService.ListarContratosGestion();

            
            ViewBag.ContratosVigentes = contratos.Count(c => c.EstadoContrato == "A");

            return View(contratos); 
        }

        // ==================================================================
        //       Acción 2: Mostrar Formulario 'Crear Contrato' [GET]
        // ==================================================================
        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            // Carga los dropdowns de Áreas y Cargos
            var formData = await _contratoService.ObtenerDatosParaFormularioContrato();

            await CargarViewBagsParaFormulario(formData);

            // Pasa los mensajes del POST (si hubo un error)
            ViewBag.Error = TempData["Error"];
            ViewBag.Exito = TempData["Exito"];

            // Devuelve la vista de 'Crear'
            return View(new Contrato()); 
        }

        // ==================================================================
        //       Acción 3: Guardar el Nuevo Contrato [POST]
        // ==================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Contrato contrato)
        {
            // Llama al servicio para aplicar las RN (RN-01, RN-06, RN-11, etc.)
            var resultado = await _contratoService.CrearContrato(contrato);

            if (resultado.Exito)
            {
                TempData["Exito"] = resultado.Mensaje;
                return RedirectToAction(nameof(Index)); 
            }

            // Si falló, vuelve al formulario y muestra el error
            TempData["Error"] = resultado.Mensaje;

            // Recarga los dropdowns
            var formData = await _contratoService.ObtenerDatosParaFormularioContrato();
            await CargarViewBagsParaFormulario(formData);

            return View(contrato);
        }

        // ==================================================================
        //       Acción 4: Editar Contrato [GET] (Corregido)
        // ==================================================================
        [HttpGet]
        public async Task<IActionResult> Editar(string id) 
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

           
            var contrato = await _contratoService.ObtenerContratoPorCodigo(id);
            if (contrato == null) return NotFound();

            var formData = await _contratoService.ObtenerDatosParaFormularioContrato();
            await CargarViewBagsParaFormulario(formData, contrato.ContratoAreaCodigo, contrato.ContratoCargoCodigo);

            ViewBag.Error = TempData["Error"];
            ViewBag.Exito = TempData["Exito"];

            return View(contrato); 
        }

        // ==================================================================
        //       Acción 5: Guardar Edición de Contrato [POST] (Corregido)
        // ==================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        // <<== El modelo que llega desde la vista ahora es 'ContratoCalculoInfo'
        public async Task<IActionResult> Editar(string id, ContratoCalculoInfo contrato)
        {
            if (id != contrato.ContratoCodigo) return NotFound();

            // El servicio 'EditarContrato' espera la entidad base 'Contrato'.
            // Como 'ContratoCalculoInfo' HEREDA de 'Contrato', C#
            // puede pasarlo sin problemas.
            var resultado = await _contratoService.EditarContrato(contrato);

            if (resultado.Exito)
            {
                TempData["Exito"] = resultado.Mensaje;
                return RedirectToAction(nameof(Index));
            }

            // ... (código de error)
            TempData["Error"] = resultado.Mensaje;
            var formData = await _contratoService.ObtenerDatosParaFormularioContrato();
            await CargarViewBagsParaFormulario(formData, contrato.ContratoAreaCodigo, contrato.ContratoCargoCodigo);

            return View(contrato);
        }

        // ==================================================================
        //       Acción 6: Finalizar Contrato [POST] (Se llama desde el Dashboard)
        // ==================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Finalizar(string contratoCodigo, string motivo)
        {
            // Llama al servicio para aplicar RN-03
            var resultado = await _contratoService.FinalizarContrato(contratoCodigo, motivo);

            if (resultado.Exito)
            {
                TempData["Exito"] = resultado.Mensaje;
            }
            else
            {
                TempData["Error"] = resultado.Mensaje;
            }

            return RedirectToAction(nameof(Index));
        }


        // ==================================================================
        //       Acción 7 (Helper): Búsqueda de Empleados [JSON]
        //       (Para el buscador del prototipo )
        // ==================================================================
        [HttpGet]
        public async Task<IActionResult> BuscarEmpleados(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Json(new List<EmpleadoBusquedaDTO>());
            }

            var empleados = await _contratoService.BuscarEmpleadosActivos(query);
            return Json(empleados); // Devuelve JSON para el JavaScript
        }



        [HttpGet]
        public async Task<IActionResult> BuscarEmpleadosParaContrato()
        {
            var empleados = await _contratoService.BuscarEmpleadosParaContrato();
            return Json(empleados); // Devuelve la lista como JSON
        }



        // ==================================================================
        //         Método Helper para cargar filtros (Dropdowns)
        // ==================================================================
        private async Task CargarViewBagsParaFormulario(ContratoFormDTO formData, string areaSel = null, string cargoSel = null)
        {
            ViewBag.Areas = formData.Areas.Select(a => new SelectListItem
            {
                Text = a.AreaDescripcion,
                Value = a.AreaCodigo,
                Selected = (a.AreaCodigo == areaSel)
            }).ToList();

            ViewBag.Cargos = formData.Cargos.Select(c => new SelectListItem
            {
                Text = c.CargoDescripcion,
                Value = c.CargoCodigo,
                Selected = (c.CargoCodigo == cargoSel)
            }).ToList();

            // (Puedes añadir más ViewBags aquí para los otros dropdowns del prototipo)
        }
    }
}