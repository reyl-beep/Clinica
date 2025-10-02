namespace Clinica.Api.Models.Requests;

public class PacienteCreateRequest
{
    public string PrimerNombre { get; set; } = string.Empty;
    public string? SegundoNombre { get; set; }
    public string ApellidoPaterno { get; set; } = string.Empty;
    public string? ApellidoMaterno { get; set; }
    public string? Telefono { get; set; }
}
