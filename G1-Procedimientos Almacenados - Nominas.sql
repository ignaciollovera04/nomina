USE Nomina
GO

/* ---------------------------
   sp_BuscarEmpleados 
   --------------------------- */
CREATE OR ALTER PROCEDURE dbo.sp_BuscarEmpleados
    @Query NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 10
        e.EmpleadosCodigo AS Codigo,
        e.EmpleadoNombreCompleto AS Nombre,
        ca.CargoDescripcion AS Cargo, 
        ar.AreaDescripcion AS Area   
    FROM dbo.Empleados e
    -- Unir con el contrato ACTIVO para obtener cargo/área actual
    LEFT JOIN dbo.Contrato c ON e.EmpleadosCodigo = c.ContratoEmpleadoCodigo AND c.ContratoEstado = 'A'
    LEFT JOIN dbo.Cargo ca ON c.ContratoCargoCodigo = ca.CargoCodigo
    LEFT JOIN dbo.Area ar ON c.ContratoAreaCodigo = ar.AreaCodigo
    WHERE e.EmpleadosEstado = 'A'
      AND (
            e.EmpleadosCodigo LIKE '%' + @Query + '%' OR
            e.EmpleadoNombreCompleto LIKE '%' + @Query + '%'
          )
    ORDER BY e.EmpleadoNombreCompleto;
END;
GO

/* ---------------------------
   sp_ObtenerEmpleadoPorCodigo
   --------------------------- */
CREATE OR ALTER PROCEDURE dbo.sp_ObtenerEmpleadoPorCodigo
    @EmpleadoCodigo NCHAR(5) 
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        EmpleadosCodigo,
        EmpleadosPaterno,
        EmpleadosMaterno,
        EmpleadosNombres,
        EmpleadosFechaNacimiento,
        EmpleadosSexo,
        EmpleadosDireccion,
        EmpleadosFono,
        EmpleadosCorreoPersonal,
        EmpleadosCorreoCorporativo,
        EmpleadosTipoDocumento,
        EmpleadosDocumento,
        EmpleadosEstado
    FROM dbo.Empleados
    WHERE EmpleadosCodigo = @EmpleadoCodigo;
END;
GO

/* ---------------------------
   sp_ObtenerContratoPorCodigo
   --------------------------- */
CREATE OR ALTER PROCEDURE dbo.sp_ObtenerContratoPorCodigo
    @ContratoCodigo NCHAR(5) 
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        C.ContratoCodigo, --
        C.ContratoEmpleadoCodigo, --
        E.EmpleadoNombreCompleto AS EmpleadoNombre, 
        C.ContratoTipoContrato,
        C.ContratoFechaInicio,
        C.ContratoFechaFin,
        C.ContratoRegimenPensionario,
        C.ContratoCargoCodigo, --
        CA.CargoDescripcion AS Cargo, 
        C.ContratoAreaCodigo, --
        AR.AreaDescripcion AS Area, 
        C.ContratoSueldo,
        C.ContratoTipoSeguro,
        C.ContratoEntidadEPS,
        C.ContratoAsignacionFamiliar,
        C.ContratoEstado
    FROM dbo.Contrato AS C --
    JOIN dbo.Empleados AS E ON C.ContratoEmpleadoCodigo = E.EmpleadosCodigo --
    JOIN dbo.Cargo AS CA ON C.ContratoCargoCodigo = CA.CargoCodigo --
    JOIN dbo.Area AS AR ON C.ContratoAreaCodigo = AR.AreaCodigo --
    WHERE C.ContratoCodigo = @ContratoCodigo; --
END;
GO

/* ---------------------------
   sp_ObtenerUltimoContratoPorEmpleado
   --------------------------- */
CREATE OR ALTER PROCEDURE dbo.sp_ObtenerUltimoContratoPorEmpleado
    @EmpleadoCodigo NCHAR(5) -- 
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1
        ContratoCodigo, --
        ContratoEmpleadoCodigo, --
        ContratoTipoContrato,
        ContratoFechaInicio,
        ContratoFechaFin,
        ContratoRegimenPensionario,
        ContratoCargoCodigo, --
        ContratoAreaCodigo, --
        ContratoSueldo,
        ContratoTipoSeguro,
        ContratoEntidadEPS,
        ContratoAsignacionFamiliar,
        ContratoMotivoFin,
        ContratoEstado
    FROM dbo.Contrato --
    WHERE ContratoEmpleadoCodigo = @EmpleadoCodigo --
    ORDER BY ContratoFechaInicio DESC; --: Ordenar por FechaInicio es más fiable
