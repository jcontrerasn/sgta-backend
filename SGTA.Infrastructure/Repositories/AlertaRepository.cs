using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SGTA.Application.Interfaces;
using SGTA.Domain.Entities;
using System.Data;

namespace SGTA.Infrastructure.Repositories;

public class AlertaRepository : IAlertaRepository
{
    private readonly string _connectionString;

    public AlertaRepository(IConfiguration configuration)
    {
        _connectionString = configuration
            .GetConnectionString("SgtaConnection")
            ?? throw new InvalidOperationException(
                "No se configuró ConnectionStrings:SgtaConnection.");
    }

    public async Task<List<Alerta>> ObtenerHistorialAsync(
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(
            "pr_trackit_ListarHistorialAlertas", connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 30
        };

        await connection.OpenAsync(cancellationToken);

        await using var reader = await command.ExecuteReaderAsync(
            cancellationToken);

        var alertas = new List<Alerta>();

        while (await reader.ReadAsync(cancellationToken))
        {
            alertas.Add(new Alerta
            {
                IdAlerta = reader.GetInt32(
                    reader.GetOrdinal("IdAlerta")),
                IdTarea = reader.GetInt32(
                    reader.GetOrdinal("IdTarea")),
                TipoAlerta = LeerTexto(reader, "TipoAlerta"),
                TituloTarea = LeerTexto(reader, "TituloTarea"),
                Asunto = LeerTexto(reader, "Asunto"),
                UsuarioCreador = LeerTexto(reader, "UsuarioCreador"),
                FechaCreacionTarea = reader.GetDateTime(
                    reader.GetOrdinal("FechaCreacionTarea")),
                FechaVencimientoTarea = DateOnly.FromDateTime(
                    reader.GetDateTime(
                        reader.GetOrdinal("FechaVencimientoTarea"))),
                EstadoEnvio = LeerTexto(reader, "EstadoEnvio"),
                FechaCreacion = reader.GetDateTime(
                    reader.GetOrdinal("FechaCreacion")),
                FechaEncolada = LeerFechaNullable(
                    reader, "FechaEncolada"),
                ErrorDetalle = LeerTextoNullable(
                    reader, "ErrorDetalle")
            });
        }

        if (await reader.NextResultAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                var idAlerta = reader.GetInt32(
                    reader.GetOrdinal("IdAlerta"));

                var alerta = alertas.FirstOrDefault(
                    item => item.IdAlerta == idAlerta);

                if (alerta is null)
                {
                    continue;
                }

                alerta.Destinatarios.Add(new AlertaDestinatario
                {
                    IdDestinatario = reader.GetInt32(
                        reader.GetOrdinal("IdDestinatario")),
                    IdAlerta = idAlerta,
                    Usuario = LeerTextoNullable(reader, "Usuario"),
                    Correo = LeerTexto(reader, "Correo"),
                    TipoDestinatario = LeerTexto(
                        reader, "TipoDestinatario"),
                    EstadoEnvio = LeerTexto(reader, "EstadoEnvio"),
                    FechaEncolada = LeerFechaNullable(
                        reader, "FechaEncolada"),
                    ErrorDetalle = LeerTextoNullable(
                        reader, "ErrorDetalle")
                });
            }
        }

        return alertas;
    }

    private static string LeerTexto(
        SqlDataReader reader,
        string nombreColumna)
    {
        var ordinal = reader.GetOrdinal(nombreColumna);
        return reader.IsDBNull(ordinal)
            ? string.Empty
            : reader.GetString(ordinal);
    }

    private static string? LeerTextoNullable(
        SqlDataReader reader,
        string nombreColumna)
    {
        var ordinal = reader.GetOrdinal(nombreColumna);
        return reader.IsDBNull(ordinal)
            ? null
            : reader.GetString(ordinal);
    }

    private static DateTime? LeerFechaNullable(
        SqlDataReader reader,
        string nombreColumna)
    {
        var ordinal = reader.GetOrdinal(nombreColumna);
        return reader.IsDBNull(ordinal)
            ? null
            : reader.GetDateTime(ordinal);
    }
}
