
CREATE DATABASE PlanillaDB

GO

USE PlanillaDB;


---Tabla de puesto de trabajo SI
CREATE TABLE PuestoTrabajo (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    NombrePuesto VARCHAR(50) NOT NULL,
    SalarioBase DECIMAL(8,2) NOT NULL,
    ValorxHora DECIMAL(8,2) NOT NULL,
    ValorExtra DECIMAL(8,2) NOT NULL,
    Estado TINYINT NULL
);


---Tabla de Tipo de horarios SI
CREATE TABLE TipodeHorario (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    NombreHorario VARCHAR(50) NOT NULL	
);

---Tabla de horarios SI
CREATE TABLE Horario (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TipoDeHorarioId INT NOT NULL,
    HorasxDia INT NOT NULL,
    Dias VARCHAR(30) NOT NULL,	
    HorasEntrada TIME NOT NULL,
    HorasSalida TIME NOT NULL,
    CONSTRAINT FK_Horarios_Tipo FOREIGN KEY (TipoDeHorarioId) REFERENCES TipoDeHorario(Id)
);


---Tabla de Empleado SI
CREATE TABLE Empleado (
    Id INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    JefeInmediatoId INT NULL,
    TipoDeHorarioId INT NULL,
    DUI VARCHAR(10) NOT NULL,
    Nombre VARCHAR(50) NOT NULL,
    Apellido VARCHAR(50) NOT NULL,
    Telefono VARCHAR(15) NOT NULL,
    Correo VARCHAR(100),
    Estado TINYINT NULL,
	SalarioBase DECIMAL(8,2) NULL,
    FechaContraInicial DATE NOT NULL,
    FechaContraFinal DATE NOT NULL,
    Usuario VARCHAR(60) NULL,
    Password CHAR(32) NULL,
    PuestoTrabajoId INT NULL,
    CONSTRAINT FK_Empleado_Jefe FOREIGN KEY (JefeInmediatoId) REFERENCES Empleado(Id),
    CONSTRAINT FK_Empleado_Horario FOREIGN KEY (TipoDeHorarioId) REFERENCES TipoDeHorario(Id),
    CONSTRAINT FK_Empleado_Puesto FOREIGN KEY (PuestoTrabajoId) REFERENCES PuestoTrabajo(Id)
);

--Tabla de jefes Inmediatos 
 --CREATE TABLE JefeInmediato(
--	Id INT IDENTITY(1,1)PRIMARY KEY NOT NULL,
--	EmpleadosId INT NULL,
--    CONSTRAINT FK_JefeInmediato_Empleados FOREIGN KEY (EmpleadosId) REFERENCES Empleado(Id) 

--);




--Tabla de vacaciones SI
CREATE TABLE Vacacion (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    EmpleadosId INT NOT NULL,
    MesVacaciones VARCHAR(50) NOT NULL,
    AnnoVacacion VARCHAR(50) NOT NULL,
    DiaInicio DATETIME NOT NULL,
    DiaFin DATETIME NOT NULL,
    Estado TINYINT NULL,
    VacacionPagada TINYINT NULL,
    PagoVacaciones DECIMAL(8,2) NULL,
    FechaPago DATETIME NULL,
    CONSTRAINT FK_Vacaciones_Empleados FOREIGN KEY (EmpleadosId) REFERENCES Empleado(Id)
);



--Tabla descuentos SI
CREATE TABLE Descuento (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(50) NOT NULL,
    Valor DECIMAL(8,2) NOT NULL,
    Estado TINYINT NULL,
    Operacion TINYINT NOT NULL,
	Planilla TINYINT NOT NULL
);


---Tabla de asignacion de descuentos NO
CREATE TABLE AsignacionDescuento (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    EmpleadosId INT NULL,
    DescuentosId INT NULL,
    CONSTRAINT FK_AsignacionDescuentos_Empleados FOREIGN KEY (EmpleadosId) REFERENCES Empleado(Id),
    CONSTRAINT FK_AsignacionDescuentos_Descuentos FOREIGN KEY (DescuentosId) REFERENCES Descuento(Id)
);


---Tabla bonos SI
CREATE TABLE Bono (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    NombreBono VARCHAR(50) NOT NULL,
    Valor DECIMAL(8,2) NOT NULL,
    Estado TINYINT NULL,
    FechaValidacion DATE NULL,
    FechaExpiracion DATE NULL,
	Operacion TINYINT NOT NULL,
	Planilla TINYINT NOT NULL,
);


