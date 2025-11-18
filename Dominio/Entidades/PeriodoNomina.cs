using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio.Entidades
{
    public class PeriodoNomina
    {
        public string PeriodoCodigo { get; set; }
        public string PeriodoTipo { get; set; }
        public DateTime PeriodoInicio { get; set; }
        public DateTime PeriodoFin { get; set; }
        public string PeriodoEstado { get; set; }
    }
}
