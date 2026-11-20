
ALTER PROCEDURE [dbo].[pr_trackit_BuscarUsuarioXCuentaTrackIT]

	@Pcuenta_n VARCHAR(50)

AS
BEGIN
	
	DECLARE @usuario VARCHAR(50)

	SET @usuario = (SELECT TOP 1 P.[Usuario] FROM [dbo].[SM_PERSONAL] P with(nolock) WHERE P.[Usuario] = @Pcuenta_n)

	IF @usuario is NULL
	BEGIN 

		SELECT U.[Usuario]
		  ,U.[NombreCompleto]
		  ,U.[Ubicación]
		  ,U.[Area]
		  ,U.[CentroCosto]
		  ,U.[Telefono]
		  ,U.[Correo]
		  ,U.[ContactoArea]
		  ,U.[TelefonoContactoArea]
		  ,Puesto.[Rol]
		  ,'0' [Nivel]
		  ,U.[Ind_Activo]
		  --,U.Acceso as [Ind_Activo]
	  FROM [dbo].[TS_USUARIO] U with(nolock)
	  OUTER APPLY
	  (
		  SELECT TOP (1)
			  PA.[ValorTexto] AS [Rol]
		  FROM [dbo].[SM_PUESTO] PU with(nolock)
		  INNER JOIN [dbo].[SM_PARAMETRO] PA with(nolock)
			  ON PU.[IdPuesto] = PA.[Id]
		  WHERE PU.[Estado] = 1
			  AND PU.[Usuario] = U.[Usuario]
			  AND PA.[ValorTexto] IN
			  (
				  'TecnicoSoporte',
				  'ResponsableHardware',
				  'ResponsableSoftware',
				  'IngenieroRedes',
				  'Jefatura GTIC'
			  )
		  ORDER BY CASE PA.[ValorTexto]
			  WHEN 'Jefatura GTIC' THEN 1
			  WHEN 'IngenieroRedes' THEN 2
			  WHEN 'ResponsableSoftware' THEN 3
			  WHEN 'ResponsableHardware' THEN 4
			  WHEN 'TecnicoSoporte' THEN 5
			  ELSE 6
		  END,
		  PU.[Id]
	  ) Puesto
	  WHERE U.[Usuario] = @Pcuenta_n
		
	END 
	ELSE
	BEGIN

	   SELECT 
	   TOP 1
	   U.[Usuario]
      ,U.[NombreCompleto]
      ,U.[Ubicación]
      ,U.[Area]
      ,U.[CentroCosto]
      ,U.[Telefono]
      ,U.[Correo]
      ,U.[ContactoArea]
      ,U.[TelefonoContactoArea]
	  ,Puesto.[Rol]
	  ,P.[Nivel]
	  ,U.[Ind_Activo]
	  FROM [dbo].[TS_USUARIO] U with(nolock)
	  INNER JOIN [dbo].[SM_PERSONAL] P with(nolock) ON U.[Usuario] = P.[Usuario] 
	  OUTER APPLY
	  (
		  SELECT TOP (1)
			  PA.[ValorTexto] AS [Rol]
		  FROM [dbo].[SM_PUESTO] PU with(nolock)
		  INNER JOIN [dbo].[SM_PARAMETRO] PA with(nolock)
			  ON PU.[IdPuesto] = PA.[Id]
		  WHERE PU.[Estado] = 1
			  AND PU.[Usuario] = U.[Usuario]
			  AND PA.[ValorTexto] IN
			  (
				  'TecnicoSoporte',
				  'ResponsableHardware',
				  'ResponsableSoftware',
				  'IngenieroRedes',
				  'Jefatura GTIC'
			  )
		  ORDER BY CASE PA.[ValorTexto]
			  WHEN 'Jefatura GTIC' THEN 1
			  WHEN 'IngenieroRedes' THEN 2
			  WHEN 'ResponsableSoftware' THEN 3
			  WHEN 'ResponsableHardware' THEN 4
			  WHEN 'TecnicoSoporte' THEN 5
			  ELSE 6
		  END,
		  PU.[Id]
	  ) Puesto
	  WHERE U.[Usuario] = @Pcuenta_n

	END

END
