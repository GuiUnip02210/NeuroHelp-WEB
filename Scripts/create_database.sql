-- =====================================================
-- Script de Criação do Banco de Dados
-- Sistema de Chamados Histórico
-- =====================================================

-- Criar o banco de dados (executar como superuser)
-- CREATE DATABASE sistema_chamados_historico;

-- Criar usuário admin (executar como superuser)
-- CREATE USER admin WITH PASSWORD 'admin123';
-- GRANT ALL PRIVILEGES ON DATABASE sistema_chamados_historico TO admin;
-- GRANT CREATEDB TO admin;

-- =====================================================
-- Conectar ao banco sistema_chamados_historico
-- =====================================================

-- Tabela de Usuários
CREATE TABLE IF NOT EXISTS "Usuarios" (
    "Id" SERIAL PRIMARY KEY,
    "Email" VARCHAR(255) NOT NULL UNIQUE,
    "SenhaHash" VARCHAR(255) NOT NULL,
    "NomeCompleto" VARCHAR(255) NOT NULL,
    "TipoUsuario" INTEGER NOT NULL DEFAULT 1,
    "Ativo" BOOLEAN NOT NULL DEFAULT TRUE,
    "DataCadastro" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Tabela de Histórico de Chamados
CREATE TABLE IF NOT EXISTS "HISTORICO_CHAMADOS" (
    "ID_DO_CASO" INTEGER PRIMARY KEY,
    "TIPO" VARCHAR(255),
    "RESUMO" VARCHAR(500),
    "DESCRICAO" TEXT,
    "DATA_ABERTURA" TIMESTAMP NOT NULL,
    "PRIORIDADE" VARCHAR(100),
    "CATEGORIA" VARCHAR(500),
    "STATUS" VARCHAR(100),
    "ATRIBUIDO" VARCHAR(255),
    "GRUPO_ATRIBUIDO" VARCHAR(255),
    "LOCALIZACAO_AFETADA" VARCHAR(500),
    "DATA_RESOLUCAO" TIMESTAMP,
    "VIOLACAO_PROJETADA" TIMESTAMP,
    "RELATADO_POR" VARCHAR(255),
    "METODO_RELATADO" VARCHAR(255),
    "CATEGORIA_REPORTE" VARCHAR(500),
    "ULTIMA_MODIFICACAO" TIMESTAMP,
    "USUARIO_FINAL_AFETADO" VARCHAR(255),
    "EMAIL_USUARIO_FINAL" VARCHAR(255),
    "CPF_USUARIO_FINAL" VARCHAR(50),
    "DESCRICAO_SOLUCAO" TEXT
);

-- Criar usuário administrador padrão
INSERT INTO "Usuarios" ("Email", "SenhaHash", "NomeCompleto", "TipoUsuario", "Ativo") 
VALUES ('admin@historico.com', '$2a$11$8K8VQlqKZ5K5K5K5K5K5KOeJ5K5K5K5K5K5K5K5K5K5K5K5K5K5K5K', 'Administrador do Sistema', 1, TRUE)
ON CONFLICT ("Email") DO NOTHING;

-- Conceder permissões ao usuário admin
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO admin;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO admin;

-- Exemplo de inserção de dados históricos
-- INSERT INTO "HISTORICO_CHAMADOS" (
--     "ID_DO_CASO", "TIPO", "RESUMO", "DESCRICAO", "DATA_ABERTURA", 
--     "PRIORIDADE", "CATEGORIA", "STATUS", "ATRIBUIDO", "GRUPO_ATRIBUIDO", 
--     "LOCALIZACAO_AFETADA", "DATA_RESOLUCAO", "VIOLACAO_PROJETADA", 
--     "RELATADO_POR", "METODO_RELATADO", "CATEGORIA_REPORTE", 
--     "ULTIMA_MODIFICACAO", "USUARIO_FINAL_AFETADO", "EMAIL_USUARIO_FINAL", 
--     "CPF_USUARIO_FINAL", "DESCRICAO_SOLUCAO"
-- ) VALUES (
--     2189, 
--     'Solicitação', 
--     'Reset de Senha', 
--     'Solicitação de reset de senha para e-mail nominal: marcos.gouveia', 
--     '2019-04-29 07:04:19'::timestamp, 
--     'NORMAL', 
--     'INFRAESTRUTURA.SERVIDOR.ACTIVE DIRECTORY.RESET DE SENHA.ELEGIVEL', 
--     'ENCERRADO', 
--     '', 
--     'SDX SERVICE DESK N1 TELEFONE', 
--     '51645-VALE PORTO – MANUT PREDIAL E HVAC', 
--     '2019-04-29 07:06:42'::timestamp, 
--     '2019-04-29 10:04:19'::timestamp, 
--     'Marcos Santos Goveia', 
--     'Outros', 
--     'INFRAESTRUTURA.SERVIDOR.ACTIVE DIRECTORY.RESET DE SENHA.ELEGIVEL', 
--     '2019-05-07 07:36:42'::timestamp, 
--     'Marcos Santos Goveia', 
--     'marcos.xxx@soxxxo.com', 
--     '516xxxx', 
--     'Efetuado reset de senha de e-mail nominal para: marcos.gouveia'
-- );

PRINT 'Banco de dados criado com sucesso!';