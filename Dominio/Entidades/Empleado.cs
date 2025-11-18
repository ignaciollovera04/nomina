using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio.Entidades
{
    public class Empleado
    {
        public string EmpleadosCodigo { get; set; }
        public string EmpleadosPaterno { get; set; }
        public string EmpleadosMaterno { get; set; }
        public string EmpleadosNombres { get; set; }
        public string EmpleadoNombreCompleto { get; set; }
        public DateTime EmpleadosFechaNacimiento { get; set; }
        public string EmpleadosSexo { get; set; }
        public string EmpleadosDireccion { get; set; }
        public string EmpleadosFono { get; set; }
        public string EmpleadosCorreoPersonal { get; set; }
        public string EmpleadosCorreoCorporativo { get; set; }
        public string EmpleadosTipoDocumento { get; set; }
        public string EmpleadosDocumento { get; set; }
        public string EmpleadosEstado { get; set; }
    }
}
