using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dominio.Resultados;

namespace Aplicacion.DTOs
{
    public class ReporteNominaCompletoDTO
    {
        public List<NominaDetalleDTO> Nominas { get; set; } = new List<NominaDetalleDTO>();

        public ReporteTotales Totales { get; set; } = new ReporteTotales();
    }
}
