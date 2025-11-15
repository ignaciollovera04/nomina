using Aplicacion.DTOs; 
using Dominio.Entidades; 
using System.Collections.Generic;
using System.Threading.Tasks;
using Dominio.Resultados;

namespace Aplicacion.Servicios
{
    public interface IContratoService
    {

        Task<List<ContratoGestionDTO>> ListarContratosGestion();

     
        Task<List<EmpleadoBusquedaDTO>> BuscarEmpleadosActivos(string query);

        Task<List<EmpleadoBusquedaDTO>> BuscarEmpleadosParaContrato();


        Task<ContratoFormDTO> ObtenerDatosParaFormularioContrato();

       
        Task<ContratoCalculoInfo> ObtenerContratoPorCodigo(string contratoCodigo);

      
        Task<ResultadoOperacionDTO> CrearContrato(Contrato contrato);

      
        Task<ResultadoOperacionDTO> EditarContrato(Contrato contrato);

    
        Task<ResultadoOperacionDTO> FinalizarContrato(string contratoCodigo, string motivo);

        
    }

  
    public class ResultadoOperacionDTO
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; }
    }

   
    public class ContratoFormDTO
    {
        public List<Area> Areas { get; set; } = new List<Area>();
        public List<Cargo> Cargos { get; set; } = new List<Cargo>();
      
    }
}