using Aplicacion.DTOs;
using Dominio.Entidades;
using Dominio.Reglas; 
using Dominio.Resultados;
using Persistencia.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aplicacion.Servicios
{
    public class ContratoService : IContratoService
    {
        private readonly IContratoRepository _contratoRepo;
        private readonly IEmpleadoRepository _empleadoRepo;
        private readonly IAreaRepository _areaRepo;
        private readonly ICargoRepository _cargoRepo;

        // Constructor con todas las dependencias
        public ContratoService(
            IContratoRepository contratoRepo,
            IEmpleadoRepository empleadoRepo,
            IAreaRepository areaRepo,
            ICargoRepository cargoRepo)
        {
            _contratoRepo = contratoRepo;
            _empleadoRepo = empleadoRepo;
            _areaRepo = areaRepo;
            _cargoRepo = cargoRepo;
        }

        // --- MÉTODOS DE LECTURA (Dashboard y Formularios) ---

        
        public async Task<List<ContratoGestionDTO>> ListarContratosGestion()
        {
            // 1. Obtener la Entidad de Dominio desde la persistencia
            var entidades = await _contratoRepo.ListarGestionContratos();

            // 2. Mapear la Entidad al DTO que verá la vista
            return entidades.Select(c => new ContratoGestionDTO
            {
                ContratoCodigo = c.ContratoCodigo,
                EmpleadoCodigo = c.EmpleadoCodigo,
                EmpleadoNombre = c.EmpleadoNombre,
                TipoContrato = c.TipoContrato,
                Cargo = c.Cargo,
                Area = c.Area,
                Sueldo = c.Sueldo,
                EstadoContrato = c.EstadoContrato,
                EstadoEmpleado = c.EstadoEmpleado,
                FechaInicio = c.FechaInicio,
                FechaFin = c.FechaFin
            }).ToList();
        }

        /// <summary>
        /// Busca empleados activos para el formulario 'Crear Contrato' 
                    /// </summary>
        public async Task<List<EmpleadoBusquedaDTO>> BuscarEmpleadosActivos(string query)
        {
            // 1. Obtener la Entidad de Dominio
            var entidades = await _empleadoRepo.BuscarEmpleados(query);

            // 2. Mapear al DTO
            return entidades.Select(e => new EmpleadoBusquedaDTO
            {
                Codigo = e.Codigo,
                Nombre = e.Nombre,
                Cargo = e.Cargo,
                Area = e.Area
            }).ToList();
        }

        /// <summary>
        /// Carga los datos necesarios para los dropdowns del formulario 
                    /// </summary>
        public async Task<ContratoFormDTO> ObtenerDatosParaFormularioContrato()
        {
            var areas = await _areaRepo.ListarActivas();
            var cargos = await _cargoRepo.ListarActivos();

           

            return new ContratoFormDTO
            {
                Areas = areas,  
                Cargos = cargos  
            };
        }

    
        public async Task<ContratoCalculoInfo> ObtenerContratoPorCodigo(string contratoCodigo)
        {
            // Pasa la llamada directamente al repositorio
            return await _contratoRepo.ObtenerPorCodigo(contratoCodigo);
        }


        // --- MÉTODOS DE ESCRITURA (CON REGLAS DE NEGOCIO) ---


        /// Crea un nuevo contrato aplicando las RN: 01, 02, 06, 11, 13 
                   
        public async Task<ResultadoOperacionDTO> CrearContrato(Contrato contrato)
        {
            try
            {
                // 1. OBTENER DATOS (Orquestación)
                var tieneActivo = await _contratoRepo.EmpleadoTieneContratoActivo(contrato.ContratoEmpleadoCodigo);
                var empleado = await _empleadoRepo.ObtenerEmpleadoPorCodigo(contrato.ContratoEmpleadoCodigo);

                // 2. VALIDAR REGLAS (Lógica de Dominio) 
                var validacion = ContratoRules.ValidarCreacion(contrato, empleado, tieneActivo);
                if (!validacion.EsValido)
                {
                    return new ResultadoOperacionDTO { Exito = false, Mensaje = validacion.Mensaje };
                }

                // 3. APLICAR REGLAS (Lógica de Dominio)
                ContratoRules.AplicarReglasPorDefecto(contrato); // Aplica ONP por defecto (RN-13) 

                // 4. GUARDAR (Orquestación)
                contrato.ContratoEstado = "A";
                var nuevoCodigo = await _contratoRepo.CrearContrato(contrato);

                return new ResultadoOperacionDTO { Exito = true, Mensaje = $"Contrato {nuevoCodigo} creado exitosamente." };
            }
            catch (Exception ex)
            {
                return new ResultadoOperacionDTO { Exito = false, Mensaje = $"Error inesperado: {ex.Message}" };
            }
        }

     
        /// Edita un contrato existente aplicando RN-11 
                    
        public async Task<ResultadoOperacionDTO> EditarContrato(Contrato contrato)
        {
            try
            {
                // RN-11: Validación de RMV al editar 
                if (contrato.ContratoSueldo < ContratoRules.RMV_VIGENTE)
                {
                    return new ResultadoOperacionDTO { Exito = false, Mensaje = $"El sueldo base no puede ser inferior a la RMV vigente (S/ {ContratoRules.RMV_VIGENTE})." };
                }

                // RN-04: Auditoría se maneja en el SP (ContratoFechaModificacion) 
                await _contratoRepo.EditarContrato(contrato);
                return new ResultadoOperacionDTO { Exito = true, Mensaje = "Contrato actualizado exitosamente." };
            }
            catch (Exception ex)
            {
                return new ResultadoOperacionDTO { Exito = false, Mensaje = $"Error inesperado: {ex.Message}" };
            }
        }

      
        public async Task<ResultadoOperacionDTO> FinalizarContrato(string contratoCodigo, string motivo)
        {
            try
            {
                // 1. VALIDAR REGLAS (Lógica de Dominio)
                var validacion = ContratoRules.ValidarFinalizacion(motivo); // RN-03 
                if (!validacion.EsValido)
                {
                    return new ResultadoOperacionDTO { Exito = false, Mensaje = validacion.Mensaje };
                }

                // 2. GUARDAR (Orquestación)
                // CA-03: El SP cambia el estado a 'F' y guarda el motivo 
                await _contratoRepo.FinalizarContrato(contratoCodigo, motivo);
                return new ResultadoOperacionDTO { Exito = true, Mensaje = "Contrato finalizado exitosamente." };
            }
            catch (Exception ex)
            {
                return new ResultadoOperacionDTO { Exito = false, Mensaje = $"Error inesperado: {ex.Message}" };
            }
        }
    }
}