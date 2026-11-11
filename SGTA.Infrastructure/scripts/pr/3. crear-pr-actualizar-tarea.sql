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

        DECLARE @EstadoAnterior VARCHAR(20);
        DECLARE @UsuarioHistorial VARCHAR(100);
        DECLARE @TituloAnterior NVARCHAR(120);
        DECLARE @DescripcionAnterior NVARCHAR(MAX);
        DECLARE @FechaVencimientoAnterior DATE;
        DECLARE @InvolucradosAnteriores NVARCHAR(MAX);
        DECLARE @InvolucradosNuevos NVARCHAR(MAX);
        DECLARE @TituloNuevo NVARCHAR(120) = LTRIM(RTRIM(@Ptitulo));
        DECLARE @DescripcionNueva NVARCHAR(MAX) = LTRIM(RTRIM(@Pdescripcion));

        IF NOT EXISTS
        (
            SELECT 1
            FROM dbo.TRACKIT_Tareas
            WHERE IdTarea = @PidTarea
        )
            THROW 50017, 'No existe una tarea con el Id indicado.', 1;

        SELECT
            @EstadoAnterior = Estado,
            @UsuarioHistorial = UsuarioCreador,
            @TituloAnterior = Titulo,
            @DescripcionAnterior = Descripcion,
            @FechaVencimientoAnterior = FechaVencimiento
        FROM dbo.TRACKIT_Tareas
        WHERE IdTarea = @PidTarea;

        SELECT @InvolucradosAnteriores = STUFF
        (
            (
                SELECT ';' + TI.Usuario
                FROM dbo.TRACKIT_TareaInvolucrados AS TI
                WHERE TI.IdTarea = @PidTarea
                ORDER BY TI.Usuario
                FOR XML PATH(''), TYPE
            ).value('.', 'NVARCHAR(MAX)'),
            1,
            1,
            ''
        );

        SELECT @InvolucradosNuevos = STUFF
        (
            (
                SELECT ';' + I.Usuario
                FROM @Involucrados AS I
                ORDER BY I.Usuario
                FOR XML PATH(''), TYPE
            ).value('.', 'NVARCHAR(MAX)'),
            1,
            1,
            ''
        );

        UPDATE dbo.TRACKIT_Tareas
        SET
            Titulo = @TituloNuevo,
            Descripcion = @DescripcionNueva,
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

        IF ISNULL(@TituloAnterior, N'') <> ISNULL(@TituloNuevo, N'')
        BEGIN
            INSERT INTO dbo.TRACKIT_TareaHistorial
            (
                IdTarea, TipoEvento, Usuario, Descripcion,
                EstadoAnterior, EstadoNuevo,
                CampoModificado, ValorAnterior, ValorNuevo
            )
            VALUES
            (
                @PidTarea, 'ACTUALIZADA', @UsuarioHistorial,
                N'Titulo actualizado.', @EstadoAnterior, @EstadoAnterior,
                'Titulo', @TituloAnterior, @TituloNuevo
            );
        END;

        IF ISNULL(@DescripcionAnterior, N'') <> ISNULL(@DescripcionNueva, N'')
        BEGIN
            INSERT INTO dbo.TRACKIT_TareaHistorial
            (
                IdTarea, TipoEvento, Usuario, Descripcion,
                EstadoAnterior, EstadoNuevo,
                CampoModificado, ValorAnterior, ValorNuevo
            )
            VALUES
            (
                @PidTarea, 'ACTUALIZADA', @UsuarioHistorial,
                N'Descripcion actualizada.', @EstadoAnterior, @EstadoAnterior,
                'Descripcion', @DescripcionAnterior, @DescripcionNueva
            );
        END;

        IF ISNULL(@FechaVencimientoAnterior, '19000101') <> @PfechaVencimiento
        BEGIN
            INSERT INTO dbo.TRACKIT_TareaHistorial
            (
                IdTarea, TipoEvento, Usuario, Descripcion,
                EstadoAnterior, EstadoNuevo,
                CampoModificado, ValorAnterior, ValorNuevo
            )
            VALUES
            (
                @PidTarea, 'ACTUALIZADA', @UsuarioHistorial,
                N'Fecha de vencimiento actualizada.', @EstadoAnterior, @EstadoAnterior,
                'FechaVencimiento',
                CONVERT(NVARCHAR(10), @FechaVencimientoAnterior, 23),
                CONVERT(NVARCHAR(10), @PfechaVencimiento, 23)
            );
        END;

        IF ISNULL(@InvolucradosAnteriores, N'') <> ISNULL(@InvolucradosNuevos, N'')
        BEGIN
            INSERT INTO dbo.TRACKIT_TareaHistorial
            (
                IdTarea, TipoEvento, Usuario, Descripcion,
                EstadoAnterior, EstadoNuevo,
                CampoModificado, ValorAnterior, ValorNuevo
            )
            VALUES
            (
                @PidTarea, 'ACTUALIZADA', @UsuarioHistorial,
                N'Involucrados actualizados.', @EstadoAnterior, @EstadoAnterior,
                'Involucrados', @InvolucradosAnteriores, @InvolucradosNuevos
            );
        END;

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
