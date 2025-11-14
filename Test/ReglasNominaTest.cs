using Xunit;
using Dominio.Reglas;
using Dominio.Entidades;
using System.Collections.Generic;
using System;

namespace Nominasoft.Tests
{
    public class ReglasNominaTest
    {
        // RN-14, RN-16: Asignación familiar debe agregarse al sueldo bruto cuando es "S"
        [Fact]
        public void CalcularNomina_ConAsignacionFamiliarS_DebeSumarMontoCorrecto()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 2000,
                ContratoAsignacionFamiliar = "S",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 1);

            Assert.Equal(102.50m, resultado.AsignacionFamiliar);
            Assert.Equal(2102.50m, resultado.SueldoBruto);
        }

        // RN-06: Descuento ONP debe ser 13% del sueldo bruto
        [Fact]
        public void CalcularNomina_ConRegimenONP_DebeCalcular13PorCiento()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 2000,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 1);

            Assert.Equal(260m, resultado.DescuentoONP);
            Assert.Equal(0, resultado.DescuentoAFP);
        }

        // RN-08: AFP Prima = 10% + 1.35% + 1.60% = 12.95%
        [Fact]
        public void CalcularNomina_ConRegimenAFPPrima_DebeCalcularTasaCorrecta()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 3000,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "AFP PRIMA"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 1);

            Assert.Equal(388.5m, resultado.DescuentoAFP);
            Assert.Equal(0, resultado.DescuentoONP);
        }

        // RN-09, CA-01: Gratificación se paga en julio = Sueldo + (Sueldo * 9%)
        [Fact]
        public void CalcularNomina_EnMesDeJulio_DebeIncluirGratificacion()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 2500,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 7);

            Assert.Equal(2725m, resultado.Gratificacion);
            Assert.Equal(5225m, resultado.TotalIngresos);
        }

        // RN-09, RN-15: En meses normales no hay gratificación ni CTS
        [Fact]
        public void CalcularNomina_EnMesNormal_NoDebeIncluirGratificacionNiCTS()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 2500,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 6);

            Assert.Equal(0, resultado.Gratificacion);
            Assert.Equal(0, resultado.CTS);
            Assert.Equal(resultado.SueldoBruto, resultado.TotalIngresos);
        }

        // RN-10, RN-11, RN-12: Renta 5ta aplica cuando ingresos anuales > 7 UIT
        [Fact]
        public void CalcularNomina_ConSueldoAlto_DebeCalcularRenta5ta()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 5000,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 1);

            Assert.True(resultado.RetencionRenta5ta > 0);
        }

        // RN-10: No aplica renta 5ta cuando ingresos anuales <= 7 UIT
        [Fact]
        public void CalcularNomina_ConSueldoBajo_NoDebeCalcularRenta5ta()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 2000,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 1);

            Assert.Equal(0, resultado.RetencionRenta5ta);
        }

        // RN-04, CA-04: No se puede procesar nómina si el mes anterior no ha sido procesado (estado no es "C")
        [Fact]
        public void ValidarPeriodoAnterior_CuandoAnteriorEstaAbierto_DebeFallar()
        {
            var periodos = new List<PeriodoNomina>
            {
                new PeriodoNomina { PeriodoCodigo = "P001", PeriodoInicio = new DateTime(2025, 1, 1), PeriodoFin = new DateTime(2025, 1, 31), PeriodoEstado = "A" },
                new PeriodoNomina { PeriodoCodigo = "P002", PeriodoInicio = new DateTime(2025, 2, 1), PeriodoFin = new DateTime(2025, 2, 28), PeriodoEstado = "A" }
            };

            var esValido = CalculoNominaRules.ValidarPeriodoAnteriorProcesado(periodos, "P002");

            Assert.False(esValido);
        }

        // RN-04: Se puede procesar nómina si el período anterior está cerrado (estado "C")
        [Fact]
        public void ValidarPeriodoAnterior_CuandoAnteriorEstaCerrado_DebePasar()
        {
            var periodos = new List<PeriodoNomina>
            {
                new PeriodoNomina { PeriodoCodigo = "P001", PeriodoInicio = new DateTime(2025, 1, 1), PeriodoFin = new DateTime(2025, 1, 31), PeriodoEstado = "C" },
                new PeriodoNomina { PeriodoCodigo = "P002", PeriodoInicio = new DateTime(2025, 2, 1), PeriodoFin = new DateTime(2025, 2, 28), PeriodoEstado = "A" }
            };

            var esValido = CalculoNominaRules.ValidarPeriodoAnteriorProcesado(periodos, "P002");

            Assert.True(esValido);
        }

        // RN-04: El primer período siempre puede procesarse (no tiene período anterior)
        [Fact]
        public void ValidarPeriodoAnterior_CuandoEsElPrimerPeriodo_DebePasar()
        {
            var periodos = new List<PeriodoNomina>
            {
                new PeriodoNomina { PeriodoCodigo = "P001", PeriodoInicio = new DateTime(2025, 1, 1), PeriodoFin = new DateTime(2025, 1, 31), PeriodoEstado = "A" }
            };

            var esValido = CalculoNominaRules.ValidarPeriodoAnteriorProcesado(periodos, "P001");

            Assert.True(esValido);
        }

        // RN-14: Cuando asignación familiar es "N", no debe sumarse al sueldo bruto
        [Fact]
        public void CalcularNomina_ConAsignacionFamiliarN_NoDebeSumarMonto()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 2000,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 1);

            Assert.Equal(0, resultado.AsignacionFamiliar);
            Assert.Equal(2000m, resultado.SueldoBruto);
        }

        // RN-08: Cada AFP tiene diferentes tasas (Prima, Comisión)
        [Theory]
        [InlineData("AFP INTEGRA", 350.4)]
        [InlineData("AFP PROFUTURO", 344.4)]
        [InlineData("AFP HABITAT", 348)]
        [InlineData("AFP PRIMA", 388.5)]
        public void CalcularNomina_ConDiferentesAFP_DebeCalcularTasaCorrecta(string afp, decimal descuentoEsperado)
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 3000,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = afp
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 1);

            Assert.Equal(descuentoEsperado, resultado.DescuentoAFP);
            Assert.Equal(0, resultado.DescuentoONP);
        }

        // RN-09: Gratificación también se paga en diciembre
        [Fact]
        public void CalcularNomina_EnMesDeDiciembre_DebeIncluirGratificacion()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 2500,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 12);

            Assert.Equal(2725m, resultado.Gratificacion);
            Assert.Equal(5225m, resultado.TotalIngresos);
        }

        // RN-15: CTS se deposita en mayo y noviembre
        [Theory]
        [InlineData(5)]
        [InlineData(11)]
        public void CalcularNomina_EnMesesDeCTS_DebeIncluirCTS(int mes)
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 2400,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: mes, mesesTrabajados: 6);

            Assert.Equal(1200m, resultado.CTS);
        }

        // RN-19: Horas extras se importan como dato procesado y se suman al sueldo bruto
        [Fact]
        public void CalcularNomina_ConHorasExtras_DebeIncluirEnSueldoBruto()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 2000,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 1, horasExtras: 10);

            Assert.Equal(150m, resultado.HorasExtras);
            Assert.Equal(2150m, resultado.SueldoBruto);
        }

        // CA-06: Cuando horas extras son cero o nulas, no afectan el sueldo bruto
        [Fact]
        public void CalcularNomina_SinHorasExtras_NoDebeAgregarAlSueldoBruto()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 2000,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 1, horasExtras: 0);

            Assert.Equal(0, resultado.HorasExtras);
            Assert.Equal(2000m, resultado.SueldoBruto);
        }

        // RN-07: ESSALUD es 9% del total de ingresos (aporte del empleador, no descuento)
        [Fact]
        public void CalcularNomina_DebeCalcularESSALUD_SobreTotalIngresos()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 3000,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 1);

            Assert.Equal(270m, resultado.AporteESSALUD);
        }

        // RN-17: Sueldo neto = Total ingresos - Total descuentos
        [Fact]
        public void CalcularNomina_DebeCalcularSueldoNeto_RestandoDescuentos()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 2000,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 1);

            Assert.Equal(resultado.TotalIngresos - resultado.TotalDescuentos, resultado.SueldoNeto);
        }

        // RN-14: Total descuentos = ONP + Renta 5ta cuando es ONP
        [Fact]
        public void CalcularNomina_TotalDescuentos_DebeSumarONPYRenta5ta()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 2000,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 1);

            Assert.Equal(resultado.DescuentoONP + resultado.RetencionRenta5ta, resultado.TotalDescuentos);
        }

        // RN-14: Total descuentos = AFP + Renta 5ta cuando es AFP
        [Fact]
        public void CalcularNomina_TotalDescuentos_DebeSumarAFPYRenta5ta()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 3000,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "AFP INTEGRA"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 1);

            Assert.Equal(resultado.DescuentoAFP + resultado.RetencionRenta5ta, resultado.TotalDescuentos);
        }

        // RN-15: CTS incluye asignación familiar si el trabajador la tiene
        [Fact]
        public void CalcularNomina_ConCTSYAsignacionFamiliar_CTSDebeIncluirAsignacion()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 2400,
                ContratoAsignacionFamiliar = "S",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 5, mesesTrabajados: 6);

            var ctsEsperado = ((2400 + 102.50m) / 12) * 6;
            Assert.Equal(ctsEsperado, resultado.CTS);
        }

        // CA-05: Verificar desglose completo de cálculo en mes con gratificación
        [Fact]
        public void CalcularNomina_MesCompleto_JulioConTodosLosConceptos()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 3000,
                ContratoAsignacionFamiliar = "S",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 7, horasExtras: 5);

            Assert.Equal(3000m, resultado.SueldoBase);
            Assert.Equal(102.50m, resultado.AsignacionFamiliar);
            Assert.Equal(75m, resultado.HorasExtras);
            Assert.Equal(3270m, resultado.Gratificacion);
            Assert.Equal(3177.50m, resultado.SueldoBruto);
        }

        // RN-04: Validación debe retornar false si el período no existe
        [Fact]
        public void ValidarPeriodoAnterior_ConPeriodoInexistente_DebeRetornarFalse()
        {
            var periodos = new List<PeriodoNomina>
            {
                new PeriodoNomina { PeriodoCodigo = "P001", PeriodoInicio = new DateTime(2025, 1, 1), PeriodoFin = new DateTime(2025, 1, 31), PeriodoEstado = "C" }
            };

            var esValido = CalculoNominaRules.ValidarPeriodoAnteriorProcesado(periodos, "P999");

            Assert.False(esValido);
        }

        // RN-04: El sistema debe buscar el período anterior inmediato, no cualquier período previo
        [Fact]
        public void ValidarPeriodoAnterior_ConMultiplesPeriodos_DebeBuscarAnteriorInmediato()
        {
            var periodos = new List<PeriodoNomina>
            {
                new PeriodoNomina { PeriodoCodigo = "P001", PeriodoInicio = new DateTime(2025, 1, 1), PeriodoFin = new DateTime(2025, 1, 31), PeriodoEstado = "C" },
                new PeriodoNomina { PeriodoCodigo = "P002", PeriodoInicio = new DateTime(2025, 2, 1), PeriodoFin = new DateTime(2025, 2, 28), PeriodoEstado = "C" },
                new PeriodoNomina { PeriodoCodigo = "P003", PeriodoInicio = new DateTime(2025, 3, 1), PeriodoFin = new DateTime(2025, 3, 31), PeriodoEstado = "A" }
            };

            var esValido = CalculoNominaRules.ValidarPeriodoAnteriorProcesado(periodos, "P003");

            Assert.True(esValido);
        }

        // RN-10: Caso límite - ingreso anual exactamente 7 UIT no paga renta 5ta
        [Fact]
        public void CalcularNomina_ConSueldoExactamente7UIT_NoDebeCalcularRenta5ta()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 2571.43m,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 1);

            Assert.Equal(0, resultado.RetencionRenta5ta);
        }

        // RN-08: Si régimen pensionario es null o no reconocido, usa tasa por defecto
        [Fact]
        public void CalcularNomina_ConRegimenPensionarioNulo_DebeUsarTasaPorDefecto()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 3000,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = null
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 1);

            Assert.True(resultado.DescuentoAFP > 0);
        }

        // RN-09, RN-15: En meses sin gratificación ni CTS, total ingresos = sueldo bruto
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(6)]
        [InlineData(8)]
        [InlineData(9)]
        [InlineData(10)]
        public void CalcularNomina_MesesSinGratificacionNiCTS_SoloSueldoBruto(int mes)
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 2500,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: mes);

            Assert.Equal(0, resultado.Gratificacion);
            Assert.Equal(0, resultado.CTS);
            Assert.Equal(resultado.SueldoBruto, resultado.TotalIngresos);
        }

        // RN-15: CTS con cero meses trabajados debe ser cero
        [Fact]
        public void CalcularNomina_ConMesesTrabajadosCero_CTSDebeSerCero()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 2400,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 5, mesesTrabajados: 0);

            Assert.Equal(0, resultado.CTS);
        }

        // RN-15: CTS con 12 meses trabajados debe ser el sueldo completo
        [Fact]
        public void CalcularNomina_ConMesesTrabajados12_CTSDebeSerCompleto()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 2400,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 5, mesesTrabajados: 12);

            Assert.Equal(2400m, resultado.CTS);
        }
    }
}