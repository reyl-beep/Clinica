namespace Clinica.Api.Models.Requests;

public class UsuarioCreateRequest
{
    public string Correo { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public int? IdMedico { get; set; }
}
