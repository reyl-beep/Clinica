namespace Clinica.Api.Models;

public class UsuarioDto
{
    public int Id { get; set; }
    public string Correo { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public int? IdMedico { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
}