END;
GO

/* ---------------------------
   sp_EmpleadoTieneContratoActivo
   --------------------------- */
CREATE OR ALTER PROCEDURE dbo.sp_EmpleadoTieneContratoActivo
    @EmpleadoCodigo NCHAR(5) 
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM dbo.Contrato --
        WHERE ContratoEmpleadoCodigo = @EmpleadoCodigo --
        AND ContratoEstado = 'A'
    )
        SELECT 1 AS TieneContratoActivo;
    ELSE
        SELECT 0 AS TieneContratoActivo;
END;
GO

/* ---------------------------
   sp_AreaActiva
   --------------------------- */
CREATE OR ALTER PROCEDURE dbo.sp_AreaActiva
    @AreaCodigo NCHAR(5) --
AS
BEGIN
    SELECT CASE WHEN EXISTS(
        SELECT 1 FROM dbo.Area WHERE AreaCodigo = @AreaCodigo AND AreaEstado = 'A' --
    ) THEN 1 ELSE 0 END;
END;
GO

/* ---------------------------
   sp_CargoActivo
   --------------------------- */
CREATE OR ALTER PROCEDURE dbo.sp_CargoActivo
    @CargoCodigo NCHAR(5) --
AS
BEGIN
    SELECT CASE WHEN EXISTS(
        SELECT 1 FROM dbo.Cargo WHERE CargoCodigo = @CargoCodigo AND CargoEstado = 'A' --
    ) THEN 1 ELSE 0 END;
END;
GO

/* ---------------------------
   sp_ListarAreasActivas
   --------------------------- */
CREATE OR ALTER PROCEDURE dbo.sp_ListarAreasActivas
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        AreaCodigo, --
        AreaDescripcion
    FROM dbo.Area --
    WHERE AreaEstado = 'A'
    ORDER BY AreaDescripcion;
END;
GO

/* ---------------------------
   sp_ListarCargosActivos
   --------------------------- */
CREATE OR ALTER PROCEDURE dbo.sp_ListarCargosActivos
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        CargoCodigo, --
        CargoDescripcion
    FROM dbo.Cargo --
    WHERE CargoEstado = 'A'
    ORDER BY CargoDescripcion;
END;
GO

/* ---------------------------
   Sp_ListarEmpleadosContratos
   --------------------------- */
CREATE OR ALTER PROCEDURE dbo.Sp_ListarEmpleadosContratos
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        C.ContratoCodigo,
        E.EmpleadosCodigo AS EmpleadoCodigo,
        E.EmpleadoNombreCompleto AS EmpleadoNombre,
        C.ContratoTipoContrato AS TipoContrato,
        CA.CargoDescripcion AS Cargo,
        AR.AreaDescripcion AS Area,
        C.ContratoSueldo AS Sueldo,
        C.ContratoEstado AS EstadoContrato,
        E.EmpleadosEstado AS EstadoEmpleado,
        C.ContratoFechaInicio AS FechaInicio,
        C.ContratoFechaFin AS FechaFin
    FROM dbo.Empleados AS E
    JOIN dbo.Contrato AS C
        ON E.EmpleadosCodigo = C.ContratoEmpleadoCodigo
    JOIN dbo.Cargo AS CA
        ON C.ContratoCargoCodigo = CA.CargoCodigo
    JOIN dbo.Area AS AR
        ON C.ContratoAreaCodigo = AR.AreaCodigo
    WHERE 
        C.ContratoEstado = 'A' -- <<== FILTRO AÑADIDO: Solo contratos activos
    ORDER BY
        E.EmpleadosPaterno,
        E.EmpleadosMaterno,
        E.EmpleadosNombres;
END;
GO

/* ---------------------------
   sp_CrearContrato
   --------------------------- */
