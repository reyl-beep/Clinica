using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Clinica.Api.Infrastructure;
using Clinica.Api.Models;
using Clinica.Api.Models.Requests;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<DatabaseService>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var jwtSection = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSection["Key"];
if (string.IsNullOrWhiteSpace(secretKey))
{
    throw new InvalidOperationException("La clave secreta JWT no está configurada. Usa la sección Jwt:Key en appsettings.");
}

var issuer = jwtSection["Issuer"];
var audience = jwtSection["Audience"];
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
var validateIssuer = !string.IsNullOrWhiteSpace(issuer);
var validateAudience = !string.IsNullOrWhiteSpace(audience);

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = validateIssuer,
            ValidIssuer = validateIssuer ? issuer : null,
            ValidateAudience = validateAudience,
            ValidAudience = validateAudience ? audience : null,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Redirect("/swagger"));

app.MapPost("/api/demo", (DemoRequest request) =>
{
    return Results.Ok(new Resultado
    {
        Value = true,
        Message = string.IsNullOrWhiteSpace(request.Message)
            ? "Operación ejecutada correctamente."
            : request.Message,
        Data = new { timestamp = DateTimeOffset.UtcNow }
    });
});

var auth = app.MapGroup("/api/auth");
auth.MapPost("/login", async (LoginRequest request, DatabaseService db) =>
{
    var usuario = await db.ExecuteAsync(
        "procAuthLogin",
        parameters =>
        {
            parameters.Add("@pCorreo", SqlDbType.VarChar, 200).Value = request.Correo;
            parameters.Add("@pPassword", SqlDbType.VarChar, 500).Value = request.Password;
        },
        async reader =>
        {
            if (await reader.ReadAsync())
            {
                return new UsuarioDto
                {
                    Id = reader.GetInt32("Id"),
                    Correo = reader.GetString("Correo"),
                    NombreCompleto = reader.GetString("NombreCompleto"),
                    IdMedico = reader.GetNullableInt32("IdMedico"),
                    Activo = reader.GetBoolean("Activo"),
                    FechaCreacion = reader.GetDateTime("FechaCreacion")
                };
            }

            return null;
        });

    if (usuario is null)
    {
        return Results.Json(
            new Resultado
            {
                Value = false,
                Message = "Credenciales incorrectas. Verifica tu correo y contraseña.",
                Data = null
            },
            statusCode: StatusCodes.Status401Unauthorized);
    }

    if (!usuario.Activo)
    {
        return Results.Json(
            new Resultado
            {
                Value = false,
                Message = "Tu cuenta está inactiva. Contacta al administrador para restablecer el acceso.",
                Data = null
            },
            statusCode: StatusCodes.Status403Forbidden);
    }

    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
        new(ClaimTypes.Name, usuario.NombreCompleto),
        new(JwtRegisteredClaimNames.Sub, usuario.Correo),
        new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

    if (usuario.IdMedico.HasValue)
    {
        claims.Add(new Claim("idMedico", usuario.IdMedico.Value.ToString()));
    }

    var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

    var tokenDescriptor = new JwtSecurityToken(
        issuer: issuer,
        audience: audience,
        claims: claims,
        expires: DateTime.UtcNow.AddHours(8),
        signingCredentials: credentials);

    var token = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

    var session = new AuthSessionDto
    {
        Token = token,
        Usuario = usuario
    };

    return Results.Ok(new Resultado
    {
        Value = true,
        Message = "Inicio de sesión exitoso.",
        Data = session
    });
});

var medicos = app.MapGroup("/api/medicos").RequireAuthorization();
medicos.MapGet(string.Empty, async (DatabaseService db) =>
{
    var resultado = await db.ExecuteAsync(
        "procCatMedicosCon",
        parameters =>
        {
            var id = parameters.Add("@pId", SqlDbType.Int);
            id.Value = DBNull.Value;
        },
        async reader =>
        {
            var items = new List<MedicoDto>();
            while (await reader.ReadAsync())
            {
                items.Add(new MedicoDto
                {
                    Id = reader.GetInt32("Id"),
                    PrimerNombre = reader.GetString("PrimerNombre"),
                    SegundoNombre = reader.GetNullableString("SegundoNombre"),
                    ApellidoPaterno = reader.GetString("ApellidoPaterno"),
                    ApellidoMaterno = reader.GetNullableString("ApellidoMaterno"),
                    Cedula = reader.GetString("Cedula"),
                    Telefono = reader.GetNullableString("Telefono"),
                    Especialidad = reader.GetNullableString("Especialidad"),
                    Email = reader.GetNullableString("Email"),
                    Activo = reader.GetBoolean("Activo"),
                    FechaCreacion = reader.GetDateTime("FechaCreacion")
                });
            }

            return items;
        });

    return Results.Ok(resultado);
});

