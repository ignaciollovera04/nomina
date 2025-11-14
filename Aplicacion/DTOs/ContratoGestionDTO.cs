using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aplicacion.DTOs
{
    public class ContratoGestionDTO
    {
        public string ContratoCodigo { get; set; }
        public string EmpleadoCodigo { get; set; }
        public string EmpleadoNombre { get; set; }
        public string TipoContrato { get; set; }
        public string Cargo { get; set; }
        public string Area { get; set; }
        public decimal Sueldo { get; set; }
        public string EstadoContrato { get; set; }
        public string EstadoEmpleado { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
    }
}