CREATE OR ALTER PROCEDURE dbo.sp_CrearContrato
    @ContratoEmpleadoCodigo NCHAR(5), --
    @ContratoTipoContrato NVARCHAR(20),
    @ContratoFechaInicio DATE,
    @ContratoFechaFin DATE = NULL,
    @ContratoRegimenPensionario NVARCHAR(50),
    @ContratoCargoCodigo NCHAR(5), --
    @ContratoAreaCodigo NCHAR(5), --
    @ContratoSueldo NUMERIC(9,2),
    @ContratoTipoSeguro NVARCHAR(20),
    @ContratoEntidadEPS NVARCHAR(50) = NULL,
    @ContratoAsignacionFamiliar NCHAR(1),
    @ContratoMotivoFin NVARCHAR(200) = NULL,
    @ContratoEstado NCHAR(1) = 'A'
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @NuevoContratoCodigo NCHAR(5); 
    DECLARE @MaxNumero INT;

    SELECT @MaxNumero = MAX(TRY_CAST(SUBSTRING(ContratoCodigo, 2, 4) AS INT)) --
    FROM dbo.Contrato; --

    IF @MaxNumero IS NULL
        SET @MaxNumero = 0;

    SET @NuevoContratoCodigo = 'C' + RIGHT('0000' + CAST(@MaxNumero + 1 AS VARCHAR(4)), 4); --

    INSERT INTO dbo.Contrato ( --
        ContratoCodigo, --
        ContratoEmpleadoCodigo, --
        ContratoTipoContrato,
        ContratoFechaInicio,
        ContratoFechaFin,
        ContratoRegimenPensionario,
        ContratoCargoCodigo, --
        ContratoAreaCodigo, --
        ContratoSueldo,
        ContratoTipoSeguro,
        ContratoEntidadEPS,
        ContratoAsignacionFamiliar,
        ContratoMotivoFin,
        ContratoEstado
    )
    VALUES (
        @NuevoContratoCodigo,
        @ContratoEmpleadoCodigo,
        @ContratoTipoContrato,
        @ContratoFechaInicio,
        @ContratoFechaFin,
        @ContratoRegimenPensionario,
        @ContratoCargoCodigo,
        @ContratoAreaCodigo,
        @ContratoSueldo,
        @ContratoTipoSeguro,
        @ContratoEntidadEPS,
        @ContratoAsignacionFamiliar,
        @ContratoMotivoFin,
        @ContratoEstado
    );

    SELECT @NuevoContratoCodigo AS NuevoContratoCodigo; --
END;
GO

/* ---------------------------
   sp_EditarContrato
   --------------------------- */
CREATE OR ALTER PROCEDURE dbo.sp_EditarContrato
    @ContratoCodigo NCHAR(5), --
    @ContratoTipoContrato NVARCHAR(20),
    @ContratoFechaInicio DATE,
    @ContratoFechaFin DATE = NULL,
    @ContratoRegimenPensionario NVARCHAR(50),
    @ContratoCargoCodigo NCHAR(5), --
    @ContratoAreaCodigo NCHAR(5), --
    @ContratoSueldo NUMERIC(9,2),
    @ContratoTipoSeguro NVARCHAR(20),
    @ContratoEntidadEPS NVARCHAR(50) = NULL,
    @ContratoAsignacionFamiliar NCHAR(1)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Contrato --
    SET 
        ContratoTipoContrato = @ContratoTipoContrato,
        ContratoFechaInicio = @ContratoFechaInicio,
        ContratoFechaFin = @ContratoFechaFin,
        ContratoRegimenPensionario = @ContratoRegimenPensionario,
        ContratoCargoCodigo = @ContratoCargoCodigo, --
        ContratoAreaCodigo = @ContratoAreaCodigo, --
        ContratoSueldo = @ContratoSueldo,
        ContratoTipoSeguro = @ContratoTipoSeguro,
        ContratoEntidadEPS = @ContratoEntidadEPS,
        ContratoAsignacionFamiliar = @ContratoAsignacionFamiliar,
        ContratoFechaModificacion = GETDATE() -- << Añadido para auditoría
    WHERE ContratoCodigo = @ContratoCodigo; --
END;
GO

/* ---------------------------
   sp_FinalizarContrato
   --------------------------- */
CREATE OR ALTER PROCEDURE dbo.sp_FinalizarContrato
    @ContratoCodigo NCHAR(5), --
    @ContratoMotivoFin NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Contrato --
    SET 
        ContratoEstado = 'F',
        ContratoMotivoFin = @ContratoMotivoFin,
        ContratoFechaFin = GETDATE(),
        ContratoFechaModificacion = GETDATE()
    WHERE ContratoCodigo = @ContratoCodigo; --
END;
GO

/* ------------------------------------------------------------------
   sp_InsertarNomina (Versión 3 - Corregido con ONP/AFP separado)
   ------------------------------------------------------------------ */
