DECLARE @ProfileMail VARCHAR(50);
SELECT TOP (1) @ProfileMail = ValorTexto FROM dbo.SM_PARAMETRO WITH (NOLOCK) WHERE tipo = 'ProfileMail';

-- Este bloque utiliza @PidTarea y @IdAnotacion del procedimiento de anotaciones.
DECLARE @usuarioCreadorCorreo VARCHAR(100);
DECLARE @asuntoCorreo NVARCHAR(250);
DECLARE @tituloCorreo NVARCHAR(120);
DECLARE @usuarioAnotacionCorreo VARCHAR(100);
DECLARE @fechaAnotacionCorreo VARCHAR(19);
DECLARE @descripcionAnotacionCorreo NVARCHAR(MAX);
DECLARE @esComentarioCierreCorreo BIT;
DECLARE @anotacionesCorreo NVARCHAR(MAX);
DECLARE @paraCorreo NVARCHAR(MAX);
DECLARE @copiaCorreo NVARCHAR(MAX);
DECLARE @tableHTMLCorreo NVARCHAR(MAX);

SELECT TOP (1)
    @usuarioCreadorCorreo = TT.UsuarioCreador,
    @paraCorreo = TU.Correo,
    @tituloCorreo = ISNULL(TT.Titulo, '')
FROM dbo.TRACKIT_Tareas TT WITH (NOLOCK)
LEFT JOIN dbo.TS_USUARIO TU WITH (NOLOCK)
    ON TU.Usuario = TT.UsuarioCreador
WHERE TT.IdTarea = @PidTarea;

SELECT @copiaCorreo = STUFF
(
    (
        SELECT ';' + TU.Correo
        FROM dbo.TRACKIT_TareaInvolucrados TI
        INNER JOIN dbo.TS_USUARIO TU
            ON TI.Usuario = TU.Usuario
        WHERE TI.IdTarea = @PidTarea
            AND TI.Usuario <> ISNULL(@usuarioCreadorCorreo, '')
            AND TU.Correo IS NOT NULL
        FOR XML PATH(''), TYPE
    ).value('.', 'VARCHAR(MAX)'),
    1,
    1,
    ''
);

SELECT TOP (1)
    @usuarioAnotacionCorreo = TA.UsuarioAutor,
    @fechaAnotacionCorreo = CONVERT(VARCHAR(19), TA.FechaCreacion, 120),
    @descripcionAnotacionCorreo = ISNULL(TA.Comentario, ''),
    @esComentarioCierreCorreo = TA.EsComentarioCierre
FROM dbo.TRACKIT_TareaAnotaciones TA WITH (NOLOCK)
WHERE TA.IdAnotacion = @IdAnotacion;

SET @asuntoCorreo = 'Tarea: ' + ISNULL(@tituloCorreo, '');

SELECT @anotacionesCorreo =
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
            ISNULL(CONVERT(NVARCHAR(MAX), TA.Comentario), '') +
            '</strong></td>' +
        '</tr>' +
        '<tr>' +
        '<td style="text-align: left;"> Es comentario de cierre: <strong>' +
            CASE WHEN TA.EsComentarioCierre = 1 THEN 'Si' ELSE 'No' END +
            '</strong></td>' +
        '</tr>' +
        '<tr><td><br /></td></tr>'
    FROM dbo.TRACKIT_TareaAnotaciones TA WITH (NOLOCK)
    WHERE TA.IdTarea = @PidTarea
    ORDER BY TA.FechaCreacion, TA.IdAnotacion
    FOR XML PATH(''), TYPE
).value('.', 'NVARCHAR(MAX)');

SET @anotacionesCorreo = ISNULL
(
    NULLIF(@anotacionesCorreo, ''),
    '<tr><td style="text-align: left;">No hay anotaciones registradas.</td></tr>'
);

SET @tableHTMLCorreo = '<table class="tabla" width="100%">' +
    '<tbody>' +
    '<tr><td style="text-align: left;">Estimados Sres.</td></tr>' +
    '<tr><td><br /></td></tr>' +
    '<tr><td style="text-align: left;">Se ha agregado una nueva anotación a la tarea <strong>' +
        ISNULL(@tituloCorreo, '') + '</strong>.</td></tr>' +
    '<tr><td><br /></td></tr>' +
    '<tr><td style="text-align: left;">Usuario: <strong>' +
        ISNULL(@usuarioAnotacionCorreo, '') + '</strong></td></tr>' +
    '<tr><td style="text-align: left;">Fecha: <strong>' +
        ISNULL(@fechaAnotacionCorreo, '') + '</strong></td></tr>' +
    '<tr><td style="text-align: left;">Descripción: <strong>' +
        ISNULL(@descripcionAnotacionCorreo, '') + '</strong></td></tr>' +
    '<tr><td style="text-align: left;">Es comentario de cierre: <strong>' +
        CASE WHEN ISNULL(@esComentarioCierreCorreo, 0) = 1 THEN 'Si' ELSE 'No' END +
        '</strong></td></tr>' +
    '<tr><td><br /></td></tr>' +
    '<tr><td style="text-align: left;">Anotaciones:</td></tr>' +
    @anotacionesCorreo +
    '<tr><td><br /></td></tr>' +
    '<tr><td style="text-align: left;">Atte.</td></tr>' +
    '<tr><td style="text-align: left;">Gestion y control de servicios TI (TrackIT)</td></tr>' +
    '</tbody></table>';

IF COALESCE(@paraCorreo, '') <> ''
BEGIN
    EXEC msdb.dbo.sp_send_dbmail
        @profile_name = @ProfileMail,
        @recipients = @paraCorreo,
        @copy_recipients = @copiaCorreo,
        @subject = @asuntoCorreo,
        @body = @tableHTMLCorreo,
        @body_format = 'HTML';
END;
