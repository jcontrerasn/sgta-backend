USE SgtaDB;
GO

CREATE TABLE dbo.Roles (
    IdRol INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(50) NOT NULL UNIQUE,
    Descripcion NVARCHAR(200) NULL,
    Activo BIT NOT NULL DEFAULT 1
);
GO

INSERT INTO dbo.Roles (Nombre)
VALUES
(N'Administrador'),
(N'Ingeniero_Red'),
(N'Tecnico_Soporte'),
(N'Responsable_HW_SW')

