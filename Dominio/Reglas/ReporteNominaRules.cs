using Dominio.Entidades;
using Dominio.Resultados;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio.Reglas
{
    public static class ReporteNominaRules
    {
        public static ReporteTotales CalcularTotales(List<NominaDetalle> nominas)
        {
            // Si la lista está vacía, devuelve un objeto con ceros
            if (nominas == null || !nominas.Any())
            {
                return new ReporteTotales();
            }

            // Realiza la suma sobre las propiedades numéricas de la entidad 'NominaDetalle'.
            // Esto es mucho más rápido y limpio que sumar strings en el Controlador.
            var totales = new ReporteTotales
            {
                TotalSueldoBase = nominas.Sum(n => n.SueldoBase),
                TotalAsignacionFamiliar = nominas.Sum(n => n.AsignacionFamiliar),
                TotalSalarioBruto = nominas.Sum(n => n.SalarioBruto),
                TotalDescuentos = nominas.Sum(n => n.Deducciones), // Suma el total de descuentos
                TotalNetoPagar = nominas.Sum(n => n.SueldoNeto),
                TotalESSALUD = nominas.Sum(n => n.ESSALUD) // Suma el aporte del empleador
            };

            return totales;
        }
    }
}
