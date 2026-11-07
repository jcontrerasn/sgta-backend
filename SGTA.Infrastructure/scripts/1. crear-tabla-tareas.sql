use SgtaDB

CREATE TABLE dbo.Tareas (
    IdTarea INT IDENTITY(1,1) PRIMARY KEY,

    Titulo NVARCHAR(120) NOT NULL,

    Descripcion NVARCHAR(MAX) NOT NULL,

    FechaVencimiento DATE NOT NULL,

    UsuarioCreador VARCHAR(100) NOT NULL,

    FechaCreacion DATETIME2(0) NOT NULL
        DEFAULT CONVERT(
            DATETIME2(7),
            SYSUTCDATETIME() AT TIME ZONE 'UTC'
                AT TIME ZONE 'SA Pacific Standard Time'
        ),

    FechaModificacion DATETIME2(0) NULL,

    FechaCierre DATETIME2(0) NULL,

    FechaEliminacion DATETIME2(0) NULL,

    IdUsuarioEliminacion INT NULL
);
GO