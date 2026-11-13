-- Este bloque se debe pegar dentro del procedimiento que recibe @PidTarea.

DECLARE @ProfileMail VARCHAR(50);
SELECT TOP (1) @ProfileMail = ValorTexto
FROM dbo.SM_PARAMETRO WITH (NOLOCK)
WHERE tipo = 'ProfileMail';

DECLARE @idTarea INT = @PidTarea;
DECLARE @IdAlerta INT;
DECLARE @usuarioCreador VARCHAR(100);
DECLARE @asunto NVARCHAR(250);
DECLARE @titulo NVARCHAR(120);
DECLARE @fechaVencimiento VARCHAR(10);
DECLARE @descripcion NVARCHAR(MAX);
DECLARE @nombreCreador VARCHAR(200);
DECLARE @anotaciones NVARCHAR(MAX);
DECLARE @para NVARCHAR(MAX);
DECLARE @copia NVARCHAR(MAX);
DECLARE @tableHTML NVARCHAR(MAX);

SELECT TOP (1)
    @usuarioCreador = TT.UsuarioCreador,
    @para = TU.Correo
FROM dbo.TRACKIT_Tareas TT WITH (NOLOCK)
LEFT JOIN dbo.TS_USUARIO TU WITH (NOLOCK)
    ON TU.Usuario = TT.UsuarioCreador
WHERE TT.IdTarea = @idTarea;

SELECT @copia = STUFF
(
    (
        SELECT ';' + TU.Correo
        FROM dbo.TRACKIT_TareaInvolucrados TI
        INNER JOIN dbo.TS_USUARIO TU
            ON TI.Usuario = TU.Usuario
        WHERE TI.IdTarea = @idTarea
            AND TI.Usuario <> ISNULL(@usuarioCreador, '')
            AND TU.Correo IS NOT NULL
        FOR XML PATH(''), TYPE
    ).value('.', 'VARCHAR(MAX)'),
    1,
    1,
    ''
);

SELECT TOP (1)
    @titulo = ISNULL(TT.Titulo, N''),
    @fechaVencimiento = ISNULL(CONVERT(VARCHAR(10), TT.FechaVencimiento, 23), ''),
    @descripcion = ISNULL(TT.Descripcion, N''),
    @nombreCreador = ISNULL(TU.NombreCompleto, '')
FROM dbo.TRACKIT_Tareas TT WITH (NOLOCK)
LEFT JOIN dbo.TS_USUARIO TU WITH (NOLOCK)
    ON TT.UsuarioCreador = TU.Usuario
WHERE TT.IdTarea = @idTarea;

SET @asunto = N'Tarea: ' + ISNULL(@titulo, N'');

SELECT @anotaciones =
(
    SELECT
        '<tr>' +
        '<td style="text-align: left;"> Usuario: <strong>' +
            ISNULL(TA.UsuarioAutor, '') +
            '</strong> - Fecha: <strong>' +
            ISNULL(CONVERT(VARCHAR(19), TA.FechaCreacion, 120), '') +
            '</strong></td>' +
        '</tr>' +
        '<tr>' +
        '<td style="text-align: left;"> Comentario: <strong>' +
            ISNULL(CONVERT(NVARCHAR(MAX), TA.Comentario), N'') +
            '</strong></td>' +
        '</tr>' +
        '<tr>' +
        '<td style="text-align: left;"> Es comentario de cierre: <strong>' +
            CASE WHEN TA.EsComentarioCierre = 1 THEN 'Si' ELSE 'No' END +
            '</strong></td>' +
        '</tr>' +
        '<tr><td><br /></td></tr>'
    FROM dbo.TRACKIT_TareaAnotaciones TA
    WHERE TA.IdTarea = @idTarea
    ORDER BY TA.FechaCreacion, TA.IdAnotacion
    FOR XML PATH(''), TYPE
).value('.', 'NVARCHAR(MAX)');

SET @anotaciones = ISNULL
(
    NULLIF(@anotaciones, N''),
    N'<tr><td style="text-align: left;">No hay anotaciones registradas.</td></tr>'
);