CREATE OR ALTER PROCEDURE dbo.sp_InsertarNomina
(
    @PeriodoCodigo           NCHAR(5),
    @ContratoCodigo          NCHAR(5),
    @NominaHorasExtras       INT = 0,
    @NominaBonificacion      NUMERIC(10,2) = 0,
    
    -- === PARÁMETROS DETALLADOS ===
    @NominaSueldoBase        NUMERIC(10,2),
    @NominaAsignacionFamiliar NUMERIC(10,2),
    @NominaDescuento_ONP     NUMERIC(10,2),
    @NominaDescuento_AFP     NUMERIC(10,2),
    @NominaDescuento_Impuesto5ta NUMERIC(10,2),
    @NominaAporte_ESSALUD    NUMERIC(10,2),
    
    -- === TOTALES ===
    @NominaTotalIngresos     NUMERIC(10,2),
    @NominaTotalDescuentos   NUMERIC(10,2),
    @NominaSueldoNeto        NUMERIC(10,2),

    @NominaEstado            NCHAR(1) = 'P', 
    @RegistrarHistorial      BIT = 1
)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        -- (Validaciones de Período y Contrato)
        IF NOT EXISTS (SELECT 1 FROM dbo.PeriodosNomina WHERE PeriodoCodigo = @PeriodoCodigo AND PeriodoEstado = 'A')
        BEGIN
            RAISERROR('Período no existe o no está Abierto (A).',16,1); ROLLBACK; RETURN;
        END
        IF NOT EXISTS (SELECT 1 FROM dbo.Contrato WHERE ContratoCodigo = @ContratoCodigo AND ContratoEstado = 'A')
        BEGIN
            RAISERROR('Contrato no existe o no está ACTIVO (A).',16,1); ROLLBACK; RETURN;
        END
        
        -- <<== ¡ESTA ES LA VALIDACIÓN CORREGIDA (RN-05)! ==>>
        -- Evitar duplicados (Ignorando nóminas Anuladas 'N')
        IF EXISTS (
            SELECT 1 
            FROM dbo.Nominas 
            WHERE PeriodoCodigo = @PeriodoCodigo 
              AND ContratoCodigo = @ContratoCodigo
              AND NominaEstado <> 'N' -- Ignora las anuladas
        )
        BEGIN
            RAISERROR('Ya existe una nómina activa (Borrador, Procesada o Aprobada) para ese período y contrato.',16,1); ROLLBACK; RETURN;
        END

        -- Generar nuevo NominaCodigo
        DECLARE @NuevoCodigo NCHAR(5), @Numero INT;
        SELECT @Numero = ISNULL(MAX(TRY_CAST(SUBSTRING(NominaCodigo, 2, 4) AS INT)), 0) + 1
        FROM dbo.Nominas;
        SET @NuevoCodigo = 'N' + RIGHT('0000' + CAST(@Numero AS VARCHAR(4)), 4);

        -- Insertar la nómina (ahora sí lo permite)
        INSERT INTO dbo.Nominas
        (
            NominaCodigo, PeriodoCodigo, ContratoCodigo,
            NominaHorasExtras, NominaBonificacion, 
            NominaTotalIngresos, NominaTotalDescuentos, NominaSueldoNeto,
            NominaSueldoBase,
            NominaAsignacionFamiliar,
            NominaDescuento_ONP,
            NominaDescuento_AFP,
            NominaDescuento_Impuesto5ta,
            NominaAporte_ESSALUD,
            NominaFechaProcesamiento, NominaEstado
        )
        VALUES
        (
            @NuevoCodigo, @PeriodoCodigo, @ContratoCodigo,
            @NominaHorasExtras, @NominaBonificacion,
            @NominaTotalIngresos, @NominaTotalDescuentos, @NominaSueldoNeto,
            @NominaSueldoBase,
            @NominaAsignacionFamiliar,
            @NominaDescuento_ONP,
            @NominaDescuento_AFP,
            @NominaDescuento_Impuesto5ta,
            @NominaAporte_ESSALUD,
            GETDATE(), @NominaEstado
        );
        
        COMMIT TRANSACTION;
        SELECT @NuevoCodigo AS NuevoCodigoGenerado;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrMsg,16,1);
        RETURN;
    END CATCH
END
GO

/* ---------------------------
   sp_ModificarNomina (Adaptado)
   --------------------------- */

