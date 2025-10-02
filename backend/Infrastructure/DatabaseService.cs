using System.Data;
using Clinica.Api.Models;
using Microsoft.Data.SqlClient;

namespace Clinica.Api.Infrastructure;

public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");
    }

    public async Task<Resultado> ExecuteAsync(
        string storedProcedure,
        Action<SqlParameterCollection>? configureParameters = null,
        Func<SqlDataReader, Task<object?>>? mapData = null)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = CreateCommand(connection, storedProcedure);

        configureParameters?.Invoke(command.Parameters);
        var (resultadoParam, mensajeParam) = AddStandardParameters(command);

        await connection.OpenAsync();

        object? data = null;
        if (mapData is null)
        {
            await command.ExecuteNonQueryAsync();
        }
        else
        {
            await using var reader = await command.ExecuteReaderAsync();
            data = await mapData(reader);
        }

        return BuildResultado(resultadoParam, mensajeParam, data);
    }

    private static SqlCommand CreateCommand(SqlConnection connection, string storedProcedure)
    {
        return new SqlCommand(storedProcedure, connection)
        {
            CommandType = CommandType.StoredProcedure
        };
    }

    private static (SqlParameter resultado, SqlParameter mensaje) AddStandardParameters(SqlCommand command)
    {
        var resultado = command.Parameters.Add("@pResultado", SqlDbType.Bit);
        resultado.Direction = ParameterDirection.Output;

        var mensaje = command.Parameters.Add("@pMsg", SqlDbType.VarChar, 500);
        mensaje.Direction = ParameterDirection.Output;

        return (resultado, mensaje);
    }

    private static Resultado BuildResultado(SqlParameter resultadoParam, SqlParameter mensajeParam, object? data)
    {
        var value = resultadoParam.Value is bool boolean && boolean;
        var message = mensajeParam.Value?.ToString();
        if (string.IsNullOrWhiteSpace(message))
        {
            message = value ? "Operación exitosa." : "Operación sin mensaje.";
        }

        return new Resultado
        {
            Value = value,
            Message = message!,
            Data = data
        };
    }
}
