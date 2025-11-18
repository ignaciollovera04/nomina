using Dapper;
using Dominio.Entidades;

using Persistencia.Interfaces;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;


namespace Persistencia.Repositorios
{
    public class EmpleadoRepository : IEmpleadoRepository
    {
        private readonly IDbConnection _connection;
        public EmpleadoRepository(IDbConnection connection) { _connection = connection; }

        public async Task<List<EmpleadoBusqueda>> BuscarEmpleados(string query)
        {
            var parameters = new { Query = query };
            
            var empleados = await _connection.QueryAsync<EmpleadoBusqueda>(
                "dbo.sp_BuscarEmpleados",
                parameters,
                commandType: CommandType.StoredProcedure);
            return empleados.ToList();
        }

        public async Task<Empleado> ObtenerEmpleadoPorCodigo(string codigo)
        {
            var parameters = new { EmpleadoCodigo = codigo };
            return await _connection.QueryFirstOrDefaultAsync<Empleado>(
                "dbo.sp_ObtenerEmpleadoPorCodigo",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<List<EmpleadoBusqueda>> BuscarEmpleadosParaContrato()
        {
            var empleados = await _connection.QueryAsync<EmpleadoBusqueda>(
                "dbo.sp_BuscarEmpleadosParaContrato",
                commandType: CommandType.StoredProcedure);
            return empleados.ToList();
        }

    }
}