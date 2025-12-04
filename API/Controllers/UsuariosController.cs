using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SistemaChamados.Shared.DTOs;
using SistemaChamados.Application.Services;
using SistemaChamados.Shared.Entities;
using SistemaChamados.Data;
using BCrypt.Net;

namespace SistemaChamados.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly ILogger<UsuariosController> _logger;

    public UsuariosController(ApplicationDbContext context, ITokenService tokenService, ILogger<UsuariosController> logger)
    {
        _context = context;
        _tokenService = tokenService;
        _logger = logger;
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
            TipoUsuario = 1, // Sempre 1 para compatibilidade com JWT
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

        return Ok(new LoginResponseDto { Token = token, TipoUsuario = 1 }); // Sempre retorna TipoUsuario = 1
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
}