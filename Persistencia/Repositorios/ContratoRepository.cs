using Dapper;
using Dominio.Entidades;
using Dominio.Resultados;
using Persistencia.Interfaces;
using System;
using System.Collections.Generic;
using System.Data; 
using System.Linq;
using System.Threading.Tasks;



namespace Persistencia.Repositorios
{
 
    public class ContratoRepository : IContratoRepository
    {
     
        private readonly IDbConnection _connection;

      
        public ContratoRepository(IDbConnection connection)
        {
           
            _connection = connection;
        }

        public async Task<List<ContratoCalculoInfo>> ListarContratosActivos()
        {

            var contratos = await _connection.QueryAsync<ContratoCalculoInfo>(
                "dbo.sp_ListarContratosActivos",
                commandType: System.Data.CommandType.StoredProcedure);
            return contratos.ToList();
        }

        public async Task<ContratoCalculoInfo> ObtenerPorCodigo(string contratoCodigo)
        {
            
            var contrato = await _connection.QueryFirstOrDefaultAsync<ContratoCalculoInfo>(
                "dbo.sp_ObtenerContratoPorCodigo",
                new { ContratoCodigo = contratoCodigo },
                commandType: CommandType.StoredProcedure);

            return contrato; 
        }


        // --- (Nuevos métodos para HU1) ---

        public async Task<List<ContratoGestion>> ListarGestionContratos()
        {
            // Dapper mapea el SP a la Entidad de Dominio
            var contratos = await _connection.QueryAsync<ContratoGestion>(
                "dbo.Sp_ListarEmpleadosContratos",
                commandType: CommandType.StoredProcedure);
            return contratos.ToList();
        }

        public async Task<string> CrearContrato(Contrato contrato)
        {
            // El SP 'sp_CrearContrato' espera los parámetros
            var parameters = new DynamicParameters();
            parameters.Add("@ContratoEmpleadoCodigo", contrato.ContratoEmpleadoCodigo);
            parameters.Add("@ContratoTipoContrato", contrato.ContratoTipoContrato);
            parameters.Add("@ContratoFechaInicio", contrato.ContratoFechaInicio);
            parameters.Add("@ContratoFechaFin", contrato.ContratoFechaFin);
            parameters.Add("@ContratoRegimenPensionario", contrato.ContratoRegimenPensionario);
            parameters.Add("@ContratoCargoCodigo", contrato.ContratoCargoCodigo);
            parameters.Add("@ContratoAreaCodigo", contrato.ContratoAreaCodigo);
            parameters.Add("@ContratoSueldo", contrato.ContratoSueldo);
            parameters.Add("@ContratoTipoSeguro", contrato.ContratoTipoSeguro);
            parameters.Add("@ContratoEntidadEPS", contrato.ContratoEntidadEPS);
            parameters.Add("@ContratoAsignacionFamiliar", contrato.ContratoAsignacionFamiliar);
            parameters.Add("@ContratoEstado", "A"); // Siempre Activo al crear

            // El SP devuelve el código generado
            var resultado = await _connection.QuerySingleAsync<string>(
                "dbo.sp_CrearContrato",
                parameters,
                commandType: CommandType.StoredProcedure);
            return resultado;
        }

        public async Task EditarContrato(Contrato contrato)
        {
            // El SP 'sp_EditarContrato' espera los parámetros
            var parameters = new DynamicParameters();
            parameters.Add("@ContratoCodigo", contrato.ContratoCodigo);
            parameters.Add("@ContratoTipoContrato", contrato.ContratoTipoContrato);
            parameters.Add("@ContratoFechaInicio", contrato.ContratoFechaInicio);
            parameters.Add("@ContratoFechaFin", contrato.ContratoFechaFin);
            parameters.Add("@ContratoRegimenPensionario", contrato.ContratoRegimenPensionario);
            parameters.Add("@ContratoCargoCodigo", contrato.ContratoCargoCodigo);
            parameters.Add("@ContratoAreaCodigo", contrato.ContratoAreaCodigo);
            parameters.Add("@ContratoSueldo", contrato.ContratoSueldo);
            parameters.Add("@ContratoTipoSeguro", contrato.ContratoTipoSeguro);
            parameters.Add("@ContratoEntidadEPS", contrato.ContratoEntidadEPS);
            parameters.Add("@ContratoAsignacionFamiliar", contrato.ContratoAsignacionFamiliar);

            await _connection.ExecuteAsync(
                "dbo.sp_EditarContrato",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task FinalizarContrato(string contratoCodigo, string motivo)
        {
            await _connection.ExecuteAsync(
                "dbo.sp_FinalizarContrato",
                new { ContratoCodigo = contratoCodigo, ContratoMotivoFin = motivo },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<bool> EmpleadoTieneContratoActivo(string empleadoCodigo)
        {
            var resultado = await _connection.QuerySingleAsync<int>(
                "dbo.sp_EmpleadoTieneContratoActivo",
                new { EmpleadoCodigo = empleadoCodigo },
                commandType: CommandType.StoredProcedure);
            return resultado == 1;
        }

    }
}