medicos.MapGet("/{id:int}", async (int id, DatabaseService db) =>
{
    var resultado = await db.ExecuteAsync(
        "procCatMedicosCon",
        parameters =>
        {
            var idParam = parameters.Add("@pId", SqlDbType.Int);
            idParam.Value = id;
        },
        async reader =>
        {
            MedicoDto? medico = null;
            if (await reader.ReadAsync())
            {
                medico = new MedicoDto
                {
                    Id = reader.GetInt32("Id"),
                    PrimerNombre = reader.GetString("PrimerNombre"),
                    SegundoNombre = reader.GetNullableString("SegundoNombre"),
                    ApellidoPaterno = reader.GetString("ApellidoPaterno"),
                    ApellidoMaterno = reader.GetNullableString("ApellidoMaterno"),
                    Cedula = reader.GetString("Cedula"),
                    Telefono = reader.GetNullableString("Telefono"),
                    Especialidad = reader.GetNullableString("Especialidad"),
                    Email = reader.GetNullableString("Email"),
                    Activo = reader.GetBoolean("Activo"),
                    FechaCreacion = reader.GetDateTime("FechaCreacion")
                };
            }

            return medico;
        });

    return Results.Ok(resultado);
});

medicos.MapPost(string.Empty, async (MedicoCreateRequest request, DatabaseService db) =>
{
    var resultado = await db.ExecuteAsync(
        "procCatMedicosIns",
        parameters =>
        {
            parameters.Add("@pPrimerNombre", SqlDbType.VarChar, 100).Value = request.PrimerNombre;
            parameters.Add("@pSegundoNombre", SqlDbType.VarChar, 100).Value = (object?)request.SegundoNombre ?? DBNull.Value;
            parameters.Add("@pApellidoPaterno", SqlDbType.VarChar, 100).Value = request.ApellidoPaterno;
            parameters.Add("@pApellidoMaterno", SqlDbType.VarChar, 100).Value = (object?)request.ApellidoMaterno ?? DBNull.Value;
            parameters.Add("@pCedula", SqlDbType.VarChar, 50).Value = request.Cedula;
            parameters.Add("@pTelefono", SqlDbType.VarChar, 20).Value = (object?)request.Telefono ?? DBNull.Value;
            parameters.Add("@pEspecialidad", SqlDbType.VarChar, 200).Value = (object?)request.Especialidad ?? DBNull.Value;
            parameters.Add("@pEmail", SqlDbType.VarChar, 200).Value = (object?)request.Email ?? DBNull.Value;
        },
        async reader =>
        {
            if (await reader.ReadAsync())
            {
                return new { id = reader.GetInt32(0) };
            }

            return null;
        });

    return Results.Ok(resultado);
});

medicos.MapPut("/{id:int}", async (int id, MedicoCreateRequest request, DatabaseService db) =>
{
    var resultado = await db.ExecuteAsync(
        "procCatMedicosUpd",
        parameters =>
        {
            parameters.Add("@pId", SqlDbType.Int).Value = id;
            parameters.Add("@pPrimerNombre", SqlDbType.VarChar, 100).Value = request.PrimerNombre;
            parameters.Add("@pSegundoNombre", SqlDbType.VarChar, 100).Value = (object?)request.SegundoNombre ?? DBNull.Value;
            parameters.Add("@pApellidoPaterno", SqlDbType.VarChar, 100).Value = request.ApellidoPaterno;
            parameters.Add("@pApellidoMaterno", SqlDbType.VarChar, 100).Value = (object?)request.ApellidoMaterno ?? DBNull.Value;
            parameters.Add("@pCedula", SqlDbType.VarChar, 50).Value = request.Cedula;
            parameters.Add("@pTelefono", SqlDbType.VarChar, 20).Value = (object?)request.Telefono ?? DBNull.Value;
            parameters.Add("@pEspecialidad", SqlDbType.VarChar, 200).Value = (object?)request.Especialidad ?? DBNull.Value;
            parameters.Add("@pEmail", SqlDbType.VarChar, 200).Value = (object?)request.Email ?? DBNull.Value;
        },
        async reader =>
        {
            if (await reader.ReadAsync())
            {
                return new { id = reader.GetInt32(0) };
            }

            return null;
        });

    return Results.Ok(resultado);
});

