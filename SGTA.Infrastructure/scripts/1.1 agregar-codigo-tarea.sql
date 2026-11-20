USE [SM_PRUEBA]
GO

IF COL_LENGTH('dbo.TRACKIT_Tareas', 'CodigoTarea') IS NULL
BEGIN
    ALTER TABLE dbo.TRACKIT_Tareas
        ADD CodigoTarea VARCHAR(20) NULL;
END;
GO

UPDATE dbo.TRACKIT_Tareas
SET CodigoTarea = CONCAT('TSK-', IdTarea)
WHERE CodigoTarea IS NULL;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UX_TRACKIT_Tareas_CodigoTarea'
      AND object_id = OBJECT_ID('dbo.TRACKIT_Tareas')
)
BEGIN
    CREATE UNIQUE INDEX UX_TRACKIT_Tareas_CodigoTarea
        ON dbo.TRACKIT_Tareas (CodigoTarea)
        WHERE CodigoTarea IS NOT NULL;
END;
GO
