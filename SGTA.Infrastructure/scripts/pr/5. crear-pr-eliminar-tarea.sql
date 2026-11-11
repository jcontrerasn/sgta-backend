USE SM_PRUEBA;
GO

CREATE OR ALTER PROCEDURE dbo.pr_trackit_EliminarTarea
    @PidTarea INT,
    @PusuarioCreador VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @PidTarea IS NULL OR @PidTarea <= 0
        THROW 50023, 'El Id de la tarea debe ser mayor que cero.', 1;

    IF NULLIF(LTRIM(RTRIM(@PusuarioCreador)), '') IS NULL
        THROW 50024, 'El usuario creador es obligatorio.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS
        (
            SELECT 1
            FROM dbo.TRACKIT_Tareas
            WHERE IdTarea = @PidTarea
                AND UsuarioCreador = LTRIM(RTRIM(@PusuarioCreador))
        )
            THROW 50025, 'No existe la tarea o el usuario no es su creador.', 1;

        UPDATE dbo.TRACKIT_Tareas
        SET Estado = 'Eliminada',
            FechaEliminacion = GETDATE(),
            UsuarioEliminacion = LTRIM(RTRIM(@PusuarioCreador))
        WHERE IdTarea = @PidTarea;

        COMMIT TRANSACTION;

        EXEC dbo.pr_trackit_EnviarCorreoTarea
            @PidTarea = @PidTarea;

        SELECT
            @PidTarea AS IdTarea,
            CAST(1 AS BIT) AS Eliminado;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH;
END;
GO