medicos.MapDelete("/{id:int}", async (int id, DatabaseService db) =>
{
    var resultado = await db.ExecuteAsync(
        "procCatMedicosDel",
        parameters =>
        {
            parameters.Add("@pId", SqlDbType.Int).Value = id;
        },
        async reader =>
        {
            if (await reader.ReadAsync())
            {
                return new { id = reader.GetInt32(0) };
            }

            return null;
        });

    return Results.Ok(resultado);
});

var pacientes = app.MapGroup("/api/pacientes").RequireAuthorization();
pacientes.MapGet(string.Empty, async (DatabaseService db) =>
{
    var resultado = await db.ExecuteAsync(
        "procCatPacientesCon",
        parameters =>
        {
            var id = parameters.Add("@pId", SqlDbType.Int);
            id.Value = DBNull.Value;
        },
        async reader =>
        {
            var items = new List<PacienteDto>();
            while (await reader.ReadAsync())
            {
                items.Add(new PacienteDto
                {
                    Id = reader.GetInt32("Id"),
                    PrimerNombre = reader.GetString("PrimerNombre"),
                    SegundoNombre = reader.GetNullableString("SegundoNombre"),
                    ApellidoPaterno = reader.GetString("ApellidoPaterno"),
                    ApellidoMaterno = reader.GetNullableString("ApellidoMaterno"),
                    Telefono = reader.GetNullableString("Telefono"),
                    Activo = reader.GetBoolean("Activo"),
                    FechaCreacion = reader.GetDateTime("FechaCreacion")
                });
            }

            return items;
        });

    return Results.Ok(resultado);
});

pacientes.MapGet("/{id:int}", async (int id, DatabaseService db) =>
{
    var resultado = await db.ExecuteAsync(
        "procCatPacientesCon",
        parameters =>
        {
            parameters.Add("@pId", SqlDbType.Int).Value = id;
        },
        async reader =>
        {
            PacienteDto? paciente = null;
            if (await reader.ReadAsync())
            {
                paciente = new PacienteDto
                {
                    Id = reader.GetInt32("Id"),
                    PrimerNombre = reader.GetString("PrimerNombre"),
                    SegundoNombre = reader.GetNullableString("SegundoNombre"),
                    ApellidoPaterno = reader.GetString("ApellidoPaterno"),
                    ApellidoMaterno = reader.GetNullableString("ApellidoMaterno"),
                    Telefono = reader.GetNullableString("Telefono"),
                    Activo = reader.GetBoolean("Activo"),
                    FechaCreacion = reader.GetDateTime("FechaCreacion")
                };
            }

            return paciente;
        });

    return Results.Ok(resultado);
});

pacientes.MapPost(string.Empty, async (PacienteCreateRequest request, DatabaseService db) =>
{
    var resultado = await db.ExecuteAsync(
        "procCatPacientesIns",
        parameters =>
        {
            parameters.Add("@pPrimerNombre", SqlDbType.VarChar, 100).Value = request.PrimerNombre;
            parameters.Add("@pSegundoNombre", SqlDbType.VarChar, 100).Value = (object?)request.SegundoNombre ?? DBNull.Value;
            parameters.Add("@pApellidoPaterno", SqlDbType.VarChar, 100).Value = request.ApellidoPaterno;
            parameters.Add("@pApellidoMaterno", SqlDbType.VarChar, 100).Value = (object?)request.ApellidoMaterno ?? DBNull.Value;
            parameters.Add("@pTelefono", SqlDbType.VarChar, 20).Value = (object?)request.Telefono ?? DBNull.Value;
        },
        async reader =>
        {
            if (await reader.ReadAsync())
            {
                return new { id = reader.GetInt32(0) };
            }

            return null;
        });

    return Results.Ok(resultado);
});

