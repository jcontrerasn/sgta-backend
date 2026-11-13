USE SM_PRUEBA;
GO

CREATE OR ALTER PROCEDURE dbo.pr_trackit_InsertarNuevaTarea
    @Ptitulo NVARCHAR(120),
    @Pdescripcion NVARCHAR(MAX),
    @PfechaVencimiento DATE,
    @PusuarioCreador VARCHAR(100),
    @PusuariosInvolucrados NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF NULLIF(LTRIM(RTRIM(@Ptitulo)), '') IS NULL
        THROW 50001, 'El titulo es obligatorio.', 1;

    IF NULLIF(LTRIM(RTRIM(@Pdescripcion)), '') IS NULL
        THROW 50002, 'La descripcion es obligatoria.', 1;

    IF @PfechaVencimiento IS NULL
        THROW 50003, 'La fecha de vencimiento es obligatoria.', 1;

    IF NULLIF(LTRIM(RTRIM(@PusuarioCreador)), '') IS NULL
        THROW 50004, 'El usuario creador es obligatorio.', 1;

    IF NULLIF(LTRIM(RTRIM(@PusuariosInvolucrados)), '') IS NULL
        THROW 50005, 'Debe indicar al menos un usuario involucrado.', 1;

    DECLARE @Involucrados TABLE
    (
        Usuario VARCHAR(100) NOT NULL PRIMARY KEY
    );

    DECLARE @TextoInvolucrados NVARCHAR(MAX) =
        LTRIM(RTRIM(@PusuariosInvolucrados));
    DECLARE @Posicion INT = 1;
    DECLARE @SiguientePosicion INT;
    DECLARE @UsuarioActual NVARCHAR(MAX);

    WHILE @Posicion <= LEN(@TextoInvolucrados) + 1
    BEGIN
        SET @SiguientePosicion = CHARINDEX(
            ';', @TextoInvolucrados, @Posicion);

        IF @SiguientePosicion = 0
            SET @SiguientePosicion = LEN(@TextoInvolucrados) + 1;

        SET @UsuarioActual = LTRIM(RTRIM(SUBSTRING(
            @TextoInvolucrados,
            @Posicion,
            @SiguientePosicion - @Posicion)));

        IF NULLIF(@UsuarioActual, '') IS NOT NULL
        BEGIN
            IF LEN(@UsuarioActual) > 100
                THROW 50006, 'El usuario involucrado no puede superar los 100 caracteres.', 1;

            IF NOT EXISTS
            (
                SELECT 1
                FROM @Involucrados
                WHERE Usuario = @UsuarioActual
            )
            BEGIN
                INSERT INTO @Involucrados (Usuario)
                VALUES (CONVERT(VARCHAR(100), @UsuarioActual));
            END;
        END;

        SET @Posicion = @SiguientePosicion + 1;
    END;

    IF NOT EXISTS (SELECT 1 FROM @Involucrados)
        THROW 50007, 'Debe indicar al menos un usuario involucrado.', 1;

    DECLARE @InvolucradosNormalizados NVARCHAR(MAX);

    SELECT @InvolucradosNormalizados = STUFF
    (
        (
            SELECT N';' + CONVERT(NVARCHAR(100), Usuario)
            FROM @Involucrados
            ORDER BY Usuario
            FOR XML PATH(''), TYPE
        ).value('.', 'NVARCHAR(MAX)'),
        1,
        1,
        N''
    );

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @IdTarea INT;

        INSERT INTO dbo.TRACKIT_Tareas
        (
            Titulo,
            Descripcion,
            FechaVencimiento,
            UsuarioCreador,
            Estado
        )
        VALUES
        (
            LTRIM(RTRIM(@Ptitulo)),
            LTRIM(RTRIM(@Pdescripcion)),
            @PfechaVencimiento,
            LTRIM(RTRIM(@PusuarioCreador)),
            'Activa'
        );

        SET @IdTarea = CONVERT(INT, SCOPE_IDENTITY());

        INSERT INTO dbo.TRACKIT_TareaInvolucrados
        (
            IdTarea,
            Usuario
        )
        SELECT
            @IdTarea,
            Usuario
        FROM @Involucrados;

        INSERT INTO dbo.TRACKIT_TareaHistorial
        (
            IdTarea,
            TipoEvento,
            Usuario,
            Descripcion,
            EstadoNuevo,
            CampoModificado,
            ValorAnterior,
            ValorNuevo
        )
        SELECT
            @IdTarea,
            'CREADA',
            LTRIM(RTRIM(@PusuarioCreador)),
            Datos.DescripcionEvento,
            'Activa',
            Datos.CampoModificado,
            NULL,
            Datos.ValorNuevo
        FROM
        (
            SELECT
                CONVERT(VARCHAR(50), 'Titulo') AS CampoModificado,
                CONVERT(NVARCHAR(MAX), LTRIM(RTRIM(@Ptitulo))) AS ValorNuevo,
                CONVERT(NVARCHAR(MAX), N'Titulo de la tarea registrado.') AS DescripcionEvento
            UNION ALL
            SELECT
                CONVERT(VARCHAR(50), 'Descripcion'),
                CONVERT(NVARCHAR(MAX), LTRIM(RTRIM(@Pdescripcion))),
                CONVERT(NVARCHAR(MAX), N'Descripcion de la tarea registrada.')
            UNION ALL
            SELECT
                CONVERT(VARCHAR(50), 'FechaVencimiento'),
                CONVERT(NVARCHAR(MAX), CONVERT(NVARCHAR(10), @PfechaVencimiento, 23)),
                CONVERT(NVARCHAR(MAX), N'Fecha de vencimiento registrada.')
            UNION ALL
            SELECT
                CONVERT(VARCHAR(50), 'Involucrados'),
                @InvolucradosNormalizados,
                CONVERT(NVARCHAR(MAX), N'Involucrados de la tarea registrados.')
            UNION ALL
            SELECT
                CONVERT(VARCHAR(50), 'Estado'),
                CONVERT(NVARCHAR(MAX), 'Activa'),
                CONVERT(NVARCHAR(MAX), N'Tarea creada.')
        ) AS Datos;

        COMMIT TRANSACTION;

        EXEC dbo.pr_trackit_EnviarCorreoTarea
            @PidTarea = @IdTarea;

        -- Primer resultado: tarea creada.
        SELECT
            IdTarea,
            Titulo,
            Descripcion,
            FechaVencimiento,
            UsuarioCreador,
            Estado,
            FechaCreacion
        FROM dbo.TRACKIT_Tareas
        WHERE IdTarea = @IdTarea;

        -- Segundo resultado: usuarios involucrados registrados.
        SELECT
            IdTareaInvolucrado,
            IdTarea,
            Usuario,
            FechaAsignacion
        FROM dbo.TRACKIT_TareaInvolucrados
        WHERE IdTarea = @IdTarea
        ORDER BY IdTareaInvolucrado;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH;
END;
GO
