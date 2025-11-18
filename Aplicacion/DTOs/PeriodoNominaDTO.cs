using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aplicacion.DTOs
{
    public class PeriodoNominaDTO
    {
        public string PeriodoCodigo { get; set; }
        public string PeriodoTipo { get; set; }
        public string PeriodoInicioStr { get; set; }
        public string PeriodoFinStr { get; set; }
        public string PeriodoEstado { get; set; }
        public string EstadoDescripcion { get; set; }
    }
}