pacientes.MapPut("/{id:int}", async (int id, PacienteCreateRequest request, DatabaseService db) =>
{
    var resultado = await db.ExecuteAsync(
        "procCatPacientesUpd",
        parameters =>
        {
            parameters.Add("@pId", SqlDbType.Int).Value = id;
            parameters.Add("@pPrimerNombre", SqlDbType.VarChar, 100).Value = request.PrimerNombre;
            parameters.Add("@pSegundoNombre", SqlDbType.VarChar, 100).Value = (object?)request.SegundoNombre ?? DBNull.Value;
            parameters.Add("@pApellidoPaterno", SqlDbType.VarChar, 100).Value = request.ApellidoPaterno;
            parameters.Add("@pApellidoMaterno", SqlDbType.VarChar, 100).Value = (object?)request.ApellidoMaterno ?? DBNull.Value;
            parameters.Add("@pTelefono", SqlDbType.VarChar, 20).Value = (object?)request.Telefono ?? DBNull.Value;
        },
        async reader =>
        {
            if (await reader.ReadAsync())
            {
                return new { id = reader.GetInt32(0) };
            }

            return null;
        });

    return Results.Ok(resultado);
});

pacientes.MapDelete("/{id:int}", async (int id, DatabaseService db) =>
{
    var resultado = await db.ExecuteAsync(
        "procCatPacientesDel",
        parameters =>
        {
            parameters.Add("@pId", SqlDbType.Int).Value = id;
        },
        async reader =>
        {
            if (await reader.ReadAsync())
            {
                return new { id = reader.GetInt32(0) };
            }

            return null;
        });

    return Results.Ok(resultado);
});

var usuarios = app.MapGroup("/api/usuarios").RequireAuthorization();
usuarios.MapGet(string.Empty, async (DatabaseService db) =>
{
    var resultado = await db.ExecuteAsync(
        "procCatUsuariosCon",
        parameters =>
        {
            var id = parameters.Add("@pId", SqlDbType.Int);
            id.Value = DBNull.Value;
        },
        async reader =>
        {
            var items = new List<UsuarioDto>();
            while (await reader.ReadAsync())
            {
                items.Add(new UsuarioDto
                {
                    Id = reader.GetInt32("Id"),
                    Correo = reader.GetString("Correo"),
                    NombreCompleto = reader.GetString("NombreCompleto"),
                    IdMedico = reader.GetNullableInt32("IdMedico"),
                    Activo = reader.GetBoolean("Activo"),
                    FechaCreacion = reader.GetDateTime("FechaCreacion")
                });
            }

            return items;
        });

    return Results.Ok(resultado);
});

usuarios.MapGet("/{id:int}", async (int id, DatabaseService db) =>
{
    var resultado = await db.ExecuteAsync(
        "procCatUsuariosCon",
        parameters =>
        {
            parameters.Add("@pId", SqlDbType.Int).Value = id;
        },
        async reader =>
        {
            UsuarioDto? usuario = null;
            if (await reader.ReadAsync())
            {
                usuario = new UsuarioDto
                {
                    Id = reader.GetInt32("Id"),
                    Correo = reader.GetString("Correo"),
                    NombreCompleto = reader.GetString("NombreCompleto"),
                    IdMedico = reader.GetNullableInt32("IdMedico"),
                    Activo = reader.GetBoolean("Activo"),
                    FechaCreacion = reader.GetDateTime("FechaCreacion")
                };
            }

            return usuario;
        });

    return Results.Ok(resultado);
});

usuarios.MapPost(string.Empty, async (UsuarioCreateRequest request, DatabaseService db) =>
{
    var resultado = await db.ExecuteAsync(
        "procCatUsuariosIns",
        parameters =>
        {
            parameters.Add("@pCorreo", SqlDbType.VarChar, 200).Value = request.Correo;
            parameters.Add("@pPassword", SqlDbType.VarChar, 500).Value = request.Password;
            parameters.Add("@pNombreCompleto", SqlDbType.VarChar, 300).Value = request.NombreCompleto;
            parameters.Add("@pIdMedico", SqlDbType.Int).Value = (object?)request.IdMedico ?? DBNull.Value;
        },
        async reader =>
        {
            if (await reader.ReadAsync())
            {
                return new { id = reader.GetInt32(0) };
            }

            return null;
        });

    return Results.Ok(resultado);
});

