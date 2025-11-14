using Aplicacion.DTOs;
using Dominio.Entidades; // Necesario
using Dominio.Reglas;
using Persistencia.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aplicacion.Servicios
{
    public class NominaService : INominaService
    {
        private readonly IContratoRepository _contratoRepo;
        private readonly IPeriodoNominaRepository _periodoRepo;
        private readonly INominaRepository _nominaRepo;

        // (El constructor y ObtenerPeriodos se mantienen igual)
        public NominaService(
            IContratoRepository contratoRepo,
            IPeriodoNominaRepository periodoRepo,
            INominaRepository nominaRepo)
        {
            _contratoRepo = contratoRepo;
            _periodoRepo = periodoRepo;
            _nominaRepo = nominaRepo;
        }

        public async Task<List<PeriodoNominaDTO>> ObtenerPeriodosDisponibles()
        {
            var periodos = await _periodoRepo.ListarTodos();
            return periodos
                .Where(p => p.PeriodoEstado == "A") // Solo activos
                .Select(p => new PeriodoNominaDTO
                {
                    PeriodoCodigo = p.PeriodoCodigo,
                    PeriodoTipo = p.PeriodoTipo,
                    PeriodoInicioStr = p.PeriodoInicio.ToString("dd/MM/yyyy"),
                    PeriodoFinStr = p.PeriodoFin.ToString("dd/MM/yyyy"),
                    PeriodoEstado = p.PeriodoEstado,
                    EstadoDescripcion = ObtenerDescripcionEstado(p.PeriodoEstado)
                })
                .ToList();
        }

        public async Task<List<PeriodoNominaDTO>> ObtenerTodosLosPeriodos()
        {
            var periodos = await _periodoRepo.ListarTodos();

            return periodos
          
                .OrderByDescending(p => p.PeriodoInicio) 
                .Select(p => new PeriodoNominaDTO
                {
                    PeriodoCodigo = p.PeriodoCodigo,
                    PeriodoTipo = p.PeriodoTipo,
                    PeriodoInicioStr = p.PeriodoInicio.ToString("dd/MM/yyyy"),
                    PeriodoFinStr = p.PeriodoFin.ToString("dd/MM/yyyy"),
                    PeriodoEstado = p.PeriodoEstado,
                    EstadoDescripcion = ObtenerDescripcionEstado(p.PeriodoEstado)
                })
                .ToList(); 
        }

        // ==================================================================
        //         MÉTODO 'ProcesarNominaPorPeriodo' 
        // ==================================================================
        public async Task<ResultadoProcesarNominaDTO> ProcesarNominaPorPeriodo(string periodoCodigo)
        {
            var resultado = new ResultadoProcesarNominaDTO();
            try
            {
                // (Validaciones 1, 2 y 3 se mantienen igual)
                var periodo = await _periodoRepo.ObtenerPorCodigo(periodoCodigo);
                if (periodo == null)
                {
                    resultado.Exito = false;
                    resultado.Mensaje = "El periodo seleccionado no existe";
                    return resultado;
                }
                var todosPeriodos = await _periodoRepo.ListarTodos();
                if (!CalculoNominaRules.ValidarPeriodoAnteriorProcesado(todosPeriodos, periodoCodigo))
                {
                    resultado.Exito = false;
                    resultado.Mensaje = "No puede procesar la nómina de este mes sin haber cerrado la nómina del período anterior";
                    return resultado;
                }
                var contratosActivos = await _contratoRepo.ListarContratosActivos();
                if (!contratosActivos.Any())
                {
                    resultado.Exito = false;
                    resultado.Mensaje = "No hay contratos activos para procesar";
                    return resultado;
                }

                var random = new Random();
                int mesDelPeriodo = periodo.PeriodoInicio.Month;

                foreach (var contrato in contratosActivos)
                {
                    try
                    {
                        var existeNomina = await _nominaRepo.ExisteNominaParaPeriodo(periodoCodigo, contrato.ContratoCodigo);
                        if (existeNomina)
                        {
                            resultado.Errores.Add($"Ya existe nómina para {contrato.EmpleadoNombre} en este período");
                            continue;
                        }

                        int horasExtras = random.Next(0, 11);
                        int mesesTrabajados = 6; // Simplificado

                        // 1. Aplicar reglas de negocio
                        var calculo = CalculoNominaRules.CalcularNomina(
                            contrato, mesDelPeriodo, horasExtras, mesesTrabajados);

                        // 2. Insertar en BD (CON LA FIRMA DETALLADA)
                        var nominaCodigo = await _nominaRepo.InsertarNomina(
                            periodoCodigo: periodoCodigo,
                            contratoCodigo: contrato.ContratoCodigo,
                            horasExtras: horasExtras,
                            bonificacion: calculo.Gratificacion + calculo.CTS, // Bonificación Total

                            // -- Campos Detallados --
                            sueldoBase: calculo.SueldoBase,
                            asignacionFamiliar: calculo.AsignacionFamiliar,
                            descuentoONP: calculo.DescuentoONP,
                            descuentoAFP: calculo.DescuentoAFP,
                            descuentoImpuesto5ta: calculo.RetencionRenta5ta,
                            aporteESSALUD: calculo.AporteESSALUD,

                            // -- Totales --
                            totalIngresos: calculo.TotalIngresos,
                            totalDescuentos: calculo.TotalDescuentos,
                            sueldoNeto: calculo.SueldoNeto,
                            estado: "P"); // Estado Procesada

                        // El DTO de resumen está bien
                        resultado.Detalles.Add(new DetalleNominaProcesadaDTO
                        {
                            NominaCodigo = nominaCodigo,
                            EmpleadoNombre = contrato.EmpleadoNombre,
                            SueldoBase = calculo.SueldoBase,
                            HorasExtras = horasExtras,
                            TotalIngresos = calculo.TotalIngresos,
                            TotalDescuentos = calculo.TotalDescuentos,
                            SueldoNeto = calculo.SueldoNeto,
                            Estado = "Procesada"
                        });
                        resultado.TotalProcesadas++;
                    }
                    catch (Exception ex)
                    {
                        resultado.Errores.Add($"Error al procesar {contrato.EmpleadoNombre}: {ex.Message}");
                    }
                }

                // 5. Actualizar estado del periodo a Cerrado
                if (resultado.TotalProcesadas > 0)
                {
                    await _periodoRepo.ActualizarEstado(periodoCodigo, "C");
                }
                resultado.Exito = resultado.TotalProcesadas > 0;
                resultado.Mensaje = resultado.Exito
                    ? $"Se procesaron {resultado.TotalProcesadas} nóminas correctamente"
                    : "No se pudo procesar ninguna nómina";
            }
            catch (Exception ex)
            {
                resultado.Exito = false;
                resultado.Mensaje = $"Error general: {ex.Message}";
            }
            return resultado;
        }

        // ==================================================================
        //        MÉTODO 'ObtenerNominasProcesadas' CORREGIDO
        // ==================================================================
        public async Task<List<NominaDetalleDTO>> ObtenerNominasProcesadas(
     string periodoCodigo = null,
     string areaCodigo = null,
     string tipoContrato = null)
        {
            // 1. Llama al repo (que llama al SP v3)
            // 'nominasDeDB' es una List<Dominio.Entidades.NominaDetalle>
            var nominasDeDB = await _nominaRepo.ListarNominasProcesadas(
                periodoCodigo,
                areaCodigo,
                tipoContrato);

        
            return nominasDeDB.Select(n => new NominaDetalleDTO
            {
                NominaCodigo = n.NominaCodigo,
                EmpleadoNombre = n.EmpleadoNombre,
                DNI = n.DNI, 
                Area = n.Area,
                Cargo = n.Cargo,
                Periodo = $"{n.PeriodoInicio} - {n.PeriodoFin}", 

                // Ingresos
                SueldoBaseStr = n.SueldoBase.ToString("N2"),
                AsignacionFamiliarStr = n.AsignacionFamiliar.ToString("N2"),
                HorasExtrasStr = n.NominaHorasExtras.ToString(), 
                GratificacionStr = n.Bonificaciones.ToString("N2"),
                CTSStr = "0.00",
                SueldoBrutoStr = n.SalarioBruto.ToString("N2"), 

                // Descuentos
                DescuentoONPStr = n.DescuentoONP.ToString("N2"),
                DescuentoAFPStr = n.DescuentoAFP.ToString("N2"),
                Renta5taStr = n.ImpuestoQuintaCategoria.ToString("N2"), 
                TotalDescuentosStr = n.Deducciones.ToString("N2"), 

                
                AporteESSALUDStr = n.ESSALUD.ToString("N2"), 

                
                SueldoNetoStr = n.SueldoNeto.ToString("N2"),

              
                FechaProcesamiento = n.FechaProcesamiento, 
            }).ToList();
        }

        private string ObtenerDescripcionEstado(string estado)
        {
            return estado switch
            {
                "A" => "Activo",
                "C" => "Cerrado",
                "P" => "Pendiente",
                _ => "Desconocido"
            };
        }
    }
}