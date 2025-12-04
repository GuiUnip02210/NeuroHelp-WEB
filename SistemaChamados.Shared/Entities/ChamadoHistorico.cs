using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaChamados.Shared.Entities;

[Table("HISTORICO_CHAMADOS")]
public class ChamadoHistorico
{
    [Key]
    [Column("ID_DO_CASO")]
    public int IdDoCaso { get; set; }
    
    [Column("TIPO")]
    public string? Tipo { get; set; }
    
    [Column("RESUMO")]
    public string? Resumo { get; set; }
    
    [Column("DESCRICAO")]
    public string? Descricao { get; set; }
    
    [Required]
    [Column("DATA_ABERTURA")]
    public DateTime DataAbertura { get; set; }
    
    [Column("PRIORIDADE")]
    public string? Prioridade { get; set; }
    
    [Column("CATEGORIA")]
    public string? Categoria { get; set; }
    
    [Column("STATUS")]
    public string? Status { get; set; }
    
    [Column("ATRIBUIDO")]
    public string? Atribuido { get; set; }
    
    [Column("GRUPO_ATRIBUIDO")]
    public string? GrupoAtribuido { get; set; }
    
    [Column("LOCALIZACAO_AFETADA")]
    public string? LocalizacaoAfetada { get; set; }
    
    [Column("DATA_RESOLUCAO")]
    public DateTime? DataResolucao { get; set; }
    
    [Column("VIOLACAO_PROJETADA")]
    public DateTime? ViolacaoProjetada { get; set; }
    
    [Column("RELATADO_POR")]
    public string? RelatadoPor { get; set; }
    
    [Column("METODO_RELATADO")]
    public string? MetodoRelatado { get; set; }
    
    [Column("CATEGORIA_REPORTE")]
    public string? CategoriaReporte { get; set; }
    
    [Column("ULTIMA_MODIFICACAO")]
    public DateTime? UltimaModificacao { get; set; }
    
    [Column("USUARIO_FINAL_AFETADO")]
    public string? UsuarioFinalAfetado { get; set; }
    
    [Column("EMAIL_USUARIO_FINAL")]
    public string? EmailUsuarioFinal { get; set; }
    
    [Column("CPF_USUARIO_FINAL")]
    public string? CpfUsuarioFinal { get; set; }
    
    [Column("DESCRICAO_SOLUCAO")]
    public string? DescricaoSolucao { get; set; }
}