CREATE OR ALTER PROCEDURE dbo.sp_ModificarNomina
(
    @NominaCodigo           NCHAR(5),
    @NominaHorasExtras      INT,
    @NominaBonificacion     NUMERIC(10,2),
    @NominaDescuentos       NUMERIC(10,2),
    @NominaTotalIngresos    NUMERIC(10,2),
    @NominaTotalDescuentos  NUMERIC(10,2),
    @NominaSueldoNeto       NUMERIC(10,2),
    @NominaEstado           NCHAR(1),
    @RegistrarHistorial     BIT = 1
)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS (SELECT 1 FROM dbo.Nominas WHERE NominaCodigo = @NominaCodigo)
        BEGIN
            RAISERROR('El código de nómina no existe.',16,1); ROLLBACK; RETURN;
        END

        -- No permitir modificar una nómina aprobada ('A')
        DECLARE @EstadoActual NCHAR(1);
        SELECT @EstadoActual = NominaEstado FROM dbo.Nominas WHERE NominaCodigo = @NominaCodigo;
        IF @EstadoActual = 'A'
        BEGIN
            RAISERROR('No se puede modificar una nómina aprobada. Anulela primero si necesita reprocesarla.',16,1); ROLLBACK; RETURN;
        END

        UPDATE dbo.Nominas
        SET NominaHorasExtras = @NominaHorasExtras,
            NominaBonificacion = @NominaBonificacion,
            NominaDescuentos = @NominaDescuentos,
            NominaTotalIngresos = @NominaTotalIngresos,
            NominaTotalDescuentos = @NominaTotalDescuentos,
            NominaSueldoNeto = @NominaSueldoNeto,
            NominaEstado = @NominaEstado,
            NominaFechaProcesamiento = GETDATE()
        WHERE NominaCodigo = @NominaCodigo;

        /* -- BLOQUE DE HISTORIAL COMENTADO --
        -- La tabla 'HistorialNomina' no existe en el script de la nueva BD.
        -- Si la crea, puede descomentar este bloque.
        
        IF @RegistrarHistorial = 1
        BEGIN
            DECLARE @UltHRaw NCHAR(5), @UltH VARCHAR(10), @NuevoH VARCHAR(10), @NumH INT;
            SELECT TOP 1 @UltHRaw = HistorialNominaCodigo FROM HistorialNomina ORDER BY HistorialNominaCodigo DESC;
            SET @UltH = LTRIM(RTRIM(@UltHRaw));
            IF @UltH IS NULL OR @UltH = '' 
                SET @NuevoH = 'H0001';
            ELSE
            BEGIN
                SET @NumH = TRY_CAST(SUBSTRING(@UltH,2,4) AS INT);
                IF @NumH IS NULL SET @NumH = 0;
                SET @NumH = @NumH + 1;
                SET @NuevoH = 'H' + RIGHT('0000' + CAST(@NumH AS VARCHAR(4)),4);
            END

            -- Evento por defecto: EN002 = Procesamiento/Modificación
            INSERT INTO HistorialNomina
            (HistorialNominaCodigo, NominaCodigo, EventoNominaCodigo, HistorialNominaFecha, HistorialNominaDetalle) -- <-- ELIMINADO UsuarioCodigo
            VALUES
            (@NuevoH, @NominaCodigo, 'EN002', GETDATE(), 'Modificación / reproceso desde sp_ModificarNomina.'); -- <-- ELIMINADO @UsuarioCodigo
        END
        */

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrMsg,16,1);
        RETURN;
    END CATCH
END
GO

/* ---------------------------
   sp_ListarNominasProcesadas
   --------------------------- */
