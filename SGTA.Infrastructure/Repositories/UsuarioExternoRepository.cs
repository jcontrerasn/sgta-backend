using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using SGTA.Application.Interfaces;
using SGTA.Domain.Entities;

namespace SGTA.Infrastructure.Repositories;

public class UsuarioExternoRepository : IUsuarioExternoRepository
{
    private readonly string _connectionString;

    public UsuarioExternoRepository(IConfiguration configuration)
    {
        _connectionString = configuration
            .GetConnectionString("SgtaConnectionExternal")
            ?? throw new InvalidOperationException(
                "No se configuró la conexión externa SgtaConnectionExternal.");
    }

    public async Task<UsuarioExterno?> BuscarPorCuentaAsync(
        string cuenta,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(
            "pr_sm_BuscarUsuarioXCuenta", connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 30
        };

        command.Parameters.Add(new SqlParameter(
            "Pcuenta_n", SqlDbType.VarChar, 50)
        {
            Value = cuenta
        });

        await connection.OpenAsync(cancellationToken);

        await using var reader = await command.ExecuteReaderAsync(
            cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new UsuarioExterno
        {
            Usuario = LeerTexto(reader, "Usuario"),
            NombreCompleto = LeerTexto(reader, "NombreCompleto"),
            Area = LeerTexto(reader, "Area")
        };
    }

    private static string? LeerTexto(
        SqlDataReader reader,
        string columna)
    {
        var valor = reader[columna];

        return valor == DBNull.Value
            ? null
            : Convert.ToString(valor);
    }
}
