namespace Clinica.Api.Models;

public class AuthSessionDto
{
    public required string Token { get; init; }

    public required UsuarioDto Usuario { get; init; }
}
