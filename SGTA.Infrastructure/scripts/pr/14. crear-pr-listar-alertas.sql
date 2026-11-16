ALTER PROCEDURE dbo.pr_trackit_ListarHistorialAlertas
AS
BEGIN
    SET NOCOUNT ON;

    -- Primer resultado: alertas registradas.
    SELECT
        A.IdAlerta,
        A.IdTarea,
        A.TipoAlerta,
        A.TituloTarea,
        A.Asunto,
        TU.NombreCompleto as UsuarioCreador,
        T.FechaCreacion AS FechaCreacionTarea,
        T.FechaVencimiento AS FechaVencimientoTarea,
        A.EstadoEnvio,
        A.FechaCreacion,
        A.FechaEncolada,
        A.ErrorDetalle
    FROM dbo.TRACKIT_Alertas A
    INNER JOIN dbo.TRACKIT_Tareas T
        ON T.IdTarea = A.IdTarea
    INNER JOIN dbo.TS_USUARIO TU
        ON A.UsuarioCreador = TU.Usuario
    ORDER BY
        A.FechaCreacion DESC,
        A.IdAlerta DESC;

    -- Segundo resultado: destinatarios de cada alerta.
    SELECT
        AD.IdDestinatario,
        AD.IdAlerta,
        TU.NombreCompleto as Usuario,
        AD.Correo,
        AD.TipoDestinatario,
        AD.EstadoEnvio,
        AD.FechaEncolada,
        AD.ErrorDetalle
    FROM dbo.TRACKIT_AlertaDestinatarios AD inner join TS_USUARIO TU on AD.Usuario = TU.Usuario
    ORDER BY
        AD.IdAlerta DESC,
        AD.IdDestinatario;
END;
GO
