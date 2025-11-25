using System.ComponentModel.DataAnnotations;

namespace SistemaChamados.Shared.DTOs
{
    public class RegistrarTecnicoDto
    {
        [Required(ErrorMessage = "Nome completo é obrigatório")]
        public string NomeCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email deve ter um formato válido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Senha é obrigatória")]
        [MinLength(6, ErrorMessage = "Senha deve ter pelo menos 6 caracteres")]
        public string Senha { get; set; } = string.Empty;

        // ID da Categoria que o técnico é especialista
        [Range(1, int.MaxValue, ErrorMessage = "Especialidade é obrigatória")]
        public int EspecialidadeCategoriaId { get; set; }
    }
}