use master
go

CREATE DATABASE Nomina
go

USE Nomina
GO

 set dateformat dmy
 go

 ------------------------------------------------------------------------------------------------------------

  --==================================================================================================================================
 --														TABLA EMPLEADOS
  --==================================================================================================================================
  
create table dbo.Empleados
(
EmpleadosCodigo nchar(5) not null,
EmpleadosPaterno nvarchar(50) not null,
EmpleadosMaterno nvarchar(50) not null,
EmpleadosNombres nvarchar(50) not null,
 EmpleadoNombreCompleto  as
Upper(Concat_ws(space(1), EmpleadosPaterno, EmpleadosMaterno, EmpleadosNombres)),
EmpleadosFechaNacimiento Date not null,
	EmpleadosSexo nchar (1),
	EmpleadosDireccion nvarchar(100),
	EmpleadosFono nvarchar (50),
	EmpleadosCorreoPersonal nvarchar (100) constraint EmpleadosCorreoPersonalDF Default 'Por definir',
	EmpleadosCorreoCorporativo nvarchar (100),
	EmpleadosTipoDocumento nvarchar(3),
	EmpleadosDocumento nvarchar (100),
	EmpleadosEstado nchar(1) constraint EmpleadosEstadoDF Default 'A',
	constraint EmpleadosPK primary key (EmpleadosCodigo),
	constraint EmpleadosEstadoCK check (EmpleadosEstado = 'A' or
	EmpleadosEstado = 'I' or EmpleadosEstado = 'E'),
	Constraint EmpleadosSexoCK check (EmpleadosSexo = 'F' or EmpleadosSexo = 'M'),
	constraint EmpleadosTipoDocumentoCK check (
    EmpleadosTipoDocumento in ('DNI', 'CE', 'PAS', 'PTP'))
)
go



CREATE TABLE dbo.Area (
    AreaCodigo NCHAR(5) NOT NULL,
    AreaDescripcion NVARCHAR(100) NOT NULL,
    AreaEstado NCHAR(1) NOT NULL CONSTRAINT AreaEstadoDF DEFAULT 'A',
    
    CONSTRAINT AreaPK PRIMARY KEY (AreaCodigo),
    CONSTRAINT AreaEstadoCK CHECK (AreaEstado IN ('A', 'I'))
)
GO


CREATE TABLE dbo.Cargo (
    CargoCodigo NCHAR(5) NOT NULL,
    CargoDescripcion NVARCHAR(100) NOT NULL,
    CargoEstado NCHAR(1) NOT NULL CONSTRAINT CargoEstadoDF DEFAULT 'A',
    
    CONSTRAINT CargoPK PRIMARY KEY (CargoCodigo),
    CONSTRAINT CargoEstadoCK CHECK (CargoEstado IN ('A', 'I'))
)





