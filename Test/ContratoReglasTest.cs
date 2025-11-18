using Dominio.Entidades;
using Dominio.Reglas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    public class ContratoReglasTest
    {
        // RN-01, CA-05: No se puede registrar un nuevo contrato si ya existe uno activo
        [Fact]
        public void ValidarCreacion_Falla_SiEmpleadoYaTieneContratoActivo()
        {
            var contrato = new Contrato { ContratoSueldo = 2000, ContratoFechaInicio = DateTime.Today };
            var empleado = new Empleado { EmpleadosEstado = "A" };
            bool empleadoYaTieneContratoActivo = true;

            var resultado = ContratoRules.ValidarCreacion(contrato, empleado, empleadoYaTieneContratoActivo);

            Assert.False(resultado.EsValido);
            Assert.Contains("El empleado ya cuenta con un contrato activo", resultado.Mensaje);
        }

        // RN-11, CA-06: El salario base no puede ser inferior a la RMV
        [Fact]
        public void ValidarCreacion_Falla_SiSueldoEsMenorARMV()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 900,
                ContratoFechaInicio = DateTime.Today
            };
            var empleado = new Empleado { EmpleadosEstado = "A" };

            var resultado = ContratoRules.ValidarCreacion(contrato, empleado, false);

            Assert.False(resultado.EsValido);
            Assert.Contains("no puede ser inferior a la RMV", resultado.Mensaje);
        }

        // RN-02, CA-11: La fecha de inicio no puede ser anterior a la fecha actual
        [Fact]
        public void ValidarCreacion_Falla_SiFechaInicioEsAnteriorAHoy()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 2000,
                ContratoFechaInicio = DateTime.Today.AddDays(-1)
            };
            var empleado = new Empleado { EmpleadosEstado = "A" };

            var resultado = ContratoRules.ValidarCreacion(contrato, empleado, false);

            Assert.False(resultado.EsValido);
            Assert.Contains("anterior a la fecha actual", resultado.Mensaje);
        }

        // RN-06, CA-10: Si el empleado ha sido expulsado no puede volver a ser contratado
        [Fact]
        public void ValidarCreacion_Falla_SiEmpleadoEstaExpulsado()
        {
            var contrato = new Contrato { ContratoSueldo = 2000, ContratoFechaInicio = DateTime.Today };
            var empleado = new Empleado { EmpleadosEstado = "E" };

            var resultado = ContratoRules.ValidarCreacion(contrato, empleado, false);

            Assert.False(resultado.EsValido);
            Assert.Contains("Expulsado", resultado.Mensaje);
        }

        // CA-01: Creación de contrato válido cuando todos los datos son correctos
        [Fact]
        public void ValidarCreacion_Pasa_SiTodosLosDatosSonValidos()
        {
            var contrato = new Contrato { ContratoSueldo = 2000, ContratoFechaInicio = DateTime.Today };
            var empleado = new Empleado { EmpleadosEstado = "A" };

            var resultado = ContratoRules.ValidarCreacion(contrato, empleado, false);

            Assert.True(resultado.EsValido);
        }

        // RN-11: Caso límite - sueldo exactamente igual a RMV debe ser válido
        [Fact]
        public void ValidarCreacion_Pasa_SiSueldoEsExactamenteIgualARMV()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 1130.00m,
                ContratoFechaInicio = DateTime.Today
            };
            var empleado = new Empleado { EmpleadosEstado = "A" };

            var resultado = ContratoRules.ValidarCreacion(contrato, empleado, false);

            Assert.True(resultado.EsValido);
        }

        // RN-02: Caso límite - fecha de inicio exactamente hoy debe ser válida
        [Fact]
        public void ValidarCreacion_Pasa_SiFechaInicioEsExactamenteHoy()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 2000,
                ContratoFechaInicio = DateTime.Today
            };
            var empleado = new Empleado { EmpleadosEstado = "A" };

            var resultado = ContratoRules.ValidarCreacion(contrato, empleado, false);

            Assert.True(resultado.EsValido);
        }

        // RN-02: La fecha de inicio puede ser posterior a la fecha actual
        [Fact]
        public void ValidarCreacion_Pasa_SiFechaInicioEsFutura()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 2000,
                ContratoFechaInicio = DateTime.Today.AddDays(10)
            };
            var empleado = new Empleado { EmpleadosEstado = "A" };

            var resultado = ContratoRules.ValidarCreacion(contrato, empleado, false);

            Assert.True(resultado.EsValido);
        }

        // RN-06: Solo empleados con estado "E" (Expulsado) no pueden ser contratados
        [Theory]
        [InlineData("A")]
        [InlineData("I")]
        [InlineData("S")]
        public void ValidarCreacion_Pasa_SiEmpleadoNoEstaExpulsado(string estadoEmpleado)
        {
            var contrato = new Contrato { ContratoSueldo = 2000, ContratoFechaInicio = DateTime.Today };
            var empleado = new Empleado { EmpleadosEstado = estadoEmpleado };

            var resultado = ContratoRules.ValidarCreacion(contrato, empleado, false);

            Assert.True(resultado.EsValido);
        }

        // RN-11: Caso extremo - sueldo cero debe ser rechazado
        [Fact]
        public void ValidarCreacion_Falla_SiSueldoEsCero()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 0,
                ContratoFechaInicio = DateTime.Today
            };
            var empleado = new Empleado { EmpleadosEstado = "A" };

            var resultado = ContratoRules.ValidarCreacion(contrato, empleado, false);

            Assert.False(resultado.EsValido);
            Assert.Contains("no puede ser inferior a la RMV", resultado.Mensaje);
        }

        // RN-11: Caso extremo - sueldo negativo debe ser rechazado
        [Fact]
        public void ValidarCreacion_Falla_SiSueldoEsNegativo()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = -1000,
                ContratoFechaInicio = DateTime.Today
            };
            var empleado = new Empleado { EmpleadosEstado = "A" };

            var resultado = ContratoRules.ValidarCreacion(contrato, empleado, false);

            Assert.False(resultado.EsValido);
        }

        // RN-11: No debe existir límite superior para el sueldo
        [Fact]
        public void ValidarCreacion_Pasa_ConSueldoMuyAlto()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 100000m,
                ContratoFechaInicio = DateTime.Today
            };
            var empleado = new Empleado { EmpleadosEstado = "A" };

            var resultado = ContratoRules.ValidarCreacion(contrato, empleado, false);

            Assert.True(resultado.EsValido);
        }

        // CA-06: El mensaje de error debe incluir los valores específicos del sueldo y RMV
        [Fact]
        public void ValidarCreacion_MensajeEspecifico_CuandoFallaRMV()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 1000m,
                ContratoFechaInicio = DateTime.Today
            };
            var empleado = new Empleado { EmpleadosEstado = "A" };

            var resultado = ContratoRules.ValidarCreacion(contrato, empleado, false);

            Assert.False(resultado.EsValido);
            Assert.Contains("S/ 1000", resultado.Mensaje);
            Assert.Contains("S/ 1130", resultado.Mensaje);
        }

        // RN-03: El motivo de terminación es obligatorio (null, vacío o espacios)
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ValidarFinalizacion_Falla_SiMotivoEsNuloOVacio(string motivoInvalido)
        {
            var resultado = ContratoRules.ValidarFinalizacion(motivoInvalido);

            Assert.False(resultado.EsValido);
            Assert.Equal("El motivo de finalización es obligatorio.", resultado.Mensaje);
        }

        // CA-03: Terminación de contrato con motivo válido debe ser aceptada
        [Fact]
        public void ValidarFinalizacion_Pasa_SiMotivoEsValido()
        {
            string motivoValido = "Renuncia voluntaria";

            var resultado = ContratoRules.ValidarFinalizacion(motivoValido);

            Assert.True(resultado.EsValido);
        }

        // RN-03: Diferentes tipos de motivos de terminación deben ser válidos
        [Theory]
        [InlineData("Renuncia voluntaria")]
        [InlineData("Despido justificado")]
        [InlineData("Vencimiento de contrato")]
        [InlineData("Mutuo acuerdo")]
        [InlineData("Jubilación")]
        [InlineData("Fallecimiento")]
        public void ValidarFinalizacion_Pasa_ConDiferentesMotivosValidos(string motivo)
        {
            var resultado = ContratoRules.ValidarFinalizacion(motivo);

            Assert.True(resultado.EsValido);
            Assert.Empty(resultado.Mensaje);
        }

        // RN-03: No debe existir límite de longitud para el motivo
        [Fact]
        public void ValidarFinalizacion_Pasa_ConMotivoMuyLargo()
        {
            string motivoLargo = new string('X', 1000);

            var resultado = ContratoRules.ValidarFinalizacion(motivoLargo);

            Assert.True(resultado.EsValido);
        }

        // RN-03: Motivos con caracteres especiales deben ser aceptados
        [Fact]
        public void ValidarFinalizacion_Pasa_ConMotivoConCaracteresEspeciales()
        {
            string motivoConCaracteres = "Despido por causa grave (Art. 25° D.S. 003-97-TR) - Falta grave";

            var resultado = ContratoRules.ValidarFinalizacion(motivoConCaracteres);

            Assert.True(resultado.EsValido);
        }

        // RN-03: Motivos con saltos de línea deben ser aceptados
        [Fact]
        public void ValidarFinalizacion_Pasa_ConMotivoConSaltosDeLinea()
        {
            string motivoConSaltos = "Motivo línea 1\nMotivo línea 2";

            var resultado = ContratoRules.ValidarFinalizacion(motivoConSaltos);

            Assert.True(resultado.EsValido);
        }

        // RN-03: Solo espacios en blanco (tabulaciones, saltos) deben ser rechazados
        [Theory]
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData("\r\n")]
        public void ValidarFinalizacion_Falla_ConSoloEspaciosEnBlanco(string motivoEspacios)
        {
            var resultado = ContratoRules.ValidarFinalizacion(motivoEspacios);

            Assert.False(resultado.EsValido);
            Assert.Equal("El motivo de finalización es obligatorio.", resultado.Mensaje);
        }

        // RN-13, CA-07: Si el empleado no tiene AFP registrada, se asigna ONP automáticamente (vacío)
        [Fact]
        public void AplicarReglasPorDefecto_AsignaONP_SiRegimenEstaVacio()
        {
            var contrato = new Contrato { ContratoRegimenPensionario = "" };

            ContratoRules.AplicarReglasPorDefecto(contrato);

            Assert.Equal("ONP", contrato.ContratoRegimenPensionario);
        }

        // RN-13: Si ya existe un régimen pensionario, no debe modificarse
        [Fact]
        public void AplicarReglasPorDefecto_NoModifica_SiRegimenYaExiste()
        {
            var contrato = new Contrato { ContratoRegimenPensionario = "AFP INTEGRA" };

            ContratoRules.AplicarReglasPorDefecto(contrato);

            Assert.Equal("AFP INTEGRA", contrato.ContratoRegimenPensionario);
        }

        // RN-13, CA-07: Si el régimen pensionario es null, se asigna ONP
        [Fact]
        public void AplicarReglasPorDefecto_AsignaONP_SiRegimenEsNull()
        {
            var contrato = new Contrato { ContratoRegimenPensionario = null };

            ContratoRules.AplicarReglasPorDefecto(contrato);

            Assert.Equal("ONP", contrato.ContratoRegimenPensionario);
        }

        // RN-13: Espacios en blanco deben tratarse como vacío y asignar ONP
        [Theory]
        [InlineData("   ")]
        [InlineData("\t")]
        [InlineData("\n")]
        public void AplicarReglasPorDefecto_AsignaONP_SiRegimenEsSoloEspacios(string regimenVacio)
        {
            var contrato = new Contrato { ContratoRegimenPensionario = regimenVacio };

            ContratoRules.AplicarReglasPorDefecto(contrato);

            Assert.Equal("ONP", contrato.ContratoRegimenPensionario);
        }

        // RN-13: Todos los tipos de régimen pensionario válidos deben mantenerse
        [Theory]
        [InlineData("AFP PROFUTURO")]
        [InlineData("AFP HABITAT")]
        [InlineData("AFP PRIMA")]
        [InlineData("ONP")]
        public void AplicarReglasPorDefecto_NoModifica_DiferentesRegimenesTodosValidos(string regimen)
        {
            var contrato = new Contrato { ContratoRegimenPensionario = regimen };

            ContratoRules.AplicarReglasPorDefecto(contrato);

            Assert.Equal(regimen, contrato.ContratoRegimenPensionario);
        }

        // RN-13: Régimen con espacios alrededor no debe ser modificado (sin trim)
        [Fact]
        public void AplicarReglasPorDefecto_NoModifica_RegimenConEspaciosAlrededor()
        {
            var contrato = new Contrato { ContratoRegimenPensionario = " AFP INTEGRA " };

            ContratoRules.AplicarReglasPorDefecto(contrato);

            Assert.Equal(" AFP INTEGRA ", contrato.ContratoRegimenPensionario);
        }

        // CA-01: Caso de uso completo - Creación de contrato válido con todos los campos
        [Fact]
        public void CasoUso_CA01_CreacionContratoValido_ConTodosLosCampos()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 3000m,
                ContratoFechaInicio = DateTime.Today,
                ContratoRegimenPensionario = "AFP INTEGRA",
                ContratoAsignacionFamiliar = "S"
            };
            var empleado = new Empleado { EmpleadosEstado = "A" };

            var resultado = ContratoRules.ValidarCreacion(contrato, empleado, false);

            Assert.True(resultado.EsValido);
            Assert.Empty(resultado.Mensaje);
        }

        // CA-05: Caso de uso completo - Evitar duplicación de contrato activo
        [Fact]
        public void CasoUso_CA05_EmpleadoConContratoActivoNoPuedeCrearOtro()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 2500m,
                ContratoFechaInicio = DateTime.Today
            };
            var empleado = new Empleado { EmpleadosEstado = "A" };
            bool yaContratoActivo = true;

            var resultado = ContratoRules.ValidarCreacion(contrato, empleado, yaContratoActivo);

            Assert.False(resultado.EsValido);
            Assert.Contains("contrato activo", resultado.Mensaje);
        }

        // CA-06: Caso de uso completo - Validación de RMV con mensaje específico
        [Fact]
        public void CasoUso_CA06_ValidacionRMV_MuestraMensajeEspecifico()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 1000m,
                ContratoFechaInicio = DateTime.Today
            };
            var empleado = new Empleado { EmpleadosEstado = "A" };

            var resultado = ContratoRules.ValidarCreacion(contrato, empleado, false);

            Assert.False(resultado.EsValido);
            Assert.Contains("RMV", resultado.Mensaje);
        }

        // CA-07: Caso de uso completo - Asignación automática de ONP por defecto
        [Fact]
        public void CasoUso_CA07_AsignacionONPPorDefecto()
        {
            var contrato = new Contrato
            {
                ContratoRegimenPensionario = null
            };

            ContratoRules.AplicarReglasPorDefecto(contrato);

            Assert.Equal("ONP", contrato.ContratoRegimenPensionario);
        }

        // CA-10: Caso de uso completo - Empleado expulsado no puede ser contratado
        [Fact]
        public void CasoUso_CA10_EmpleadoExpulsadoNoEsContratado()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 2000m,
                ContratoFechaInicio = DateTime.Today
            };
            var empleado = new Empleado { EmpleadosEstado = "E" };

            var resultado = ContratoRules.ValidarCreacion(contrato, empleado, false);

            Assert.False(resultado.EsValido);
            Assert.Contains("Expulsado", resultado.Mensaje);
        }

        // CA-11: Caso de uso completo - Fecha de inicio inválida (anterior a hoy)
        [Fact]
        public void CasoUso_CA11_FechaInicioInvalida()
        {
            var contrato = new Contrato
            {
                ContratoSueldo = 2000m,
                ContratoFechaInicio = DateTime.Today.AddDays(-5)
            };
            var empleado = new Empleado { EmpleadosEstado = "A" };

            var resultado = ContratoRules.ValidarCreacion(contrato, empleado, false);

            Assert.False(resultado.EsValido);
            Assert.Contains("anterior a la fecha actual", resultado.Mensaje);
        }
    
}
}
