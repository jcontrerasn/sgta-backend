DECLARE @ProfileMail VARCHAR(50);
SELECT TOP (1) @ProfileMail = ValorTexto FROM dbo.SM_PARAMETRO WITH (NOLOCK) WHERE tipo = 'ProfileMail';

-- Este bloque utiliza @PidTarea y @PusuarioCreador del procedimiento de eliminación.
DECLARE @usuarioCreadorCorreo VARCHAR(100);
DECLARE @usuarioEliminacionCorreo VARCHAR(100);
DECLARE @fechaEliminacionCorreo VARCHAR(19);
DECLARE @asuntoCorreo NVARCHAR(250);
DECLARE @tituloCorreo NVARCHAR(120);
DECLARE @fechaVencimientoCorreo VARCHAR(10);
DECLARE @descripcionCorreo NVARCHAR(MAX);
DECLARE @listaUsuariosCorreo NVARCHAR(MAX);
DECLARE @anotacionesCorreo NVARCHAR(MAX);
DECLARE @paraCorreo NVARCHAR(MAX);
DECLARE @copiaCorreo NVARCHAR(MAX);
DECLARE @tableHTMLCorreo NVARCHAR(MAX);

SELECT TOP (1)
    @usuarioCreadorCorreo = TT.UsuarioCreador,
    @usuarioEliminacionCorreo = ISNULL(TT.UsuarioEliminacion, @PusuarioCreador),
    @fechaEliminacionCorreo = ISNULL(CONVERT(VARCHAR(19), TT.FechaEliminacion, 120), ''),
    @paraCorreo = TU.Correo,
    @tituloCorreo = ISNULL(TT.Titulo, ''),
    @fechaVencimientoCorreo = ISNULL(CONVERT(VARCHAR(10), TT.FechaVencimiento, 23), ''),
    @descripcionCorreo = ISNULL(TT.Descripcion, '')
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

SELECT @listaUsuariosCorreo = STUFF
(
    (
        SELECT '; ' + TI.Usuario
        FROM dbo.TRACKIT_TareaInvolucrados TI WITH (NOLOCK)
        WHERE TI.IdTarea = @PidTarea
        ORDER BY TI.Usuario
        FOR XML PATH(''), TYPE
    ).value('.', 'NVARCHAR(MAX)'),
    1,
    2,
    ''
);

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

SET @asuntoCorreo = 'Tarea: ' + ISNULL(@tituloCorreo, '');

SET @listaUsuariosCorreo = ISNULL
(
    NULLIF(@listaUsuariosCorreo, ''),
    'No hay usuarios involucrados registrados.'
);

SET @anotacionesCorreo = ISNULL
(
    NULLIF(@anotacionesCorreo, ''),
    '<tr><td style="text-align: left;">No hay anotaciones registradas.</td></tr>'
);

SET @tableHTMLCorreo = '<table class="tabla" width="100%">' +
    '<tbody>' +
    '<tr><td style="text-align: left;">Estimados Sres.</td></tr>' +
    '<tr><td><br /></td></tr>' +
    '<tr><td style="text-align: left;">La tarea <strong>' +
        ISNULL(@tituloCorreo, '') + '</strong> acaba de ser eliminada.</td></tr>' +
    '<tr><td><br /></td></tr>' +
    '<tr><td style="text-align: left;">Datos de la tarea:</td></tr>' +
    '<tr><td style="text-align: left;">Titulo de la tarea: <strong>' +
        ISNULL(@tituloCorreo, '') + '</strong></td></tr>' +
    '<tr><td style="text-align: left;">Fecha de vencimiento: <strong>' +
        ISNULL(@fechaVencimientoCorreo, '') + '</strong></td></tr>' +
    '<tr><td style="text-align: left;">Descripcion: <strong>' +
        ISNULL(@descripcionCorreo, '') + '</strong></td></tr>' +
    '<tr><td style="text-align: left;">Involucrados: <strong>' +
        ISNULL(@listaUsuariosCorreo, '') + '</strong></td></tr>' +
    '<tr><td style="text-align: left;">Usuario que eliminó: <strong>' +
        ISNULL(@usuarioEliminacionCorreo, '') + '</strong></td></tr>' +
    '<tr><td style="text-align: left;">Fecha de eliminación: <strong>' +
        ISNULL(@fechaEliminacionCorreo, '') + '</strong></td></tr>' +
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