CREATE TABLE dbo.Contrato (
    ContratoCodigo NCHAR(5) NOT NULL,
    ContratoEmpleadoCodigo NCHAR(5) NOT NULL,
    ContratoTipoContrato NVARCHAR(20) NOT NULL 
        CONSTRAINT ContratoTipoContratoDF DEFAULT 'INDEFINIDO',
    ContratoFechaInicio DATE NOT NULL,
    ContratoFechaFin DATE NULL,
    ContratoRegimenPensionario NVARCHAR(50) NOT NULL 
        CONSTRAINT ContratoRegimenPensionarioDF DEFAULT 'ONP',
    ContratoCargoCodigo NCHAR(5) NOT NULL,
    ContratoAreaCodigo NCHAR(5) NOT NULL,
    ContratoSueldo NUMERIC(9,2) NOT NULL,

    ContratoTipoSeguro NVARCHAR(20) NOT NULL 
        CONSTRAINT ContratoTipoSeguroDF DEFAULT 'ESSALUD'
        CONSTRAINT ContratoTipoSeguroCK CHECK (ContratoTipoSeguro IN ('ESSALUD','EPS','SIS')),
    ContratoEntidadEPS NVARCHAR(50) NULL,

    ContratoAsignacionFamiliar NCHAR(1) NOT NULL 
        CONSTRAINT ContratoAsignacionFamiliarDF DEFAULT 'N'
        CONSTRAINT ContratoAsignacionFamiliarCK CHECK (ContratoAsignacionFamiliar IN ('S','N')),

    ContratoMotivoFin NVARCHAR(200) NULL,
	ContratoFechaRegistro DATETIME DEFAULT GETDATE(),
	ContratoFechaModificacion DATETIME,
    ContratoEstado NCHAR(1) NOT NULL 
        CONSTRAINT ContratoEstadoDF DEFAULT 'A'
        CONSTRAINT ContratoEstadoCK CHECK (ContratoEstado IN ('A','I','F')), -- F = Finalizado

    CONSTRAINT ContratoPK PRIMARY KEY (ContratoCodigo),

    CONSTRAINT ContratoEmpleadoFK FOREIGN KEY (ContratoEmpleadoCodigo)
        REFERENCES dbo.Empleados (EmpleadosCodigo),

    CONSTRAINT ContratoCargoFK FOREIGN KEY (ContratoCargoCodigo)
        REFERENCES dbo.Cargo (CargoCodigo),

    CONSTRAINT ContratoAreaFK FOREIGN KEY (ContratoAreaCodigo)
        REFERENCES dbo.Area (AreaCodigo),


    CONSTRAINT ContratoTipoContratoCK CHECK (ContratoTipoContrato IN ('INDEFINIDO','PLAZO_FIJO'))
);
GO

-- Índice filtrado RN-01: solo un contrato activo por empleado
CREATE UNIQUE INDEX UX_Contrato_Empleado_Activo
ON dbo.Contrato (ContratoEmpleadoCodigo)
WHERE ContratoEstado = 'A';
GO




CREATE TABLE PeriodosNomina
(
    PeriodoCodigo NCHAR(5),
    PeriodoTipo NVARCHAR(20),
    PeriodoInicio DATE,
    PeriodoFin DATE,
    PeriodoEstado NCHAR(1),
    CONSTRAINT PeriodosNominaPK PRIMARY KEY (PeriodoCodigo),
    CONSTRAINT Periodos_CHK_Estado CHECK (PeriodoEstado IN ('A', 'C', 'P'))
);
GO



CREATE TABLE Nominas
(
    NominaCodigo NCHAR(5),
    PeriodoCodigo NCHAR(5),
    ContratoCodigo NCHAR(5),
    NominaHorasExtras INT DEFAULT 0,                
    NominaBonificacion NUMERIC(10,2) DEFAULT 0,
    NominaDescuentos NUMERIC(10,2) DEFAULT 0,
    NominaTotalIngresos NUMERIC(10,2),
    NominaTotalDescuentos NUMERIC(10,2),
    NominaSueldoNeto NUMERIC(10,2),
	NominaSueldoBase NUMERIC(10,2) DEFAULT 0,
    NominaAsignacionFamiliar NUMERIC(10,2) DEFAULT 0,
    NominaDescuento_ONP NUMERIC(10,2) DEFAULT 0,
    NominaDescuento_AFP NUMERIC(10,2) DEFAULT 0,
    NominaDescuento_Impuesto5ta NUMERIC(10,2) DEFAULT 0,
    NominaAporte_ESSALUD NUMERIC(10,2) DEFAULT 0,
    NominaFechaProcesamiento DATETIME DEFAULT GETDATE(),
    NominaEstado NCHAR(1) DEFAULT 'A',

    CONSTRAINT NominasPK PRIMARY KEY (NominaCodigo),
    CONSTRAINT Nominas_FK_Periodo FOREIGN KEY (PeriodoCodigo) REFERENCES PeriodosNomina (PeriodoCodigo),
    CONSTRAINT Nominas_FK_Contrato FOREIGN KEY (ContratoCodigo) REFERENCES dbo.Contrato (ContratoCodigo),
    CONSTRAINT Nominas_CHK_Estado CHECK (NominaEstado IN ('B', 'P', 'A', 'N'))

	/*
B	Borrador	Cuando la nómina está siendo calculada o editada, aún no se ha procesado.
P	Procesada	Cuando se validó y calculó correctamente; pendiente de aprobación o revisión.
A	Aprobada	Cuando fue revisada, aprobada y guardada como definitiva (ya no editable).
N	Anulada	Si hubo error, duplicado o se canceló la nómina.
	*/

);
GO


