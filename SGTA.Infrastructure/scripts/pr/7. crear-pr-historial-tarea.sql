CREATE OR ALTER PROCEDURE dbo.pr_trackit_HistorialTarea
	@PidTarea INT
AS
BEGIN
	SET NOCOUNT ON;

	IF @PidTarea IS NULL OR @PidTarea <= 0
		THROW 50026, 'El Id de la tarea debe ser mayor que cero.', 1;

	IF NOT EXISTS
	(
		SELECT 1
		FROM dbo.TRACKIT_Tareas
		WHERE IdTarea = @PidTarea
	)
		THROW 50027, 'No existe una tarea con el Id indicado.', 1;

	SELECT
		IdHistorial,
		IdTarea,
		TipoEvento,
		Usuario,
		FechaEvento,
		Descripcion,
		EstadoAnterior,
		EstadoNuevo,
		IdAnotacion,
		CampoModificado,
		ValorAnterior,
		ValorNuevo
	FROM dbo.TRACKIT_TareaHistorial
	WHERE IdTarea = @PidTarea
	ORDER BY FechaEvento ASC, IdHistorial ASC;
END
GO
