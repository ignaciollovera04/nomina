using Dominio.Entidades;
using Dominio.Reglas;
using Dominio.Resultados;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    public class ReglasReporteNominaTest
    {
        // ==================================================================
        //    PRUEBAS DE: CalcularTotales()
        // ==================================================================

        // RN-02: El sistema debe calcular totales acumulados para todos los conceptos
        [Fact]
        public void CalcularTotales_ConUnEmpleado_DebeCalcularCorrectamente()
        {
            var nominas = new List<NominaDetalle>
            {
                new NominaDetalle
                {
                    SueldoBase = 2000m,
                    AsignacionFamiliar = 113m,
                    SalarioBruto = 2113m,
                    Deducciones = 300m,
                    SueldoNeto = 1813m,
                    ESSALUD = 190m
                }
            };

            var totales = ReporteNominaRules.CalcularTotales(nominas);

            Assert.Equal(2000m, totales.TotalSueldoBase);
            Assert.Equal(113m, totales.TotalAsignacionFamiliar);
            Assert.Equal(2113m, totales.TotalSalarioBruto);
            Assert.Equal(300m, totales.TotalDescuentos);
            Assert.Equal(1813m, totales.TotalNetoPagar);
            Assert.Equal(190m, totales.TotalESSALUD);
        }

        // RN-02: Los totales deben acumularse correctamente con múltiples empleados
        [Fact]
        public void CalcularTotales_ConMultiplesEmpleados_DebeAcumularCorrectamente()
        {
            var nominas = new List<NominaDetalle>
            {
                new NominaDetalle
                {
                    SueldoBase = 2000m,
                    AsignacionFamiliar = 113m,
                    SalarioBruto = 2113m,
                    Deducciones = 300m,
                    SueldoNeto = 1813m,
                    ESSALUD = 190m
                },
                new NominaDetalle
                {
                    SueldoBase = 3000m,
                    AsignacionFamiliar = 0m,
                    SalarioBruto = 3000m,
                    Deducciones = 450m,
                    SueldoNeto = 2550m,
                    ESSALUD = 270m
                },
                new NominaDetalle
                {
                    SueldoBase = 1500m,
                    AsignacionFamiliar = 113m,
                    SalarioBruto = 1613m,
                    Deducciones = 225m,
                    SueldoNeto = 1388m,
                    ESSALUD = 145m
                }
            };

            var totales = ReporteNominaRules.CalcularTotales(nominas);

            Assert.Equal(6500m, totales.TotalSueldoBase);
            Assert.Equal(226m, totales.TotalAsignacionFamiliar);
            Assert.Equal(6726m, totales.TotalSalarioBruto);
            Assert.Equal(975m, totales.TotalDescuentos);
            Assert.Equal(5751m, totales.TotalNetoPagar);
            Assert.Equal(605m, totales.TotalESSALUD);
        }

        // RN-02, CA-07: Los totales deben ser consistentes con la suma de valores individuales
        [Fact]
        public void CalcularTotales_DebenSerConsistentesConSumaIndividual()
        {
            var nominas = new List<NominaDetalle>
            {
                new NominaDetalle { SueldoBase = 2500m, AsignacionFamiliar = 113m, SalarioBruto = 2613m, Deducciones = 350m, SueldoNeto = 2263m, ESSALUD = 235m },
                new NominaDetalle { SueldoBase = 3500m, AsignacionFamiliar = 0m, SalarioBruto = 3500m, Deducciones = 500m, SueldoNeto = 3000m, ESSALUD = 315m },
                new NominaDetalle { SueldoBase = 4000m, AsignacionFamiliar = 113m, SalarioBruto = 4113m, Deducciones = 600m, SueldoNeto = 3513m, ESSALUD = 370m }
            };

            var totales = ReporteNominaRules.CalcularTotales(nominas);

            var sumaManualBase = 2500m + 3500m + 4000m;
            var sumaManualAsignacion = 113m + 0m + 113m;
            var sumaManualBruto = 2613m + 3500m + 4113m;
            var sumaManualDescuentos = 350m + 500m + 600m;
            var sumaManualNeto = 2263m + 3000m + 3513m;
            var sumaManualEssalud = 235m + 315m + 370m;

            Assert.Equal(sumaManualBase, totales.TotalSueldoBase);
            Assert.Equal(sumaManualAsignacion, totales.TotalAsignacionFamiliar);
            Assert.Equal(sumaManualBruto, totales.TotalSalarioBruto);
            Assert.Equal(sumaManualDescuentos, totales.TotalDescuentos);
            Assert.Equal(sumaManualNeto, totales.TotalNetoPagar);
            Assert.Equal(sumaManualEssalud, totales.TotalESSALUD);
        }

        // CA-04: Cuando no hay información disponible, debe retornar totales en cero
        [Fact]
        public void CalcularTotales_ConListaVacia_DebeRetornarCeros()
        {
            var nominas = new List<NominaDetalle>();

            var totales = ReporteNominaRules.CalcularTotales(nominas);

            Assert.Equal(0m, totales.TotalSueldoBase);
            Assert.Equal(0m, totales.TotalAsignacionFamiliar);
            Assert.Equal(0m, totales.TotalSalarioBruto);
            Assert.Equal(0m, totales.TotalDescuentos);
            Assert.Equal(0m, totales.TotalNetoPagar);
            Assert.Equal(0m, totales.TotalESSALUD);
        }

        // CA-04: Cuando la lista es null, debe retornar totales en cero
        [Fact]
        public void CalcularTotales_ConListaNull_DebeRetornarCeros()
        {
            List<NominaDetalle> nominas = null;

            var totales = ReporteNominaRules.CalcularTotales(nominas);

            Assert.Equal(0m, totales.TotalSueldoBase);
            Assert.Equal(0m, totales.TotalAsignacionFamiliar);
            Assert.Equal(0m, totales.TotalSalarioBruto);
            Assert.Equal(0m, totales.TotalDescuentos);
            Assert.Equal(0m, totales.TotalNetoPagar);
            Assert.Equal(0m, totales.TotalESSALUD);
        }

        // RN-02: Los totales deben manejar valores decimales con precisión
        [Fact]
        public void CalcularTotales_ConValoresDecimales_DebeMantenerPrecision()
        {
            var nominas = new List<NominaDetalle>
            {
                new NominaDetalle
                {
                    SueldoBase = 2500.50m,
                    AsignacionFamiliar = 113.00m,
                    SalarioBruto = 2613.50m,
                    Deducciones = 350.75m,
                    SueldoNeto = 2262.75m,
                    ESSALUD = 235.22m
                },
                new NominaDetalle
                {
                    SueldoBase = 3000.75m,
                    AsignacionFamiliar = 0m,
                    SalarioBruto = 3000.75m,
                    Deducciones = 425.33m,
                    SueldoNeto = 2575.42m,
                    ESSALUD = 270.07m
                }
            };

            var totales = ReporteNominaRules.CalcularTotales(nominas);

            Assert.Equal(5501.25m, totales.TotalSueldoBase);
            Assert.Equal(113.00m, totales.TotalAsignacionFamiliar);
            Assert.Equal(5614.25m, totales.TotalSalarioBruto);
            Assert.Equal(776.08m, totales.TotalDescuentos);
            Assert.Equal(4838.17m, totales.TotalNetoPagar);
            Assert.Equal(505.29m, totales.TotalESSALUD);
        }

        // RN-02: Totales con valores en cero deben funcionar correctamente
        [Fact]
        public void CalcularTotales_ConValoresEnCero_DebeFuncionar()
        {
            var nominas = new List<NominaDetalle>
            {
                new NominaDetalle
                {
                    SueldoBase = 2000m,
                    AsignacionFamiliar = 0m,
                    SalarioBruto = 2000m,
                    Deducciones = 0m,
                    SueldoNeto = 2000m,
                    ESSALUD = 0m
                },
                new NominaDetalle
                {
                    SueldoBase = 0m,
                    AsignacionFamiliar = 0m,
                    SalarioBruto = 0m,
                    Deducciones = 0m,
                    SueldoNeto = 0m,
                    ESSALUD = 0m
                }
            };

            var totales = ReporteNominaRules.CalcularTotales(nominas);

            Assert.Equal(2000m, totales.TotalSueldoBase);
            Assert.Equal(0m, totales.TotalAsignacionFamiliar);
            Assert.Equal(2000m, totales.TotalSalarioBruto);
            Assert.Equal(0m, totales.TotalDescuentos);
            Assert.Equal(2000m, totales.TotalNetoPagar);
            Assert.Equal(0m, totales.TotalESSALUD);
        }

        // CA-05: Los totales deben incluir empleados inactivos o históricos del período
        [Fact]
        public void CalcularTotales_DebeIncluirTodosLosEmpleadosDelPeriodo()
        {
            var nominas = new List<NominaDetalle>
            {
                new NominaDetalle { SueldoBase = 2000m, AsignacionFamiliar = 113m, SalarioBruto = 2113m, Deducciones = 300m, SueldoNeto = 1813m, ESSALUD = 190m },
                new NominaDetalle { SueldoBase = 3000m, AsignacionFamiliar = 0m, SalarioBruto = 3000m, Deducciones = 450m, SueldoNeto = 2550m, ESSALUD = 270m },
                new NominaDetalle { SueldoBase = 2500m, AsignacionFamiliar = 113m, SalarioBruto = 2613m, Deducciones = 375m, SueldoNeto = 2238m, ESSALUD = 235m }
            };

            var totales = ReporteNominaRules.CalcularTotales(nominas);

            Assert.Equal(7500m, totales.TotalSueldoBase);
            Assert.Equal(226m, totales.TotalAsignacionFamiliar);
        }

        // RN-02: Totales con un gran número de empleados
        [Fact]
        public void CalcularTotales_ConMuchosEmpleados_DebeCalcularCorrectamente()
        {
            var nominas = new List<NominaDetalle>();
            for (int i = 0; i < 100; i++)
            {
                nominas.Add(new NominaDetalle
                {
                    SueldoBase = 2000m,
                    AsignacionFamiliar = 113m,
                    SalarioBruto = 2113m,
                    Deducciones = 300m,
                    SueldoNeto = 1813m,
                    ESSALUD = 190m
                });
            }

            var totales = ReporteNominaRules.CalcularTotales(nominas);

            Assert.Equal(200000m, totales.TotalSueldoBase);
            Assert.Equal(11300m, totales.TotalAsignacionFamiliar);
            Assert.Equal(211300m, totales.TotalSalarioBruto);
            Assert.Equal(30000m, totales.TotalDescuentos);
            Assert.Equal(181300m, totales.TotalNetoPagar);
            Assert.Equal(19000m, totales.TotalESSALUD);
        }

        // RN-02: Verificar que todos los campos del resultado se inicializan
        [Fact]
        public void CalcularTotales_ResultadoDebeInicializarTodosCampos()
        {
            var nominas = new List<NominaDetalle>();

            var totales = ReporteNominaRules.CalcularTotales(nominas);

            Assert.NotNull(totales);
            Assert.IsType<ReporteTotales>(totales);
            Assert.Equal(0m, totales.TotalSueldoBase);
            Assert.Equal(0m, totales.TotalAsignacionFamiliar);
            Assert.Equal(0m, totales.TotalSalarioBruto);
            Assert.Equal(0m, totales.TotalDescuentos);
            Assert.Equal(0m, totales.TotalNetoPagar);
            Assert.Equal(0m, totales.TotalESSALUD);
        }

        // RN-02: Totales con solo asignación familiar (sin sueldo base)
        [Fact]
        public void CalcularTotales_ConSoloAsignacionFamiliar_DebeSumarCorrectamente()
        {
            var nominas = new List<NominaDetalle>
            {
                new NominaDetalle
                {
                    SueldoBase = 0m,
                    AsignacionFamiliar = 113m,
                    SalarioBruto = 113m,
                    Deducciones = 15m,
                    SueldoNeto = 98m,
                    ESSALUD = 10m
                }
            };

            var totales = ReporteNominaRules.CalcularTotales(nominas);

            Assert.Equal(0m, totales.TotalSueldoBase);
            Assert.Equal(113m, totales.TotalAsignacionFamiliar);
        }

        // CA-06: Los totales deben calcularse independientemente de filtros aplicados
        [Fact]
        public void CalcularTotales_ConSubconjuntoEmpleados_DebeCalcularSoloEsos()
        {
            var nominasDepartamentoA = new List<NominaDetalle>
            {
                new NominaDetalle { SueldoBase = 2000m, AsignacionFamiliar = 113m, SalarioBruto = 2113m, Deducciones = 300m, SueldoNeto = 1813m, ESSALUD = 190m },
                new NominaDetalle { SueldoBase = 2500m, AsignacionFamiliar = 0m, SalarioBruto = 2500m, Deducciones = 350m, SueldoNeto = 2150m, ESSALUD = 225m }
            };

            var totales = ReporteNominaRules.CalcularTotales(nominasDepartamentoA);

            Assert.Equal(4500m, totales.TotalSueldoBase);
            Assert.Equal(113m, totales.TotalAsignacionFamiliar);
            Assert.Equal(4613m, totales.TotalSalarioBruto);
        }

        // RN-02: Verificar que el orden de los elementos no afecta el resultado
        [Fact]
        public void CalcularTotales_OrdenNoDebeAfectarResultado()
        {
            var nominasOrden1 = new List<NominaDetalle>
            {
                new NominaDetalle { SueldoBase = 2000m, AsignacionFamiliar = 113m, SalarioBruto = 2113m, Deducciones = 300m, SueldoNeto = 1813m, ESSALUD = 190m },
                new NominaDetalle { SueldoBase = 3000m, AsignacionFamiliar = 0m, SalarioBruto = 3000m, Deducciones = 450m, SueldoNeto = 2550m, ESSALUD = 270m }
            };

            var nominasOrden2 = new List<NominaDetalle>
            {
                new NominaDetalle { SueldoBase = 3000m, AsignacionFamiliar = 0m, SalarioBruto = 3000m, Deducciones = 450m, SueldoNeto = 2550m, ESSALUD = 270m },
                new NominaDetalle { SueldoBase = 2000m, AsignacionFamiliar = 113m, SalarioBruto = 2113m, Deducciones = 300m, SueldoNeto = 1813m, ESSALUD = 190m }
            };

            var totales1 = ReporteNominaRules.CalcularTotales(nominasOrden1);
            var totales2 = ReporteNominaRules.CalcularTotales(nominasOrden2);

            Assert.Equal(totales1.TotalSueldoBase, totales2.TotalSueldoBase);
            Assert.Equal(totales1.TotalAsignacionFamiliar, totales2.TotalAsignacionFamiliar);
            Assert.Equal(totales1.TotalSalarioBruto, totales2.TotalSalarioBruto);
            Assert.Equal(totales1.TotalDescuentos, totales2.TotalDescuentos);
            Assert.Equal(totales1.TotalNetoPagar, totales2.TotalNetoPagar);
            Assert.Equal(totales1.TotalESSALUD, totales2.TotalESSALUD);
        }

        // RN-02: Totales con valores negativos (caso extremo, no debería ocurrir pero se valida)
        [Fact]
        public void CalcularTotales_ConValoresNegativos_DebeSumarCorrectamente()
        {
            var nominas = new List<NominaDetalle>
            {
                new NominaDetalle
                {
                    SueldoBase = 2000m,
                    AsignacionFamiliar = 113m,
                    SalarioBruto = 2113m,
                    Deducciones = 300m,
                    SueldoNeto = 1813m,
                    ESSALUD = 190m
                },
                new NominaDetalle
                {
                    SueldoBase = -500m,
                    AsignacionFamiliar = 0m,
                    SalarioBruto = -500m,
                    Deducciones = 0m,
                    SueldoNeto = -500m,
                    ESSALUD = 0m
                }
            };

            var totales = ReporteNominaRules.CalcularTotales(nominas);

            Assert.Equal(1500m, totales.TotalSueldoBase);
            Assert.Equal(113m, totales.TotalAsignacionFamiliar);
            Assert.Equal(1613m, totales.TotalSalarioBruto);
        }

        // CA-01, CA-02: Totales calculados deben poder usarse en reportes PDF y Excel
        [Fact]
        public void CalcularTotales_ResultadoDebeSerUtilizableEnReportes()
        {
            var nominas = new List<NominaDetalle>
            {
                new NominaDetalle { SueldoBase = 2500m, AsignacionFamiliar = 113m, SalarioBruto = 2613m, Deducciones = 350m, SueldoNeto = 2263m, ESSALUD = 235m },
                new NominaDetalle { SueldoBase = 3000m, AsignacionFamiliar = 0m, SalarioBruto = 3000m, Deducciones = 450m, SueldoNeto = 2550m, ESSALUD = 270m }
            };

            var totales = ReporteNominaRules.CalcularTotales(nominas);

            Assert.NotNull(totales);
            Assert.True(totales.TotalSueldoBase > 0);
            Assert.True(totales.TotalSalarioBruto > 0);
            Assert.True(totales.TotalNetoPagar > 0);
            Assert.True(totales.TotalSalarioBruto >= totales.TotalNetoPagar);
        }



    }
}
