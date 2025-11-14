using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio.Entidades
{
    public class NominaDetalle
    {
        public string NominaCodigo { get; set; }
        public string DNI { get; set; }
        public string EmpleadoNombre { get; set; }
        public string Cargo { get; set; }
        public string Area { get; set; }
        public string TipoContrato { get; set; }

      
        public decimal SueldoBase { get; set; }
        public decimal AsignacionFamiliar { get; set; }
        public int NominaHorasExtras { get; set; }
        public decimal Bonificaciones { get; set; }
        public decimal SalarioBruto { get; set; } 

        
        public decimal DescuentoONP { get; set; }
        public decimal DescuentoAFP { get; set; }
        public decimal ImpuestoQuintaCategoria { get; set; }
        public decimal Deducciones { get; set; } 

        
        public decimal ESSALUD { get; set; }

        
        public decimal SueldoNeto { get; set; }

       
        public string FechaProcesamiento { get; set; } 
        public string PeriodoCodigo { get; set; }
        public string PeriodoInicio { get; set; }
        public string PeriodoFin { get; set; }
        public string EstadoNomina { get; set; } 
    }
}
