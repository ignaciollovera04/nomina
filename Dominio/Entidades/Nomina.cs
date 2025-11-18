using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio.Entidades
{
    public class Nomina
    {
        public string NominaCodigo { get; set; }
        public string PeriodoCodigo { get; set; }
        public string ContratoCodigo { get; set; }
        public int NominaHorasExtras { get; set; }
        public decimal NominaBonificacion { get; set; }
        public decimal NominaDescuentos { get; set; }
        public decimal NominaTotalIngresos { get; set; }
        public decimal NominaTotalDescuentos { get; set; }
        public decimal NominaSueldoNeto { get; set; }
        public DateTime NominaFechaProcesamiento { get; set; }
        public string NominaEstado { get; set; }

        // Propiedades adicionales para cálculos
        public string EmpleadoNombre { get; set; }
        public decimal SueldoBase { get; set; }
        public string RegimenPensionario { get; set; }
        public string AsignacionFamiliar { get; set; }
    }
}
