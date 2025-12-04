#!/bin/bash

# =====================================================
# Script de Setup do Banco de Dados PostgreSQL
# Sistema de Chamados Histórico
# =====================================================

echo "🚀 Iniciando setup do banco de dados..."

# Verificar se PostgreSQL está rodando
if ! pg_isready -q; then
    echo "❌ PostgreSQL não está rodando. Por favor, inicie o serviço PostgreSQL."
    exit 1
fi

echo "✅ PostgreSQL está rodando."

# Criar banco de dados e usuário (como superuser)
echo "📦 Criando banco de dados e usuário..."

sudo -u postgres psql << EOF
-- Criar banco de dados
DROP DATABASE IF EXISTS sistema_chamados_historico;
CREATE DATABASE sistema_chamados_historico;

-- Criar usuário admin
DROP USER IF EXISTS admin;
CREATE USER admin WITH PASSWORD 'admin123';

-- Conceder privilégios
GRANT ALL PRIVILEGES ON DATABASE sistema_chamados_historico TO admin;
GRANT CREATEDB TO admin;

\q
EOF

if [ $? -eq 0 ]; then
    echo "✅ Banco de dados e usuário criados com sucesso."
else
    echo "❌ Erro ao criar banco de dados e usuário."
    exit 1
fi

# Executar script de criação das tabelas
echo "🏗️ Criando tabelas..."

PGPASSWORD=admin123 psql -h localhost -U admin -d sistema_chamados_historico -f Scripts/create_database.sql

if [ $? -eq 0 ]; then
    echo "✅ Tabelas criadas com sucesso."
else
    echo "❌ Erro ao criar tabelas."
    exit 1
fi

# Verificar se as tabelas foram criadas
echo "🔍 Verificando tabelas criadas..."

PGPASSWORD=admin123 psql -h localhost -U admin -d sistema_chamados_historico -c "\dt"

echo ""
echo "🎉 Setup do banco de dados concluído com sucesso!"
echo ""
echo "📋 Informações de conexão:"
echo "   Host: localhost"
echo "   Database: sistema_chamados_historico"
echo "   Username: admin"
echo "   Password: admin123"
echo ""
echo "🚀 Agora você pode executar a aplicação com: dotnet run"