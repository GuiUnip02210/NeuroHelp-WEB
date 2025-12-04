using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SistemaChamados.Application.Services;
using SistemaChamados.Data;
using SistemaChamados.Shared.Entities;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configurar Entity Framework
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    // Se a string de conexão estiver vazia, lance uma exceção clara.
    throw new InvalidOperationException("A string de conexão 'DefaultConnection' não está configurada. Configure o PostgreSQL no appsettings.json.");
}

// Configuração para PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
    
// Registrar serviços
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Garante que não haja ciclos de referência (mantendo a funcionalidade original)
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles; 
        
    });

// Configurar CORS para permitir o frontend (Live Server) E A NOVA APLICAÇÃO WEB
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(
                  "http://127.0.0.1:5500", "http://localhost:5500", // Live Server antigo
                  "http://localhost:5027", "https://localhost:7086"  // Portas padrão do ASP.NET MVC Web App (verificadas no launchSettings.json)
              ) 
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configurar autenticação JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });
builder.Services.AddAuthorization();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Adiciona a definição de segurança para o Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Description = "Insira o token JWT desta forma: Bearer {seu token}"
    });

    // Adiciona o requisito de segurança que aplica a definição acima
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// Criar banco de dados e adicionar dados de teste
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    // Garantir que o banco de dados seja criado
    context.Database.EnsureCreated();
    
    // Adicionar usuário admin padrão se não existir
    if (!context.Usuarios.Any())
    {
        var adminUser = new Usuario
        {
            NomeCompleto = "Administrador do Sistema",
            Email = "admin@historico.com",
            SenhaHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            DataCadastro = DateTime.UtcNow,
            Ativo = true
        };
        
        context.Usuarios.Add(adminUser);
        context.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configurar arquivos estáticos (deve vir antes de UseRouting)
app.UseDefaultFiles(); // Serve index.html automaticamente
app.UseStaticFiles();  // Serve arquivos estáticos da pasta wwwroot

// Usar CORS (comentado pois não é mais necessário - mesmo servidor)
// app.UseCors("AllowAll");

//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
