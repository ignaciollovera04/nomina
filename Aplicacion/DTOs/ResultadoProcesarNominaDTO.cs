using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aplicacion.DTOs
{
    public class ResultadoProcesarNominaDTO
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; }
        public int TotalProcesadas { get; set; }
        public List<string> Errores { get; set; } = new List<string>();
        public List<DetalleNominaProcesadaDTO> Detalles { get; set; } = new List<DetalleNominaProcesadaDTO>();
    }
}
