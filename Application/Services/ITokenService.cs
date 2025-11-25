using SistemaChamados.Shared.Entities;

namespace SistemaChamados.Application.Services;

public interface ITokenService
{
    string GenerateToken(Usuario usuario);
}