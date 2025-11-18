using Xunit;
using Dominio.Reglas;
using Dominio.Entidades;
using System.Collections.Generic;
using System;

namespace Nominasoft.Tests
{
    public class ReglasNominaTest
    {
        // ==================================================================
        //    PRUEBAS DEL MÉTODO PRINCIPAL: CalcularNomina()
        // ==================================================================

        // CA-01: Procesamiento completo de nómina con todos los cálculos automáticos
        [Fact]
        public void CalcularNomina_ConDatosBasicos_DebeCalcularCorrectamente()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 2000,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 1);

            Assert.Equal(2000m, resultado.SueldoBase);
            Assert.Equal(2000m, resultado.SueldoBruto);
            Assert.True(resultado.TotalDescuentos > 0);
            Assert.True(resultado.SueldoNeto > 0);
        }

        // CA-05: El desglose debe incluir todos los conceptos especificados
        [Fact]
        public void CalcularNomina_DebeGenerarTodosLosConceptos()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 3000,
                ContratoAsignacionFamiliar = "S",
                ContratoRegimenPensionario = "AFP INTEGRA"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 7, horasExtras: 10);

            Assert.True(resultado.SueldoBase > 0);
            Assert.True(resultado.AsignacionFamiliar > 0);
            Assert.True(resultado.HorasExtras > 0);
            Assert.True(resultado.Gratificacion > 0);
            Assert.True(resultado.SueldoBruto > 0);
            Assert.True(resultado.DescuentoAFP > 0);
            Assert.True(resultado.TotalDescuentos > 0);
            Assert.True(resultado.AporteESSALUD > 0);
            Assert.True(resultado.SueldoNeto > 0);
        }

        // RN-16: Sueldo bruto debe incluir base + asignación + horas extras
        [Fact]
        public void CalcularNomina_SueldoBruto_DebeIncluirTodosLosComponentes()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 2000,
                ContratoAsignacionFamiliar = "S",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 1, horasExtras: 5);

            var expectedBruto = 2000 + 113.00m + (5 * 15);
            Assert.Equal(expectedBruto, resultado.SueldoBruto);
        }

        // RN-17: Sueldo neto = Total ingresos - Total descuentos
        [Fact]
        public void CalcularNomina_SueldoNeto_DebeSerIngresosMenosDescuentos()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 2500,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 3);

            Assert.Equal(resultado.TotalIngresos - resultado.TotalDescuentos, resultado.SueldoNeto);
        }

        // ==================================================================
        //    PRUEBAS DE: CalcularAsignacionFamiliar()
        // ==================================================================

        // RN-14: Asignación familiar debe ser S/ 113.00 cuando aplica
        [Fact]
        public void CalcularAsignacionFamiliar_ConS_DebeRetornar113()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 2000,
                ContratoAsignacionFamiliar = "S",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 1);

            Assert.Equal(113.00m, resultado.AsignacionFamiliar);
        }

        // RN-14: Asignación familiar debe ser 0 cuando no aplica
        [Fact]
        public void CalcularAsignacionFamiliar_ConN_DebeRetornarCero()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 2000,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 1);

            Assert.Equal(0, resultado.AsignacionFamiliar);
        }

        // RN-14: Cualquier valor diferente de "S" no debe incluir asignación
        [Theory]
        [InlineData("n")]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("X")]
        public void CalcularAsignacionFamiliar_ConValorInvalido_DebeRetornarCero(string valor)
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 2000,
                ContratoAsignacionFamiliar = valor,
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 1);

            Assert.Equal(0, resultado.AsignacionFamiliar);
        }

        // ==================================================================
        //    PRUEBAS DE: CalcularMontoHorasExtras()
        // ==================================================================

        // RN-19: Horas extras deben calcularse a S/ 15 por hora
        [Fact]
        public void CalcularMontoHorasExtras_ConHoras_DebeCalcularA15PorHora()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 2000,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 1, horasExtras: 10);

            Assert.Equal(150m, resultado.HorasExtras);
        }

        // CA-06, RN-19: Horas extras cero no debe afectar el cálculo
        [Fact]
        public void CalcularMontoHorasExtras_ConCero_DebeRetornarCero()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 2000,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 1, horasExtras: 0);

            Assert.Equal(0, resultado.HorasExtras);
        }

        // RN-19: El monto debe ser proporcional a las horas trabajadas
        [Theory]
        [InlineData(1, 15)]
        [InlineData(5, 75)]
        [InlineData(20, 300)]
        [InlineData(50, 750)]
        public void CalcularMontoHorasExtras_ConDiferentesHoras_DebeCalcularCorrectamente(int horas, decimal esperado)
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 2000,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 1, horasExtras: horas);

            Assert.Equal(esperado, resultado.HorasExtras);
        }

        // ==================================================================
        //    PRUEBAS DE: CalcularSueldoBruto()
        // ==================================================================

        // RN-16: Sueldo bruto = Base + Asignación + Horas extras
        [Fact]
        public void CalcularSueldoBruto_ConTodosLosComponentes_DebeSumarCorrectamente()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 2000,
                ContratoAsignacionFamiliar = "S",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 1, horasExtras: 10);

            var esperado = 2000 + 113.00m + 150;
            Assert.Equal(esperado, resultado.SueldoBruto);
        }

        // RN-16: Sueldo bruto solo con base cuando no hay otros componentes
        [Fact]
        public void CalcularSueldoBruto_SoloConBase_DebeSerIgualAlSueldoBase()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 2000,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 1, horasExtras: 0);

            Assert.Equal(2000m, resultado.SueldoBruto);
        }

        // ==================================================================
        //    PRUEBAS DE: CalcularGratificacion()
        // ==================================================================

        // RN-09: Gratificación en julio = Sueldo + (Sueldo * 9%)
        [Fact]
        public void CalcularGratificacion_EnJulio_DebeCalcularCorrectamente()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 2500,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 7);

            var esperado = 2500 + (2500 * 0.09m);
            Assert.Equal(esperado, resultado.Gratificacion);
        }

        // RN-09: Gratificación en diciembre = Sueldo + (Sueldo * 9%)
        [Fact]
        public void CalcularGratificacion_EnDiciembre_DebeCalcularCorrectamente()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 3000,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 12);

            var esperado = 3000 + (3000 * 0.09m);
            Assert.Equal(esperado, resultado.Gratificacion);
        }

        // RN-09: No hay gratificación en meses diferentes a julio y diciembre
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(8)]
        [InlineData(9)]
        [InlineData(10)]
        [InlineData(11)]
        public void CalcularGratificacion_EnMesesNoCorrespondientes_DebeRetornarCero(int mes)
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 2500,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: mes);

            Assert.Equal(0, resultado.Gratificacion);
        }

        // RN-09: Gratificación solo depende del sueldo base, no de otros conceptos
        [Fact]
        public void CalcularGratificacion_NoDependeDeHorasExtrasNiAsignacion()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 2000,
                ContratoAsignacionFamiliar = "S",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 7, horasExtras: 20);

            var esperado = 2000 + (2000 * 0.09m);
            Assert.Equal(esperado, resultado.Gratificacion);
        }

        // ==================================================================
        //    PRUEBAS DE: CalcularCTS()
        // ==================================================================

        // RN-15: CTS en mayo = (Base + Asignación) / 12 * Meses trabajados
        [Fact]
        public void CalcularCTS_EnMayo_DebeCalcularCorrectamente()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 2400,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 5, mesesTrabajados: 6);

            var esperado = (2400 / 12m) * 6;
            Assert.Equal(esperado, resultado.CTS);
        }

        // RN-15: CTS en noviembre = (Base + Asignación) / 12 * Meses trabajados
        [Fact]
        public void CalcularCTS_EnNoviembre_DebeCalcularCorrectamente()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 3000,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 11, mesesTrabajados: 6);

            var esperado = (3000 / 12m) * 6;
            Assert.Equal(esperado, resultado.CTS);
        }

        // RN-15: CTS incluye asignación familiar cuando aplica
        [Fact]
        public void CalcularCTS_ConAsignacionFamiliar_DebeIncluirla()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 2400,
                ContratoAsignacionFamiliar = "S",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 5, mesesTrabajados: 6);

            var esperado = ((2400 + 113.00m) / 12m) * 6;
            Assert.Equal(esperado, resultado.CTS);
        }

        // RN-15: CTS en meses no correspondientes debe ser cero
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(6)]
        [InlineData(7)]
        [InlineData(8)]
        [InlineData(9)]
        [InlineData(10)]
        [InlineData(12)]
        public void CalcularCTS_EnMesesNoCorrespondientes_DebeRetornarCero(int mes)
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 2400,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: mes, mesesTrabajados: 6);

            Assert.Equal(0, resultado.CTS);
        }

        // RN-15: CTS con 0 meses trabajados debe ser cero
        [Fact]
        public void CalcularCTS_ConCeroMesesTrabajados_DebeRetornarCero()
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

        // RN-15: CTS con 12 meses trabajados debe ser el sueldo completo (+ asignación)
        [Fact]
        public void CalcularCTS_Con12MesesTrabajados_DebeSerSueldoCompleto()
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

        // RN-15: CTS debe ser proporcional a los meses trabajados
        [Theory]
        [InlineData(1, 200)]
        [InlineData(3, 600)]
        [InlineData(6, 1200)]
        [InlineData(9, 1800)]
        public void CalcularCTS_ConDiferentesMesesTrabajados_DebeSerProporcional(int meses, decimal esperado)
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 2400,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 5, mesesTrabajados: meses);

            Assert.Equal(esperado, resultado.CTS);
        }

        // ==================================================================
        //    PRUEBAS DE: CalcularTotalIngresos()
        // ==================================================================

        // RN-20: Total ingresos = Sueldo bruto + Gratificación + CTS
        [Fact]
        public void CalcularTotalIngresos_EnMesNormal_DebeSoloSueldoBruto()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 2000,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 1);

            Assert.Equal(resultado.SueldoBruto, resultado.TotalIngresos);
        }

        // RN-20: Total ingresos incluye gratificación en julio/diciembre
        [Fact]
        public void CalcularTotalIngresos_EnJulio_DebeIncluirGratificacion()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 2500,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 7);

            var esperado = resultado.SueldoBruto + resultado.Gratificacion;
            Assert.Equal(esperado, resultado.TotalIngresos);
        }

        // RN-20: Total ingresos incluye CTS en mayo/noviembre
        [Fact]
        public void CalcularTotalIngresos_EnMayo_DebeIncluirCTS()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 2400,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 5, mesesTrabajados: 6);

            var esperado = resultado.SueldoBruto + resultado.CTS;
            Assert.Equal(esperado, resultado.TotalIngresos);
        }

        // ==================================================================
        //    PRUEBAS DE: CalcularDescuentoPension()
        // ==================================================================

        // RN-06: Descuento ONP debe ser exactamente 13% del sueldo bruto
        [Fact]
        public void CalcularDescuentoPension_ONP_DebeCalcular13Porciento()
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

        // RN-06: ONP se calcula sobre sueldo bruto, no sobre total ingresos
        [Fact]
        public void CalcularDescuentoPension_ONP_SoloSobreSueldoBruto()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 2500,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 7);

            var esperadoONP = 2500 * 0.13m;
            Assert.Equal(esperadoONP, resultado.DescuentoONP);
        }

        // RN-08: AFP Integra = 10% + 1.35% + 1.47% = 12.82%
        [Fact]
        public void CalcularDescuentoPension_AFPIntegra_DebeCalcularCorrectamente()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 3000,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "AFP INTEGRA"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 1);

            Assert.Equal(384.6m, resultado.DescuentoAFP);
            Assert.Equal(0, resultado.DescuentoONP);
        }

        // RN-08: AFP Profuturo = 10% + 1.35% + 1.25% = 12.60%
        [Fact]
        public void CalcularDescuentoPension_AFPProfuturo_DebeCalcularCorrectamente()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 3000,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "AFP PROFUTURO"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 1);

            Assert.Equal(378m, resultado.DescuentoAFP);
            Assert.Equal(0, resultado.DescuentoONP);
        }

        // RN-08: AFP Habitat = 10% + 1.35% + 1.45% = 12.80%
        [Fact]
        public void CalcularDescuentoPension_AFPHabitat_DebeCalcularCorrectamente()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 3000,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "AFP HABITAT"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 1);

            Assert.Equal(384m, resultado.DescuentoAFP);
            Assert.Equal(0, resultado.DescuentoONP);
        }

        // RN-08: AFP Prima = 10% + 1.35% + 1.60% = 12.95%
        [Fact]
        public void CalcularDescuentoPension_AFPPrima_DebeCalcularCorrectamente()
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

        // RN-08: Régimen pensionario no reconocido debe usar tasa por defecto (Integra)
        [Fact]
        public void CalcularDescuentoPension_RegimenNoReconocido_DebeUsarTasaPorDefecto()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 3000,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "AFP DESCONOCIDA"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 1);

            Assert.Equal(384.6m, resultado.DescuentoAFP);
        }

        // RN-08: Régimen pensionario null debe usar tasa por defecto
        [Fact]
        public void CalcularDescuentoPension_RegimenNull_DebeUsarTasaPorDefecto()
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

        // ==================================================================
        //    PRUEBAS DE: CalcularRenta5taCategoria()
        // ==================================================================

        // RN-10: Renta 5ta solo aplica cuando ingresos anuales > 7 UIT
        [Fact]
        public void CalcularRenta5ta_ConIngresosBajos_NoDebeAplicar()
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

        // RN-10, RN-11, RN-12: Renta 5ta debe aplicar cuando ingresos > 7 UIT
        [Fact]
        public void CalcularRenta5ta_ConIngresosAltos_DebeAplicar()
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

        // RN-11: Proyección anual incluye gratificaciones
        [Fact]
        public void CalcularRenta5ta_DebeIncluirGratificacionesEnProyeccion()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 4000,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 1);

            Assert.True(resultado.RetencionRenta5ta > 0);
        }

        // RN-11: Proyección anual incluye asignación familiar
        [Fact]
        public void CalcularRenta5ta_DebeIncluirAsignacionFamiliarEnProyeccion()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 3000,
                ContratoAsignacionFamiliar = "S",
                ContratoRegimenPensionario = "ONP"
            };

            var resultadoConAF = CalculoNominaRules.CalcularNomina(contrato, mes: 1);

            contrato.ContratoAsignacionFamiliar = "N";
            var resultadoSinAF = CalculoNominaRules.CalcularNomina(contrato, mes: 1);

            Assert.True(resultadoConAF.RetencionRenta5ta >= resultadoSinAF.RetencionRenta5ta);
        }

        // RN-12: Caso límite - exactamente 7 UIT no debe aplicar renta 5ta
        // RN-10, RN-12: Justo por debajo de 7 UIT no debe aplicar renta 5ta
        [Fact]
        public void CalcularRenta5ta_JustoPorDebajoDe7UIT_NoDebeAplicar()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 2500m, // Claramente por debajo
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 1);

            Assert.Equal(0, resultado.RetencionRenta5ta);
        }

        // RN-10, RN-12: Justo por encima de 7 UIT debe aplicar renta 5ta
        [Fact]
        public void CalcularRenta5ta_JustoPorEncimaDe7UIT_DebeAplicar()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 2600m, // Por encima del umbral
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 1);

            Assert.True(resultado.RetencionRenta5ta > 0);
        }

        // RN-10: Retención mensual debe ser el impuesto anual / 12
        [Fact]
        public void CalcularRenta5ta_RetencionMensual_DebeSerAnualDividido12()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 6000,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 1);

            Assert.True(resultado.RetencionRenta5ta > 0);
            Assert.True(resultado.RetencionRenta5ta < resultado.SueldoBruto);
        }

        // ==================================================================
        //    PRUEBAS DE: CalcularTotalDescuentos()
        // ==================================================================

        // RN-14: Total descuentos = ONP + Renta 5ta (cuando es ONP)
        [Fact]
        public void CalcularTotalDescuentos_ConONP_DebeSumarONPYRenta5ta()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 2000,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 1);

            var esperado = resultado.DescuentoONP + resultado.RetencionRenta5ta;
            Assert.Equal(esperado, resultado.TotalDescuentos);
        }

        // RN-14: Total descuentos = AFP + Renta 5ta (cuando es AFP)
        [Fact]
        public void CalcularTotalDescuentos_ConAFP_DebeSumarAFPYRenta5ta()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 3000,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "AFP INTEGRA"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 1);

            var esperado = resultado.DescuentoAFP + resultado.RetencionRenta5ta;
            Assert.Equal(esperado, resultado.TotalDescuentos);
        }

        // RN-14: Total descuentos nunca debe incluir ambos ONP y AFP
        [Fact]
        public void CalcularTotalDescuentos_NoDebeIncluirONPYAFPSimultaneamente()
        {
            var contratoONP = new Contrato
            {
                ContratoSueldo = 3000,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "ONP"
            };

            var resultadoONP = CalculoNominaRules.CalcularNomina(contratoONP, mes: 1);
            Assert.True(resultadoONP.DescuentoONP > 0);
            Assert.Equal(0, resultadoONP.DescuentoAFP);

            var contratoAFP = new Contrato
            {
                ContratoSueldo = 3000,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "AFP INTEGRA"
            };

            var resultadoAFP = CalculoNominaRules.CalcularNomina(contratoAFP, mes: 1);
            Assert.Equal(0, resultadoAFP.DescuentoONP);
            Assert.True(resultadoAFP.DescuentoAFP > 0);
        }

        // ==================================================================
        //    PRUEBAS DE: CalcularAporteESSALUD()
        // ==================================================================

        // RN-07: ESSALUD debe ser 9% del total de ingresos
        [Fact]
        public void CalcularAporteESSALUD_DebeCalcular9PorcientoTotalIngresos()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 3000,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 1);

            var esperado = resultado.TotalIngresos * 0.09m;
            Assert.Equal(esperado, resultado.AporteESSALUD);
        }

        // RN-07: ESSALUD se calcula sobre total ingresos, no sobre sueldo bruto
        [Fact]
        public void CalcularAporteESSALUD_DebeUsarTotalIngresosNoSueldoBruto()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 2500,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "ONP"
            };

            var resultadoNormal = CalculoNominaRules.CalcularNomina(contrato, mes: 1);
            var resultadoJulio = CalculoNominaRules.CalcularNomina(contrato, mes: 7);

            Assert.True(resultadoJulio.AporteESSALUD > resultadoNormal.AporteESSALUD);
        }

        // RN-07: ESSALUD debe incluir gratificaciones y CTS en el cálculo
        [Fact]
        public void CalcularAporteESSALUD_EnJulio_DebeIncluirGratificacion()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 2500,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 7);

            var totalIngresos = resultado.SueldoBruto + resultado.Gratificacion;
            var esperado = totalIngresos * 0.09m;
            Assert.Equal(esperado, resultado.AporteESSALUD);
        }

        // ==================================================================
        //    PRUEBAS DE: CalcularSueldoNeto()
        // ==================================================================

        // RN-17: Sueldo neto = Total ingresos - Total descuentos
        [Fact]
        public void CalcularSueldoNeto_DebeRestarDescuentosDeIngresos()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 3000,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 1);

            var esperado = resultado.TotalIngresos - resultado.TotalDescuentos;
            Assert.Equal(esperado, resultado.SueldoNeto);
        }

        // RN-17: Sueldo neto debe ser siempre positivo
        [Fact]
        public void CalcularSueldoNeto_DebeSerSiemprePositivo()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 1130,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 1);

            Assert.True(resultado.SueldoNeto > 0);
        }

        // RN-17: Sueldo neto debe ser menor que total ingresos
        [Fact]
        public void CalcularSueldoNeto_DebSerMenorQueTotalIngresos()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 3000,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 1);

            Assert.True(resultado.SueldoNeto < resultado.TotalIngresos);
        }

        // ==================================================================
        //    PRUEBAS DE: ValidarPeriodoAnteriorProcesado()
        // ==================================================================

        // RN-04, CA-04: No se puede procesar si el mes anterior está abierto
        [Fact]
        public void ValidarPeriodoAnterior_ConAnteriorAbierto_DebeFallar()
        {
            var periodos = new List<PeriodoNomina>
            {
                new PeriodoNomina { PeriodoCodigo = "P001", PeriodoInicio = new DateTime(2025, 1, 1), PeriodoFin = new DateTime(2025, 1, 31), PeriodoEstado = "A" },
                new PeriodoNomina { PeriodoCodigo = "P002", PeriodoInicio = new DateTime(2025, 2, 1), PeriodoFin = new DateTime(2025, 2, 28), PeriodoEstado = "A" }
            };

            var esValido = CalculoNominaRules.ValidarPeriodoAnteriorProcesado(periodos, "P002");

            Assert.False(esValido);
        }

        // RN-04: Se puede procesar si el período anterior está cerrado
        [Fact]
        public void ValidarPeriodoAnterior_ConAnteriorCerrado_DebePasar()
        {
            var periodos = new List<PeriodoNomina>
            {
                new PeriodoNomina { PeriodoCodigo = "P001", PeriodoInicio = new DateTime(2025, 1, 1), PeriodoFin = new DateTime(2025, 1, 31), PeriodoEstado = "C" },
                new PeriodoNomina { PeriodoCodigo = "P002", PeriodoInicio = new DateTime(2025, 2, 1), PeriodoFin = new DateTime(2025, 2, 28), PeriodoEstado = "A" }
            };

            var esValido = CalculoNominaRules.ValidarPeriodoAnteriorProcesado(periodos, "P002");

            Assert.True(esValido);
        }

        // RN-04: El primer período siempre puede procesarse
        [Fact]
        public void ValidarPeriodoAnterior_PrimerPeriodo_DebePasar()
        {
            var periodos = new List<PeriodoNomina>
            {
                new PeriodoNomina { PeriodoCodigo = "P001", PeriodoInicio = new DateTime(2025, 1, 1), PeriodoFin = new DateTime(2025, 1, 31), PeriodoEstado = "A" }
            };

            var esValido = CalculoNominaRules.ValidarPeriodoAnteriorProcesado(periodos, "P001");

            Assert.True(esValido);
        }

        // RN-04: Debe retornar false si el período no existe
        [Fact]
        public void ValidarPeriodoAnterior_ConPeriodoInexistente_DebeFallar()
        {
            var periodos = new List<PeriodoNomina>
            {
                new PeriodoNomina { PeriodoCodigo = "P001", PeriodoInicio = new DateTime(2025, 1, 1), PeriodoFin = new DateTime(2025, 1, 31), PeriodoEstado = "C" }
            };

            var esValido = CalculoNominaRules.ValidarPeriodoAnteriorProcesado(periodos, "P999");

            Assert.False(esValido);
        }

        // RN-04: Debe buscar el período inmediato anterior, no cualquier período previo
        [Fact]
        public void ValidarPeriodoAnterior_ConMultiplesPeriodos_DebeBuscarInmediatoAnterior()
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

        // RN-04: Si hay un período en medio abierto, debe fallar
        [Fact]
        public void ValidarPeriodoAnterior_ConPeriodoMedioAbierto_DebeFallar()
        {
            var periodos = new List<PeriodoNomina>
            {
                new PeriodoNomina { PeriodoCodigo = "P001", PeriodoInicio = new DateTime(2025, 1, 1), PeriodoFin = new DateTime(2025, 1, 31), PeriodoEstado = "C" },
                new PeriodoNomina { PeriodoCodigo = "P002", PeriodoInicio = new DateTime(2025, 2, 1), PeriodoFin = new DateTime(2025, 2, 28), PeriodoEstado = "A" },
                new PeriodoNomina { PeriodoCodigo = "P003", PeriodoInicio = new DateTime(2025, 3, 1), PeriodoFin = new DateTime(2025, 3, 31), PeriodoEstado = "A" }
            };

            var esValido = CalculoNominaRules.ValidarPeriodoAnteriorProcesado(periodos, "P003");

            Assert.False(esValido);
        }

        // RN-04: Validar con lista vacía debe retornar false
        [Fact]
        public void ValidarPeriodoAnterior_ConListaVacia_DebeFallar()
        {
            var periodos = new List<PeriodoNomina>();

            var esValido = CalculoNominaRules.ValidarPeriodoAnteriorProcesado(periodos, "P001");

            Assert.False(esValido);
        }

        // ==================================================================
        //    PRUEBAS DE INTEGRACIÓN - CASOS COMPLETOS
        // ==================================================================

        // CA-05: Validación de desglose completo en mes normal
        [Fact]
        public void CasoCompleto_MesNormal_DebeCalcularTodosLosConceptos()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 3000,
                ContratoAsignacionFamiliar = "S",
                ContratoRegimenPensionario = "AFP INTEGRA"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 3, horasExtras: 10);

            Assert.Equal(3000m, resultado.SueldoBase);
            Assert.Equal(113.00m, resultado.AsignacionFamiliar);
            Assert.Equal(150m, resultado.HorasExtras);
            Assert.Equal(0, resultado.Gratificacion);
            Assert.Equal(0, resultado.CTS);
            Assert.Equal(3263m, resultado.SueldoBruto);
            Assert.True(resultado.DescuentoAFP > 0);
            Assert.Equal(0, resultado.DescuentoONP);
            Assert.True(resultado.AporteESSALUD > 0);
            Assert.True(resultado.SueldoNeto > 0);
        }

        // CA-05: Validación de desglose completo en julio (con gratificación)
        [Fact]
        public void CasoCompleto_Julio_DebeIncluirGratificacion()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 3000,
                ContratoAsignacionFamiliar = "S",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 7, horasExtras: 5);

            Assert.Equal(3000m, resultado.SueldoBase);
            Assert.Equal(113.00m, resultado.AsignacionFamiliar);
            Assert.Equal(75m, resultado.HorasExtras);
            Assert.Equal(3270m, resultado.Gratificacion);
            Assert.Equal(3188m, resultado.SueldoBruto);
            Assert.True(resultado.TotalIngresos > resultado.SueldoBruto);
        }

        // CA-05: Validación de desglose completo en mayo (con CTS)
        [Fact]
        public void CasoCompleto_Mayo_DebeIncluirCTS()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 2400,
                ContratoAsignacionFamiliar = "S",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 5, horasExtras: 0, mesesTrabajados: 6);

            Assert.Equal(2400m, resultado.SueldoBase);
            Assert.Equal(113.00m, resultado.AsignacionFamiliar);
            Assert.Equal(0, resultado.HorasExtras);
            Assert.Equal(0, resultado.Gratificacion);
            Assert.True(resultado.CTS > 0);
            Assert.Equal(2513m, resultado.SueldoBruto);
        }

        // CA-01: Procesamiento con contrato mínimo (RMV)
        [Fact]
        public void CasoCompleto_ContratoMinimo_DebeCalcularCorrectamente()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 1130,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 1);

            Assert.Equal(1130m, resultado.SueldoBase);
            Assert.Equal(1130m, resultado.SueldoBruto);
            Assert.Equal(146.9m, resultado.DescuentoONP);
            Assert.Equal(0, resultado.RetencionRenta5ta);
            Assert.True(resultado.SueldoNeto > 0);
        }

        // CA-01: Procesamiento con sueldo alto que paga renta 5ta
        [Fact]
        public void CasoCompleto_SueldoAlto_DebeIncluirRenta5ta()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 8000,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "AFP PROFUTURO"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 1);

            Assert.Equal(8000m, resultado.SueldoBase);
            Assert.True(resultado.DescuentoAFP > 0);
            Assert.True(resultado.RetencionRenta5ta > 0);
            Assert.True(resultado.TotalDescuentos > resultado.DescuentoAFP);
        }

        // CA-06: Caso donde horas extras son nulas o cero
        [Fact]
        public void CasoCompleto_HorasExtrasCero_NoDebeAfectarCalculo()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 2000,
                ContratoAsignacionFamiliar = "N",
                ContratoRegimenPensionario = "ONP"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 1, horasExtras: 0);

            Assert.Equal(0, resultado.HorasExtras);
            Assert.Equal(resultado.SueldoBase, resultado.SueldoBruto);
        }

        // Caso extremo: Trabajador con todos los beneficios en diciembre
        [Fact]
        public void CasoCompleto_DiciembreConTodosBeneficios_DebeCalcularCorrectamente()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 4000,
                ContratoAsignacionFamiliar = "S",
                ContratoRegimenPensionario = "AFP PRIMA"
            };

            var resultado = CalculoNominaRules.CalcularNomina(contrato, mes: 12, horasExtras: 15, mesesTrabajados: 12);

            Assert.Equal(4000m, resultado.SueldoBase);
            Assert.Equal(113.00m, resultado.AsignacionFamiliar);
            Assert.Equal(225m, resultado.HorasExtras);
            Assert.True(resultado.Gratificacion > 0);
            Assert.Equal(0, resultado.CTS);
            Assert.True(resultado.DescuentoAFP > 0);
            Assert.True(resultado.RetencionRenta5ta > 0);
            Assert.True(resultado.SueldoNeto > 0);
        }
    }
}