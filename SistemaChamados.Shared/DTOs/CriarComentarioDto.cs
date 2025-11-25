using System.ComponentModel.DataAnnotations;

namespace SistemaChamados.Shared.DTOs
{
    public class CriarComentarioDto
    {
        [Required(ErrorMessage = "O texto do comentário é obrigatório")]
        [StringLength(1000, MinimumLength = 1, ErrorMessage = "Comentário deve ter entre 1 e 1000 caracteres")]
        public string Texto { get; set; } = string.Empty;
    }
}