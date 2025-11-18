using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio.Resultados
{
    public class ReporteTotales
    {
        public decimal TotalSueldoBase { get; set; }
        public decimal TotalAsignacionFamiliar { get; set; }
        public decimal TotalSalarioBruto { get; set; }
        public decimal TotalDescuentos { get; set; }
        public decimal TotalNetoPagar { get; set; }

        public decimal TotalESSALUD { get; set; }
    }
}
