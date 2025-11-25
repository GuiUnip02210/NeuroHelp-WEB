-- =================================================================
-- Script de Criação do Banco de Dados (Versão Final)
-- Sistema de Chamados - PIM
-- =================================================================

-- Apaga o banco antigo para garantir um recomeço limpo
USE master;
GO
DROP DATABASE IF EXISTS SistemaChamados;
GO
CREATE DATABASE SistemaChamados;
GO
USE SistemaChamados;
GO

-- =============================================
-- 1. Tabela Status, Prioridades, Categorias
-- =============================================
CREATE TABLE Status (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nome NVARCHAR(50) NOT NULL,
    Descricao NVARCHAR(255) NULL,
    DataCadastro DATETIME NOT NULL DEFAULT GETDATE(),
    Ativo BIT NOT NULL DEFAULT 1
);
GO

CREATE TABLE Prioridades (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nome NVARCHAR(50) NOT NULL,
    Nivel INT NOT NULL,
    Descricao NVARCHAR(255) NULL,
    DataCadastro DATETIME NOT NULL DEFAULT GETDATE(),
    Ativo BIT NOT NULL DEFAULT 1
);
GO

CREATE TABLE Categorias (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nome NVARCHAR(100) NOT NULL UNIQUE,
    Descricao NVARCHAR(255) NULL,
    DataCadastro DATETIME NOT NULL DEFAULT GETDATE(),
    Ativo BIT NOT NULL DEFAULT 1
);
GO

-- =============================================
-- 2. Tabela Usuarios
-- =============================================
CREATE TABLE Usuarios (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    NomeCompleto NVARCHAR(150) NOT NULL,
    Email NVARCHAR(150) NOT NULL UNIQUE,
    SenhaHash NVARCHAR(255) NOT NULL,
    TipoUsuario INT NOT NULL, -- 1: Usuário Comum, 2: Técnico, 3: Admin
    DataCadastro DATETIME NOT NULL DEFAULT GETDATE(),
    Ativo BIT NOT NULL DEFAULT 1,
    PasswordResetToken NVARCHAR(255) NULL,
    ResetTokenExpires DATETIME NULL,
    EspecialidadeCategoriaId INT NULL,
    CONSTRAINT FK_Usuarios_Categorias_Especialidade 
    FOREIGN KEY (EspecialidadeCategoriaId) REFERENCES Categorias(Id)
);
GO

-- =============================================
-- 3. Tabela Chamados
-- =============================================
CREATE TABLE Chamados (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Titulo NVARCHAR(200) NOT NULL,
    Descricao NVARCHAR(MAX) NOT NULL,
    DataAbertura DATETIME NOT NULL DEFAULT GETDATE(),
    DataUltimaAtualizacao DATETIME NULL,
    DataFechamento DATETIME NULL,
    SolicitanteId INT NOT NULL,
    TecnicoId INT NULL,
    StatusId INT NOT NULL,
    PrioridadeId INT NOT NULL,
    CategoriaId INT NOT NULL,
    SlaDataExpiracao DATETIME NULL, -- ADICIONAR ESTA LINHA
    CONSTRAINT FK_Chamados_Usuario_Solicitante FOREIGN KEY (SolicitanteId) REFERENCES Usuarios(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Chamados_Usuario_Tecnico FOREIGN KEY (TecnicoId) REFERENCES Usuarios(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Chamados_Status FOREIGN KEY (StatusId) REFERENCES Status(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Chamados_Prioridades FOREIGN KEY (PrioridadeId) REFERENCES Prioridades(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Chamados_Categorias FOREIGN KEY (CategoriaId) REFERENCES Categorias(Id) ON DELETE NO ACTION
);
GO

-- =============================================
-- 4. Tabela Comentarios (NOVA)
-- =============================================
CREATE TABLE Comentarios (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Texto NVARCHAR(1000) NOT NULL,
    DataCriacao DATETIME NOT NULL DEFAULT GETDATE(),
    ChamadoId INT NOT NULL,
    UsuarioId INT NOT NULL,
    CONSTRAINT FK_Comentarios_Chamados_ChamadoId 
    FOREIGN KEY (ChamadoId) REFERENCES Chamados(Id) ON DELETE CASCADE, -- Se apagar o chamado, apaga os comentários
    CONSTRAINT FK_Comentarios_Usuarios_UsuarioId 
    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id) ON DELETE NO ACTION -- Não deixa apagar usuário que comentou
);
GO


-- =============================================
-- 5. Dados Iniciais (Seed Data)
-- =============================================
INSERT INTO dbo.Status (Nome, Descricao) VALUES 
('Aberto', 'Chamado recém criado e aguardando atribuição.'),
('Em Andamento', 'Um técnico já está trabalhando no chamado.'),
('Aguardando Resposta', 'Aguardando mais informações do usuário.'),
('Fechado', 'O chamado foi resolvido.'),
('Violado', 'O prazo de resolução (SLA) do chamado foi excedido.'); -- ADICIONAR ESTA LINHA
GO

INSERT INTO dbo.Prioridades (Nome, Nivel, Descricao) VALUES 
('Baixa', 1, 'Resolver quando possível.'),
('Média', 2, 'Prioridade normal.'),
('Alta', 3, 'Resolver com urgência.');
GO

INSERT INTO dbo.Categorias (Nome, Descricao) VALUES 
('Hardware', 'Problemas com peças físicas do computador.'),
('Software', 'Problemas com programas e sistemas.'),
('Rede', 'Problemas de conexão com a internet ou rede interna.'),
('Acesso/Login', 'Problemas de senha ou acesso a sistemas.');
GO

-- Inserindo o usuário Administrador Padrão (Tipo 3)
-- A senha é 'admin123'
INSERT INTO dbo.Usuarios 
    (NomeCompleto, Email, SenhaHash, TipoUsuario, EspecialidadeCategoriaId, Ativo)
VALUES 
    ('Administrador Neuro Help', 'admin@helpdesk.com', '$2a$11$7Wm1iN97aWdOZpK0IptKiOqE6rW1MikaR9Jv66YE.TJLDJ/Qce/BS', 3, NULL, 1);
GO

PRINT 'Banco de dados para o sistema de chamados (Versão Final) criado com sucesso!';