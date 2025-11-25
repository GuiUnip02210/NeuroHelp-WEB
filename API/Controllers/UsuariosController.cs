using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SistemaChamados.Shared.DTOs;
using SistemaChamados.Application.Services;
using SistemaChamados.Shared.Entities;
using SistemaChamados.Data;
using SistemaChamados.Services;
using BCrypt.Net;
using System.Security.Claims;
using System.Security.Cryptography;

namespace SistemaChamados.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly ILogger<UsuariosController> _logger;

    public UsuariosController(ApplicationDbContext context, ITokenService tokenService, IEmailService emailService, ILogger<UsuariosController> logger)
    {
        _context = context;
        _tokenService = tokenService;
        _emailService = emailService;
        _logger = logger;
    }

    [HttpPost("registrar-admin")]
    public async Task<ActionResult<UsuarioResponseDto>> RegistrarAdmin(AdminRegisterDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Verificar se email já existe
        if (await _context.Usuarios.AnyAsync(u => u.Email == dto.Email))
        {
            return BadRequest(new { message = "Email já está em uso" });
        }

        // Criar usuário admin
        var usuario = new Usuario
        {
            NomeCompleto = dto.NomeCompleto,
            Email = dto.Email,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha),
            TipoUsuario = 3, // Admin
            DataCadastro = DateTime.Now,
            Ativo = true
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        var response = new UsuarioResponseDto
        {
            Id = usuario.Id,
            NomeCompleto = usuario.NomeCompleto,
            Email = usuario.Email,
            TipoUsuario = usuario.TipoUsuario,
            DataCadastro = usuario.DataCadastro,
            Ativo = usuario.Ativo
        };

        return CreatedAtAction(nameof(RegistrarAdmin), new { id = usuario.Id }, response);
    }

    [HttpPost("registrar")]
    [AllowAnonymous]
    public async Task<ActionResult<UsuarioResponseDto>> Registrar([FromBody] RegistrarUsuarioDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (await _context.Usuarios.AnyAsync(u => u.Email == dto.Email))
        {
            return BadRequest(new { message = "Email já está em uso" });
        }

        var usuario = new Usuario
        {
            NomeCompleto = dto.NomeCompleto,
            Email = dto.Email,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha),
            TipoUsuario = 1, // 1 = Usuário Comum
            DataCadastro = DateTime.UtcNow,
            Ativo = true
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        var response = new UsuarioResponseDto
        {
            Id = usuario.Id,
            NomeCompleto = usuario.NomeCompleto,
            Email = usuario.Email,
            TipoUsuario = usuario.TipoUsuario,
            DataCadastro = usuario.DataCadastro,
            Ativo = usuario.Ativo
        };

        return CreatedAtAction(nameof(Registrar), new { id = usuario.Id }, response);
    }

    [HttpPost("registrar-tecnico")]
    [Authorize(Policy = "AdminOnly")] // <-- Protege o endpoint
    public async Task<ActionResult<UsuarioResponseDto>> RegistrarTecnico([FromBody] RegistrarTecnicoDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (await _context.Usuarios.AnyAsync(u => u.Email == dto.Email))
        {
            return BadRequest(new { message = "Email já está em uso" });
        }

        // Valida se a categoria de especialidade existe
        var categoriaExiste = await _context.Categorias.AnyAsync(c => c.Id == dto.EspecialidadeCategoriaId);
        if (!categoriaExiste)
        {
            return BadRequest(new { message = "ID da Categoria de especialidade inválido." });
        }

        var usuario = new Usuario
        {
            NomeCompleto = dto.NomeCompleto,
            Email = dto.Email,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha),
            TipoUsuario = 2, // 2 = Técnico
            Ativo = true,
            DataCadastro = DateTime.UtcNow,
            EspecialidadeCategoriaId = dto.EspecialidadeCategoriaId // Define a especialidade
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        var response = new UsuarioResponseDto
        {
            Id = usuario.Id,
            NomeCompleto = usuario.NomeCompleto,
            Email = usuario.Email,
            TipoUsuario = usuario.TipoUsuario,
            DataCadastro = usuario.DataCadastro,
            Ativo = usuario.Ativo
        };

        return CreatedAtAction(nameof(Registrar), new { id = usuario.Id }, response);
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto loginRequest)
    {
        _logger.LogInformation("Tentativa de login recebida para o email: {Email}", loginRequest.Email); // LOG 1
        
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Modelo inválido para o email: {Email}", loginRequest.Email); // LOG 2
            return BadRequest(ModelState);
        }

        // Buscar usuário pelo email
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == loginRequest.Email);

        if (usuario == null)
        {
            _logger.LogWarning("Usuário não encontrado para o email: {Email}", loginRequest.Email); // LOG 3
            return Unauthorized("Email ou senha inválidos.");
        }

        _logger.LogInformation("Usuário encontrado: ID {UserId}, Email {UserEmail}", usuario.Id, usuario.Email); // LOG 4

        // Verificar senha
        bool senhaValida = false;
        try
        {
            // LOG ADICIONAL 1: Ver a senha recebida ANTES da verificação
            _logger.LogInformation("Verificando senha recebida: '{SenhaRecebida}' contra o hash do usuário ID {UserId}", loginRequest.Senha, usuario.Id);
            senhaValida = BCrypt.Net.BCrypt.Verify(loginRequest.Senha, usuario.SenhaHash);
            // LOG ADICIONAL 2: Confirma o resultado IMEDIATAMENTE após
            _logger.LogInformation("Resultado IMEDIATO do BCrypt.Verify para usuário ID {UserId}: {SenhaValida}", usuario.Id, senhaValida);
            _logger.LogInformation("Resultado da verificação de senha para o usuário ID {UserId}: {SenhaValida}", usuario.Id, senhaValida); // LOG 5
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante a verificação de senha para o usuário ID {UserId}", usuario.Id); // LOG 6
            return StatusCode(500, "Erro interno ao verificar a senha.");
        }

        if (!senhaValida)
        {
            _logger.LogWarning("Senha inválida para o usuário ID {UserId}", usuario.Id); // LOG 7
            return Unauthorized("Email ou senha inválidos.");
        }

        // Verificar se usuário está ativo
        if (!usuario.Ativo)
        {
            _logger.LogWarning("Tentativa de login por usuário inativo: ID {UserId}", usuario.Id); // LOG 8
            return Unauthorized("Usuário inativo.");
        }

        // Gerar token JWT
        var token = _tokenService.GenerateToken(usuario);
        _logger.LogInformation("Login bem-sucedido e token gerado para o usuário ID {UserId}", usuario.Id); // LOG 9

        return Ok(new LoginResponseDto { Token = token, TipoUsuario = usuario.TipoUsuario });
    }

    [HttpGet("perfil")]
    [Authorize]
    public IActionResult ObterPerfilUsuario()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized();
        }
        return Ok($"Acesso autorizado. Perfil do usuário com ID: {userId}.");
    }

    [HttpPost("esqueci-senha")]
    [AllowAnonymous] // Permite que usuários não logados acessem este endpoint
    public async Task<IActionResult> EsqueciSenha([FromBody] EsqueciSenhaDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == request.Email);
        
        // Por segurança, não informamos se o usuário foi encontrado ou não.
        // Apenas retornamos Ok para não permitir que alguém descubra e-mails válidos no sistema.
        if (usuario == null)
        {
            return Ok(new { message = "Se um usuário com este e-mail existir, um link de redefinição de senha foi enviado." });
        }

        // 1. Gerar um token seguro e aleatório
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));

        // 2. Salvar o token e a data de expiração no registro do usuário
        usuario.PasswordResetToken = token;
        usuario.ResetTokenExpires = DateTime.UtcNow.AddMinutes(30); // Token expira em 30 minutos

        _context.Usuarios.Update(usuario);
        await _context.SaveChangesAsync();

        // 3. Montar o link de redefinição (a URL do seu frontend)
        // ATUALIZADO PARA A ROTA DO NOVO SITE WEB (porta 5027)
        var resetLink = $"http://localhost:5027/Home/ResetarSenha?token={token}";

        // 4. Enviar o e-mail usando o IEmailService
        var subject = "Redefinição de Senha - Sistema de Chamados";
        var message = $@"
            <h1>Redefinição de Senha</h1>
            <p>Olá, {usuario.NomeCompleto},</p>
            <p>Você solicitou a redefinição da sua senha. Por favor, clique no link abaixo para criar uma nova senha:</p>
            <a href='{resetLink}'>Redefinir Minha Senha</a>
            <p>Se você não solicitou isso, por favor, ignore este e-mail.</p>
            <p>Este link expirará em 30 minutos.</p>";

        try
        {
            await _emailService.SendEmailAsync(usuario.Email, subject, message);
        }
        catch (Exception ex)
        {
            // Log do erro usando o logger apropriado
            // Por segurança, não revelamos detalhes do erro para o usuário
            _logger.LogError(ex, "Erro ao enviar email de redefinição de senha para o usuário ID {UserId}", usuario.Id);
        }

        return Ok(new { message = "Se um usuário com este e-mail existir, um link de redefinição de senha foi enviado." });
    }

    [HttpPost("resetar-senha")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetarSenha([FromBody] ResetarSenhaDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // 1. Encontra o usuário pelo token de redefinição de senha
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.PasswordResetToken == request.Token);
        
        // 2. Valida o token
        if (usuario == null || usuario.ResetTokenExpires < DateTime.UtcNow)
        {
            return BadRequest(new { message = "Token inválido ou expirado." });
        }
        
        // 3. Cria o hash da nova senha
        var novoSenhaHash = BCrypt.Net.BCrypt.HashPassword(request.NovaSenha);
        
        // 4. Atualiza a senha do usuário e limpa os campos do token
        usuario.SenhaHash = novoSenhaHash;
        usuario.PasswordResetToken = null;
        usuario.ResetTokenExpires = null;
        
        _context.Usuarios.Update(usuario);
        await _context.SaveChangesAsync();
        
        return Ok(new { message = "Senha redefinida com sucesso." });
    }

    [HttpGet("tecnicos")]
    [Authorize] // Apenas usuários logados podem ver a lista
    public async Task<ActionResult<IEnumerable<UsuarioResponseDto>>> GetTecnicosAtivos()
    {
        _logger.LogInformation("Buscando lista de técnicos (TipoUsuario == 2) ativos.");
        
        var tecnicos = await _context.Usuarios
            .Where(u => u.TipoUsuario == 2 && u.Ativo)
            .Select(u => new UsuarioResponseDto
            {
                Id = u.Id,
                NomeCompleto = u.NomeCompleto,
                Email = u.Email,
                TipoUsuario = u.TipoUsuario,
                DataCadastro = u.DataCadastro,
                Ativo = u.Ativo
            })
            .ToListAsync();
            
        _logger.LogInformation("Encontrados {Count} técnicos ativos.", tecnicos.Count);

        return Ok(tecnicos);
    }
}