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
    public class AreaRepository : IAreaRepository
    {
        private readonly IDbConnection _connection;

        public AreaRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<List<Area>> ListarActivas()
        {
            // Llama al SP que ya tienes
            var query = "dbo.sp_ListarAreasActivas";
            var areas = await _connection.QueryAsync<Area>(query, commandType: CommandType.StoredProcedure);
            return areas.ToList();
        }
    }
}