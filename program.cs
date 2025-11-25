using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SistemaChamados.Application.Services;
using SistemaChamados.Services;
using SistemaChamados.Data;
using SistemaChamados.Configuration;
using SistemaChamados.Shared.Entities;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configurar Entity Framework
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    // Se a string de conexão estiver vazia, lance uma exceção clara.
    throw new InvalidOperationException("A string de conexão 'DefaultConnection' não está configurada. Configure o SQL Server no appsettings.json.");
}

// Configuração forçada para o SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
    
// Registrar serviços
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IOpenAIService, OpenAIService>();

// Configurar HttpClient para o OpenAIService
builder.Services.AddHttpClient<IOpenAIService, OpenAIService>();

// Configura a seção EmailSettings do appsettings.json
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Registra o EmailService para injeção de dependência
builder.Services.AddTransient<IEmailService, EmailService>();

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
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireClaim("TipoUsuario", "3"));
});

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
    
    // Adicionar dados de seed apenas se não existirem
    if (!context.Usuarios.Any())
    {
        // Criar status
        var statusAberto = new Status { Nome = "Aberto" };
        var statusAndamento = new Status { Nome = "Em Andamento" };
        var statusFechado = new Status { Nome = "Fechado" };
        
        context.Status.AddRange(statusAberto, statusAndamento, statusFechado);
        
        // Criar prioridades
        var prioridadeBaixa = new Prioridade { Nome = "Baixa" };
        var prioridadeMedia = new Prioridade { Nome = "Média" };
        var prioridadeAlta = new Prioridade { Nome = "Alta" };
        
        context.Prioridades.AddRange(prioridadeBaixa, prioridadeMedia, prioridadeAlta);
        
        // Criar categorias
        var categoriaHardware = new Categoria { Nome = "Hardware", Descricao = "Problemas relacionados a equipamentos" };
        var categoriaSoftware = new Categoria { Nome = "Software", Descricao = "Problemas relacionados a programas" };
        var categoriaRede = new Categoria { Nome = "Rede", Descricao = "Problemas relacionados à conectividade" };
        
        context.Categorias.AddRange(categoriaHardware, categoriaSoftware, categoriaRede);
        
        // Criar usuário admin
        var adminUser = new Usuario
        {
            NomeCompleto = "Administrador",
            Email = "admin@helpdesk.com",
            SenhaHash = BCrypt.Net.BCrypt.HashPassword("senha123"),
            TipoUsuario = 3, // Admin
            DataCadastro = DateTime.Now,
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
