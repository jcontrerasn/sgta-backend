
ALTER PROCEDURE [dbo].[pr_trackit_BuscarTareaXId]
    @PidTarea INT
AS
BEGIN
    SET NOCOUNT ON;

    IF @PidTarea IS NULL OR @PidTarea <= 0
        THROW 50008, 'El Id de la tarea debe ser mayor que cero.', 1;

    -- Primer resultado: datos principales de la tarea.
    SELECT
        TT.Titulo,
        TT.FechaVencimiento,
        TT.Descripcion,
		TU.NombreCompleto as UsuarioCreador,
        TT.Estado
    FROM dbo.TRACKIT_Tareas TT inner join TS_USUARIO TU on TT.UsuarioCreador = TU.Usuario
    WHERE IdTarea = @PidTarea;

    -- Segundo resultado: usuarios asignados a la tarea.
    SELECT
        TI.Usuario,
        TU.NombreCompleto
    FROM dbo.TRACKIT_TareaInvolucrados AS TI
    INNER JOIN TS_USUARIO AS TU
        ON TI.Usuario = TU.Usuario
    WHERE TI.IdTarea = @PidTarea
    ORDER BY IdTareaInvolucrado;

    -- Tercer resultado: anotaciones de la tarea.
    SELECT
        IdAnotacion,
        IdTarea,
        UsuarioAutor,
        Comentario,
        EsComentarioCierre,
        FechaCreacion
    FROM dbo.TRACKIT_TareaAnotaciones
    WHERE IdTarea = @PidTarea
    ORDER BY IdAnotacion;
END;