CREATE OR ALTER PROCEDURE dbo.sp_ListarNominasProcesadas
(
    @PeriodoCodigo NCHAR(5) = NULL,
    @AreaCodigo    NCHAR(5) = NULL,       
    @TipoContrato  NVARCHAR(20) = NULL  
)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- RN-19: Definimos el monto por hora extra (S/ 15)
    DECLARE @MontoPorHoraExtra NUMERIC(10,2) = 15.00;

    SELECT 
        -- --- Identificación ---
        N.NominaCodigo,
        CONTR.ContratoCodigo,
        E.EmpleadosDocumento AS DNI,
        E.EmpleadoNombreCompleto AS EmpleadoNombre,
        C.CargoDescripcion AS Cargo,
        AR.AreaDescripcion AS Area,
        CONTR.ContratoTipoContrato AS TipoContrato,

        -- --- Ingresos Detallados (RN-01, RN-16, RN-20) ---
        N.NominaSueldoBase AS SueldoBase,
        N.NominaAsignacionFamiliar AS AsignacionFamiliar,
        
        -- <<== COLUMNAS SEPARADAS ==>>
        N.NominaHorasExtras, -- Columna 1: La cantidad (ej: 8)
        (N.NominaHorasExtras * @MontoPorHoraExtra) AS HorasExtrasMonto, 
        
        N.NominaBonificacion AS Bonificaciones, -- (Contiene Grati/CTS)
        N.NominaTotalIngresos AS SalarioBruto, 

        -- --- Descuentos Detallados (RN-06, RN-08, RN-10) ---
        N.NominaDescuento_ONP AS DescuentoONP,
        N.NominaDescuento_AFP AS DescuentoAFP,
        N.NominaDescuento_Impuesto5ta AS ImpuestoQuintaCategoria,
        N.NominaTotalDescuentos AS Deducciones, 

        -- --- Aporte Empleador (RN-07) ---
        N.NominaAporte_ESSALUD AS ESSALUD,

        -- --- Totales y Auditoría ---
        N.NominaSueldoNeto AS SueldoNeto,
        CONVERT(VARCHAR(19), N.NominaFechaProcesamiento, 120) AS FechaProcesamiento,
        P.PeriodoCodigo,
        P.PeriodoInicio,
        P.PeriodoFin,
        MONTH(P.PeriodoInicio) AS Mes,
        CASE 
            WHEN N.NominaEstado = 'C' THEN 'Cerrado'
            ELSE 'Desconocido'
        END AS EstadoNomina
        
    FROM dbo.Nominas N
    INNER JOIN dbo.PeriodosNomina P ON N.PeriodoCodigo = P.PeriodoCodigo
    INNER JOIN dbo.Contrato CONTR ON N.ContratoCodigo = CONTR.ContratoCodigo
    INNER JOIN dbo.Empleados E ON CONTR.ContratoEmpleadoCodigo = E.EmpleadosCodigo
    INNER JOIN dbo.Cargo C ON CONTR.ContratoCargoCodigo = C.CargoCodigo
    INNER JOIN dbo.Area AR ON CONTR.ContratoAreaCodigo = AR.AreaCodigo
    WHERE 
        (P.PeriodoEstado = 'C') -- Solo períodos cerrados
        AND (@PeriodoCodigo IS NULL OR N.PeriodoCodigo = @PeriodoCodigo)
        AND (@AreaCodigo IS NULL OR CONTR.ContratoAreaCodigo = @AreaCodigo)
        AND (@TipoContrato IS NULL OR CONTR.ContratoTipoContrato = @TipoContrato)
    ORDER BY 
        E.EmpleadosPaterno, E.EmpleadosMaterno, E.EmpleadosNombres;
END
GO

/* ---------------------------
   sp_ListarContratosActivos
   --------------------------- */

CREATE OR ALTER PROCEDURE dbo.sp_ListarContratosActivos
AS
BEGIN
    SET NOCOUNT ON;


SELECT 
        -- Propiedades de la clase base 'Contrato'
        C.ContratoCodigo,
        C.ContratoEmpleadoCodigo,
        C.ContratoTipoContrato,
        C.ContratoFechaInicio,
        C.ContratoFechaFin,
        C.ContratoRegimenPensionario,
        C.ContratoCargoCodigo,
        C.ContratoAreaCodigo,
        C.ContratoSueldo,
        C.ContratoTipoSeguro,
        C.ContratoEntidadEPS,
        C.ContratoAsignacionFamiliar,
        C.ContratoMotivoFin,
        C.ContratoFechaRegistro,
        C.ContratoFechaModificacion,
        C.ContratoEstado,
        
        -- Propiedades de la clase derivada 'ContratoCalculoInfo'
        E.EmpleadoNombreCompleto AS EmpleadoNombre, 
        E.EmpleadosDocumento AS DNI, 
        CA.CargoDescripcion AS Cargo,
        AR.AreaDescripcion AS Area
        
    FROM dbo.Contrato C
    INNER JOIN dbo.Empleados E ON C.ContratoEmpleadoCodigo = E.EmpleadosCodigo
    LEFT JOIN dbo.Cargo CA ON C.ContratoCargoCodigo = CA.CargoCodigo
    LEFT JOIN dbo.Area AR ON C.ContratoAreaCodigo = AR.AreaCodigo
    WHERE 
        C.ContratoEstado = 'A'; -- 'A' de Activo
END
GO



