using Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio.Reglas
{
    public static class CalculoNominaRules
    {
        // --- Constantes de Reglas de Negocio (Parametrización RN-18) ---
        private const decimal UIT_2025 = 5200m;
        private const decimal TASA_ONP = 0.13m; // RN-06 
        private const decimal TASA_AFP_APORTE = 0.10m; // RN-08 
        private const decimal TASA_ESSALUD = 0.09m; // RN-07 
        private const decimal ASIGNACION_FAMILIAR_MONTO = 113.00m; // RN-16  (10% de 1130)
        private const decimal GRATIFICACION_ADICIONAL = 0.09m; // RN-09 

        // REQUERIDO: Definir monto por horas extras
        private const decimal MONTO_POR_HORA_EXTRA = 15m; // RN-19  (Monto definido por RRHH)


        // El DTO de resultado no cambia
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

            // 1. CÁLCULO DE INGRESOS
            resultado.SueldoBase = contrato.ContratoSueldo;
            resultado.AsignacionFamiliar = CalcularAsignacionFamiliar(contrato.ContratoAsignacionFamiliar); // RN-16 
            resultado.HorasExtras = CalcularMontoHorasExtras(horasExtras); // RN-19 

            // 2. CÁLCULO DE SUELDO BRUTO (Base para ONP/AFP)
            resultado.SueldoBruto = CalcularSueldoBruto(resultado.SueldoBase, resultado.AsignacionFamiliar, resultado.HorasExtras); // RN-16 

            // 3. CÁLCULO DE BENEFICIOS (Gratificación y CTS)
            resultado.Gratificacion = CalcularGratificacion(mes, resultado.SueldoBase); // RN-09 
            resultado.CTS = CalcularCTS(mes, resultado.SueldoBase, resultado.AsignacionFamiliar, mesesTrabajados); // RN-15 

            // 4. CÁLCULO DE TOTAL INGRESOS (Base para ESSALUD)
            resultado.TotalIngresos = CalcularTotalIngresos(resultado.SueldoBruto, resultado.Gratificacion, resultado.CTS); // RN-20 

            // 5. CÁLCULO DE DESCUENTOS
            var descuentosPension = CalcularDescuentoPension(contrato.ContratoRegimenPensionario, resultado.SueldoBruto); // RN-06, RN-08 
            resultado.DescuentoONP = descuentosPension.onp;
            resultado.DescuentoAFP = descuentosPension.afp;

            resultado.RetencionRenta5ta = CalcularRenta5taCategoria(resultado.SueldoBase, resultado.AsignacionFamiliar); // RN-10 a RN-13 [cite: 447, 452]

            resultado.TotalDescuentos = CalcularTotalDescuentos(resultado.DescuentoONP, resultado.DescuentoAFP, resultado.RetencionRenta5ta); // RN-14 

            // 6. CÁLCULO DE APORTE EMPLEADOR
            resultado.AporteESSALUD = CalcularAporteESSALUD(resultado.TotalIngresos); // RN-07 

            // 7. CÁLCULO NETO
            resultado.SueldoNeto = CalcularSueldoNeto(resultado.TotalIngresos, resultado.TotalDescuentos); // RN-17 

            return resultado;
        }

        // ==================================================================
        //         MÉTODOS PRIVADOS DE CÁLCULO (Refactorizados)
        // ==================================================================

        // --- MÉTODOS DE INGRESOS ---

        
        private static decimal CalcularAsignacionFamiliar(string aplicaAsignacion) // RN-16 
        {
            return (aplicaAsignacion == "S") ? ASIGNACION_FAMILIAR_MONTO : 0;
        }

        
        private static decimal CalcularMontoHorasExtras(int horasExtras) // RN-19 
        {
            // Tu lógica define S/ 15 por hora extra (RN-19 dice que es un monto, lo parametrizamos)
            return horasExtras * MONTO_POR_HORA_EXTRA;
        }

        
        private static decimal CalcularSueldoBruto(decimal sueldoBase, decimal asignacion, decimal horasExtras) // RN-16 
        {
            // SueldoBruto = SueldoBase + Asignaciones(solo AF) + HorasExtras
            return sueldoBase + asignacion + horasExtras;
        }

        
        private static decimal CalcularGratificacion(int mes, decimal sueldoBase) // RN-09 
        {
            // REQUERIDO: Validar meses correspondientes
            if (mes == 7 || mes == 12)
            {
                // TotalGratificación = SueldoBase + (SueldoBase × 9%)
                return sueldoBase + (sueldoBase * GRATIFICACION_ADICIONAL);
            }
            return 0;
        }

        
        private static decimal CalcularCTS(int mes, decimal sueldoBase, decimal asignacionFamiliar, int mesesTrabajados) // RN-15 
        {
            // REQUERIDO: Validar meses correspondientes
            if (mes == 5 || mes == 11)
            {
                // CTS = (SueldoBase + Asignación familiar) / 12 * MesesTrabajados
                return ((sueldoBase + asignacionFamiliar) / 12m) * mesesTrabajados;
            }
            return 0;
        }

        
        private static decimal CalcularTotalIngresos(decimal sueldoBruto, decimal gratificacion, decimal cts) // RN-20 
        {
            // Total Ingresos = SueldoBruto + Asignaciones (Grati y CTS)
            return sueldoBruto + gratificacion + cts;
        }

        // --- MÉTODOS DE DESCUENTOS Y APORTES ---

        private static (decimal onp, decimal afp) CalcularDescuentoPension(string regimen, decimal sueldoBruto)
        {
            if (regimen == "ONP")
            {
                // RN-06: ONP = 13% 
                return (onp: sueldoBruto * TASA_ONP, afp: 0);
            }
            else
            {
                // RN-08: AFP = 10% + Prima + Comisión 
                var (prima, comision) = ObtenerTasasAFP(regimen);
                var descuentoAFP = sueldoBruto * (TASA_AFP_APORTE + prima + comision);
                return (onp: 0, afp: descuentoAFP);
            }
        }

        
        private static decimal CalcularTotalDescuentos(decimal onp, decimal afp, decimal renta5ta) // RN-14 
        {
            return onp + afp + renta5ta;
        }

        
        private static decimal CalcularAporteESSALUD(decimal totalIngresos) // RN-07 
        {
            // $AporteSalud = TotalIngresos × 9%
            return totalIngresos * TASA_ESSALUD;
        }

        
        private static decimal CalcularSueldoNeto(decimal totalIngresos, decimal totalDescuentos) // RN-17 
        {
            return totalIngresos - totalDescuentos;
        }


        // --- MÉTODOS AYUDANTES

        
        private static (decimal prima, decimal comision) ObtenerTasasAFP(string regimenPensionario) // RN-08 
        {
            return regimenPensionario?.ToUpper() switch
            {
                "AFP INTEGRA" => (0.0135m, 0.0147m),
                "AFP PROFUTURO" => (0.0135m, 0.0125m),
                "AFP HABITAT" => (0.0135m, 0.0145m),
                "AFP PRIMA" => (0.0135m, 0.0160m),
                _ => (0.0135m, 0.0147m) // Por defecto Integra
            };
        }

        
        private static decimal CalcularRenta5taCategoria(decimal sueldoBase, decimal asignacionFamiliar) // RN-10 a 13 
        {
   

            // RN-11: Proyección anual 
            decimal gratificaciones = (sueldoBase * 2) + (sueldoBase * 2 * GRATIFICACION_ADICIONAL);
            decimal ingresosAnuales = (sueldoBase * 12) + gratificaciones + (asignacionFamiliar * 12);

            // RN-12: Deducción de 7 UIT 
            decimal rentaNeta = ingresosAnuales - (7 * UIT_2025);
            if (rentaNeta <= 0) return 0;

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

   
        public static bool ValidarPeriodoAnteriorProcesado(List<PeriodoNomina> periodos, string periodoActual)
        {
            // RN-04: No se puede procesar si el mes anterior no ha sido procesado 
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