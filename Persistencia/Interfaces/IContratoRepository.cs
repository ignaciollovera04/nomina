using Dominio.Entidades;
using Dominio.Resultados;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Persistencia.Interfaces
{
    public interface IContratoRepository
    {
        Task<List<ContratoCalculoInfo>> ListarContratosActivos();
        Task<ContratoCalculoInfo> ObtenerPorCodigo(string contratoCodigo);


        // --- (Nuevos métodos para HU1) ---

        // Llama a sp_ListarEmpleadosContratos 
        Task<List<ContratoGestion>> ListarGestionContratos();

     
        Task<string> CrearContrato(Contrato contrato);

 
        Task EditarContrato(Contrato contrato);

     
        Task FinalizarContrato(string contratoCodigo, string motivo);

       
        Task<bool> EmpleadoTieneContratoActivo(string empleadoCodigo);
    }



}
