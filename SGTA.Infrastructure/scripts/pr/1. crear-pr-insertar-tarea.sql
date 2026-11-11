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
        VALUES
        (
            @IdTarea,
            'CREADA',
            LTRIM(RTRIM(@PusuarioCreador)),
            N'Tarea creada.',
            'Activa',
            'Estado',
            NULL,
            'Activa'
        );

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
