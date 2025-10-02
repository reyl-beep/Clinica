namespace Clinica.Api.Models;

public class ConsultaDto
{
    public int Id { get; set; }
    public int IdMedico { get; set; }
    public int IdPaciente { get; set; }
    public string? Sintomas { get; set; }
    public string? Recomendaciones { get; set; }
    public string? Diagnostico { get; set; }
    public DateTime FechaConsulta { get; set; }
}
