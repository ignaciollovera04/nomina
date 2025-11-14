using Dominio.Entidades;
using System;
using System.Collections.Generic;

namespace Dominio.Reglas
{
   
    public static class ContratoRules
    {
        public const decimal RMV_VIGENTE = 1130.00m; // RN-11

        
        public class ValidacionRuleResult
        {
            public bool EsValido { get; set; }
            public string Mensaje { get; set; } = string.Empty;

            public static ValidacionRuleResult Exito() => new ValidacionRuleResult { EsValido = true };
            public static ValidacionRuleResult Falla(string mensaje) => new ValidacionRuleResult { EsValido = false, Mensaje = mensaje };
        }

     
        public static ValidacionRuleResult ValidarCreacion(Contrato contrato, Empleado empleado, bool empleadoYaTieneContratoActivo)
        {
            // RN-01: No se puede registrar un nuevo contrato si ya existe uno activo 
            
            if (empleadoYaTieneContratoActivo)
            {
                return ValidacionRuleResult.Falla("El empleado ya cuenta con un contrato activo. Finalice el contrato actual antes de registrar uno nuevo.");
            }

            // RN-11: Salario no puede ser inferior a RMV 
            // CA-06: Mensaje de error [cite: 286]
            if (contrato.ContratoSueldo < RMV_VIGENTE)
            {
                return ValidacionRuleResult.Falla($"El sueldo base (S/ {contrato.ContratoSueldo}) no puede ser inferior a la RMV vigente (S/ {RMV_VIGENTE}).");
            }

            // RN-02: Validación de Fecha de Inicio 
            // CA-11: Mensaje de error 
            if (contrato.ContratoFechaInicio.Date < DateTime.Today)
            {
                return ValidacionRuleResult.Falla("La fecha de inicio no puede ser anterior a la fecha actual.");
            }

            // RN-06: Empleado expulsado no puede ser contratado
            // CA-10: Mensaje de error 
            if (empleado.EmpleadosEstado == "E") // 'E' = Expulsado
            {
                return ValidacionRuleResult.Falla("El empleado tiene estado 'Expulsado' y no puede volver a ser contratado.");
            }

            return ValidacionRuleResult.Exito();
        }

      
        public static ValidacionRuleResult ValidarFinalizacion(string motivo)
        {
           
            if (string.IsNullOrWhiteSpace(motivo))
            {
                return ValidacionRuleResult.Falla("El motivo de finalización es obligatorio.");
            }
            return ValidacionRuleResult.Exito();
        }

      
        public static void AplicarReglasPorDefecto(Contrato contrato)
        {
        
            if (string.IsNullOrWhiteSpace(contrato.ContratoRegimenPensionario))
            {
                contrato.ContratoRegimenPensionario = "ONP";
            }
        }
    }
}