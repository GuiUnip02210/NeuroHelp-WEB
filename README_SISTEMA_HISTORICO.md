# Sistema de Chamados Histórico

Sistema de consulta somente leitura para dados históricos de chamados descontinuados.

## 🚀 Tecnologias Utilizadas

- **Backend:** ASP.NET Core 8 Web API
- **Frontend:** HTML, CSS, JavaScript
- **Banco de Dados:** PostgreSQL
- **Autenticação:** JWT (JSON Web Tokens)

## ✨ Funcionalidades

- ✅ **Login e Registro de Usuários**
- ✅ **Consulta de Dados Históricos** (somente leitura)
- ✅ **Filtros de Pesquisa** (ID, Resumo, Descrição, Categoria, Status, Prioridade, Data)
- ✅ **Visualização Detalhada** em modal
- ✅ **Paginação** de resultados
- ✅ **Interface Responsiva**

## 🗄️ Estrutura do Banco de Dados

### Tabela: Usuarios
- Id (PK)
- Email
- SenhaHash
- NomeCompleto
- TipoUsuario (sempre 1 - Visualizador)
- DataCriacao

### Tabela: HISTORICO_CHAMADOS (21 campos)
- ID_DO_CASO (PK)
- TIPO
- RESUMO
- DESCRICAO
- DATA_ABERTURA
- PRIORIDADE
- CATEGORIA
- STATUS
- ATRIBUIDO
- GRUPO_ATRIBUIDO
- LOCALIZACAO_AFETADA
- DATA_RESOLUCAO
- VIOLACAO_PROJETADA
- RELATADO_POR
- METODO_RELATADO
- CATEGORIA_REPORTE
- ULTIMA_MODIFICACAO
- USUARIO_FINAL_AFETADO
- EMAIL_USUARIO_FINAL
- CPF_USUARIO_FINAL
- DESCRICAO_SOLUCAO

## ⚙️ Configuração e Instalação

### Pré-requisitos
- .NET 8 SDK
- PostgreSQL 12+
- Git

### 🔧 Instalação Automática

#### Linux/Mac:
```bash
# 1. Clone o repositório
git clone <url-do-repositorio>
cd NeuroHelp-WEB
git checkout CA-Antigo

# 2. Execute o script de setup (cria banco e tabelas automaticamente)
chmod +x Scripts/setup_database.sh
./Scripts/setup_database.sh

# 3. Restaure as dependências
dotnet restore

# 4. Execute a aplicação
dotnet run
```

#### Windows:
```cmd
# 1. Clone o repositório
git clone <url-do-repositorio>
cd NeuroHelp-WEB
git checkout CA-Antigo

# 2. Execute o script de setup (cria banco e tabelas automaticamente)
Scripts\setup_database.bat

# 3. Restaure as dependências
dotnet restore

# 4. Execute a aplicação
dotnet run
```

### 🔧 Instalação Manual

Se preferir configurar manualmente:

1. **Criar banco PostgreSQL:**
   ```sql
   CREATE DATABASE sistema_chamados_historico;
   CREATE USER admin WITH PASSWORD 'admin123';
   GRANT ALL PRIVILEGES ON DATABASE sistema_chamados_historico TO admin;
   ```

2. **Executar script SQL:**
   ```bash
   PGPASSWORD=admin123 psql -h localhost -U admin -d sistema_chamados_historico -f Scripts/create_database.sql
   ```

3. **Configurar aplicação:**
   ```bash
   dotnet restore
   dotnet run
   ```

### 🌐 Acesso ao Sistema

- **URL:** http://localhost:5000
- **Usuário padrão:** admin@historico.com
- **Senha:** admin123

## 📁 Estrutura do Projeto

