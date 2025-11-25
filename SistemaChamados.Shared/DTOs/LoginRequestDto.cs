using System.ComponentModel.DataAnnotations;

namespace SistemaChamados.Shared.DTOs;

public class LoginRequestDto
{
    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email deve ter um formato válido")]
    public string Email { get; set; } = string.Empty;

    // [Required(ErrorMessage = "Senha é obrigatória")] // Comentado para diagnóstico
    // [MinLength(6, ErrorMessage = "Senha deve ter pelo menos 6 caracteres")] // Comentado para diagnóstico
    public string Senha { get; set; } = string.Empty;
}