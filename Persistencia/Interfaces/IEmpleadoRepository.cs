using Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistencia.Interfaces
{
    public interface IEmpleadoRepository
    {
        Task<List<EmpleadoBusqueda>> BuscarEmpleados(string query);
        Task<Empleado> ObtenerEmpleadoPorCodigo(string codigo);
    }
}
