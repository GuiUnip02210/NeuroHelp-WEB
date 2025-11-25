using System;

namespace SistemaChamados.Shared.DTOs;

/// <summary>
/// Representa um item na timeline de histórico do chamado
/// </summary>
public class HistoricoItemDto
{
    public int Id { get; set; }
    public DateTime DataHora { get; set; }
    public string TipoEvento { get; set; } = string.Empty; // "Criacao", "MudancaStatus", "Atribuicao", "Comentario", "Fechamento"
    public string Descricao { get; set; } = string.Empty;
    public string? Usuario { get; set; }
    public string? ValorAnterior { get; set; }
    public string? ValorNovo { get; set; }

    // Helpers para UI
    public string IconeEvento => TipoEvento switch
    {
        "Criacao" => "🆕",
        "MudancaStatus" => "📊",
        "Atribuicao" => "👤",
        "Comentario" => "💬",
        "Fechamento" => "✅",
        "Reabertura" => "🔄",
        _ => "📝"
    };

    public string CorEvento => TipoEvento switch
    {
        "Criacao" => "#2A5FDF",      // Primary
        "MudancaStatus" => "#F59E0B", // Warning
        "Atribuicao" => "#8B5CF6",   // Purple
        "Comentario" => "#6B7280",   // Gray
        "Fechamento" => "#10B981",   // Success
        "Reabertura" => "#EF4444",   // Danger
        _ => "#6B7280"               // Gray
    };

    public string DataHoraFormatada => $"{DataHora:dd/MM/yyyy} às {DataHora:HH:mm}";

    public string DescricaoCompleta
    {
        get
        {
            if (string.IsNullOrEmpty(ValorAnterior) && string.IsNullOrEmpty(ValorNovo))
                return Descricao;

            if (string.IsNullOrEmpty(ValorAnterior))
                return $"{Descricao}: {ValorNovo}";

            return $"{Descricao}: {ValorAnterior} → {ValorNovo}";
        }
    }
}