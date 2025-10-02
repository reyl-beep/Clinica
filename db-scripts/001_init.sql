IF DB_ID('ClinicaDB') IS NULL
BEGIN
    CREATE DATABASE ClinicaDB;
END;
GO

USE ClinicaDB;
GO

IF OBJECT_ID('dbo.CatMedicos', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CatMedicos
    (
        Id INT IDENTITY(1, 1) PRIMARY KEY,
        PrimerNombre VARCHAR(100) NOT NULL,
        SegundoNombre VARCHAR(100) NULL,
        ApellidoPaterno VARCHAR(100) NOT NULL,
        ApellidoMaterno VARCHAR(100) NULL,
        Cedula VARCHAR(50) NOT NULL UNIQUE,
        Telefono VARCHAR(20) NULL,
        Especialidad VARCHAR(200) NULL,
        Email VARCHAR(200) NULL,
        Activo BIT NOT NULL DEFAULT 1,
        FechaCreacion DATETIME NOT NULL DEFAULT GETDATE()
    );
END;
GO

IF OBJECT_ID('dbo.CatPacientes', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CatPacientes
    (
        Id INT IDENTITY(1, 1) PRIMARY KEY,
        PrimerNombre VARCHAR(100) NOT NULL,
        SegundoNombre VARCHAR(100) NULL,
        ApellidoPaterno VARCHAR(100) NOT NULL,
        ApellidoMaterno VARCHAR(100) NULL,
        Telefono VARCHAR(20) NULL,
        Activo BIT NOT NULL DEFAULT 1,
        FechaCreacion DATETIME NOT NULL DEFAULT GETDATE()
    );
END;
GO

IF OBJECT_ID('dbo.CatUsuarios', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CatUsuarios
    (
        Id INT IDENTITY(1, 1) PRIMARY KEY,
        Correo VARCHAR(200) NOT NULL UNIQUE,
        Password VARCHAR(500) NOT NULL,
        NombreCompleto VARCHAR(300) NOT NULL,
        IdMedico INT NULL,
        Activo BIT NOT NULL DEFAULT 1,
        FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_CatUsuarios_CatMedicos FOREIGN KEY (IdMedico) REFERENCES dbo.CatMedicos (Id)
    );
END;
GO

IF OBJECT_ID('dbo.Consultas', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Consultas
    (
        Id INT IDENTITY(1, 1) PRIMARY KEY,
        IdMedico INT NOT NULL,
        IdPaciente INT NOT NULL,
        Sintomas VARCHAR(MAX) NULL,
        Recomendaciones VARCHAR(MAX) NULL,
        Diagnostico VARCHAR(MAX) NULL,
        FechaConsulta DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_Consultas_CatMedicos FOREIGN KEY (IdMedico) REFERENCES dbo.CatMedicos (Id),
        CONSTRAINT FK_Consultas_CatPacientes FOREIGN KEY (IdPaciente) REFERENCES dbo.CatPacientes (Id)
    );
END;
GO

CREATE OR ALTER PROCEDURE dbo.procCatMedicosCon
    @pId INT = NULL,
    @pResultado BIT OUTPUT,
    @pMsg VARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        SELECT
            Id,
            PrimerNombre,
            SegundoNombre,
            ApellidoPaterno,
            ApellidoMaterno,
            Cedula,
            Telefono,
            Especialidad,
            Email,
            Activo,
            FechaCreacion
        FROM dbo.CatMedicos
        WHERE @pId IS NULL OR Id = @pId;

        SET @pResultado = 1;
        SET @pMsg = 'Consulta de médicos realizada correctamente.';
    END TRY
    BEGIN CATCH
        SET @pResultado = 0;
        SET @pMsg = ERROR_MESSAGE();
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE dbo.procCatMedicosIns
    @pPrimerNombre VARCHAR(100),
    @pSegundoNombre VARCHAR(100) = NULL,
    @pApellidoPaterno VARCHAR(100),
    @pApellidoMaterno VARCHAR(100) = NULL,
    @pCedula VARCHAR(50),
    @pTelefono VARCHAR(20) = NULL,
    @pEspecialidad VARCHAR(200) = NULL,
    @pEmail VARCHAR(200) = NULL,
    @pResultado BIT OUTPUT,
    @pMsg VARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO dbo.CatMedicos
        (
            PrimerNombre,
            SegundoNombre,
            ApellidoPaterno,
            ApellidoMaterno,
            Cedula,
            Telefono,
            Especialidad,
            Email
        )
        VALUES
        (
            @pPrimerNombre,
            @pSegundoNombre,
            @pApellidoPaterno,
            @pApellidoMaterno,
            @pCedula,
            @pTelefono,
            @pEspecialidad,
            @pEmail
        );

        DECLARE @NuevoId INT = SCOPE_IDENTITY();

        COMMIT TRANSACTION;

        SET @pResultado = 1;
        SET @pMsg = 'Médico registrado correctamente.';

        SELECT @NuevoId AS Id;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @pResultado = 0;
        SET @pMsg = ERROR_MESSAGE();
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE dbo.procCatMedicosUpd
    @pId INT,
    @pPrimerNombre VARCHAR(100),
    @pSegundoNombre VARCHAR(100) = NULL,
    @pApellidoPaterno VARCHAR(100),
    @pApellidoMaterno VARCHAR(100) = NULL,
    @pCedula VARCHAR(50),
    @pTelefono VARCHAR(20) = NULL,
    @pEspecialidad VARCHAR(200) = NULL,
    @pEmail VARCHAR(200) = NULL,
    @pResultado BIT OUTPUT,
    @pMsg VARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE dbo.CatMedicos
        SET
            PrimerNombre = @pPrimerNombre,
            SegundoNombre = @pSegundoNombre,
            ApellidoPaterno = @pApellidoPaterno,
            ApellidoMaterno = @pApellidoMaterno,
            Cedula = @pCedula,
            Telefono = @pTelefono,
            Especialidad = @pEspecialidad,
            Email = @pEmail
        WHERE Id = @pId;

        COMMIT TRANSACTION;

        SET @pResultado = 1;
        SET @pMsg = 'Médico actualizado correctamente.';

        SELECT @pId AS Id;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @pResultado = 0;
        SET @pMsg = ERROR_MESSAGE();
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE dbo.procCatMedicosDel
    @pId INT,
    @pResultado BIT OUTPUT,
    @pMsg VARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE dbo.CatMedicos
        SET Activo = 0
        WHERE Id = @pId;

        COMMIT TRANSACTION;

        SET @pResultado = 1;
        SET @pMsg = 'Médico desactivado correctamente.';

        SELECT @pId AS Id;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @pResultado = 0;
        SET @pMsg = ERROR_MESSAGE();
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE dbo.procCatPacientesCon
    @pId INT = NULL,
    @pResultado BIT OUTPUT,
    @pMsg VARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        SELECT
            Id,
            PrimerNombre,
            SegundoNombre,
            ApellidoPaterno,
            ApellidoMaterno,
            Telefono,
            Activo,
            FechaCreacion
        FROM dbo.CatPacientes
        WHERE @pId IS NULL OR Id = @pId;

        SET @pResultado = 1;
        SET @pMsg = 'Consulta de pacientes realizada correctamente.';
    END TRY
    BEGIN CATCH
        SET @pResultado = 0;
        SET @pMsg = ERROR_MESSAGE();
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE dbo.procCatPacientesIns
    @pPrimerNombre VARCHAR(100),
    @pSegundoNombre VARCHAR(100) = NULL,
    @pApellidoPaterno VARCHAR(100),
    @pApellidoMaterno VARCHAR(100) = NULL,
    @pTelefono VARCHAR(20) = NULL,
    @pResultado BIT OUTPUT,
    @pMsg VARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO dbo.CatPacientes
        (
            PrimerNombre,
            SegundoNombre,
            ApellidoPaterno,
            ApellidoMaterno,
            Telefono
        )
        VALUES
        (
            @pPrimerNombre,
            @pSegundoNombre,
            @pApellidoPaterno,
            @pApellidoMaterno,
            @pTelefono
        );

        DECLARE @NuevoId INT = SCOPE_IDENTITY();

        COMMIT TRANSACTION;

        SET @pResultado = 1;
        SET @pMsg = 'Paciente registrado correctamente.';

        SELECT @NuevoId AS Id;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @pResultado = 0;
        SET @pMsg = ERROR_MESSAGE();
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE dbo.procCatPacientesUpd
    @pId INT,
    @pPrimerNombre VARCHAR(100),
    @pSegundoNombre VARCHAR(100) = NULL,
    @pApellidoPaterno VARCHAR(100),
    @pApellidoMaterno VARCHAR(100) = NULL,
    @pTelefono VARCHAR(20) = NULL,
    @pResultado BIT OUTPUT,
    @pMsg VARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE dbo.CatPacientes
        SET
            PrimerNombre = @pPrimerNombre,
            SegundoNombre = @pSegundoNombre,
            ApellidoPaterno = @pApellidoPaterno,
            ApellidoMaterno = @pApellidoMaterno,
            Telefono = @pTelefono
        WHERE Id = @pId;

        COMMIT TRANSACTION;

        SET @pResultado = 1;
        SET @pMsg = 'Paciente actualizado correctamente.';

        SELECT @pId AS Id;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @pResultado = 0;
        SET @pMsg = ERROR_MESSAGE();
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE dbo.procCatPacientesDel
    @pId INT,
    @pResultado BIT OUTPUT,
    @pMsg VARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE dbo.CatPacientes
        SET Activo = 0
        WHERE Id = @pId;

        COMMIT TRANSACTION;

        SET @pResultado = 1;
        SET @pMsg = 'Paciente desactivado correctamente.';

        SELECT @pId AS Id;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @pResultado = 0;
        SET @pMsg = ERROR_MESSAGE();
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE dbo.procCatUsuariosCon
    @pId INT = NULL,
    @pResultado BIT OUTPUT,
    @pMsg VARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        SELECT
            Id,
            Correo,
            NombreCompleto,
            IdMedico,
            Activo,
            FechaCreacion
        FROM dbo.CatUsuarios
        WHERE @pId IS NULL OR Id = @pId;

        SET @pResultado = 1;
        SET @pMsg = 'Consulta de usuarios realizada correctamente.';
    END TRY
    BEGIN CATCH
        SET @pResultado = 0;
        SET @pMsg = ERROR_MESSAGE();
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE dbo.procCatUsuariosIns
    @pCorreo VARCHAR(200),
    @pPassword VARCHAR(500),
    @pNombreCompleto VARCHAR(300),
    @pIdMedico INT = NULL,
    @pResultado BIT OUTPUT,
    @pMsg VARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO dbo.CatUsuarios
        (
            Correo,
            Password,
            NombreCompleto,
            IdMedico
        )
        VALUES
        (
            @pCorreo,
            @pPassword,
            @pNombreCompleto,
            @pIdMedico
        );

        DECLARE @NuevoId INT = SCOPE_IDENTITY();

        COMMIT TRANSACTION;

        SET @pResultado = 1;
        SET @pMsg = 'Usuario registrado correctamente.';

        SELECT @NuevoId AS Id;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @pResultado = 0;
        SET @pMsg = ERROR_MESSAGE();
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE dbo.procCatUsuariosUpd
    @pId INT,
    @pCorreo VARCHAR(200),
    @pPassword VARCHAR(500),
    @pNombreCompleto VARCHAR(300),
    @pIdMedico INT = NULL,
    @pResultado BIT OUTPUT,
    @pMsg VARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE dbo.CatUsuarios
        SET
            Correo = @pCorreo,
            Password = @pPassword,
            NombreCompleto = @pNombreCompleto,
            IdMedico = @pIdMedico
        WHERE Id = @pId;

        COMMIT TRANSACTION;

        SET @pResultado = 1;
        SET @pMsg = 'Usuario actualizado correctamente.';

        SELECT @pId AS Id;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @pResultado = 0;
        SET @pMsg = ERROR_MESSAGE();
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE dbo.procCatUsuariosDel
    @pId INT,
    @pResultado BIT OUTPUT,
    @pMsg VARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE dbo.CatUsuarios
        SET Activo = 0
        WHERE Id = @pId;

        COMMIT TRANSACTION;

        SET @pResultado = 1;
        SET @pMsg = 'Usuario desactivado correctamente.';

        SELECT @pId AS Id;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @pResultado = 0;
        SET @pMsg = ERROR_MESSAGE();
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE dbo.procConsultasCon
    @pId INT = NULL,
    @pResultado BIT OUTPUT,
    @pMsg VARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        SELECT
            Id,
            IdMedico,
            IdPaciente,
            Sintomas,
            Recomendaciones,
            Diagnostico,
            FechaConsulta
        FROM dbo.Consultas
        WHERE @pId IS NULL OR Id = @pId;

        SET @pResultado = 1;
        SET @pMsg = 'Consulta de consultas realizada correctamente.';
    END TRY
    BEGIN CATCH
        SET @pResultado = 0;
        SET @pMsg = ERROR_MESSAGE();
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE dbo.procConsultasIns
    @pIdMedico INT,
    @pIdPaciente INT,
    @pSintomas VARCHAR(MAX) = NULL,
    @pRecomendaciones VARCHAR(MAX) = NULL,
    @pDiagnostico VARCHAR(MAX) = NULL,
    @pFechaConsulta DATETIME = NULL,
    @pResultado BIT OUTPUT,
    @pMsg VARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO dbo.Consultas
        (
            IdMedico,
            IdPaciente,
            Sintomas,
            Recomendaciones,
            Diagnostico,
            FechaConsulta
        )
        VALUES
        (
            @pIdMedico,
            @pIdPaciente,
            @pSintomas,
            @pRecomendaciones,
            @pDiagnostico,
            ISNULL(@pFechaConsulta, GETDATE())
        );

        DECLARE @NuevoId INT = SCOPE_IDENTITY();

        COMMIT TRANSACTION;

        SET @pResultado = 1;
        SET @pMsg = 'Consulta registrada correctamente.';

        SELECT @NuevoId AS Id;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @pResultado = 0;
        SET @pMsg = ERROR_MESSAGE();
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE dbo.procConsultasUpd
    @pId INT,
    @pIdMedico INT,
    @pIdPaciente INT,
    @pSintomas VARCHAR(MAX) = NULL,
    @pRecomendaciones VARCHAR(MAX) = NULL,
    @pDiagnostico VARCHAR(MAX) = NULL,
    @pFechaConsulta DATETIME = NULL,
    @pResultado BIT OUTPUT,
    @pMsg VARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE dbo.Consultas
        SET
            IdMedico = @pIdMedico,
            IdPaciente = @pIdPaciente,
            Sintomas = @pSintomas,
            Recomendaciones = @pRecomendaciones,
            Diagnostico = @pDiagnostico,
            FechaConsulta = ISNULL(@pFechaConsulta, FechaConsulta)
        WHERE Id = @pId;

        COMMIT TRANSACTION;

        SET @pResultado = 1;
        SET @pMsg = 'Consulta actualizada correctamente.';

        SELECT @pId AS Id;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @pResultado = 0;
        SET @pMsg = ERROR_MESSAGE();
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE dbo.procConsultasDel
    @pId INT,
    @pResultado BIT OUTPUT,
    @pMsg VARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        DELETE FROM dbo.Consultas
        WHERE Id = @pId;

        COMMIT TRANSACTION;

        SET @pResultado = 1;
        SET @pMsg = 'Consulta eliminada correctamente.';

        SELECT @pId AS Id;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @pResultado = 0;
        SET @pMsg = ERROR_MESSAGE();
    END CATCH
END;
GO
