using Dapper;
using Dominio.Entidades;
using Persistencia.Interfaces;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Persistencia.Repositorios
{
    public class CargoRepository : ICargoRepository
    {
        private readonly IDbConnection _connection;
        public CargoRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<List<Cargo>> ListarActivos()
        {
            var cargos = await _connection.QueryAsync<Cargo>(
                "dbo.sp_ListarCargosActivos",
                commandType: CommandType.StoredProcedure);
            return cargos.ToList();
        }
    }
}