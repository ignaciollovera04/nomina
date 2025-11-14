using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aplicacion.DTOs
{
    public class NominaDetalleDTO
    {
        public string NominaCodigo { get; set; }
        public string EmpleadoNombre { get; set; }
        public string DNI { get; set; } 
        public string Area { get; set; }
        public string Cargo { get; set; }
        public string Periodo { get; set; }

        // Ingresos
        public string SueldoBaseStr { get; set; }
        public string AsignacionFamiliarStr { get; set; }
        public string HorasExtrasStr { get; set; }
        public string GratificacionStr { get; set; }
        public string CTSStr { get; set; }
        public string SueldoBrutoStr { get; set; }

        // Descuentos
        public string DescuentoONPStr { get; set; }
        public string DescuentoAFPStr { get; set; }
        public string Renta5taStr { get; set; }
        public string TotalDescuentosStr { get; set; }

        // Aporte empleador
        public string AporteESSALUDStr { get; set; }

        // Neto
        public string SueldoNetoStr { get; set; }

        public string FechaProcesamiento { get; set; }
        public string EstadoDescripcion { get; set; }
    }
}
