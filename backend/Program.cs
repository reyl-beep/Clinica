using System.Data;
using Clinica.Api.Infrastructure;
using Clinica.Api.Models;
using Clinica.Api.Models.Requests;

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

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseCors();

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
    var resultado = await db.ExecuteAsync(
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

    return Results.Ok(resultado);
});

var medicos = app.MapGroup("/api/medicos");
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

var pacientes = app.MapGroup("/api/pacientes");
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

var usuarios = app.MapGroup("/api/usuarios");
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

var consultas = app.MapGroup("/api/consultas");
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
