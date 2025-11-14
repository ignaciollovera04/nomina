using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aplicacion.DTOs
{
    public class DetalleNominaProcesadaDTO
    {
        public string NominaCodigo { get; set; }
        public string EmpleadoNombre { get; set; }
        public decimal SueldoBase { get; set; }
        public int HorasExtras { get; set; }
        public decimal TotalIngresos { get; set; }
        public decimal TotalDescuentos { get; set; }
        public decimal SueldoNeto { get; set; }
        public string Estado { get; set; }
    }
}
