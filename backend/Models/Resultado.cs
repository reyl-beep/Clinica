namespace Clinica.Api.Models;

public class Resultado
{
    public bool Value { get; set; }

    public string Message { get; set; } = string.Empty;

    public object? Data { get; set; }
}