-------------------------------------------------------------------------------------------------------------------------------------
--Inserts
-------------------------------------------------------------------------------------------------------------------------------------

--==================================================================================================================================
 --													TABLA Empleados
--==================================================================================================================================


INSERT INTO dbo.Empleados (
    EmpleadosCodigo,
    EmpleadosPaterno,
    EmpleadosMaterno,
    EmpleadosNombres,
    EmpleadosFechaNacimiento,
    EmpleadosSexo,
    EmpleadosDireccion,
    EmpleadosFono,
    EmpleadosCorreoCorporativo,
    EmpleadosTipoDocumento,
    EmpleadosDocumento,
    EmpleadosEstado
)
VALUES
('E0001', 'GARCIA', 'LOPEZ', 'JUAN',       '1990-05-15', 'M', 'Av. Lima 123',     '987654321', 'juan.garcia@shalom.com', 'DNI', '12345678', 'A'),
('E0002', 'RAMIREZ', 'QUISPE', 'MARIA',     '1985-08-22', 'F', 'Calle Sol 456',    '976543210', 'maria.ramirez@shalom.com', 'CE', 'CE123456', 'A'),
('E0003', 'MENDOZA', 'SANCHEZ', 'LUIS',     '1992-12-01', 'M', 'Jr. Luna 789',     '965432109', 'luis.mendoza@shalom.com', 'DNI', '87654321', 'A'),
('E0004', 'TORRES', 'ROJAS', 'ANA',         '1995-07-09', 'F', 'Av. Arequipa 100', '954321098', 'ana.torres@shalom.com', 'PAS', 'PA987654', 'A'),
('E0005', 'FERNANDEZ', 'VEGA', 'CARLOS',    '1980-03-30', 'M', 'Jr. Cusco 111',    '943210987', 'carlos.fernandez@shalom.com', 'PTP', 'PT123456', 'A'),
('E0006', 'CASTRO', 'HUAMAN', 'LUISA',      '1998-11-12', 'F', 'Av. Tacna 222',    '932109876', 'luisa.castro@shalom.com', 'CE', 'CE654321', 'A'),
('E0007', 'RODRIGUEZ', 'DIAZ', 'MIGUEL',    '1987-02-05', 'M', 'Jr. Callao 333',   '921098765', 'miguel.rodriguez@shalom.com', 'DNI', '23456789', 'I'),
('E0008', 'SALAZAR', 'MORALES', 'SOFIA',    '1993-09-18', 'F', 'Av. Perú 444',     '910987654', 'sofia.salazar@shalom.com', 'PAS', 'PA123789', 'A'),
('E0009', 'PEREZ', 'CASTILLO', 'ANDREA',    '1991-06-25', 'F', 'Jr. Amazonas 555', '999888777', 'andrea.perez@shalom.com', 'DNI', '34567890', 'A'),
('E0010', 'LOPEZ', 'GOMEZ', 'DIEGO',        '1983-10-10', 'M', 'Av. Javier Prado', '988877766', 'diego.lopez@shalom.com', 'PTP', 'PT654321', 'E')

go


select*from  dbo.Empleados
go





