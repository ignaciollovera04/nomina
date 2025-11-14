using Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistencia.Interfaces
{
    public interface IPeriodoNominaRepository
    {
        Task<List<PeriodoNomina>> ListarTodos();
        Task<PeriodoNomina> ObtenerPorCodigo(string periodoCodigo);
        Task<bool> ActualizarEstado(string periodoCodigo, string nuevoEstado);
    }
}
