CREATE TABLE dbo.TRACKIT_Alertas
(
    IdAlerta INT IDENTITY(1,1) PRIMARY KEY,

    IdTarea INT NOT NULL,

    TipoAlerta VARCHAR(40) NOT NULL,

    TituloTarea NVARCHAR(120) NOT NULL,

    Asunto NVARCHAR(250) NOT NULL,

    UsuarioCreador VARCHAR(100) NOT NULL,

    EstadoEnvio VARCHAR(20) NOT NULL
        DEFAULT 'PENDIENTE',
    -- PENDIENTE, ENCOLADA, ERROR

    FechaCreacion DATETIME2(0) NOT NULL
        DEFAULT GETDATE(),

    FechaEncolada DATETIME2(0) NULL,

    ErrorDetalle NVARCHAR(2000) NULL,

    CONSTRAINT FK_Alertas_Tareas
        FOREIGN KEY (IdTarea)
        REFERENCES dbo.TRACKIT_Tareas(IdTarea)
);
GO