use nomina
go
CREATE OR ALTER PROCEDURE dbo.sp_ListarContratosActivos
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        C.ContratoCodigo,
        C.ContratoSueldo,
        C.ContratoAsignacionFamiliar,
        C.ContratoRegimenPensionario,
        C.ContratoTipoSeguro,
        C.ContratoEntidadEPS,
        E.EmpleadoNombreCompleto AS EmpleadoNombre 
    FROM 
        dbo.Contrato C
     JOIN 
        dbo.Empleados E ON C.ContratoEmpleadoCodigo = E.EmpleadosCodigo
    WHERE 
        C.ContratoEstado = 'A'; -- 'A' de Activo
END
GO



SELECT 
    name AS NombreProcedimiento,
    SCHEMA_NAME(schema_id) AS Esquema
FROM sys.procedures
WHERE name LIKE '%contrato%';




use nomina
go
/* ---------------------------
   sp_ExisteNominaParaPeriodo (RN-05)
   --------------------------- */
CREATE OR ALTER PROCEDURE dbo.sp_ExisteNominaParaPeriodo
(
    @PeriodoCodigo NCHAR(5),
    @ContratoCodigo NCHAR(5)
)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Verifica si ya existe una nómina NO ANULADA.
    IF EXISTS (
        SELECT 1
        FROM dbo.Nominas
        WHERE PeriodoCodigo = @PeriodoCodigo 
          AND ContratoCodigo = @ContratoCodigo
          AND NominaEstado <> 'N' -- <<== ¡ESTA ES LA LÓGICA CLAVE!
    )
        SELECT 1; -- 1 = Sí, existe (Borrador, Procesada o Aprobada)
    ELSE
        SELECT 0; -- 0 = No, no existe (o está Anulada)
END
GO




/* ---------------------------
   sp_ListarTodosPeriodos
   --------------------------- */
CREATE OR ALTER PROCEDURE dbo.sp_ListarTodosPeriodos
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        PeriodoCodigo, 
        PeriodoTipo, 
        PeriodoInicio, 
        PeriodoFin, 
        PeriodoEstado
    FROM 
        dbo.PeriodosNomina
    ORDER BY 
        PeriodoInicio DESC;
END
GO


/* ---------------------------
   sp_ObtenerPeriodoPorCodigo
   --------------------------- */
CREATE OR ALTER PROCEDURE dbo.sp_ObtenerPeriodoPorCodigo
(
    @PeriodoCodigo NCHAR(5)
)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        PeriodoCodigo, 
        PeriodoTipo, 
        PeriodoInicio, 
        PeriodoFin, 
        PeriodoEstado
    FROM 
        dbo.PeriodosNomina
    WHERE 
        PeriodoCodigo = @PeriodoCodigo;
END
GO

/* ---------------------------
   sp_ActualizarEstadoPeriodo
   --------------------------- */
CREATE OR ALTER PROCEDURE dbo.sp_ActualizarEstadoPeriodo
(
    @PeriodoCodigo NCHAR(5),
    @NuevoEstado NCHAR(1)
)
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE dbo.PeriodosNomina
    SET 
        PeriodoEstado = @NuevoEstado
    WHERE 
        PeriodoCodigo = @PeriodoCodigo;
        
    -- Devuelve 1 si se afectó alguna fila, para que C# pueda verificar el éxito
    SELECT CAST(@@ROWCOUNT AS INT);
END
GO



---------------------------------------------------------------------------------------------

/* -------------------------------------------
   sp_BuscarEmpleadosParaContrato (NUEVO)
   ------------------------------------------- */
CREATE OR ALTER PROCEDURE dbo.sp_BuscarEmpleadosParaContrato
AS
BEGIN
    SET NOCOUNT ON;

    -- Selecciona empleados 'Inactivos' ('I') o 'Expulsados' ('E')
    -- Y que NO tengan un contrato 'Activo' ('A')
    SELECT 
        E.EmpleadosCodigo AS Codigo,
        E.EmpleadoNombreCompleto AS Nombre,
        E.EmpleadosEstado AS Estado, -- ¡NUEVO! Devolvemos 'I' o 'E'
        
        -- Obtenemos el último cargo y área que tuvieron (de contratos 'Finalizados')
        (SELECT TOP 1 CA.CargoDescripcion 
         FROM dbo.Contrato K 
         JOIN dbo.Cargo CA ON K.ContratoCargoCodigo = CA.CargoCodigo 
         WHERE K.ContratoEmpleadoCodigo = E.EmpleadosCodigo 
         ORDER BY K.ContratoFechaFin DESC) AS Cargo,
         
        (SELECT TOP 1 AR.AreaDescripcion 
         FROM dbo.Contrato K 
         JOIN dbo.Area AR ON K.ContratoAreaCodigo = AR.AreaCodigo 
         WHERE K.ContratoEmpleadoCodigo = E.EmpleadosCodigo 
         ORDER BY K.ContratoFechaFin DESC) AS Area
    FROM 
        dbo.Empleados E
    WHERE 
        E.EmpleadosEstado IN ('I', 'E')
        AND NOT EXISTS (
            -- RN-01: Regla de que no exista uno activo 
            SELECT 1 
            FROM dbo.Contrato C
            WHERE C.ContratoEmpleadoCodigo = E.EmpleadosCodigo 
              AND C.ContratoEstado = 'A'
        )
    ORDER BY 
        E.EmpleadosPaterno, E.EmpleadosMaterno;
