using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaChamados.Data;
using SistemaChamados.Shared.Entities;

namespace SistemaChamados.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Protege todos os endpoints deste controller
public class HistoricoController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HistoricoController> _logger;

    public HistoricoController(ApplicationDbContext context, ILogger<HistoricoController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ChamadoHistorico>>> GetHistorico(
        [FromQuery] int? idDoCaso = null,
        [FromQuery] string? resumo = null,
        [FromQuery] string? descricao = null,
        [FromQuery] string? categoria = null,
        [FromQuery] string? status = null,
        [FromQuery] string? prioridade = null,
        [FromQuery] DateTime? dataAberturaInicio = null,
        [FromQuery] DateTime? dataAberturaFim = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            _logger.LogInformation("Consultando histórico de chamados com filtros: IdDoCaso={IdDoCaso}, Resumo={Resumo}, Descricao={Descricao}", 
                idDoCaso, resumo, descricao);

            var query = _context.ChamadosHistorico.AsQueryable();

            // Aplicar filtros
            if (idDoCaso.HasValue)
            {
                query = query.Where(h => h.IdDoCaso == idDoCaso.Value);
            }

            if (!string.IsNullOrEmpty(resumo))
            {
                query = query.Where(h => h.Resumo != null && h.Resumo.Contains(resumo));
            }

            if (!string.IsNullOrEmpty(descricao))
            {
                query = query.Where(h => h.Descricao != null && h.Descricao.Contains(descricao));
            }

            if (!string.IsNullOrEmpty(categoria))
            {
                query = query.Where(h => h.Categoria != null && h.Categoria.Contains(categoria));
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(h => h.Status != null && h.Status.Contains(status));
            }

            if (!string.IsNullOrEmpty(prioridade))
            {
                query = query.Where(h => h.Prioridade != null && h.Prioridade.Contains(prioridade));
            }

            if (dataAberturaInicio.HasValue)
            {
                query = query.Where(h => h.DataAbertura >= dataAberturaInicio.Value);
            }

            if (dataAberturaFim.HasValue)
            {
                query = query.Where(h => h.DataAbertura <= dataAberturaFim.Value);
            }

            // Aplicar paginação
            var totalRecords = await query.CountAsync();
            var historicos = await query
                .OrderByDescending(h => h.DataAbertura)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            _logger.LogInformation("Encontrados {Count} registros de histórico (página {Page} de {PageSize})", 
                historicos.Count, page, pageSize);

            // Retornar dados com informações de paginação
            var response = new
            {
                Data = historicos,
                TotalRecords = totalRecords,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize)
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar histórico de chamados");
            return StatusCode(500, new { message = "Erro interno do servidor ao consultar histórico" });
        }
    }

    [HttpGet("{idDoCaso}")]
    public async Task<ActionResult<ChamadoHistorico>> GetHistoricoById(int idDoCaso)
    {
        try
        {
            _logger.LogInformation("Consultando histórico específico com ID: {IdDoCaso}", idDoCaso);

            var historico = await _context.ChamadosHistorico
                .FirstOrDefaultAsync(h => h.IdDoCaso == idDoCaso);

            if (historico == null)
            {
                _logger.LogWarning("Histórico não encontrado para ID: {IdDoCaso}", idDoCaso);
                return NotFound(new { message = "Histórico não encontrado" });
            }

            return Ok(historico);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar histórico específico com ID: {IdDoCaso}", idDoCaso);
            return StatusCode(500, new { message = "Erro interno do servidor ao consultar histórico" });
        }
    }

    [HttpGet("estatisticas")]
    public async Task<ActionResult> GetEstatisticas()
    {
        try
        {
            _logger.LogInformation("Consultando estatísticas do histórico");

            var totalChamados = await _context.ChamadosHistorico.CountAsync();
            
            var chamadosPorStatus = await _context.ChamadosHistorico
                .Where(h => h.Status != null)
                .GroupBy(h => h.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var chamadosPorCategoria = await _context.ChamadosHistorico
                .Where(h => h.Categoria != null)
                .GroupBy(h => h.Categoria)
                .Select(g => new { Categoria = g.Key, Count = g.Count() })
                .ToListAsync();

            var chamadosPorPrioridade = await _context.ChamadosHistorico
                .Where(h => h.Prioridade != null)
                .GroupBy(h => h.Prioridade)
                .Select(g => new { Prioridade = g.Key, Count = g.Count() })
                .ToListAsync();

            var estatisticas = new
            {
                TotalChamados = totalChamados,
                ChamadosPorStatus = chamadosPorStatus,
                ChamadosPorCategoria = chamadosPorCategoria,
                ChamadosPorPrioridade = chamadosPorPrioridade
            };

            return Ok(estatisticas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar estatísticas do histórico");
            return StatusCode(500, new { message = "Erro interno do servidor ao consultar estatísticas" });
        }
    }

    [HttpPost("inserir-dados-reais")]
    [Authorize]
    public async Task<IActionResult> InserirDadosReais()
    {
        try
        {
            // Verificar se o registro já existe
            var existeRegistro = await _context.ChamadosHistorico
                .AnyAsync(c => c.IdDoCaso == 2189);

            if (existeRegistro)
            {
                return BadRequest("Registro com ID 2189 já existe");
            }

            var dadoReal = new ChamadoHistorico
            {
                IdDoCaso = 2189,
                Tipo = "Solicitação",
                Resumo = "Reset de Senha",
                Descricao = "Solicitação de reset de senha para e-mail nominal: marcos.gouveia",
                DataAbertura = new DateTime(2019, 4, 29, 7, 4, 19, DateTimeKind.Utc),
                Prioridade = "NORMAL",
                Categoria = "INFRAESTRUTURA.SERVIDOR.ACTIVE DIRECTORY.RESET DE SENHA.ELEGIVEL",
                Status = "ENCERRADO",
                Atribuido = "",
                GrupoAtribuido = "SDX SERVICE DESK N1 TELEFONE",
                LocalizacaoAfetada = "51645-VALE PORTO – MANUT PREDIAL E HVAC",
                DataResolucao = new DateTime(2019, 4, 29, 7, 6, 42, DateTimeKind.Utc),
                ViolacaoProjetada = new DateTime(2019, 4, 29, 10, 4, 19, DateTimeKind.Utc),
                RelatadoPor = "Marcos Santos Goveia",
                MetodoRelatado = "Outros",
                CategoriaReporte = "INFRAESTRUTURA.SERVIDOR.ACTIVE DIRECTORY.RESET DE SENHA.ELEGIVEL",
                UltimaModificacao = new DateTime(2019, 5, 7, 7, 36, 42, DateTimeKind.Utc),
                UsuarioFinalAfetado = "Marcos Santos Goveia",
                EmailUsuarioFinal = "marcos.xxx@soxxxo.com",
                CpfUsuarioFinal = "516xxxx",
                DescricaoSolucao = "Efetuado reset de senha de e-mail nominal para: marcos.gouveia"
            };

            _context.ChamadosHistorico.Add(dadoReal);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Dados reais inseridos com sucesso - ID: {IdDoCaso}", dadoReal.IdDoCaso);

            return Ok(new { message = "Dados reais inseridos com sucesso", id = dadoReal.IdDoCaso });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao inserir dados reais");
            return StatusCode(500, "Erro interno do servidor");
        }
    }
}