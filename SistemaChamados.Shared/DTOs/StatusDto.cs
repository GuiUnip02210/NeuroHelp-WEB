namespace SistemaChamados.Shared.DTOs;

public class StatusDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;

    // Helper para cor baseado no nome
    public string CorHexadecimal => Nome?.ToLower() switch
    {
        "aberto" => "#2A5FDF",        // Azul
        "em andamento" => "#F59E0B",  // Laranja
        "encerrado" => "#10B981",     // Verde
        "cancelado" => "#EF4444",     // Vermelho
        _ => "#8C9AB6"                // Cinza
    };
}