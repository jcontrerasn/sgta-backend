using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using SGTA.Application.Interfaces;
using SGTA.Domain.Entities;

namespace SGTA.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly string _connectionString;

    public UsuarioRepository(IConfiguration configuration)
    {
        _connectionString = configuration
            .GetConnectionString("SgtaConnection")
            ?? throw new InvalidOperationException(
                "No se configuró ConnectionStrings:SgtaConnection.");
    }

    public async Task<Usuario?> BuscarPorCuentaAsync(
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

        return new Usuario
        {
            UsuarioNombre = LeerTexto(reader, "Usuario"),
            NombreCompleto = LeerTexto(reader, "NombreCompleto"),
            Area = LeerTexto(reader, "Area")
        };
    }

    public async Task<List<Usuario>> BuscarPorFiltroAsync(
        string filtro,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(
            "pr_sm_ListarUsuarioXFiltro", connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 30
        };

        command.Parameters.Add(new SqlParameter(
            "Pfiltro_c", SqlDbType.VarChar, 50)
        {
            Value = filtro
        });

        await connection.OpenAsync(cancellationToken);

        await using var reader = await command.ExecuteReaderAsync(
            cancellationToken);

        var usuarios = new List<Usuario>();

        while (await reader.ReadAsync(cancellationToken))
        {
            usuarios.Add(new Usuario
            {
                UsuarioNombre = LeerTexto(reader, "Usuario"),
                NombreCompleto = LeerTexto(reader, "NombreCompleto"),
                Area = LeerTexto(reader, "Area")
            });
        }

        return usuarios;
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
