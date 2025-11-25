using SistemaChamados.Shared.DTOs;

namespace SistemaChamados.Services
{
    public interface IOpenAIService
    {
        Task<AnaliseChamadoResponseDto?> AnalisarChamadoAsync(string descricaoProblema);
    }
}