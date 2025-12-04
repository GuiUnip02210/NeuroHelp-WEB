# Sistema de Chamados Histórico - Somente Leitura

## 📋 Descrição

O **SistemaChamadosHistorico** é um aplicativo web de consulta e arquivamento de dados históricos de chamados de um sistema descontinuado. O sistema oferece acesso seguro e restrito apenas para visualização de dados, com funcionalidades de login, registro de usuários e consulta de histórico.

## 🏗️ Arquitetura

- **Backend**: C# ASP.NET Core 8 Web API
- **Frontend**: HTML/CSS/JavaScript (servido estaticamente)
- **Banco de Dados**: PostgreSQL
- **Autenticação**: JWT (JSON Web Tokens)
- **Criptografia**: BCrypt para senhas

## 🚀 Funcionalidades

### ✅ Implementadas
- **Login de usuários** com autenticação JWT
- **Registro de novos usuários** (perfil único de Visualizador)
- **Consulta de histórico** de chamados com filtros avançados
- **Visualização detalhada** de chamados em modal
- **Paginação** e estatísticas dos dados
- **Interface responsiva** e intuitiva

### 🔒 Segurança
- Todas as rotas de dados protegidas por autenticação
- Senhas criptografadas com BCrypt
- Tokens JWT com expiração configurável
- Validação de entrada em todos os endpoints

## 📊 Modelo de Dados

### Tabela: `ChamadoHistorico` (21 campos)
- **ID_DO_CASO** (int) - Chave primária
- **TIPO** (string) - Tipo do chamado
- **RESUMO** (string) - Resumo do problema
- **DESCRICAO** (string) - Descrição detalhada
- **DATA_ABERTURA** (DateTime) - Data de abertura
- **PRIORIDADE** (string) - Nível de prioridade
- **CATEGORIA** (string) - Categoria do chamado
- **STATUS** (string) - Status atual
- **ATRIBUIDO** (string) - Responsável
- **GRUPO_ATRIBUIDO** (string) - Grupo responsável
- **LOCALIZACAO_AFETADA** (string) - Local afetado
- **DATA_RESOLUCAO** (DateTime?) - Data de resolução
- **VIOLACAO_PROJETADA** (DateTime?) - Violação projetada
- **RELATADO_POR** (string) - Quem relatou
- **METODO_RELATADO** (string) - Como foi relatado
- **CATEGORIA_REPORTE** (string) - Categoria do reporte
- **ULTIMA_MODIFICACAO** (DateTime?) - Última modificação
- **USUARIO_FINAL_AFETADO** (string) - Usuário afetado
- **EMAIL_USUARIO_FINAL** (string) - Email do usuário
- **CPF_USUARIO_FINAL** (string) - CPF do usuário
- **DESCRICAO_SOLUCAO** (string) - Descrição da solução

### Tabela: `Usuarios`
- **Id** (int) - Chave primária
- **Email** (string) - Email único
- **SenhaHash** (string) - Senha criptografada
- **NomeCompleto** (string) - Nome completo
- **DataCadastro** (DateTime) - Data de cadastro
- **Ativo** (bool) - Status ativo

## 🛠️ Instalação e Configuração

### Pré-requisitos
- .NET 8 SDK
- PostgreSQL 12+
- Git (opcional)

### 1. Configuração do Banco de Dados

```sql
-- Criar banco de dados
CREATE DATABASE sistema_chamados_historico;

-- Conectar ao banco e executar o script de criação das tabelas
-- (O script está incluído em Scripts/CreateDatabase.sql)
```

### 2. Configuração da Aplicação

1. **Extrair o projeto**:
   ```bash
   unzip SistemaChamadosHistorico.zip
   cd NeuroHelp-WEB
   ```

2. **Configurar string de conexão** em `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=sistema_chamados_historico;Username=seu_usuario;Password=sua_senha"
     }
   }
   ```

3. **Restaurar dependências**:
   ```bash
   dotnet restore
   ```