```
NeuroHelp-WEB/
├── Controllers/
│   ├── UsuariosController.cs      # Login e registro
│   └── HistoricoController.cs     # Consulta histórico
├── Core/
│   ├── Entities/
│   │   ├── Usuario.cs             # Modelo de usuário
│   │   └── ChamadoHistorico.cs    # Modelo histórico (21 campos)
│   └── Services/
│       └── ITokenService.cs       # Interface JWT
├── Data/
│   └── ApplicationDbContext.cs    # Contexto EF Core
├── Services/
│   └── TokenService.cs            # Implementação JWT
├── wwwroot/
│   ├── index.html                 # Página de login
│   ├── cadastro-desktop.html      # Página de registro
│   ├── user-dashboard-historico.html # Dashboard principal
│   ├── css/                       # Estilos
│   ├── js/                        # Scripts JavaScript
│   └── img/                       # Imagens
├── Scripts/
│   ├── create_database.sql        # Script criação tabelas
│   ├── setup_database.sh          # Setup Linux/Mac
│   └── setup_database.bat         # Setup Windows
└── README_SISTEMA_HISTORICO.md
```

## 🔌 API Endpoints

### Autenticação
- `POST /api/usuarios/login` - Login do usuário
- `POST /api/usuarios/registrar` - Registro de novo usuário

### Histórico (Protegido por JWT)
- `GET /api/historico` - Lista todos os chamados históricos
- `GET /api/historico?id={id}` - Busca por ID específico
- `GET /api/historico?resumo={texto}` - Busca por resumo
- `GET /api/historico?descricao={texto}` - Busca por descrição

### Exemplo de Uso da API:
```bash
# 1. Login
curl -X POST http://localhost:5000/api/usuarios/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@historico.com","senha":"admin123"}'

# 2. Usar token retornado
curl -X GET http://localhost:5000/api/historico \
  -H "Authorization: Bearer SEU_TOKEN_JWT"
```

## 🔒 Segurança

- ✅ **Autenticação JWT obrigatória** para todos os endpoints de dados
- ✅ **Senhas criptografadas** com BCrypt
- ✅ **Validação de entrada** de dados
- ✅ **CORS configurado** adequadamente
- ✅ **Sistema somente leitura** (sem operações de escrita nos dados históricos)
- ✅ **Proteção contra SQL Injection** via Entity Framework

## 🛠️ Scripts Disponíveis

### Scripts de Banco de Dados:
- `Scripts/create_database.sql` - Cria tabelas e usuário padrão
- `Scripts/setup_database.sh` - Setup completo Linux/Mac
- `Scripts/setup_database.bat` - Setup completo Windows

### Comandos .NET:
```bash
dotnet restore          # Restaurar dependências
dotnet build           # Compilar projeto
dotnet run             # Executar aplicação
dotnet clean           # Limpar build
```

## 📊 Dados de Exemplo

O sistema inclui um usuário administrador padrão:
- **Email:** admin@historico.com
- **Senha:** admin123

Para inserir dados históricos, use o endpoint POST (disponível apenas para desenvolvimento):
```bash
curl -X POST http://localhost:5000/api/historico/inserir-dados-reais \
  -H "Authorization: Bearer SEU_TOKEN_JWT"
```

## 🚨 Troubleshooting

### Problemas Comuns:

1. **Erro de conexão com PostgreSQL:**
   - Verifique se o PostgreSQL está rodando
   - Confirme as credenciais no `appsettings.json`

2. **Porta já em uso:**
   - Altere a porta no `launchSettings.json`
   - Ou use: `dotnet run --urls="http://localhost:NOVA_PORTA"`

3. **Erro de autenticação:**
   - Verifique se o token JWT não expirou
   - Confirme se o usuário foi criado corretamente

4. **Tabelas não encontradas:**
   - Execute novamente o script `create_database.sql`
   - Verifique se o banco foi criado corretamente

## 🎯 Desenvolvimento

Este sistema foi desenvolvido especificamente para consulta de dados históricos de um sistema descontinuado. 

**Características importantes:**
- ✅ **Somente leitura:** Não permite criação, edição ou exclusão de chamados
- ✅ **Seguro:** Todos os endpoints de dados requerem autenticação
- ✅ **Simples:** Interface limpa focada na consulta de dados
- ✅ **Completo:** Exibe todos os 21 campos dos dados históricos

## 📞 Suporte

Para dúvidas ou problemas:
1. Consulte este README
2. Verifique os logs da aplicação
3. Execute os scripts de troubleshooting
4. Entre em contato com a equipe de desenvolvimento

---

**Sistema de Chamados Histórico v1.0** - Desenvolvido para consulta segura de dados históricos.