INSERT INTO dbo.Empleados (
    EmpleadosCodigo,
    EmpleadosPaterno,
    EmpleadosMaterno,
    EmpleadosNombres,
    EmpleadosFechaNacimiento,
    EmpleadosSexo,
    EmpleadosDireccion,
    EmpleadosFono,
    EmpleadosCorreoCorporativo,
    EmpleadosTipoDocumento,
    EmpleadosDocumento,
    EmpleadosEstado
)
VALUES
('E0011', 'HUERTA', 'NAVARRO', 'VALERIA',   '1996-04-22', 'F', 'Av. Los Olivos 123',  '977665544', 'valeria.huerta@shalom.com', 'DNI', '56789012', 'A'),
('E0012', 'MORA', 'ESPINOZA', 'JORGE',      '1989-01-13', 'M', 'Jr. Trujillo 456',     '988554433', 'jorge.mora@shalom.com', 'CE', 'CE789012', 'A'),
('E0013', 'RIVERA', 'CAMPOS', 'NATALIA',    '1994-09-03', 'F', 'Av. Miraflores 789',   '999443322', 'natalia.rivera@shalom.com', 'PAS', 'PA456789', 'A'),
('E0014', 'FLORES', 'PONCE', 'RICARDO',     '1986-07-28', 'M', 'Calle Central 321',    '955332211', 'ricardo.flores@shalom.com', 'DNI', '67890123', 'I'),
('E0015', 'CARRASCO', 'LEON', 'PAULA',      '1999-12-19', 'F', 'Jr. Libertad 654',     '966221100', 'paula.carrasco@shalom.com', 'PTP', 'PT987654', 'A');

go


--==================================================================================================================================
 --													TABLA Área y Cargos
--==================================================================================================================================


INSERT INTO dbo.Cargo VALUES
('CAR01', 'Analista de Sistemas', 'A'),
('CAR02', 'Contador', 'A'),
('CAR03', 'Asistente Administrativo', 'A'),
('CAR04', 'Gerente de Recursos Humanos', 'A'),
('CAR05', 'Técnico en Mantenimiento', 'A');

INSERT INTO dbo.Area VALUES
('AR001', 'Tecnología', 'A'),
('AR002', 'Finanzas', 'A'),
('AR003', 'Recursos Humanos', 'A'),
('AR004', 'Operaciones', 'A'),
('AR005', 'Mantenimiento', 'A');



--==================================================================================================================================
 --													TABLA Contrato
--==================================================================================================================================


INSERT INTO dbo.Contrato
(
    ContratoCodigo,
    ContratoEmpleadoCodigo,
    ContratoTipoContrato,
    ContratoFechaInicio,
    ContratoFechaFin,
    ContratoRegimenPensionario,
    ContratoCargoCodigo,
    ContratoAreaCodigo,
    ContratoSueldo,
    ContratoTipoSeguro,
    ContratoEntidadEPS,
    ContratoAsignacionFamiliar,
    ContratoMotivoFin,
    ContratoFechaRegistro,
    ContratoFechaModificacion,
    ContratoEstado
)
VALUES
-- 1. Juan García - Activo / ESSALUD
('C0001','E0001','INDEFINIDO','2023-03-01',NULL,'ONP','CAR01','AR001',3200.00,
 'ESSALUD',NULL,'N',NULL,GETDATE(),NULL,'A'),

-- 2. María Ramírez - Activa / EPS Rímac
('C0002','E0002','INDEFINIDO','2022-05-15',NULL,'AFP Integra','CAR02','AR002',4800.00,
 'EPS','Rímac','N',NULL,GETDATE(),NULL,'A'),

-- 3. Luis Mendoza - Activo / ESSALUD
('C0003','E0003','PLAZO_FIJO','2024-01-10','2025-12-31','ONP','CAR03','AR004',2500.00,
 'ESSALUD',NULL,'N',NULL,GETDATE(),NULL,'A'),

