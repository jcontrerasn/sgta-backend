USE SM_PRUEBA;
GO

CREATE OR ALTER PROCEDURE dbo.pr_trackit_EnviarCorreoTarea
    @PidTarea INT
AS
BEGIN
    SET NOCOUNT ON;

    IF @PidTarea IS NULL OR @PidTarea <= 0
        RETURN;

    DECLARE @para VARCHAR(MAX);
    DECLARE @asunto NVARCHAR(400);
    DECLARE @cuerpo NVARCHAR(MAX);
    DECLARE @anotaciones NVARCHAR(MAX);
    DECLARE @anotacionesXml XML;

    DECLARE @destinatarios TABLE
    (
        Correo VARCHAR(320) NOT NULL PRIMARY KEY
    );

    INSERT INTO @destinatarios (Correo)
    SELECT DISTINCT
        LTRIM(RTRIM(U.Correo))
    FROM dbo.TRACKIT_Tareas AS T
    INNER JOIN dbo.TS_USUARIO AS U
        ON U.Usuario = T.UsuarioCreador
    WHERE T.IdTarea = @PidTarea
        AND NULLIF(LTRIM(RTRIM(U.Correo)), '') IS NOT NULL;

    INSERT INTO @destinatarios (Correo)
    SELECT DISTINCT
        LTRIM(RTRIM(U.Correo))
    FROM dbo.TRACKIT_TareaInvolucrados AS TI
    INNER JOIN dbo.TS_USUARIO AS U
        ON U.Usuario = TI.Usuario
    WHERE TI.IdTarea = @PidTarea
        AND NULLIF(LTRIM(RTRIM(U.Correo)), '') IS NOT NULL
        AND NOT EXISTS
        (
            SELECT 1
            FROM @destinatarios AS D
            WHERE D.Correo = LTRIM(RTRIM(U.Correo))
        );

    SELECT
        @asunto = N'Tarea: ' + T.Titulo,
        @cuerpo =
            N'<table class="tabla" width="100%">' +
            N'<tbody>' +
            N'<tr>' +
            N'<td style="text-align: left;"> Estimado/a Sr. /Sra. </td>' +
            N'</tr>' +
            N'<tr>' +
            N'<td></td>' +
            N'</tr>' +
            N'<tr>' +
            N'<td style="text-align: left;"><strong>Tarea: </strong>' +
                T.Titulo + N'</td>' +
            N'</tr>' +
            N'<tr>' +
            N'<td style="text-align: left;"><strong>Descripcion: </strong>' +
                ISNULL(T.Descripcion, N'') + N'</td>' +
            N'</tr>' +
            N'<tr>' +
            N'<td style="text-align: left;"><strong>Fecha de vencimiento: </strong>' +
                CONVERT(NVARCHAR(10), T.FechaVencimiento, 103) + N'</td>' +
            N'</tr>' +
            N'<tr>' +
            N'<td style="text-align: left;"><strong>Estado: </strong>' +
                ISNULL(T.Estado, N'') + N'</td>' +
            N'</tr>' +
            N'<tr>' +
            N'<td></td>' +
            N'</tr>' +
            N'<tr>' +
            N'<td style="text-align: left;"><strong>Anotaciones:</strong></td>' +
            N'</tr>'
    FROM dbo.TRACKIT_Tareas AS T
    WHERE T.IdTarea = @PidTarea;

    IF @asunto IS NULL
        RETURN;

    SELECT @anotacionesXml =
    (
        SELECT
            N'<tr><td style="text-align: left;"><strong>' +
                CONVERT(NVARCHAR(10), A.FechaCreacion, 103) +
                N' - ' + A.UsuarioAutor + N':</strong> ' +
                A.Comentario +
                CASE
                    WHEN A.EsComentarioCierre = 1
                        THEN N' <em>(Comentario de cierre)</em>'
                    ELSE N''
                END +
                N'</td></tr>'
        FROM dbo.TRACKIT_TareaAnotaciones AS A
        WHERE A.IdTarea = @PidTarea
        ORDER BY A.IdAnotacion
        FOR XML PATH(''), TYPE
    );

    SET @anotaciones =
        CASE
            WHEN @anotacionesXml IS NULL
                THEN N'<tr><td style="text-align: left;">No hay anotaciones registradas.</td></tr>'
            ELSE @anotacionesXml.value('.', 'NVARCHAR(MAX)')
        END;

    SET @cuerpo = @cuerpo +
        @anotaciones +
        N'<tr>' +
        N'<td></td>' +
        N'</tr>' +
        N'<tr>' +
        N'<td style="text-align: left;">Atte.</td>' +
        N'</tr>' +
        N'<tr>' +
        N'<td style="text-align: left;">Sistema de Gestion de Tareas Administrativas (SGTA)</td>' +
        N'</tr>' +
        N'</tbody>' +
        N'</table>';

    SELECT @para = STUFF
    (
        (
            SELECT ';' + D.Correo
            FROM @destinatarios AS D
            ORDER BY D.Correo
            FOR XML PATH(''), TYPE
        ).value('.', 'VARCHAR(MAX)'),
        1,
        1,
        ''
    );

    IF NULLIF(@para, '') IS NULL
        RETURN;

    BEGIN TRY
        EXEC msdb.dbo.sp_send_dbmail
            @recipients = @para,
            @subject = @asunto,
            @body = @cuerpo,
            @body_format = 'HTML';
    END TRY
    BEGIN CATCH
        RETURN;
    END CATCH;
END;
GO
