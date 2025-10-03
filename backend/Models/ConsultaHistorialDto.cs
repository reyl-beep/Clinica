namespace Clinica.Api.Models;

public class ConsultaHistorialDto
{
    public int Id { get; set; }
    public int IdMedico { get; set; }
    public int IdPaciente { get; set; }
    public string NombreMedico { get; set; } = string.Empty;
    public string NombrePaciente { get; set; } = string.Empty;
    public string? Sintomas { get; set; }
    public string? Recomendaciones { get; set; }
    public string? Diagnostico { get; set; }
    public DateTime FechaConsulta { get; set; }
}
