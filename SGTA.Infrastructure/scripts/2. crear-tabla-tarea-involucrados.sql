USE SM_PRUEBA;

CREATE TABLE dbo.TRACKIT_TareaInvolucrados (
    IdTareaInvolucrado INT IDENTITY(1,1) PRIMARY KEY,

    IdTarea INT NOT NULL,

    Usuario VARCHAR(100) NOT NULL,

    FechaAsignacion DATETIME2(0) NOT NULL
        DEFAULT GETDATE(),

    CONSTRAINT FK_TareaInvolucrados_Tareas
        FOREIGN KEY (IdTarea)
        REFERENCES dbo.TRACKIT_Tareas(IdTarea),

    CONSTRAINT UQ_TareaInvolucrados_Tarea_Usuario
        UNIQUE (IdTarea, Usuario),

    CONSTRAINT CK_TareaInvolucrados_Usuario
		CHECK (LEN(LTRIM(RTRIM(Usuario))) > 0)
);

