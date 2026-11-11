USE SM_PRUEBA

CREATE TABLE dbo.TRACKIT_Tareas (
    IdTarea INT IDENTITY(1,1) PRIMARY KEY,

    Titulo NVARCHAR(120) NOT NULL,

    Descripcion NVARCHAR(MAX) NOT NULL,

    FechaVencimiento DATE NOT NULL,

    UsuarioCreador VARCHAR(100) NOT NULL,

    FechaCreacion DATETIME2(0) NOT NULL
        DEFAULT GETDATE(),

    FechaModificacion DATETIME2(0) NULL,

    FechaCierre DATETIME2(0) NULL,

    UsuarioCierre VARCHAR(100) NULL,

    FechaEliminacion DATETIME2(0) NULL,

    UsuarioEliminacion VARCHAR(100) NULL,

    Estado VARCHAR(20) NULL
);
