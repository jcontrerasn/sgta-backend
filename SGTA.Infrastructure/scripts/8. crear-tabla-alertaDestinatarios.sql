CREATE TABLE dbo.TRACKIT_AlertaDestinatarios
(
    IdDestinatario INT IDENTITY(1,1) PRIMARY KEY,

    IdAlerta INT NOT NULL,

    Usuario VARCHAR(100) NULL,

    Correo VARCHAR(250) NOT NULL,

    TipoDestinatario VARCHAR(10) NOT NULL,
    -- PARA, CC

    EstadoEnvio VARCHAR(20) NOT NULL
        DEFAULT 'PENDIENTE',

    FechaEncolada DATETIME2(0) NULL,

    ErrorDetalle NVARCHAR(2000) NULL,

    CONSTRAINT FK_AlertaDestinatarios_Alertas
        FOREIGN KEY (IdAlerta)
        REFERENCES dbo.TRACKIT_Alertas(IdAlerta)
);
GO