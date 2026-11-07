USE SgtaDB;
GO

CREATE TABLE dbo.TareaAnotaciones (
    IdAnotacion INT IDENTITY(1,1) PRIMARY KEY,

    IdTarea INT NOT NULL,

    UsuarioAutor VARCHAR(100) NOT NULL,

    Comentario NVARCHAR(MAX) NOT NULL,

    EsComentarioCierre BIT NOT NULL
        DEFAULT 0,

    FechaCreacion DATETIME2(0) NOT NULL
        DEFAULT CONVERT(
            DATETIME2(0),
            SYSUTCDATETIME() AT TIME ZONE 'UTC'
                AT TIME ZONE 'SA Pacific Standard Time'
        ),

    CONSTRAINT FK_TareaAnotaciones_Tareas
        FOREIGN KEY (IdTarea)
        REFERENCES dbo.Tareas(IdTarea),

    CONSTRAINT CK_TareaAnotaciones_Comentario
        CHECK (LEN(TRIM(Comentario)) > 0),

    CONSTRAINT CK_TareaAnotaciones_UsuarioAutor
        CHECK (LEN(TRIM(UsuarioAutor)) > 0)
);
GO