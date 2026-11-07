CREATE TABLE dbo.UsuarioRoles (
    IdUsuarioRol INT IDENTITY(1,1) PRIMARY KEY,

    Usuario VARCHAR(100) NOT NULL,

    IdRol INT NOT NULL,

    Activo BIT NOT NULL DEFAULT 1,

    CONSTRAINT FK_UsuarioRoles_Roles
        FOREIGN KEY (IdRol)
        REFERENCES dbo.Roles(IdRol),

    CONSTRAINT UQ_UsuarioRoles
        UNIQUE (Usuario, IdRol)
);
GO




USE SgtaDB
INSERT INTO dbo.UsuarioRoles (Usuario, IdRol)
SELECT
    'BBARBARAN',
    IdRol
FROM dbo.Roles
WHERE Nombre = N'Administrador';

INSERT INTO dbo.UsuarioRoles (Usuario, IdRol)
SELECT
    'LRIVERA',
    IdRol
FROM dbo.Roles
WHERE Nombre = N'Ingeniero_Red';

INSERT INTO dbo.UsuarioRoles (Usuario, IdRol)
SELECT
    'RBUSTAMANTEZ',
    IdRol
FROM dbo.Roles
WHERE Nombre = N'Tecnico_Soporte';

INSERT INTO dbo.UsuarioRoles (Usuario, IdRol)
SELECT
    'JROSALESV',
    IdRol
FROM dbo.Roles
WHERE Nombre = N'Tecnico_Soporte';

INSERT INTO dbo.UsuarioRoles (Usuario, IdRol)
SELECT
    'JAGUIRREH',
    IdRol
FROM dbo.Roles
WHERE Nombre = N'Responsable_HW_SW';
