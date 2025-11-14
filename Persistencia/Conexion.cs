using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistencia
{
    public class Conexion
    {
        private readonly string _connectionString;

        public Conexion(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IDbConnection ObtenerConexion()
        {
            return new SqlConnection(_connectionString);
        }

    }
}