-- 4. Ana Torres - Finalizado
('C0004','E0004','PLAZO_FIJO','2022-06-01','2023-05-31','ONP','CAR03','AR003',2100.00,
 'ESSALUD',NULL,'N','Contrato culminado por término de plazo',GETDATE(),NULL,'F'),

-- 5. Carlos Fernández - Activo / EPS Pacífico / tiene hijos
('C0005','E0005','INDEFINIDO','2020-09-01',NULL,'AFP Prima','CAR05','AR005',3600.00,
 'EPS','Pacífico','S',NULL,GETDATE(),NULL,'A'),

-- 6. Luisa Castro - Activa / ESSALUD / tiene hijos
('C0006','E0006','INDEFINIDO','2023-02-01',NULL,'ONP','CAR03','AR003',2700.00,
 'ESSALUD',NULL,'S',NULL,GETDATE(),NULL,'A'),

-- 7. Miguel Rodríguez - Finalizado
('C0007','E0007','PLAZO_FIJO','2021-01-15','2022-01-15','ONP','CAR05','AR005',2300.00,
 'ESSALUD',NULL,'N','Fin de contrato temporal',GETDATE(),NULL,'F'),

-- 8. Sofía Salazar - Activa / EPS Rímac / con hijos
('C0008','E0008','INDEFINIDO','2023-07-01',NULL,'AFP Hábitat','CAR01','AR001',3900.00,
 'EPS','Rímac','S',NULL,GETDATE(),NULL,'A'),

-- 9. Andrea Pérez - Activa / ESSALUD
('C0009','E0009','INDEFINIDO','2022-11-01',NULL,'ONP','CAR02','AR002',4200.00,
 'ESSALUD',NULL,'N',NULL,GETDATE(),NULL,'A'),

-- 10. Diego López - Finalizado
('C0010','E0010','INDEFINIDO','2018-03-01','2019-03-01','ONP','CAR05','AR005',2500.00,
 'ESSALUD',NULL,'N','Contrato terminado por sanción',GETDATE(),NULL,'F');
GO



--==================================================================================================================================
 --													TABLA PeriodosNomina
--==================================================================================================================================


INSERT INTO PeriodosNomina (PeriodoCodigo, PeriodoTipo, PeriodoInicio, PeriodoFin, PeriodoEstado)
VALUES
('P0001', 'MENSUAL', '2025-01-01', '2025-01-31', 'C'),
('P0002', 'MENSUAL', '2025-02-01', '2025-02-28', 'A'),
('P0003', 'MENSUAL', '2025-03-01', '2025-03-31', 'A'),
('P0004', 'MENSUAL', '2025-04-01', '2025-04-30', 'A')
GO

use nomina
go

INSERT INTO PeriodosNomina (PeriodoCodigo, PeriodoTipo, PeriodoInicio, PeriodoFin, PeriodoEstado)
VALUES
('P0005', 'MENSUAL', '2025-05-01', '2025-05-31', 'A'),
('P0006', 'MENSUAL', '2025-06-01', '2025-06-30', 'A'),
('P0007', 'MENSUAL', '2025-07-01', '2025-07-31', 'A'),
('P0008', 'MENSUAL', '2025-08-01', '2025-08-31', 'A'),
('P0009', 'MENSUAL', '2025-09-01', '2025-09-30', 'A'),
('P0010', 'MENSUAL', '2025-10-01', '2025-10-31', 'A');

GO



INSERT INTO PeriodosNomina (PeriodoCodigo, PeriodoTipo, PeriodoInicio, PeriodoFin, PeriodoEstado)
VALUES

('P0011', 'MENSUAL', '2025-11-01', '2025-11-30', 'A'),
('P0012', 'MENSUAL', '2025-12-01', '2025-12-31', 'A');

GO



select * from PeriodosNomina
go
--==================================================================================================================================
 --													TABLA Nominas
--==================================================================================================================================

  

select*from Nominas
go