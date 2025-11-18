using Aplicacion.DTOs;
using Dominio.Entidades; 
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
            // 1. Obtener TODOS los períodos de la base de datos
            var todosLosPeriodos = await _periodoRepo.ListarTodos();

            // 2. Ordenarlos por fecha de inicio para asegurar la secuencia
            var periodosOrdenados = todosLosPeriodos.OrderBy(p => p.PeriodoInicio).ToList();

          
            //    Buscar el PRIMER período 'Activo' ('A') en toda la lista.
         //    Esto implementa RN-04 [cite: 284, 294] y maneja automáticamente las anulaciones.
            var proximoPeriodoParaProcesar = periodosOrdenados.FirstOrDefault(p => p.PeriodoEstado == "A");

            // 4. Crear la lista de DTOs (que ahora tendrá 0 o 1 elemento)
            var listaDto = new List<PeriodoNominaDTO>();

            if (proximoPeriodoParaProcesar != null)
            {
                // Si encontramos el período que sigue, lo añadimos a la lista
                listaDto.Add(new PeriodoNominaDTO
                {
                    PeriodoCodigo = proximoPeriodoParaProcesar.PeriodoCodigo,
                    PeriodoTipo = proximoPeriodoParaProcesar.PeriodoTipo,
                    PeriodoInicioStr = proximoPeriodoParaProcesar.PeriodoInicio.ToString("dd/MM/yyyy"),
                    PeriodoFinStr = proximoPeriodoParaProcesar.PeriodoFin.ToString("dd/MM/yyyy"),
                    PeriodoEstado = proximoPeriodoParaProcesar.PeriodoEstado,
                    EstadoDescripcion = ObtenerDescripcionEstado(proximoPeriodoParaProcesar.PeriodoEstado)
                });
            }

            // 5. Devolver la lista (que solo tiene el próximo período válido)
            return listaDto;
        }

        public async Task<List<PeriodoNominaDTO>> ObtenerTodosLosPeriodos()
        {
            var periodos = await _periodoRepo.ListarTodos();

            return periodos
          
                .OrderByDescending(p => p.PeriodoInicio)
                .Where(p => p.PeriodoEstado == "C")
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

                     
                        resultado.Detalles.Add(new NominaDetalleDTO
                        {
                            NominaCodigo = nominaCodigo,
                            ContratoCodigo = contrato.ContratoCodigo,
                            EmpleadoNombre = contrato.EmpleadoNombre,
                            DNI = contrato.DNI, 
                            Area = contrato.Area,
                            Cargo = contrato.Cargo,
                            Periodo = $"{periodo.PeriodoInicio:dd/MM/yyyy} - {periodo.PeriodoFin:dd/MM/yyyy}",

                            PeriodoCodigo = periodo.PeriodoCodigo,

                            SueldoBaseStr = calculo.SueldoBase.ToString("N2"),
                            AsignacionFamiliarStr = calculo.AsignacionFamiliar.ToString("N2"),
                            HorasExtrasCantStr = $"[{horasExtras}h]",
                            HorasExtrasMontoStr = $"S/ {calculo.HorasExtras:N2}",


                            // Lógica de Beneficios (RN-09, RN-15)
                            GratificacionStr = (mesDelPeriodo == 7 || mesDelPeriodo == 12) ? calculo.Gratificacion.ToString("N2") : "0.00",
                            CTSStr = (mesDelPeriodo == 5 || mesDelPeriodo == 11) ? calculo.CTS.ToString("N2") : "0.00",
                            SueldoBrutoStr = calculo.TotalIngresos.ToString("N2"), // Total Ingresos

                            // Descuentos
                            DescuentoONPStr = calculo.DescuentoONP.ToString("N2"),
                            DescuentoAFPStr = calculo.DescuentoAFP.ToString("N2"),
                            Renta5taStr = calculo.RetencionRenta5ta.ToString("N2"),
                            TotalDescuentosStr = calculo.TotalDescuentos.ToString("N2"),

                            AporteESSALUDStr = calculo.AporteESSALUD.ToString("N2"),
                            SueldoNetoStr = calculo.SueldoNeto.ToString("N2"),

                            FechaProcesamiento = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                            EstadoDescripcion = "Procesada"
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
        //        MÉTODO 'ObtenerNominasProcesadas' 
        // ==================================================================
        public async Task<ReporteNominaCompletoDTO> ObtenerNominasProcesadas(
                string periodoCodigo = null,
                string areaCodigo = null,
                string tipoContrato = null)
        {
            // 1. Obtener las ENTIDADES (ahora con todos los campos)
            var nominasDeDB = await _nominaRepo.ListarNominasProcesadas(
                periodoCodigo,
                areaCodigo,
                tipoContrato);

            // 2. APLICAR REGLA DE DOMINIO (RN-02 - Calcular Totales)
            var totales = ReporteNominaRules.CalcularTotales(nominasDeDB);

            // 3. Mapear las Entidades a DTOs (para la vista)
            var nominasDTO = nominasDeDB.Select(n => new NominaDetalleDTO
            {
                // Identificación
                NominaCodigo = n.NominaCodigo,
                ContratoCodigo = n.ContratoCodigo,
                EmpleadoNombre = n.EmpleadoNombre,
                DNI = n.DNI,
                Area = n.Area,
                Cargo = n.Cargo,
                Periodo = $"{n.PeriodoInicio:dd/MM/yyyy} - {n.PeriodoFin:dd/MM/yyyy}",

                // Ingresos (RN-01)
                SueldoBaseStr = n.SueldoBase.ToString("N2"),
                AsignacionFamiliarStr = n.AsignacionFamiliar.ToString("N2"),
                HorasExtrasCantStr = n.NominaHorasExtras.ToString(),
                HorasExtrasMontoStr = n.HorasExtrasMonto.ToString("N2"),

                // Lógica de Beneficios (RN-09, RN-15)
                GratificacionStr = (n.Mes == 7 || n.Mes == 12) ? n.Bonificaciones.ToString("N2") : "0.00",
                CTSStr = (n.Mes == 5 || n.Mes == 11) ? n.Bonificaciones.ToString("N2") : "0.00",

                SueldoBrutoStr = n.SalarioBruto.ToString("N2"), // Total Ingresos

                // Descuentos (RN-06, RN-08, RN-10)
                DescuentoONPStr = n.DescuentoONP.ToString("N2"),
                DescuentoAFPStr = n.DescuentoAFP.ToString("N2"),
                Renta5taStr = n.ImpuestoQuintaCategoria.ToString("N2"),
                TotalDescuentosStr = n.Deducciones.ToString("N2"), // Total Descuentos

                // Aporte empleador (RN-07)
                AporteESSALUDStr = n.ESSALUD.ToString("N2"),

                // Neto (RN-17)
                SueldoNetoStr = n.SueldoNeto.ToString("N2"),

                // Auditoría
                FechaProcesamiento = n.FechaProcesamiento,
                EstadoDescripcion = n.EstadoNomina
            }).ToList();

            // 4. Devolver el DTO wrapper
            return new ReporteNominaCompletoDTO
            {
                Nominas = nominasDTO,
                Totales = totales
            };
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


        public async Task<ResultadoOperacionDTO> AnularNomina(string periodoCodigo)
        {
            try
            {
                // 1. Validar que el período exista (prevención)
                var periodo = await _periodoRepo.ObtenerPorCodigo(periodoCodigo);
                if (periodo == null)
                {
                    return new ResultadoOperacionDTO { Exito = false, Mensaje = "El período especificado no existe." };
                }

                // 2. Validar que el período esté 'Cerrado'
                if (periodo.PeriodoEstado != "C")
                {
                    return new ResultadoOperacionDTO { Exito = false, Mensaje = $"El período {periodoCodigo} no está 'Cerrado'. No se puede anular." };
                }

                // 3. <<== NUEVA VALIDACIÓN (LA CLAVE) ==>>
                //    Verificar si existe algún período CERRADO *posterior* a este.
                var todosPeriodos = await _periodoRepo.ListarTodos();
                var periodoPosteriorCerrado = todosPeriodos.FirstOrDefault(p =>
                    p.PeriodoInicio > periodo.PeriodoInicio &&
                    p.PeriodoEstado == "C");

                if (periodoPosteriorCerrado != null)
                {
                    // Si existe, bloqueamos la anulación
                    return new ResultadoOperacionDTO
                    {
                        Exito = false,
                        Mensaje = $"No se puede anular {periodoCodigo}. Debe anular primero el período posterior ({periodoPosteriorCerrado.PeriodoCodigo})."
                    };
                }

                // 4. Si pasa todas las validaciones, Llama al SP
                await _periodoRepo.AnularNominaPeriodo(periodoCodigo);

                return new ResultadoOperacionDTO
                {
                    Exito = true,
                    Mensaje = $"Nómina del período {periodoCodigo} anulada correctamente. El período se ha re-abierto."
                };
            }
            catch (Exception ex)
            {
                return new ResultadoOperacionDTO { Exito = false, Mensaje = $"Error inesperado al anular: {ex.Message}" };
            }
        }

        public async Task<ResultadoOperacionDTO> GenerarNuevosPeriodos()
        {
            try
            {
                await _periodoRepo.GenerarNuevosPeriodosAnuales();
                return new ResultadoOperacionDTO
                {
                    Exito = true,
                    Mensaje = "Se generaron 12 nuevos períodos (próximo año) exitosamente."
                };
            }
            catch (Exception ex)
            {
                return new ResultadoOperacionDTO { Exito = false, Mensaje = ex.Message };
            }
        }
    }

}