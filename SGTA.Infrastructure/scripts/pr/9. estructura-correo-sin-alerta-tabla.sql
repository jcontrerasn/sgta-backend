DECLARE @ProfileMail VARCHAR(50);
SELECT TOP (1) @ProfileMail = ValorTexto FROM dbo.SM_PARAMETRO WITH (NOLOCK) WHERE tipo = 'ProfileMail';

--DECLARE @idTarea INT = @PidTarea;
DECLARE @idTarea INT = 19;
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
    @titulo = ISNULL(TT.Titulo, ''),
    @fechaVencimiento = ISNULL(CONVERT(VARCHAR(10), TT.FechaVencimiento, 23), ''),
    @descripcion = ISNULL(TT.Descripcion, ''),
    @nombreCreador = ISNULL(TU.NombreCompleto, '')
FROM dbo.TRACKIT_Tareas TT WITH (NOLOCK)
LEFT JOIN dbo.TS_USUARIO TU WITH (NOLOCK)
    ON TT.UsuarioCreador = TU.Usuario
WHERE TT.IdTarea = @idTarea;

SET @asunto = 'Tarea: ' + ISNULL(@titulo, '');

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
            ISNULL(CONVERT(NVARCHAR(MAX), TA.Comentario), '') +
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
    NULLIF(@anotaciones, ''),
    '<tr><td style="text-align: left;">No hay anotaciones registradas.</td></tr>'
);

SET @tableHTML = '<table class="tabla" width="100%">' +
    '<tbody>' +
    '<tr><td style="text-align: left;">Estimado Sr. <strong>' +
        ISNULL(@nombreCreador, '') + '</strong></td></tr>' +
    '<tr><td><br /></td></tr>' +
    '<tr><td style="text-align: left;">Se notifica que quedan <strong>5 meses</strong> para el vencimiento de la siguiente tarea:</td></tr>' +
    '<tr><td style="text-align: left;">Titulo de la tarea: <strong>' +
        ISNULL(@titulo, '') + '</strong></td></tr>' +
    '<tr><td style="text-align: left;">Fecha de vencimiento: <strong>' +
        ISNULL(@fechaVencimiento, '') + '</strong></td></tr>' +
    '<tr><td style="text-align: left;">Descripcion: <strong>' +
        ISNULL(@descripcion, '') + '</strong></td></tr>' +
    '<tr><td><br /></td></tr>' +
    '<tr><td style="text-align: left;">Anotaciones:</td></tr>' +
    @anotaciones +
    '<tr><td><br /></td></tr>' +
    '<tr><td style="text-align: left;">Atte.</td></tr>' +
    '<tr><td style="text-align: left;">Gestion y control de servicios TI (TrackIT)</td></tr>' +
    '</tbody></table>';

IF COALESCE(@para, '') <> ''
BEGIN
    EXEC msdb.dbo.sp_send_dbmail
        @profile_name = @ProfileMail,
        @recipients = @para,
        @copy_recipients = @copia,
        @subject = @asunto,
        @body = @tableHTML,
        @body_format = 'HTML';
END;
