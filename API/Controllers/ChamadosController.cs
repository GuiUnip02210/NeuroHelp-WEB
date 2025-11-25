using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaChamados.Shared.DTOs;
using SistemaChamados.Shared.Entities;
using SistemaChamados.Data;
using SistemaChamados.Services;
using System.Security.Claims;

namespace SistemaChamados.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChamadosController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IOpenAIService _openAIService;
    private readonly ILogger<ChamadosController> _logger;

    public ChamadosController(ApplicationDbContext context, IOpenAIService openAIService, ILogger<ChamadosController> logger)
    {
        _context = context;
        _openAIService = openAIService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CriarChamado([FromBody] CriarChamadoRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Pega o ID do usuário logado a partir do token JWT.
        var solicitanteIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (solicitanteIdStr == null)
        {
            return Unauthorized(); // Token inválido ou não contém o ID
        }

        // Validar se a categoria existe e está ativa
        var categoria = await _context.Categorias.FindAsync(request.CategoriaId);
        if (categoria == null)
        {
            return BadRequest("Categoria não encontrada ou inativa");
        }

        // Validar se a prioridade existe e está ativa
        var prioridade = await _context.Prioridades.FindAsync(request.PrioridadeId);
        if (prioridade == null)
        {
            return BadRequest("Prioridade não encontrada ou inativa");
        }

        // Validar se o usuário existe
        var solicitanteId = int.Parse(solicitanteIdStr);
        var usuario = await _context.Usuarios.FindAsync(solicitanteId);
        if (usuario == null)
        {
            return Unauthorized("Usuário não encontrado ou inativo");
        }
        
        // --- INÍCIO LÓGICA SLA (POST) ---
        // Busca o 'Nivel' da prioridade para o cálculo
        var prioridadeSla = await _context.Prioridades.FindAsync(request.PrioridadeId);
        if (prioridadeSla == null)
        {
            return BadRequest("PrioridadeId inválido.");
        }
        // --- FIM LÓGICA SLA (POST) ---

        var novoChamado = new Chamado
        {
            Titulo = request.Titulo,
            Descricao = request.Descricao,
            DataAbertura = DateTime.UtcNow,
            SolicitanteId = solicitanteId,
            StatusId = 1, // Assumindo que o ID 1 seja "Aberto"
            PrioridadeId = request.PrioridadeId,
            CategoriaId = request.CategoriaId,
            // --- ADICIONAR ESTA LINHA ---
            SlaDataExpiracao = CalcularSla(prioridadeSla.Nivel, DateTime.UtcNow)
        };

        _context.Chamados.Add(novoChamado);
        await _context.SaveChangesAsync();

        return Ok(novoChamado);
    }
[HttpGet]
public async Task<IActionResult> GetChamados([FromQuery] int? statusId, [FromQuery] int? tecnicoId, [FromQuery] int? solicitanteId, [FromQuery] int? prioridadeId, [FromQuery] string? termoBusca)
{
    _logger.LogInformation("GetChamados - Recebido pedido com filtros: statusId={StatusId}, tecnicoId={TecnicoId}, solicitanteId={SolicitanteId}, prioridadeId={PrioridadeId}, termoBusca={TermoBusca}", statusId, tecnicoId, solicitanteId, prioridadeId, termoBusca);
    try
    {
        // --- INÍCIO LÓGICA VERIFICAÇÃO SLA ---
        // IDs: 1=Aberto, 2=Em Andamento, 3=Aguardando Resposta, 5=Violado
        var statusParaVerificar = new[] { 1, 2, 3 };
        var statusVioladoId = 5; 
        
        // Busca chamados abertos/em andamento/aguardando que passaram do prazo
        var chamadosViolados = await _context.Chamados
            .Where(c => statusParaVerificar.Contains(c.StatusId) &&
                        c.SlaDataExpiracao.HasValue &&
                        c.SlaDataExpiracao < DateTime.UtcNow)
            .ToListAsync();

        if (chamadosViolados.Any())
        {
            foreach (var chamado in chamadosViolados)
            {
                _logger.LogWarning("SLA VIOLADO: Chamado ID {ChamadoId} movido para status 'Violado'.", chamado.Id);
                chamado.StatusId = statusVioladoId;
                chamado.DataUltimaAtualizacao = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync(); // Salva as alterações de status
        }
        // --- FIM LÓGICA VERIFICAÇÃO SLA ---

        // 1. Começa com a consulta base
        var query = _context.Chamados.AsQueryable();
        // 2. Aplica filtro de StatusId se fornecido
        if (statusId.HasValue)
        {
            query = query.Where(c => c.StatusId == statusId.Value);
            _logger.LogInformation("Filtro StatusId={StatusId} aplicado.", statusId.Value);
        }
        // 3. Aplica filtro de TecnicoId se fornecido
        if (tecnicoId.HasValue)
        {
            if (tecnicoId.Value == 0) 
            {
               query = query.Where(c => c.TecnicoId == null); // Busca não atribuídos
               _logger.LogInformation("Filtro TecnicoId=NULL aplicado.");
            }
            else 
            {
               query = query.Where(c => c.TecnicoId == tecnicoId.Value); // Busca por técnico específico
               _logger.LogInformation("Filtro TecnicoId={TecnicoId} aplicado.", tecnicoId.Value);
            }
        }
        // 4. Aplica filtro de SolicitanteId se fornecido
        if (solicitanteId.HasValue)
        {
            query = query.Where(c => c.SolicitanteId == solicitanteId.Value);
            _logger.LogInformation("Filtro SolicitanteId={SolicitanteId} aplicado.", solicitanteId.Value);
        }

        // 5. Aplica filtro de PrioridadeId se fornecido
        if (prioridadeId.HasValue)
        {
            query = query.Where(c => c.PrioridadeId == prioridadeId.Value);
            _logger.LogInformation("Filtro PrioridadeId={PrioridadeId} aplicado.", prioridadeId.Value);
        }

        // 6. Aplica filtro de Termo de Busca se fornecido
        if (!string.IsNullOrWhiteSpace(termoBusca))
        {
            string busca = termoBusca.ToLower().Trim();
            _logger.LogInformation("Filtro termoBusca='{TermoBusca}' aplicado.", busca);

            // Tenta converter a busca para ID
            int.TryParse(busca, out int idBusca);

            query = query.Where(c => 
                c.Id == idBusca || // Busca por ID
                c.Titulo.ToLower().Contains(busca) || // Busca no Título
                c.Categoria.Nome.ToLower().Contains(busca) // Busca no Nome da Categoria
            );
        }

        // 7. Executa a consulta com todos os filtros e projeta para o DTO
        var chamadosDto = await query
            .Include(c => c.Categoria)
            .Include(c => c.Status)
            .Include(c => c.Prioridade) // <-- Adicione este Include
            .OrderByDescending(c => c.DataAbertura)
            .Select(c => new ChamadoListDto 
            {
                Id = c.Id,
                Titulo = c.Titulo,
                CategoriaNome = c.Categoria.Nome, 
                StatusNome = c.Status.Nome,
                PrioridadeNome = c.Prioridade.Nome // <-- Adicione esta linha
            })
            .ToListAsync();
        _logger.LogInformation("GetChamados - Consulta finalizada. Resultados encontrados: {Count}", chamadosDto.Count);
        return Ok(chamadosDto); 
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Erro ao buscar chamados com filtros.");
        return StatusCode(500, "Erro interno ao buscar chamados.");
    }
}

[HttpGet("{id}")]
public async Task<IActionResult> GetChamadoPorId(int id)
{
    var chamado = await _context.Chamados
        .Include(c => c.Solicitante)
        .Include(c => c.Tecnico) // Inclui o técnico também, se houver
        .Include(c => c.Status)
        .Include(c => c.Prioridade)
        .Include(c => c.Categoria)
        .FirstOrDefaultAsync(c => c.Id == id);

    if (chamado == null)
    {
        return NotFound("Chamado não encontrado.");
    }

    return Ok(chamado);
}

[HttpPut("{id}")]
public async Task<IActionResult> AtualizarChamado(int id, [FromBody] AtualizarChamadoDto request)
{
    var chamado = await _context.Chamados.FindAsync(id);
    if (chamado == null)
    {
        return NotFound("Chamado não encontrado.");
    }

    // Valida se o novo StatusId existe
    var statusExiste = await _context.Status.AnyAsync(s => s.Id == request.StatusId);
    if (!statusExiste)
    {
        return BadRequest("O StatusId fornecido é inválido.");
    }

    // --- INÍCIO DA NOVA LÓGICA ---
    // 1. Atualiza sempre a data da última modificação
    chamado.DataUltimaAtualizacao = DateTime.UtcNow;

    // 2. Verifica se o novo status é 'Fechado' (vamos assumir que o ID 4 é "Fechado")
    if (request.StatusId == 4) 
    {
        chamado.DataFechamento = DateTime.UtcNow;
    }
    else
    {
        // Garante que a data de fechamento seja nula se o chamado for reaberto
        chamado.DataFechamento = null;
    }
    // --- FIM DA NOVA LÓGICA ---

    // Atualiza os campos do chamado
    chamado.StatusId = request.StatusId;

    if (request.TecnicoId.HasValue)
    {
        var tecnicoExiste = await _context.Usuarios.AnyAsync(u => u.Id == request.TecnicoId.Value && u.Ativo);
        if (!tecnicoExiste)
        {
            return BadRequest("O TecnicoId fornecido é inválido ou o usuário está inativo.");
        }
        chamado.TecnicoId = request.TecnicoId;
    }

    _context.Chamados.Update(chamado);
    await _context.SaveChangesAsync();

    return Ok(chamado);
}

[HttpPost("analisar")]
public async Task<IActionResult> AnalisarChamado([FromBody] AnalisarChamadoRequestDto request)
{
    if (string.IsNullOrWhiteSpace(request.DescricaoProblema))
    {
        return BadRequest("Descrição do problema é obrigatória.");
    }

    try
    {
        // 1. Pede a análise da IA (como antes)
        var analise = await _openAIService.AnalisarChamadoAsync(request.DescricaoProblema);
        
        if (analise == null)
        {
            return StatusCode(500, "Erro ao obter análise da IA. Tente novamente.");
        }

        // 2. Pega o ID do usuário que fez a requisição
        var solicitanteIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (solicitanteIdStr == null)
        {
            return Unauthorized();
        }
        var solicitanteId = int.Parse(solicitanteIdStr);

        // --- INÍCIO LÓGICA SLA (ANALISAR) ---
        // Busca o 'Nivel' da prioridade para o cálculo
        var prioridadeSla = await _context.Prioridades.FindAsync(analise.PrioridadeId);
        if (prioridadeSla == null)
        {
            return StatusCode(500, "A IA retornou uma PrioridadeId inválida.");
        }
        // --- FIM LÓGICA SLA (ANALISAR) ---

        // 3. Cria o novo chamado com os dados da IA e do usuário
        var novoChamado = new Chamado
        {
            Titulo = analise.TituloSugerido,
            Descricao = request.DescricaoProblema,
            DataAbertura = DateTime.UtcNow,
            SolicitanteId = solicitanteId,
            StatusId = 1, // Padrão: "Aberto"
            PrioridadeId = analise.PrioridadeId,
            CategoriaId = analise.CategoriaId,
            TecnicoId = analise.TecnicoId,
             // --- ADICIONAR ESTA LINHA ---
            SlaDataExpiracao = CalcularSla(prioridadeSla.Nivel, DateTime.UtcNow)
        };

        // 4. Salva o novo chamado no banco de dados
        _context.Chamados.Add(novoChamado);
        await _context.SaveChangesAsync();

        // 5. Retorna o chamado que foi CRIADO no banco (com seu novo ID)
        return CreatedAtAction(nameof(GetChamadoPorId), new { id = novoChamado.Id }, novoChamado);
    }
    catch (Exception ex)
    {
        // Logar o erro real no seu console
        _logger.LogError(ex, "Erro no processo de análise e criação de chamado.");
        return StatusCode(500, "Ocorreu um erro inesperado ao processar seu chamado.");
    }
}

// ===============================================
// INÍCIO - NOVOS ENDPOINTS DE COMENTÁRIOS
// ===============================================

[HttpGet("{chamadoId}/comentarios")]
public async Task<IActionResult> GetComentarios(int chamadoId)
{
    // Verifica se o chamado existe
    var chamadoExiste = await _context.Chamados.AnyAsync(c => c.Id == chamadoId);
    if (!chamadoExiste)
    {
        return NotFound("Chamado não encontrado.");
    }

    var comentarios = await _context.Comentarios
        .Where(c => c.ChamadoId == chamadoId)
        .Include(c => c.Usuario) // Inclui dados do usuário
        .OrderBy(c => c.DataCriacao)
        .Select(c => new ComentarioResponseDto
        {
            Id = c.Id,
            Texto = c.Texto,
            DataCriacao = c.DataCriacao,
            UsuarioId = c.UsuarioId,
            UsuarioNome = c.Usuario.NomeCompleto, // Pega o nome do usuário
            ChamadoId = c.ChamadoId
        })
        .ToListAsync();
        
    // Lidar com a serialização $values
    if (!comentarios.Any())
    {
        return Ok(new List<ComentarioResponseDto>());
    }

    return Ok(comentarios);
}

[HttpPost("{chamadoId}/comentarios")]
public async Task<IActionResult> AdicionarComentario(int chamadoId, [FromBody] CriarComentarioDto request)
{
    if (!ModelState.IsValid)
    {
        return BadRequest(ModelState);
    }

    // Pega o ID do usuário logado a partir do token
    var usuarioIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (usuarioIdStr == null)
    {
        return Unauthorized();
    }
    var usuarioId = int.Parse(usuarioIdStr);

    // Verifica se o chamado existe
    var chamado = await _context.Chamados.FindAsync(chamadoId);
    if (chamado == null)
    {
        return NotFound("Chamado não encontrado.");
    }

    var novoComentario = new Comentario
    {
        Texto = request.Texto,
        ChamadoId = chamadoId,
        UsuarioId = usuarioId,
        DataCriacao = DateTime.UtcNow
    };

    // Atualiza a data de última atualização do chamado
    chamado.DataUltimaAtualizacao = DateTime.UtcNow;
    _context.Chamados.Update(chamado);

    _context.Comentarios.Add(novoComentario);
    await _context.SaveChangesAsync();

    // Busca o nome do usuário para retornar no DTO
    var nomeUsuario = await _context.Usuarios
        .Where(u => u.Id == usuarioId)
        .Select(u => u.NomeCompleto)
        .FirstOrDefaultAsync();

    var responseDto = new ComentarioResponseDto
    {
        Id = novoComentario.Id,
        Texto = novoComentario.Texto,
        DataCriacao = novoComentario.DataCriacao,
        UsuarioId = novoComentario.UsuarioId,
        UsuarioNome = nomeUsuario ?? "Usuário",
        ChamadoId = novoComentario.ChamadoId
    };

    return CreatedAtAction(nameof(GetComentarios), new { chamadoId = chamadoId }, responseDto);
}

// ===============================================
// FIM - NOVOS ENDPOINTS DE COMENTÁRIOS
// ===============================================

// ===============================================
// INÍCIO - FUNÇÕES HELPER DO SLA
// ===============================================

/**
 * Calcula a data de expiração do SLA baseado no nível de prioridade.
 * Nivel 3 (Alto) = 1 dia útil
 * Nivel 2 (Médio) = 3 dias úteis
 * Nivel 1 (Baixo) = 5 dias úteis
 */
private DateTime? CalcularSla(int nivelPrioridade, DateTime dataAbertura)
{
    int diasUteisParaAdicionar;

        switch (nivelPrioridade)
        {
            case 3: // Alta
                diasUteisParaAdicionar = 1;
                break;
            case 2: // Média
                diasUteisParaAdicionar = 3;
                break;
            case 1: // Baixa
                diasUteisParaAdicionar = 5;
                break;
            default: // Prioridade desconhecida, não define SLA
                return null;
        }

        var dataFinalDoDia = AddBusinessDays(dataAbertura.Date, diasUteisParaAdicionar);
        return dataFinalDoDia.AddDays(1);

}

/**
 * Adiciona um número de dias úteis (Seg-Sex) a uma data.
 */
private DateTime AddBusinessDays(DateTime date, int days)
{
    if (days == 0) return date;

    DateTime result = date;
    int daysAdded = 0;
    
    while (daysAdded < days)
    {
        result = result.AddDays(1);
        // Só incrementa o contador se o dia não for Sábado (6) ou Domingo (0)
        if (result.DayOfWeek != DayOfWeek.Saturday && result.DayOfWeek != DayOfWeek.Sunday)
        {
            daysAdded++;
        }
    }
    return result;
}

// ===============================================
// FIM - FUNÇÕES HELPER DO SLA
// ===============================================

}

// DTO para requisição de análise
public class AnalisarChamadoRequestDto
{
    public string DescricaoProblema { get; set; } = string.Empty;
}