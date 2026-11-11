CREATE TABLE dbo.TRACKIT_TareaHistorial
(
    IdHistorial INT IDENTITY(1,1) PRIMARY KEY,

    IdTarea INT NOT NULL,

    TipoEvento VARCHAR(40) NOT NULL,
    -- CREADA, ACTUALIZADA, ANOTACION_AGREGADA,
    -- COMPLETADA, ELIMINADA

    Usuario VARCHAR(100) NOT NULL,

    FechaEvento DATETIME2(0) NOT NULL
        DEFAULT GETDATE(),

    Descripcion NVARCHAR(MAX) NULL,

    EstadoAnterior VARCHAR(20) NULL,

    EstadoNuevo VARCHAR(20) NULL,

    IdAnotacion INT NULL,

    CONSTRAINT FK_TareaHistorial_Tarea
        FOREIGN KEY (IdTarea)
        REFERENCES dbo.TRACKIT_Tareas(IdTarea)
);

ALTER TABLE dbo.TRACKIT_TareaHistorial
ADD
    CampoModificado VARCHAR(50) NULL,
    ValorAnterior NVARCHAR(MAX) NULL,
    ValorNuevo NVARCHAR(MAX) NULL;
