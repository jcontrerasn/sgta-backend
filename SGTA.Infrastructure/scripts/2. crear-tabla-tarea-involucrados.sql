USE SgtaDB;
GO

CREATE TABLE dbo.TareaInvolucrados (
    IdTareaInvolucrado INT IDENTITY(1,1) PRIMARY KEY,

    IdTarea INT NOT NULL,

    Usuario VARCHAR(100) NOT NULL,

    FechaAsignacion DATETIME2(0) NOT NULL
        DEFAULT CONVERT(
            DATETIME2(0),
            SYSUTCDATETIME() AT TIME ZONE 'UTC'
                AT TIME ZONE 'SA Pacific Standard Time'
        ),

    CONSTRAINT FK_TareaInvolucrados_Tareas
        FOREIGN KEY (IdTarea)
        REFERENCES dbo.Tareas(IdTarea),

    CONSTRAINT UQ_TareaInvolucrados_Tarea_Usuario
        UNIQUE (IdTarea, Usuario),

    CONSTRAINT CK_TareaInvolucrados_Usuario
        CHECK (LEN(TRIM(Usuario)) > 0)
);
GO