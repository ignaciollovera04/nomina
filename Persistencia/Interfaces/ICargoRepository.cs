using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dominio.Entidades;
namespace Persistencia.Interfaces
{
    public interface ICargoRepository
    {
        Task<List<Cargo>> ListarActivos();
    }
}
