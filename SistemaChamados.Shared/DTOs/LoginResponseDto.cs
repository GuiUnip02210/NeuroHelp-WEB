namespace SistemaChamados.Shared.DTOs;

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public int TipoUsuario { get; set; }
    public UsuarioResponseDto? Usuario { get; set; }
}