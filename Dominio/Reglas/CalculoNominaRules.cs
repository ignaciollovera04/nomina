using Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio.Reglas
{
    public class CalculoNominaRules
    {
        private const decimal UIT_2025 = 5200m;
        private const decimal TASA_ONP = 0.13m;
        private const decimal TASA_AFP_APORTE = 0.10m;
        private const decimal TASA_ESSALUD = 0.09m;
        private const decimal ASIGNACION_FAMILIAR_MONTO = 102.50m; // 2025
        private const decimal GRATIFICACION_ADICIONAL = 0.09m;

        public class ResultadoCalculoNomina
        {
            // Ingresos
            public decimal SueldoBase { get; set; }
            public decimal AsignacionFamiliar { get; set; }
            public decimal HorasExtras { get; set; }
            public decimal Gratificacion { get; set; }
            public decimal CTS { get; set; }
            public decimal SueldoBruto { get; set; }
            public decimal TotalIngresos { get; set; }

            // Descuentos
            public decimal DescuentoONP { get; set; }
            public decimal DescuentoAFP { get; set; }
            public decimal RetencionRenta5ta { get; set; }
            public decimal TotalDescuentos { get; set; }

            // Aportes Empleador
            public decimal AporteESSALUD { get; set; }

            // Neto
            public decimal SueldoNeto { get; set; }
        }

        public static ResultadoCalculoNomina CalcularNomina(
            Contrato contrato,
            int mes,
            int horasExtras = 0,
            int mesesTrabajados = 6)
        {
            var resultado = new ResultadoCalculoNomina();

            // 1. INGRESOS BASE
            resultado.SueldoBase = contrato.ContratoSueldo;

            // 2. ASIGNACIÓN FAMILIAR (RN-16)
            resultado.AsignacionFamiliar = contrato.ContratoAsignacionFamiliar == "S"
                ? ASIGNACION_FAMILIAR_MONTO
                : 0;

            // 3. HORAS EXTRAS (monto fijo por ahora, se puede calcular)
            // Asumiendo S/ 15 por hora extra
            resultado.HorasExtras = horasExtras * 15m;

            // 4. GRATIFICACIÓN (RN-09) - Solo Julio y Diciembre
            if (mes == 7 || mes == 12)
            {
                resultado.Gratificacion = resultado.SueldoBase +
                    (resultado.SueldoBase * GRATIFICACION_ADICIONAL);
            }

            // 5. CTS (RN-15) - Solo Mayo y Noviembre
            if (mes == 5 || mes == 11)
            {
                resultado.CTS = ((resultado.SueldoBase + resultado.AsignacionFamiliar) / 12m)
                    * mesesTrabajados;
            }

            // 6. SUELDO BRUTO (RN-16)
            resultado.SueldoBruto = resultado.SueldoBase +
                                   resultado.AsignacionFamiliar +
                                   resultado.HorasExtras;

            // 7. TOTAL INGRESOS (incluye gratificación y CTS)
            resultado.TotalIngresos = resultado.SueldoBruto +
                                     resultado.Gratificacion +
                                     resultado.CTS;

            // 8. DESCUENTOS SISTEMA PENSIONARIO
            if (contrato.ContratoRegimenPensionario == "ONP")
            {
                // RN-06: ONP = 13%
                resultado.DescuentoONP = resultado.SueldoBruto * TASA_ONP;
            }
            else
            {
                // RN-08: AFP = 10% + Prima + Comisión
                var (prima, comision) = ObtenerTasasAFP(contrato.ContratoRegimenPensionario);
                resultado.DescuentoAFP = resultado.SueldoBruto *
                    (TASA_AFP_APORTE + prima + comision);
            }

            // 9. RETENCIÓN RENTA 5TA CATEGORÍA (RN-10, RN-11, RN-12, RN-13)
            resultado.RetencionRenta5ta = CalcularRenta5taCategoria(
                resultado.SueldoBase,
                resultado.AsignacionFamiliar);

            // 10. TOTAL DESCUENTOS (RN-14)
            resultado.TotalDescuentos = resultado.DescuentoONP +
                                       resultado.DescuentoAFP +
                                       resultado.RetencionRenta5ta;

            // 11. APORTE ESSALUD (RN-07) - No es descuento, es aporte empleador
            resultado.AporteESSALUD = resultado.TotalIngresos * TASA_ESSALUD;

            // 12. SUELDO NETO (RN-17)
            resultado.SueldoNeto = resultado.TotalIngresos - resultado.TotalDescuentos;

            return resultado;
        }

        private static (decimal prima, decimal comision) ObtenerTasasAFP(string regimenPensionario)
        {
            // RN-08: Tasas según AFP
            return regimenPensionario?.ToUpper() switch
            {
                "AFP INTEGRA" => (0.0135m, 0.0147m),
                "AFP PROFUTURO" => (0.0135m, 0.0125m),
                "AFP HABITAT" => (0.0135m, 0.0145m),
                "AFP PRIMA" => (0.0135m, 0.0160m),
                _ => (0.0135m, 0.0147m) 
            };
        }

        private static decimal CalcularRenta5taCategoria(decimal sueldoBase, decimal asignacionFamiliar)
        {
            // RN-10, RN-11: Proyección anual
            decimal gratificaciones = (sueldoBase * 2) + (sueldoBase * 2 * GRATIFICACION_ADICIONAL);
            decimal ingresosAnuales = (sueldoBase * 12) + gratificaciones + (asignacionFamiliar * 12);

            // RN-12: Deducción de 7 UIT
            decimal rentaNeta = ingresosAnuales - (7 * UIT_2025);

            if (rentaNeta <= 0)
                return 0;

            // RN-12, RN-13: Aplicar tramos
            decimal impuestoAnual = 0;
            decimal[] limites = { 5 * UIT_2025, 20 * UIT_2025, 35 * UIT_2025, 45 * UIT_2025 };
            decimal[] tasas = { 0.08m, 0.14m, 0.17m, 0.20m, 0.30m };

            decimal acumulado = 0;
            for (int i = 0; i < limites.Length; i++)
            {
                if (rentaNeta > limites[i])
                {
                    decimal tramo = limites[i] - acumulado;
                    impuestoAnual += tramo * tasas[i];
                    acumulado = limites[i];
                }
                else
                {
                    decimal tramo = rentaNeta - acumulado;
                    impuestoAnual += tramo * tasas[i];
                    break;
                }
            }

            if (rentaNeta > limites[limites.Length - 1])
            {
                decimal tramoFinal = rentaNeta - limites[limites.Length - 1];
                impuestoAnual += tramoFinal * tasas[tasas.Length - 1];
            }

            // RN-10: Retención mensual
            return impuestoAnual / 12m;
        }

        public static bool ValidarPeriodoAnteriorProcesado(
            List<PeriodoNomina> periodos,
            string periodoActual)
        {
            // RN-04: No se puede procesar si el mes anterior no está procesado
            var periodo = periodos.FirstOrDefault(p => p.PeriodoCodigo == periodoActual);
            if (periodo == null) return false;

            var periodoAnterior = periodos
                .Where(p => p.PeriodoFin < periodo.PeriodoInicio)
                .OrderByDescending(p => p.PeriodoFin)
                .FirstOrDefault();

            // Si no hay periodo anterior, está OK
            if (periodoAnterior == null) return true;

            // El periodo anterior debe estar Cerrado (C)
            return periodoAnterior.PeriodoEstado == "C";
        }
    }
}