SET @tableHTML = N'<table class="tabla" width="100%">' +
    N'<tbody>' +
    N'<tr><td style="text-align: left;">Estimado/a Sr. /Sra. <strong>' +
        ISNULL(@nombreCreador, '') + N'</strong></td></tr>' +
    N'<tr><td><br /></td></tr>' +
    N'<tr><td style="text-align: left;">Se notifica que quedan <strong>5 meses</strong> para el vencimiento de la siguiente tarea:</td></tr>' +
    N'<tr><td style="text-align: left;">Titulo de la tarea: <strong>' +
        ISNULL(@titulo, N'') + N'</strong></td></tr>' +
    N'<tr><td style="text-align: left;">Fecha de vencimiento: <strong>' +
        ISNULL(@fechaVencimiento, '') + N'</strong></td></tr>' +
    N'<tr><td style="text-align: left;">Descripcion: <strong>' +
        ISNULL(@descripcion, N'') + N'</strong></td></tr>' +
    N'<tr><td><br /></td></tr>' +
    N'<tr><td style="text-align: left;">Anotaciones:</td></tr>' +
    @anotaciones +
    N'<tr><td><br /></td></tr>' +
    N'<tr><td style="text-align: left;">Atte.</td></tr>' +
    N'<tr><td style="text-align: left;">Gestion y control de servicios TI (TrackIT)</td></tr>' +
    N'</tbody></table>';

INSERT INTO dbo.TRACKIT_Alertas
(
    IdTarea,
    TipoAlerta,
    TituloTarea,
    Asunto,
    UsuarioCreador,
    EstadoEnvio
)
VALUES
(
    @idTarea,
    'TAREA',
    ISNULL(@titulo, N''),
    ISNULL(@asunto, N''),
    ISNULL(@usuarioCreador, ''),
    'PENDIENTE'
);

SET @IdAlerta = CONVERT(INT, SCOPE_IDENTITY());

IF COALESCE(@para, '') = ''
BEGIN
    UPDATE dbo.TRACKIT_Alertas
    SET EstadoEnvio = 'ERROR',
        ErrorDetalle = N'La tarea no tiene un correo de destinatario principal.'
    WHERE IdAlerta = @IdAlerta;
END
ELSE
BEGIN
    BEGIN TRY
        INSERT INTO dbo.TRACKIT_AlertaDestinatarios
        (
            IdAlerta,
            Usuario,
            Correo,
            TipoDestinatario,
            EstadoEnvio
        )
        VALUES
        (
            @IdAlerta,
            @usuarioCreador,
            @para,
            'PARA',
            'PENDIENTE'
        );

        INSERT INTO dbo.TRACKIT_AlertaDestinatarios
        (
            IdAlerta,
            Usuario,
            Correo,
            TipoDestinatario,
            EstadoEnvio
        )
        SELECT
            @IdAlerta,
            TI.Usuario,
            TU.Correo,
            'CC',
            'PENDIENTE'
        FROM dbo.TRACKIT_TareaInvolucrados TI
        INNER JOIN dbo.TS_USUARIO TU
            ON TI.Usuario = TU.Usuario
        WHERE TI.IdTarea = @idTarea
            AND TI.Usuario <> ISNULL(@usuarioCreador, '')
            AND TU.Correo IS NOT NULL;

        EXEC msdb.dbo.sp_send_dbmail
            @profile_name = @ProfileMail,
            @recipients = @para,
            @copy_recipients = @copia,
            @subject = @asunto,
            @body = @tableHTML,
            @body_format = 'HTML';

        UPDATE dbo.TRACKIT_Alertas
        SET EstadoEnvio = 'ENCOLADA',
            FechaEncolada = GETDATE()
        WHERE IdAlerta = @IdAlerta;

        UPDATE dbo.TRACKIT_AlertaDestinatarios
        SET EstadoEnvio = 'ENCOLADA',
            FechaEncolada = GETDATE()
        WHERE IdAlerta = @IdAlerta;
    END TRY
    BEGIN CATCH
        UPDATE dbo.TRACKIT_Alertas
        SET EstadoEnvio = 'ERROR',
            ErrorDetalle = ERROR_MESSAGE()
        WHERE IdAlerta = @IdAlerta;

        UPDATE dbo.TRACKIT_AlertaDestinatarios
        SET EstadoEnvio = 'ERROR',
            ErrorDetalle = ERROR_MESSAGE()
        WHERE IdAlerta = @IdAlerta;

    END CATCH;
END;