4. **Executar migrações** (se necessário):
   ```bash
   dotnet ef database update
   ```

### 3. Executar a Aplicação

```bash
dotnet run --project SistemaChamados.csproj --urls=http://localhost:5000
```

A aplicação estará disponível em: `http://localhost:5000`

## 📱 Como Usar

### 1. Primeiro Acesso
1. Acesse `http://localhost:5000`
2. Clique em "Criar conta"
3. Preencha os dados de registro
4. Faça login com as credenciais criadas

### 2. Navegação
- **Catálogo Histórico**: Visualizar todos os chamados históricos
- **Filtros**: Pesquisar por ID, resumo, descrição, categoria, status, prioridade e datas
- **Detalhes**: Clicar em "👁️ Ver" para visualizar informações completas
- **Paginação**: Navegar entre páginas de resultados

### 3. Funcionalidades de Filtro
- **ID do Caso**: Busca exata por número
- **Resumo/Descrição**: Busca textual parcial
- **Categoria**: Filtro por dropdown
- **Status**: Filtro por dropdown
- **Prioridade**: Filtro por dropdown
- **Datas**: Filtro por período de abertura

## 🔧 Estrutura do Projeto

```
NeuroHelp-WEB/
├── API/
│   └── Controllers/
│       ├── UsuariosController.cs      # Login e registro
│       └── HistoricoController.cs     # Consulta de histórico
├── Application/
│   └── Services/
│       ├── ITokenService.cs           # Interface JWT
│       └── TokenService.cs            # Implementação JWT
├── Data/
│   └── ApplicationDbContext.cs        # Contexto EF Core
├── SistemaChamados.Shared/
│   ├── DTOs/                          # Objetos de transferência
│   └── Entities/                      # Modelos de dados
├── wwwroot/                           # Frontend estático
│   ├── index.html                     # Página de login
│   ├── cadastro-desktop.html          # Página de registro
│   ├── user-dashboard-historico.html  # Dashboard principal
│   ├── script-desktop.js              # JavaScript principal
│   └── style-desktop.css              # Estilos CSS
├── Scripts/
│   └── CreateDatabase.sql             # Script de criação do BD
└── appsettings.json                   # Configurações
```

## 🧪 Dados de Teste

O sistema inclui 5 registros de exemplo na tabela `HISTORICO_CHAMADOS`:

1. **ID 1**: Sistema lento na rede corporativa (Infraestrutura, Alta prioridade)
2. **ID 2**: Criação de nova conta de usuário (Acesso, Média prioridade)
3. **ID 3**: Impressora não funciona (Hardware, Baixa prioridade)
4. **ID 4**: Falha no backup automático (Backup, Crítica prioridade)
5. **ID 5**: Instalação de software (Software, Média prioridade)

## 🔐 Segurança

- **Autenticação obrigatória** para todas as funcionalidades
- **Senhas criptografadas** com BCrypt
- **Tokens JWT** com expiração configurável
- **Validação de entrada** em todos os endpoints
- **Acesso somente leitura** aos dados históricos

## 🐛 Solução de Problemas

### Erro de Conexão com Banco
- Verificar se o PostgreSQL está rodando
- Confirmar credenciais na string de conexão
- Verificar se o banco de dados existe

### Erro de Autenticação
- Verificar se o JWT está configurado corretamente
- Confirmar se o usuário está ativo no banco

### Problemas de Frontend
- Verificar se os arquivos estão em `wwwroot/`
- Confirmar se a API está respondendo nas rotas corretas

## 📞 Suporte

Para dúvidas ou problemas:
1. Verificar logs da aplicação
2. Consultar documentação do .NET Core
3. Verificar configurações do PostgreSQL

## 📄 Licença

Sistema desenvolvido para fins acadêmicos e de arquivamento histórico.

---

**Versão**: 1.0  
**Data**: Dezembro 2024  
**Tecnologias**: .NET 8, PostgreSQL, HTML/CSS/JS