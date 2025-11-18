using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio.Entidades
{
    public class Contrato
    {
        public string ContratoCodigo { get; set; }
        public string ContratoEmpleadoCodigo { get; set; }
        public string ContratoTipoContrato { get; set; }
        public DateTime ContratoFechaInicio { get; set; }
        public DateTime? ContratoFechaFin { get; set; } // Permite nulos
        public string ContratoRegimenPensionario { get; set; }
        public string ContratoCargoCodigo { get; set; }
        public string ContratoAreaCodigo { get; set; }
        public decimal ContratoSueldo { get; set; }
        public string ContratoTipoSeguro { get; set; }
        public string ContratoEntidadEPS { get; set; } // Permite nulos
        public string ContratoAsignacionFamiliar { get; set; }
        public string ContratoMotivoFin { get; set; } // Permite nulos
        public DateTime ContratoFechaRegistro { get; set; }
        public DateTime? ContratoFechaModificacion { get; set; } // Permite nulos
        public string ContratoEstado { get; set; }
    }
}
