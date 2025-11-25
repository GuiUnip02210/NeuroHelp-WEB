using System;

namespace SistemaChamados.Shared.DTOs
{
    public class ComentarioResponseDto
    {
        public int Id { get; set; }
        public string Texto { get; set; } = string.Empty;
        public DateTime DataCriacao { get; set; }
        public int UsuarioId { get; set; }
        public string UsuarioNome { get; set; } = string.Empty;
        public int ChamadoId { get; set; }
    }
}