usuarios.MapPut("/{id:int}", async (int id, UsuarioCreateRequest request, DatabaseService db) =>
{
    var resultado = await db.ExecuteAsync(
        "procCatUsuariosUpd",
        parameters =>
        {
            parameters.Add("@pId", SqlDbType.Int).Value = id;
            parameters.Add("@pCorreo", SqlDbType.VarChar, 200).Value = request.Correo;
            parameters.Add("@pPassword", SqlDbType.VarChar, 500).Value = request.Password;
            parameters.Add("@pNombreCompleto", SqlDbType.VarChar, 300).Value = request.NombreCompleto;
            parameters.Add("@pIdMedico", SqlDbType.Int).Value = (object?)request.IdMedico ?? DBNull.Value;
        },
        async reader =>
        {
            if (await reader.ReadAsync())
            {
                return new { id = reader.GetInt32(0) };
            }

            return null;
        });

    return Results.Ok(resultado);
});

usuarios.MapDelete("/{id:int}", async (int id, DatabaseService db) =>
{
    var resultado = await db.ExecuteAsync(
        "procCatUsuariosDel",
        parameters =>
        {
            parameters.Add("@pId", SqlDbType.Int).Value = id;
        },
        async reader =>
        {
            if (await reader.ReadAsync())
            {
                return new { id = reader.GetInt32(0) };
            }

            return null;
        });

    return Results.Ok(resultado);
});

var consultas = app.MapGroup("/api/consultas").RequireAuthorization();
consultas.MapGet(string.Empty, async (DatabaseService db) =>
{
    var resultado = await db.ExecuteAsync(
        "procConsultasCon",
        parameters =>
        {
            var id = parameters.Add("@pId", SqlDbType.Int);
            id.Value = DBNull.Value;
        },
        async reader =>
        {
            var items = new List<ConsultaDto>();
            while (await reader.ReadAsync())
            {
                items.Add(new ConsultaDto
                {
                    Id = reader.GetInt32("Id"),
                    IdMedico = reader.GetInt32("IdMedico"),
                    IdPaciente = reader.GetInt32("IdPaciente"),
                    Sintomas = reader.GetNullableString("Sintomas"),
                    Recomendaciones = reader.GetNullableString("Recomendaciones"),
                    Diagnostico = reader.GetNullableString("Diagnostico"),
                    FechaConsulta = reader.GetDateTime("FechaConsulta")
                });
            }

            return items;
        });

    return Results.Ok(resultado);
});

consultas.MapGet("/{id:int}", async (int id, DatabaseService db) =>
{
    var resultado = await db.ExecuteAsync(
        "procConsultasCon",
        parameters =>
        {
            parameters.Add("@pId", SqlDbType.Int).Value = id;
        },
        async reader =>
        {
            ConsultaDto? consulta = null;
            if (await reader.ReadAsync())
            {
                consulta = new ConsultaDto
                {
                    Id = reader.GetInt32("Id"),
                    IdMedico = reader.GetInt32("IdMedico"),
                    IdPaciente = reader.GetInt32("IdPaciente"),
                    Sintomas = reader.GetNullableString("Sintomas"),
                    Recomendaciones = reader.GetNullableString("Recomendaciones"),
                    Diagnostico = reader.GetNullableString("Diagnostico"),
                    FechaConsulta = reader.GetDateTime("FechaConsulta")
                };
            }

            return consulta;
        });

    return Results.Ok(resultado);
});

