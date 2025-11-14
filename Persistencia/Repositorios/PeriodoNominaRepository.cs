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
    public class PeriodoNominaRepository : IPeriodoNominaRepository
    {

        private readonly IDbConnection _connection;

        
        public PeriodoNominaRepository(IDbConnection connection)
        {
            
            _connection = connection;
        }

        public async Task<List<PeriodoNomina>> ListarTodos()
        {
            var periodos = await _connection.QueryAsync<PeriodoNomina>(
                "dbo.sp_ListarTodosPeriodos",
                commandType: System.Data.CommandType.StoredProcedure);

            return periodos.ToList();
        }

        public async Task<PeriodoNomina> ObtenerPorCodigo(string periodoCodigo)
        {
            var parameters = new { PeriodoCodigo = periodoCodigo };

            return await _connection.QueryFirstOrDefaultAsync<PeriodoNomina>(
                "dbo.sp_ObtenerPeriodoPorCodigo",
                parameters,
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<bool> ActualizarEstado(string periodoCodigo, string nuevoEstado)
        {
            var parameters = new
            {
                PeriodoCodigo = periodoCodigo,
                NuevoEstado = nuevoEstado
            };

            var filasAfectadas = await _connection.ExecuteAsync(
                "dbo.sp_ActualizarEstadoPeriodo",
                parameters,
                commandType: System.Data.CommandType.StoredProcedure);

            return filasAfectadas > 0;
        }
    }
}