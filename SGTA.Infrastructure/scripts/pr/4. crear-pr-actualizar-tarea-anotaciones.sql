ALTER PROCEDURE [dbo].[pr_trackit_ActualizarTareaAnotaciones]
    @PidTarea INT,
    @PusuarioAutor VARCHAR(100),
    @Pcomentario NVARCHAR(MAX),
    @PesComentarioCierre BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @PidTarea IS NULL OR @PidTarea <= 0
        THROW 50018, 'El Id de la tarea debe ser mayor que cero.', 1;

    IF NULLIF(LTRIM(RTRIM(@PusuarioAutor)), '') IS NULL
        THROW 50019, 'El usuario autor es obligatorio.', 1;

    IF NULLIF(LTRIM(RTRIM(@Pcomentario)), '') IS NULL
        THROW 50020, 'El comentario es obligatorio.', 1;

    IF LEN(@PusuarioAutor) > 100
        THROW 50021, 'El usuario autor no puede superar los 100 caracteres.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS
        (
            SELECT 1
            FROM dbo.TRACKIT_Tareas
            WHERE IdTarea = @PidTarea
        )
            THROW 50022, 'No existe una tarea con el Id indicado.', 1;

        DECLARE @EstadoAnterior VARCHAR(20);

        SELECT @EstadoAnterior = Estado
        FROM dbo.TRACKIT_Tareas
        WHERE IdTarea = @PidTarea;

        DECLARE @IdAnotacion INT;

        INSERT INTO dbo.TRACKIT_TareaAnotaciones
        (
            IdTarea,
            UsuarioAutor,
            Comentario,
            EsComentarioCierre
        )
        VALUES
        (
            @PidTarea,
            LTRIM(RTRIM(@PusuarioAutor)),
            LTRIM(RTRIM(@Pcomentario)),
            ISNULL(@PesComentarioCierre, 0)
        );

        SET @IdAnotacion = CONVERT(INT, SCOPE_IDENTITY());

        INSERT INTO dbo.TRACKIT_TareaHistorial
        (
            IdTarea,
            TipoEvento,
            Usuario,
            Descripcion,
            EstadoAnterior,
            EstadoNuevo,
            IdAnotacion,
            CampoModificado,
            ValorAnterior,
            ValorNuevo
        )
        VALUES
        (
            @PidTarea,
            'ANOTACION_AGREGADA',
            LTRIM(RTRIM(@PusuarioAutor)),
            LTRIM(RTRIM(@Pcomentario)),
            @EstadoAnterior,
            @EstadoAnterior,
            @IdAnotacion,
            'Anotacion',
            NULL,
            LTRIM(RTRIM(@Pcomentario))
        );

        -- Marca la tarea como cerrada cuando la anotacion es de cierre.
        IF ISNULL(@PesComentarioCierre, 0) = 1
        BEGIN
            UPDATE dbo.TRACKIT_Tareas
            SET FechaCierre = GETDATE(),
                UsuarioCierre = LTRIM(RTRIM(@PusuarioAutor)),
                Estado = 'Completada'
            WHERE IdTarea = @PidTarea;

            INSERT INTO dbo.TRACKIT_TareaHistorial
            (
                IdTarea,
                TipoEvento,
                Usuario,
                Descripcion,
                EstadoAnterior,
                EstadoNuevo,
                IdAnotacion,
                CampoModificado,
                ValorAnterior,
                ValorNuevo
            )
            VALUES
            (
                @PidTarea,
                'COMPLETADA',
                LTRIM(RTRIM(@PusuarioAutor)),
                N'Tarea completada mediante anotacion.',
                @EstadoAnterior,
                'Completada',
                @IdAnotacion,
                'Estado',
                @EstadoAnterior,
                'Completada'
            );
        END;

        COMMIT TRANSACTION;

        EXEC dbo.pr_trackit_EnviarCorreoTarea
            @PidTarea = @PidTarea;

        SELECT
            IdAnotacion,
            IdTarea,
            UsuarioAutor,
            Comentario,
            EsComentarioCierre,
            FechaCreacion
        FROM dbo.TRACKIT_TareaAnotaciones
        WHERE IdAnotacion = @IdAnotacion;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH;
END;
