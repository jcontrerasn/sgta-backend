CREATE TABLE dbo.TRACKIT_TareaAnotaciones (
    IdAnotacion INT IDENTITY(1,1) PRIMARY KEY,

    IdTarea INT NOT NULL,

    UsuarioAutor VARCHAR(100) NOT NULL,

    Comentario NVARCHAR(MAX) NOT NULL,

    EsComentarioCierre BIT NOT NULL
        DEFAULT 0,

	Adjuntos NVARCHAR(MAX) NULL,

    FechaCreacion DATETIME2(0) NOT NULL
        DEFAULT GETDATE(),

    CONSTRAINT FK_TareaAnotaciones_Tareas
        FOREIGN KEY (IdTarea)
        REFERENCES dbo.TRACKIT_Tareas(IdTarea),

);