END
GO


/* -------------------------------------------
   sp_GenerarNuevosPeriodosAnuales 
   ------------------------------------------- */
CREATE OR ALTER PROCEDURE dbo.sp_GenerarNuevosPeriodosAnuales
AS
BEGIN
    SET NOCOUNT ON;

    PRINT 'Generando 12 nuevos períodos...';

    -- 1. Encontrar el último período registrado
    DECLARE @UltimoCodigoN INT;
    DECLARE @UltimaFechaFin DATE;

    SELECT TOP 1
        -- Extrae el número (ej. 'P0012' -> 12)
        @UltimoCodigoN = TRY_CAST(SUBSTRING(PeriodoCodigo, 2, 4) AS INT),
        @UltimaFechaFin = PeriodoFin
    FROM
        dbo.PeriodosNomina
    ORDER BY
        PeriodoInicio DESC;

    -- Si no hay períodos, empieza desde 0
    IF @UltimoCodigoN IS NULL SET @UltimoCodigoN = 0;
    IF @UltimaFechaFin IS NULL SET @UltimaFechaFin = DATEADD(day, -1, GETDATE()); -- (Un día antes de hoy)

    PRINT 'Último período encontrado: P' + CAST(@UltimoCodigoN AS VARCHAR) + ' (Fin: ' + CONVERT(VARCHAR, @UltimaFechaFin, 23) + ')';

    -- 2. Bucle para insertar los 12 meses siguientes
    DECLARE @Contador INT = 1;
    DECLARE @NuevoCodigoN INT;
    DECLARE @NuevoCodigoSTR NCHAR(5);
    DECLARE @NuevaFechaInicio DATE;
    DECLARE @NuevaFechaFin DATE;

    WHILE @Contador <= 12
    BEGIN
        -- Calcular nuevos valores
        SET @NuevoCodigoN = @UltimoCodigoN + @Contador;
        SET @NuevoCodigoSTR = 'P' + RIGHT('0000' + CAST(@NuevoCodigoN AS VARCHAR(4)), 4);
        SET @NuevaFechaInicio = DATEADD(day, 1, @UltimaFechaFin);
        SET @NuevaFechaFin = EOMONTH(@NuevaFechaInicio); -- Fin de ese mes

        -- 3. Insertar el nuevo período
        IF NOT EXISTS (SELECT 1 FROM dbo.PeriodosNomina WHERE PeriodoCodigo = @NuevoCodigoSTR)
        BEGIN
            INSERT INTO dbo.PeriodosNomina (PeriodoCodigo, PeriodoTipo, PeriodoInicio, PeriodoFin, PeriodoEstado)
            VALUES (@NuevoCodigoSTR, 'MENSUAL', @NuevaFechaInicio, @NuevaFechaFin, 'A');
        END

        -- Preparar para la siguiente iteración
        SET @UltimaFechaFin = @NuevaFechaFin;
        SET @Contador = @Contador + 1;
    END

    PRINT '12 períodos generados exitosamente.';
END
GO


/* -------------------------------------------
   sp_AnularNomina (NUEVO)
   ------------------------------------------- */
CREATE OR ALTER PROCEDURE dbo.sp_AnularNomina
(
    @PeriodoCodigo NCHAR(5)
)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- 1. Marcar todas las nóminas de ese período como 'N' (Anulada)
        UPDATE dbo.Nominas
        SET NominaEstado = 'N' -- 'N' = Anulada
        WHERE PeriodoCodigo = @PeriodoCodigo;
        
        -- 2. Re-abrir el período (de 'C' a 'A')
        UPDATE dbo.PeriodosNomina
        SET PeriodoEstado = 'A' -- 'A' = Activo/Abierto
        WHERE PeriodoCodigo = @PeriodoCodigo;
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO


EXEC dbo.sp_ListarContratosActivos;
GO