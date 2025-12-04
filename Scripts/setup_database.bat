@echo off
REM =====================================================
REM Script de Setup do Banco de Dados PostgreSQL (Windows)
REM Sistema de Chamados Histórico
REM =====================================================

echo 🚀 Iniciando setup do banco de dados...

REM Verificar se PostgreSQL está disponível
pg_isready -q
if %errorlevel% neq 0 (
    echo ❌ PostgreSQL não está rodando. Por favor, inicie o serviço PostgreSQL.
    pause
    exit /b 1
)

echo ✅ PostgreSQL está rodando.

REM Criar banco de dados e usuário
echo 📦 Criando banco de dados e usuário...

psql -U postgres -c "DROP DATABASE IF EXISTS sistema_chamados_historico;"
psql -U postgres -c "CREATE DATABASE sistema_chamados_historico;"
psql -U postgres -c "DROP USER IF EXISTS admin;"
psql -U postgres -c "CREATE USER admin WITH PASSWORD 'admin123';"
psql -U postgres -c "GRANT ALL PRIVILEGES ON DATABASE sistema_chamados_historico TO admin;"
psql -U postgres -c "GRANT CREATEDB TO admin;"

if %errorlevel% neq 0 (
    echo ❌ Erro ao criar banco de dados e usuário.
    pause
    exit /b 1
)

echo ✅ Banco de dados e usuário criados com sucesso.

REM Executar script de criação das tabelas
echo 🏗️ Criando tabelas...

set PGPASSWORD=admin123
psql -h localhost -U admin -d sistema_chamados_historico -f Scripts\create_database.sql

if %errorlevel% neq 0 (
    echo ❌ Erro ao criar tabelas.
    pause
    exit /b 1
)

echo ✅ Tabelas criadas com sucesso.

REM Verificar se as tabelas foram criadas
echo 🔍 Verificando tabelas criadas...

psql -h localhost -U admin -d sistema_chamados_historico -c "\dt"

echo.
echo 🎉 Setup do banco de dados concluído com sucesso!
echo.
echo 📋 Informações de conexão:
echo    Host: localhost
echo    Database: sistema_chamados_historico
echo    Username: admin
echo    Password: admin123
echo.
echo 🚀 Agora você pode executar a aplicação com: dotnet run
pause