namespace SistemaChamados.Shared.DTOs;

public class UsuarioResumoDto
{
    public int Id { get; set; }
    public string NomeCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int TipoUsuario { get; set; } // 1=Colaborador, 2=Técnico TI, 3=Admin
}