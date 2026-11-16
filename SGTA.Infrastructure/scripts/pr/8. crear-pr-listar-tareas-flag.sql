USE SM_PRUEBA;
GO

IF OBJECT_ID(N'dbo.pr_trackit_ListarTareasFlag', N'P') IS NULL
BEGIN
    EXEC(N'
        CREATE PROCEDURE dbo.pr_trackit_ListarTareasFlag
        AS
        BEGIN
            SET NOCOUNT ON;
        END
    ');
END;
GO

ALTER PROCEDURE dbo.pr_trackit_ListarTareasFlag
    @Pflag VARCHAR(20),
    @Pusuario VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SET @Pflag = LOWER(LTRIM(RTRIM(@Pflag)));

    IF NULLIF(@Pflag, '') IS NULL
        OR @Pflag NOT IN ('creadas', 'asignadas', 'otras')
    BEGIN
        RAISERROR(
            'El flag debe ser creadas, asignadas u otras.',
            16,
            1);
        RETURN;
    END;

    SET @Pusuario = LTRIM(RTRIM(@Pusuario));

    IF NULLIF(@Pusuario, '') IS NULL
    BEGIN
        RAISERROR(
            'El usuario es obligatorio.',
            16,
            1);
        RETURN;
    END;

    DECLARE @RolUsuario VARCHAR(100);

    IF @Pflag = 'otras'
    BEGIN
        SELECT TOP (1)
            @RolUsuario = R.Nombre
        FROM dbo.TRACKIT_UsuarioRoles UR
        INNER JOIN dbo.TRACKIT_Roles R
            ON R.IdRol = UR.IdRol
        WHERE UR.Usuario = @Pusuario
            AND UR.Activo = 1
            AND R.Activo = 1;

        IF NULLIF(@RolUsuario, '') IS NULL
        BEGIN
            RAISERROR(
                'El usuario no esta registrado o no tiene un rol activo.',
                16,
                1);
            RETURN;
        END;
    END;

    DECLARE @TareasSeleccionadas TABLE
    (
        IdTarea INT NOT NULL PRIMARY KEY
    );

    IF @Pflag = 'creadas'
    BEGIN
        -- Flag creadas: tareas creadas por el usuario.
        INSERT INTO @TareasSeleccionadas (IdTarea)
        SELECT T.IdTarea
        FROM dbo.TRACKIT_Tareas T
        WHERE T.UsuarioCreador = @Pusuario;
    END
    ELSE IF @Pflag = 'asignadas'
    BEGIN
        -- Flag asignadas: tareas donde el usuario esta involucrado.
        INSERT INTO @TareasSeleccionadas (IdTarea)
        SELECT T.IdTarea
        FROM dbo.TRACKIT_Tareas T
        WHERE EXISTS
        (
            SELECT 1
            FROM dbo.TRACKIT_TareaInvolucrados TI
            WHERE TI.IdTarea = T.IdTarea
                AND TI.Usuario = @Pusuario
        );
    END
    ELSE
    BEGIN
        -- Flag otras: tareas donde el usuario no es creador ni involucrado.
        INSERT INTO @TareasSeleccionadas (IdTarea)
        SELECT T.IdTarea
        FROM dbo.TRACKIT_Tareas T
        WHERE T.UsuarioCreador <> @Pusuario
            AND NOT EXISTS
            (
                SELECT 1
                FROM dbo.TRACKIT_TareaInvolucrados TI
                WHERE TI.IdTarea = T.IdTarea
                    AND TI.Usuario = @Pusuario
            )
            AND
            (
                @RolUsuario <> 'Tecnico_Soporte'
                OR EXISTS
                (
                    SELECT 1
                    FROM dbo.TRACKIT_UsuarioRoles UR
                    INNER JOIN dbo.TRACKIT_Roles R
                        ON R.IdRol = UR.IdRol
                    WHERE UR.Usuario = T.UsuarioCreador
                        AND UR.Activo = 1
                        AND R.Activo = 1
                        AND R.Nombre IN
                        (
                            'Tecnico_Soporte',
                            'Responsable_HW_SW'
                        )
                )
            );
    END;

    -- Primer resultado: tareas seleccionadas.
    SELECT
        T.IdTarea,
        T.Titulo,
        T.Descripcion,
        T.FechaVencimiento,
        T.UsuarioCreador,
        RC.NombreRol AS RolUsuarioCreador,
        T.Estado,
        T.FechaCreacion
    FROM dbo.TRACKIT_Tareas T
    INNER JOIN @TareasSeleccionadas TS
        ON TS.IdTarea = T.IdTarea
    OUTER APPLY
    (
        SELECT TOP (1)
            R.Nombre AS NombreRol
        FROM dbo.TRACKIT_UsuarioRoles UR
        INNER JOIN dbo.TRACKIT_Roles R
            ON R.IdRol = UR.IdRol
        WHERE UR.Usuario = T.UsuarioCreador
            AND UR.Activo = 1
            AND R.Activo = 1
    ) RC
    ORDER BY
        T.FechaVencimiento ASC,
        T.FechaCreacion DESC,
        T.IdTarea DESC;

    -- Segundo resultado: involucrados de las tareas seleccionadas.
    SELECT
        TI.IdTareaInvolucrado,
        TI.IdTarea,
        TI.Usuario,
        TI.FechaAsignacion
    FROM dbo.TRACKIT_TareaInvolucrados TI
    INNER JOIN @TareasSeleccionadas TS
        ON TS.IdTarea = TI.IdTarea
    ORDER BY
        TI.IdTarea,
        TI.IdTareaInvolucrado;
END;
GO