---Tabla de asignacion de bonos NO
CREATE TABLE AsignacionBono (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    EmpleadosId INT NOT NULL,
    BonosId INT NOT NULL,
	Estado TINYINT NOT NULL,
    CONSTRAINT FK_AsignacionBonos_Empleados FOREIGN KEY (EmpleadosId) REFERENCES Empleado(id),
    CONSTRAINT FK_AsignacionBonos_Bonos FOREIGN KEY (BonosId) REFERENCES Bono(Id)
);


---Tabla de control de asistencia NO
CREATE TABLE ControlAsistencia (
    Id INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    EmpleadosId INT NOT NULL, 
    Dia VARCHAR(10) NOT NULL, 
    Entrada TIME NOT NULL, 
    Salida TIME NOT NULL, 
    Fecha DATE NOT NULL, 
    Asistencia VARCHAR(50) NULL,
    HoraTardia INT NULL, 
    HorasExtra INT NULL,
	EstadoPlanilla TINYINT NULL
    CONSTRAINT FK_ControlAsistencias_Empleados FOREIGN KEY (EmpleadosId) REFERENCES Empleado(Id) 
);


-- Tabla de Descuento Planilla NO
CREATE TABLE DescuentoPlanilla (
    Id INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    AsignacionDescuentoId INT NOT NULL,
	Estado TINYINT NULL,
    CONSTRAINT FK_DescuentoPlanilla_AsignacionDescuento FOREIGN KEY (AsignacionDescuentoId) REFERENCES AsignacionDescuento(Id)
);
	

	
-- Tabla de tipos de Planilla SI
CREATE TABLE TipoPlanilla (
    Id INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    NombreTipo VARCHAR(50) NOT NULL,
	FechaCreacion DATETIME NOT NULL,
	FechaModificacion DATETIME NULL
);


--Tabla de planillas NO
CREATE TABLE Planilla (
    Id INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    NombrePlanilla VARCHAR(30) NOT NULL, 
    TipoPlanillaId INT NOT NULL,
    FechaInicio DATETIME NOT NULL,
    FechaFin DATETIME NOT NULL,
    Autorizacion TINYINT NULL,
    TotalPago DECIMAL(8,2) NULL,
    CONSTRAINT FK_Planilla_TipoPlanilla FOREIGN KEY (TipoPlanillaId) REFERENCES TipoPlanilla(Id)
);

-- Tabla de Detalle de Planilla NO
CREATE TABLE DevengoPlanilla (
    Id INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    AsignacionBonosId INT NULL,
	EmpleadoPlanillaId INT  NULL,
    CONSTRAINT FK_DivingoPlanilla_AsignacionBonos FOREIGN KEY (AsignacionBonosId) REFERENCES AsignacionBono(Id),
    CONSTRAINT FK_DivingoPlanilla_EmpleadoPlanilla FOREIGN KEY (EmpleadoPlanillaId) REFERENCES Vacacion(Id),

);

-- Tabla de Empleados Planilla NO
CREATE TABLE EmpleadoPlanilla (
    Id INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    EmpleadosId INT NULL,
    DescuentoPlanillaId INT NULL,
	PlanillaId INT NULL,
    SueldoBase DECIMAL(8,2)NULL,
	TotalDiasTrabajados INT NULL,
	TotalHorasTardias INT NULL,
    TotalHorasTrabajadas INT NULL,
	ValorDeHorasExtra DECIMAL(8,2) NULL,
	TotalHorasExtra INT NULL,
    TotalPagoHorasExtra DECIMAL(8,2),
    TotalDevengos DECIMAL(8,2) NULL,
    SubTotal DECIMAL(8,2) NULL,
	VacacionId INT NULL,
	TotalPagoVacacion DECIMAL(8,2),
    TotalDescuentos DECIMAL(8,2) NULL,
    LiquidoTotal DECIMAL(8,2) NULL,
    CONSTRAINT FK_EmpleadoPlanilla_Empleados FOREIGN KEY (EmpleadosId) REFERENCES Empleado(Id), 
    CONSTRAINT FK_EmpleadoPlanilla_DescuentoPlanilla FOREIGN KEY (DescuentoPlanillaId) REFERENCES DescuentoPlanilla(Id),
    CONSTRAINT FK_EmpleadoPlanilla_Planilla FOREIGN KEY (PlanillaId) REFERENCES Planilla(Id),
    CONSTRAINT FK_EmpleadoPlanilla_Vacacion FOREIGN KEY (VacacionId) REFERENCES Vacacion(Id)

);