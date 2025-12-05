# Como Executar o NeuroHelp-WEB

## Pré-requisitos

1. **.NET 8.0 SDK** instalado
2. **PostgreSQL** instalado e rodando
3. **Banco de dados** configurado

## Configuração do Banco de Dados

1. Instalar PostgreSQL
2. Criar usuário e banco:
```bash
sudo -u postgres createuser admin
sudo -u postgres createdb sistema_chamados_historico -O admin
sudo -u postgres psql -c "ALTER USER admin PASSWORD 'admin123';"
```

3. Executar scripts de criação:
```bash
PGPASSWORD=admin123 psql -h localhost -U admin -d sistema_chamados_historico -f Scripts/create_database.sql
PGPASSWORD=admin123 psql -h localhost -U admin -d sistema_chamados_historico -f Scripts/InsertRealData.sql
```

## Executar a Aplicação

1. Restaurar dependências:
```bash
dotnet restore
```

2. Compilar o projeto:
```bash
dotnet build
```

3. Executar o servidor:
```bash
dotnet run --urls="http://0.0.0.0:5246"
```

## Acessar a Aplicação

- **Interface Web**: http://localhost:5246
- **Swagger API**: http://localhost:5246/swagger
- **Histórico de Chamados**: http://localhost:5246/user-dashboard-historico.html

## Credenciais Padrão

- **Email**: admin@historico.com
- **Senha**: admin123

## Estrutura do Projeto

- `Controllers/` - Controladores da API
- `Models/` - Modelos de dados
- `wwwroot/` - Arquivos estáticos (HTML, CSS, JS)
- `Scripts/` - Scripts SQL para criação do banco
- `appsettings.json` - Configurações da aplicação

## Problemas Comuns

1. **Erro 404 na API**: Verificar se o servidor está rodando na porta 5246
2. **Erro de conexão com banco**: Verificar se PostgreSQL está rodando e credenciais estão corretas
3. **Dados não carregam**: Verificar console do navegador para erros de JavaScript

## Logs

Os logs do servidor são salvos em `server.log` quando executado em background.