USE SM_PRUEBA;
GO

CREATE OR ALTER PROCEDURE dbo.pr_trackit_ActualizarTarea
    @PidTarea INT,
    @Ptitulo NVARCHAR(120),
    @Pdescripcion NVARCHAR(MAX),
    @PfechaVencimiento DATE,
    @PusuariosInvolucrados NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @PidTarea IS NULL OR @PidTarea <= 0
        THROW 50010, 'El Id de la tarea debe ser mayor que cero.', 1;

    IF NULLIF(LTRIM(RTRIM(@Ptitulo)), '') IS NULL
        THROW 50011, 'El titulo es obligatorio.', 1;

    IF NULLIF(LTRIM(RTRIM(@Pdescripcion)), '') IS NULL
        THROW 50012, 'La descripcion es obligatoria.', 1;

    IF @PfechaVencimiento IS NULL
        THROW 50013, 'La fecha de vencimiento es obligatoria.', 1;

    IF NULLIF(LTRIM(RTRIM(@PusuariosInvolucrados)), '') IS NULL
        THROW 50014, 'Debe indicar al menos un usuario involucrado.', 1;

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
                THROW 50015, 'El usuario involucrado no puede superar los 100 caracteres.', 1;

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
        THROW 50016, 'Debe indicar al menos un usuario involucrado.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS
        (
            SELECT 1
            FROM dbo.TRACKIT_Tareas
            WHERE IdTarea = @PidTarea
        )
            THROW 50017, 'No existe una tarea con el Id indicado.', 1;

        UPDATE dbo.TRACKIT_Tareas
        SET
            Titulo = LTRIM(RTRIM(@Ptitulo)),
            Descripcion = LTRIM(RTRIM(@Pdescripcion)),
            FechaVencimiento = @PfechaVencimiento,
            FechaModificacion = GETDATE()
        WHERE IdTarea = @PidTarea;

        -- Se reemplazan los involucrados actuales por los recibidos.
        DELETE FROM dbo.TRACKIT_TareaInvolucrados
        WHERE IdTarea = @PidTarea;

        INSERT INTO dbo.TRACKIT_TareaInvolucrados
        (
            IdTarea,
            Usuario
        )
        SELECT
            @PidTarea,
            Usuario
        FROM @Involucrados;

        COMMIT TRANSACTION;

        EXEC dbo.pr_trackit_EnviarCorreoTarea
            @PidTarea = @PidTarea;

        -- Primer resultado: tarea actualizada.
        SELECT
            IdTarea,
            Titulo,
            Descripcion,
            FechaVencimiento,
            UsuarioCreador,
            Estado,
            FechaCreacion,
            FechaModificacion
        FROM dbo.TRACKIT_Tareas
        WHERE IdTarea = @PidTarea;

        -- Segundo resultado: involucrados actualizados.
        SELECT
            IdTareaInvolucrado,
            IdTarea,
            Usuario,
            FechaAsignacion
        FROM dbo.TRACKIT_TareaInvolucrados
        WHERE IdTarea = @PidTarea
        ORDER BY IdTareaInvolucrado;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH;
END;
GO
