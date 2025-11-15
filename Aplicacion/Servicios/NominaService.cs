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
            // 1. Obtener TODOS los períodos de la base de datos
            var todosLosPeriodos = await _periodoRepo.ListarTodos();

            // 2. Ordenarlos por fecha de inicio para asegurar la secuencia
            var periodosOrdenados = todosLosPeriodos.OrderBy(p => p.PeriodoInicio).ToList();

            // 3. Encontrar el ÚLTIMO período que ya fue 'Cerrado' ('C')
            var ultimoPeriodoCerrado = periodosOrdenados.LastOrDefault(p => p.PeriodoEstado == "C");

            PeriodoNomina proximoPeriodoParaProcesar = null;

            if (ultimoPeriodoCerrado == null)
            {
                // CASO 1: Es el inicio. No hay ningún período cerrado.
                // Buscamos el PRIMER período 'Activo' ('A') de toda la lista.
                proximoPeriodoParaProcesar = periodosOrdenados.FirstOrDefault(p => p.PeriodoEstado == "A");
            }
            else
            {
                // CASO 2: Ya hay períodos cerrados.
                // Buscamos el primer período 'Activo' ('A') que venga DESPUÉS del último cerrado.
                proximoPeriodoParaProcesar = periodosOrdenados.FirstOrDefault(p =>
                    p.PeriodoInicio > ultimoPeriodoCerrado.PeriodoInicio &&
                    p.PeriodoEstado == "A");
            }

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
        public async Task<ReporteNominaCompletoDTO> ObtenerNominasProcesadas(
      string periodoCodigo = null,
      string areaCodigo = null,
      string tipoContrato = null)
        {
            // 1. Obtener las ENTIDADES de la base de datos (con todos los detalles)
            // 'nominasDeDB' es una List<Dominio.Entidades.NominaDetalle>
            var nominasDeDB = await _nominaRepo.ListarNominasProcesadas(
                periodoCodigo,
                areaCodigo,
                tipoContrato);

            // 2. APLICAR REGLA DE DOMINIO (RN-02: Calcular Totales)
            // ¡Aquí es donde llamamos a la lógica testeable!
            var totales = ReporteNominaRules.CalcularTotales(nominasDeDB);

            // 3. Mapear las Entidades a DTOs (para la vista)
            // Tu DTO 'NominaDetalleDTO' está perfecto para esto
            var nominasDTO = nominasDeDB.Select(n => new NominaDetalleDTO
            {
                // Identificación
                NominaCodigo = n.NominaCodigo,
                ContratoCodigo = n.ContratoCodigo,
                EmpleadoNombre = n.EmpleadoNombre,
                DNI = n.DNI,
                Area = n.Area,
                Cargo = n.Cargo,
                Periodo = $"{n.PeriodoInicio} - {n.PeriodoFin}",

                // Ingresos (RN-01)
                SueldoBaseStr = n.SueldoBase.ToString("N2"),
                AsignacionFamiliarStr = n.AsignacionFamiliar.ToString("N2"),
                HorasExtrasStr = n.NominaHorasExtras.ToString(),
                GratificacionStr = n.Bonificaciones.ToString("N2"), // (Grati+CTS)
                CTSStr = "0.00",
                SueldoBrutoStr = n.SalarioBruto.ToString("N2"),

                // Descuentos (RN-06, RN-08, RN-10)
                DescuentoONPStr = n.DescuentoONP.ToString("N2"),
                DescuentoAFPStr = n.DescuentoAFP.ToString("N2"),
                Renta5taStr = n.ImpuestoQuintaCategoria.ToString("N2"),
                TotalDescuentosStr = n.Deducciones.ToString("N2"),

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
    }
}