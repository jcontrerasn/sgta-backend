USE SM_PRUEBA;
GO

CREATE TABLE dbo.TRACKIT_UsuarioRoles (
    IdUsuarioRol INT IDENTITY(1,1) PRIMARY KEY,

    Usuario VARCHAR(100) NOT NULL,

    IdRol INT NOT NULL,

    Activo BIT NOT NULL DEFAULT 1,

    CONSTRAINT FK_UsuarioRoles_Roles
        FOREIGN KEY (IdRol)
        REFERENCES dbo.TRACKIT_Roles(IdRol),

    CONSTRAINT UQ_UsuarioRoles
        UNIQUE (Usuario, IdRol)
);
GO




INSERT INTO dbo.TRACKIT_UsuarioRoles (Usuario, IdRol)
SELECT
    'BBARBARAN',
    IdRol
FROM dbo.TRACKIT_Roles
WHERE Nombre = N'Administrador';

INSERT INTO dbo.TRACKIT_UsuarioRoles (Usuario, IdRol)
SELECT
    'LRIVERA',
    IdRol
FROM dbo.TRACKIT_Roles
WHERE Nombre = N'Ingeniero_Red';

INSERT INTO dbo.TRACKIT_UsuarioRoles (Usuario, IdRol)
SELECT
    'RBUSTAMANTEZ',
    IdRol
FROM dbo.TRACKIT_Roles
WHERE Nombre = N'Tecnico_Soporte';

INSERT INTO dbo.TRACKIT_UsuarioRoles (Usuario, IdRol)
SELECT
    'JROSALESV',
    IdRol
FROM dbo.TRACKIT_Roles
WHERE Nombre = N'Tecnico_Soporte';

INSERT INTO dbo.TRACKIT_UsuarioRoles (Usuario, IdRol)
SELECT
    'JAGUIRREH',
    IdRol
FROM dbo.TRACKIT_Roles
WHERE Nombre = N'Responsable_HW_SW';
