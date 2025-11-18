using Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio.Resultados
{
    public class ContratoCalculoInfo : Contrato
    {
        public string EmpleadoNombre { get; set; }
        public string Cargo { get; set; }
        public string Area { get; set; }
        public string DNI { get; set; }
    }
}
