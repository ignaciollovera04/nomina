  using Dapper;
using Dominio.Entidades;
using Persistencia.Interfaces;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistencia.Repositorios
{
    public class NominaRepository : INominaRepository
    {
     
        private readonly IDbConnection _connection;


        public NominaRepository(IDbConnection connection)
        {
            
            _connection = connection;
        }

        public async Task<string> InsertarNomina(
            string periodoCodigo,
            string contratoCodigo,
            int horasExtras,
            decimal bonificacion,
            decimal sueldoBase,
            decimal asignacionFamiliar,
            decimal descuentoONP, 
            decimal descuentoAFP, 
            decimal descuentoImpuesto5ta,
            decimal aporteESSALUD,
            decimal totalIngresos,
            decimal totalDescuentos,
            decimal sueldoNeto,
            string estado = "P")
        {
            var parameters = new DynamicParameters();
            parameters.Add("@PeriodoCodigo", periodoCodigo);
            parameters.Add("@ContratoCodigo", contratoCodigo);
            parameters.Add("@NominaHorasExtras", horasExtras);
            parameters.Add("@NominaBonificacion", bonificacion);

            
            parameters.Add("@NominaSueldoBase", sueldoBase);
            parameters.Add("@NominaAsignacionFamiliar", asignacionFamiliar);
            parameters.Add("@NominaDescuento_ONP", descuentoONP); 
            parameters.Add("@NominaDescuento_AFP", descuentoAFP); 
            parameters.Add("@NominaDescuento_Impuesto5ta", descuentoImpuesto5ta);
            parameters.Add("@NominaAporte_ESSALUD", aporteESSALUD);

          
            parameters.Add("@NominaTotalIngresos", totalIngresos);
            parameters.Add("@NominaTotalDescuentos", totalDescuentos);
            parameters.Add("@NominaSueldoNeto", sueldoNeto);

            parameters.Add("@NominaEstado", estado);
            parameters.Add("@RegistrarHistorial", 1);

            var resultado = await _connection.QueryFirstOrDefaultAsync<dynamic>(
                "dbo.sp_InsertarNomina",
                parameters,
                commandType: CommandType.StoredProcedure);

            return resultado?.NuevoCodigoGenerado?.ToString() ?? string.Empty;
        }

        public async Task<List<NominaDetalle>> ListarNominasProcesadas(
            string periodoCodigo = null,
            string areaCodigo = null,
            string tipoContrato = null)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@PeriodoCodigo", periodoCodigo);
            parameters.Add("@AreaCodigo", areaCodigo);
            parameters.Add("@TipoContrato", tipoContrato);

     
            var nominas = await _connection.QueryAsync<NominaDetalle>(
                "dbo.sp_ListarNominasProcesadas",
                parameters,
                commandType: CommandType.StoredProcedure);

            return nominas.ToList();
        }

        public async Task<List<NominaDetalle>> ListarNominasProcesadas()
        {
     
            var nominas = await _connection.QueryAsync<NominaDetalle>(
                "dbo.sp_ListarNominasProcesadas", 
                commandType: CommandType.StoredProcedure);

            return nominas.ToList();
        }

        public async Task<bool> ExisteNominaParaPeriodo(string periodoCodigo, string contratoCodigo)
        {
            var parameters = new
            {
                PeriodoCodigo = periodoCodigo,
                ContratoCodigo = contratoCodigo
            };

        
            var existe = await _connection.ExecuteScalarAsync<int>(
                "dbo.sp_ExisteNominaParaPeriodo", 
                parameters,
                commandType: System.Data.CommandType.StoredProcedure); 

            return existe > 0; 
        }

        public async Task<List<NominaDetalle>> ListarNominasProcesadas(string periodoCodigo = null)
        {
       
            var parameters = new DynamicParameters();
            parameters.Add("@PeriodoCodigo", periodoCodigo);

            var nominas = await _connection.QueryAsync<NominaDetalle>(
                "dbo.sp_ListarNominasProcesadas",
                parameters, 
                commandType: CommandType.StoredProcedure);

            return nominas.ToList();
        }
    }


}