consultas.MapGet("/historial", async (
    int? idMedico,
    int? idPaciente,
    DateTime? fechaInicio,
    DateTime? fechaFin,
    DatabaseService db) =>
{
    var resultado = await db.ExecuteAsync(
        "procConsultasHistorial",
        parameters =>
        {
            var medicoParam = parameters.Add("@pIdMedico", SqlDbType.Int);
            medicoParam.Value = (object?)idMedico ?? DBNull.Value;

            var pacienteParam = parameters.Add("@pIdPaciente", SqlDbType.Int);
            pacienteParam.Value = (object?)idPaciente ?? DBNull.Value;

            parameters.Add("@pFechaInicio", SqlDbType.DateTime).Value = (object?)fechaInicio ?? DBNull.Value;
            parameters.Add("@pFechaFin", SqlDbType.DateTime).Value = (object?)fechaFin ?? DBNull.Value;
        },
        async reader =>
        {
            var items = new List<ConsultaHistorialDto>();
            while (await reader.ReadAsync())
            {
                items.Add(new ConsultaHistorialDto
                {
                    Id = reader.GetInt32("Id"),
                    IdMedico = reader.GetInt32("IdMedico"),
                    IdPaciente = reader.GetInt32("IdPaciente"),
                    NombreMedico = reader.GetString("NombreMedico"),
                    NombrePaciente = reader.GetString("NombrePaciente"),
                    Sintomas = reader.GetNullableString("Sintomas"),
                    Recomendaciones = reader.GetNullableString("Recomendaciones"),
                    Diagnostico = reader.GetNullableString("Diagnostico"),
                    FechaConsulta = reader.GetDateTime("FechaConsulta")
                });
            }

            return items;
        });

    return Results.Ok(resultado);
});

consultas.MapPost(string.Empty, async (ConsultaCreateRequest request, DatabaseService db) =>
{
    var resultado = await db.ExecuteAsync(
        "procConsultasIns",
        parameters =>
        {
            parameters.Add("@pIdMedico", SqlDbType.Int).Value = request.IdMedico;
            parameters.Add("@pIdPaciente", SqlDbType.Int).Value = request.IdPaciente;
            parameters.Add("@pSintomas", SqlDbType.VarChar, -1).Value = (object?)request.Sintomas ?? DBNull.Value;
            parameters.Add("@pRecomendaciones", SqlDbType.VarChar, -1).Value = (object?)request.Recomendaciones ?? DBNull.Value;
            parameters.Add("@pDiagnostico", SqlDbType.VarChar, -1).Value = (object?)request.Diagnostico ?? DBNull.Value;
            parameters.Add("@pFechaConsulta", SqlDbType.DateTime).Value = (object?)request.FechaConsulta ?? DBNull.Value;
        },
        async reader =>
        {
            if (await reader.ReadAsync())
            {
                return new { id = reader.GetInt32(0) };
            }

            return null;
        });

    return Results.Ok(resultado);
});

consultas.MapPut("/{id:int}", async (int id, ConsultaCreateRequest request, DatabaseService db) =>
{
    var resultado = await db.ExecuteAsync(
        "procConsultasUpd",
        parameters =>
        {
            parameters.Add("@pId", SqlDbType.Int).Value = id;
            parameters.Add("@pIdMedico", SqlDbType.Int).Value = request.IdMedico;
            parameters.Add("@pIdPaciente", SqlDbType.Int).Value = request.IdPaciente;
            parameters.Add("@pSintomas", SqlDbType.VarChar, -1).Value = (object?)request.Sintomas ?? DBNull.Value;
            parameters.Add("@pRecomendaciones", SqlDbType.VarChar, -1).Value = (object?)request.Recomendaciones ?? DBNull.Value;
            parameters.Add("@pDiagnostico", SqlDbType.VarChar, -1).Value = (object?)request.Diagnostico ?? DBNull.Value;
            parameters.Add("@pFechaConsulta", SqlDbType.DateTime).Value = (object?)request.FechaConsulta ?? DBNull.Value;
        },
        async reader =>
        {
            if (await reader.ReadAsync())
            {
                return new { id = reader.GetInt32(0) };
            }

            return null;
        });

    return Results.Ok(resultado);
});

consultas.MapDelete("/{id:int}", async (int id, DatabaseService db) =>
{
    var resultado = await db.ExecuteAsync(
        "procConsultasDel",
        parameters =>
        {
            parameters.Add("@pId", SqlDbType.Int).Value = id;
        },
        async reader =>
        {
            if (await reader.ReadAsync())
            {
                return new { id = reader.GetInt32(0) };
            }

            return null;
        });

    return Results.Ok(resultado);
});

app.Run();
