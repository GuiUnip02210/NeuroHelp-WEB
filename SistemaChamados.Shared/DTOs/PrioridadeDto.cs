namespace SistemaChamados.Shared.DTOs;

public class PrioridadeDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int Nivel { get; set; }

    // Helper para cor baseado no nível (Design System)
    public string CorHexadecimal => Nivel switch
    {
        4 => "#EF4444", // 🔴 Crítica - Danger/Vermelho
        3 => "#2A5FDF", // 🔵 Alta - Primary/Azul
        2 => "#F59E0B", // 🟠 Média - Warning/Laranja
        _ => "#8C9AB6"  // ⚪ Baixa - Gray/Cinza
    };
}