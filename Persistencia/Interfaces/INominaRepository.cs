using Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistencia.Interfaces
{
    public interface INominaRepository
    {
        Task<string> InsertarNomina(
                string periodoCodigo,
                string contratoCodigo,
                int horasExtras,
                decimal bonificacion,

                // --- Parámetros Detallados (Corregidos) ---
                decimal sueldoBase,
                decimal asignacionFamiliar,
                decimal descuentoONP, // <-- Corregido
                decimal descuentoAFP, // <-- Corregido
                decimal descuentoImpuesto5ta,
                decimal aporteESSALUD,

                // --- Totales ---
                decimal totalIngresos,
                decimal totalDescuentos,
                decimal sueldoNeto,
                string estado = "P");

        // (Tu NominaDetalle puede ser un DTO, ajusta el tipo si es necesario)
        Task<List<NominaDetalle>> ListarNominasProcesadas(
            string periodoCodigo = null,
            string areaCodigo = null,
            string tipoContrato = null);

        Task<bool> ExisteNominaParaPeriodo(string periodoCodigo, string contratoCodigo);

  
    }
}
