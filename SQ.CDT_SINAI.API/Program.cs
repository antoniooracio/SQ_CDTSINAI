using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SQ.CDT_SINAI.API.Data;
using SQ.CDT_SINAI.API.Workers;
using System.Text;
using QuestPDF.Infrastructure;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(x =>
    x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

// Configuração do Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuração do QuestPDF (Licença Community)
QuestPDF.Settings.License = LicenseType.Community;

// Configuração de CORS para permitir chamadas do Frontend (Web)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWeb",
        policy =>
        {
            policy.WithOrigins("http://localhost:5002") // URL do Frontend
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// 1. Configuração do Banco de Dados (MariaDB)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, new MariaDbServerVersion(new Version(10, 6)),
        mySqlOptions => mySqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null)));

// Registrar Serviço de Renovação Automática
builder.Services.AddHostedService<SQ.CDT_SINAI.API.Services.DocumentRenewalWorker>();

// Registrar Worker de Renovação de Contratos
builder.Services.AddHostedService<ContractRenewalWorker>();

// Adicionar HealthChecks (Verifica se a API e o Banco estão respondendo)
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>();

// 2. Configuração do JWT
var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"] ?? "ChavePadraoParaDesenvolvimentoNaoSegura123!");
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

var app = builder.Build();

// Garantir que a pasta de Uploads exista ao iniciar a API
var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "Uploads", "Profiles");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

// Aplicar Migrations automaticamente ao iniciar (Essencial para Docker)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var db = services.GetRequiredService<AppDbContext>();
    
    var maxRetry = 10;
    var delay = TimeSpan.FromSeconds(5);

    for (int i = 0; i < maxRetry; i++)
    {
        try
        {
            db.Database.Migrate();
            logger.LogInformation("Banco de dados migrado com sucesso.");
            break;
        }
        catch (Exception ex)
        {
            if (i == maxRetry - 1)
            {
                logger.LogCritical(ex, "Falha ao migrar o banco de dados após várias tentativas.");
                throw;
            }
            logger.LogWarning($"Tentativa {i + 1} de {maxRetry} falhou. Aguardando banco de dados... Erro: {ex.Message}");
            System.Threading.Thread.Sleep(delay);
        }
    }
}

// Configure the HTTP request pipeline.
// Habilita Swagger sempre para facilitar debug em Docker
app.UseSwagger();
app.UseSwaggerUI();

// app.UseHttpsRedirection();

app.UseCors("AllowWeb");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
