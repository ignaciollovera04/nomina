using Aplicacion.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aplicacion
{
    public interface INominaService
    {

        Task<List<PeriodoNominaDTO>> ObtenerTodosLosPeriodos();
        Task<List<PeriodoNominaDTO>> ObtenerPeriodosDisponibles();
        Task<ResultadoProcesarNominaDTO> ProcesarNominaPorPeriodo(string periodoCodigo);
        Task<List<NominaDetalleDTO>> ObtenerNominasProcesadas(
            string periodoCodigo = null,
            string areaCodigo = null,
            string tipoContrato = null);
    }
}
