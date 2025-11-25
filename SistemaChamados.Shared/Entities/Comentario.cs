using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaChamados.Shared.Entities
{
    public class Comentario
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(1000, ErrorMessage = "Comentário não pode exceder 1000 caracteres")]
        public string Texto { get; set; } = string.Empty;

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        // Chaves Estrangeiras
        public int ChamadoId { get; set; }
        public int UsuarioId { get; set; }

        // Propriedades de Navegação
        [ForeignKey("ChamadoId")]
        public virtual Chamado Chamado { get; set; } = null!;

        [ForeignKey("UsuarioId")]
        public virtual Usuario Usuario { get; set; } = null!;
    }
}