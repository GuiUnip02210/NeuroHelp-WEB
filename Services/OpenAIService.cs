using Microsoft.EntityFrameworkCore;
using SistemaChamados.Shared.DTOs;
using SistemaChamados.Data;
using System.Text;
using System.Text.Json;

namespace SistemaChamados.Services
{
    public class OpenAIService : IOpenAIService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OpenAIService> _logger;

        public OpenAIService(HttpClient httpClient, IConfiguration configuration, ApplicationDbContext context, ILogger<OpenAIService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _context = context;
            _logger = logger;
        }

        public async Task<AnaliseChamadoResponseDto?> AnalisarChamadoAsync(string descricaoProblema)
        {
            var apiKey = _configuration["OpenAI:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogError("Chave da API da OpenAI não configurada.");
                return null;
            }

            try
            {
                var prompt = await CriarPromptAnalise(descricaoProblema);

                var requestBody = new OpenAIRequest
                {
                    model = "gpt-3.5-turbo",
                    messages = new List<OpenAIMessage>
                    {
                        new OpenAIMessage { role = "system", content = "Você é um especialista em triagem de chamados de TI. Responda apenas com o formato JSON solicitado." },
                        new OpenAIMessage { role = "user", content = prompt }
                    },
                    temperature = 0.2
                };

                var jsonContent = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

                var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Erro na API da OpenAI: {response.StatusCode} - {errorContent}");
                    return null;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var openAIResponse = JsonSerializer.Deserialize<OpenAIResponse>(responseContent);

                var resultText = openAIResponse?.choices?.FirstOrDefault()?.message?.content;

                if (string.IsNullOrWhiteSpace(resultText))
                {
                    return null;
                }

                var analise = JsonSerializer.Deserialize<AnaliseChamadoResponseDto>(resultText);
                
                if (analise != null)
                {
                    // Nova Lógica: Buscar técnico especialista (TipoUsuario == 2)
                    var tecnicoEspecialista = await _context.Usuarios
                        .FirstOrDefaultAsync(u => 
                            u.TipoUsuario == 2 && // <-- CORRIGIDO para 2
                            u.EspecialidadeCategoriaId == analise.CategoriaId && 
                            u.Ativo);
                    
                    // Se não encontrar especialista, busca qualquer técnico (TipoUsuario == 2) como fallback
                    if (tecnicoEspecialista == null)
                    {
                        _logger.LogWarning("Nenhum técnico especialista (Tipo 2) encontrado para CategoriaId {CategoriaId}. Procurando técnico geral (Tipo 2).", analise.CategoriaId);
                        tecnicoEspecialista = await _context.Usuarios
                            .FirstOrDefaultAsync(u => u.TipoUsuario == 2 && u.Ativo); // <-- Busca qualquer técnico Tipo 2
                    }

                    if (tecnicoEspecialista != null)
                    {
                        analise.TecnicoId = tecnicoEspecialista.Id;
                        analise.TecnicoNome = tecnicoEspecialista.NomeCompleto;
                        _logger.LogInformation("Chamado atribuído ao técnico ID {TecnicoId}", tecnicoEspecialista.Id);
                    }
                    else
                    {
                        _logger.LogWarning("Nenhum técnico (TipoUsuario == 2) encontrado no sistema para atribuição.");
                        // Se nenhum técnico for encontrado, analise.TecnicoId permanecerá nulo
                    }
                }
                
                return analise;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao se comunicar com a OpenAI.");
                return null;
            }
        }

        private async Task<string> CriarPromptAnalise(string descricaoProblema)
        {
            var categorias = await _context.Categorias.Where(c => c.Ativo).ToListAsync();
            var prioridades = await _context.Prioridades.Where(p => p.Ativo).ToListAsync();

            var categoriasTexto = string.Join("\n", categorias.Select(c => $"- ID: {c.Id}, Nome: {c.Nome}"));
            var prioridadesTexto = string.Join("\n", prioridades.Select(p => $"- ID: {p.Id}, Nome: {p.Nome}"));

            return $@"Analise o problema de TI a seguir e classifique-o.

Descrição do Problema: ""{descricaoProblema}""

Categorias Disponíveis:
{categoriasTexto}

Prioridades Disponíveis:
{prioridadesTexto}

Responda APENAS com um JSON no seguinte formato, sem texto adicional:
{{
  ""CategoriaId"": <ID da categoria>,
  ""CategoriaNome"": ""<Nome da categoria>"",
  ""PrioridadeId"": <ID da prioridade>,
  ""PrioridadeNome"": ""<Nome da prioridade>"",
  ""TituloSugerido"": ""<Um título curto e claro para o chamado>"",
  ""Justificativa"": ""<Uma breve justificativa para a sua escolha>""
}}";
        }
    }
}