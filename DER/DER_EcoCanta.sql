-- =============================================================================
-- PROJETO ECOCANTA - PORTAL AMBIENTAL E CIENTÍFICO DA SERRA DA CANTAREIRA
-- MODELO ENTIDADE-RELACIONAMENTO (DER) - SCRIPT DDL MYSQL 8.0+ / MARIADB
-- Arquivo: DER_EcoCanta.sql
-- Gerado para: TCC - Grupo 12 (3º Módulo de Informática)
-- Compatível com: MySQL Workbench 8.0 CE, phpMyAdmin, MySQL Server 8.0+
-- =============================================================================

SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0;
SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0;
SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION';

-- -----------------------------------------------------------------------------
-- CRIAÇÃO DO BANCO DE DADOS
-- -----------------------------------------------------------------------------
DROP SCHEMA IF EXISTS `ecocanta_db`;
CREATE SCHEMA IF NOT EXISTS `ecocanta_db` DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE `ecocanta_db`;

-- =============================================================================
-- MÓDULO 1: USUÁRIOS, AUTENTICAÇÃO E MODERAÇÃO
-- =============================================================================

-- Tabela: Usuarios
-- Representa todos os tipos de atores no sistema: Guardião, Moderador, Jornalista e Administrador.
CREATE TABLE IF NOT EXISTS `Usuarios` (
  `Id` INT NOT NULL AUTO_INCREMENT COMMENT 'Chave primária do usuário',
  `Nome` VARCHAR(100) NOT NULL COMMENT 'Nome completo ou exibido do usuário',
  `Email` VARCHAR(150) NOT NULL COMMENT 'E-mail para login no sistema (único)',
  `Senha` VARCHAR(255) NOT NULL COMMENT 'Senha criptografada do usuário',
  `Xp` INT NOT NULL DEFAULT 0 COMMENT 'Pontuação de experiência acumulada no portal',
  `IsAdmin` TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Flag que concede privilégios de administração global',
  `IsModerador` TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Flag que concede privilégios de moderação de conteúdo',
  `IsJornalista` TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Flag para credencial oficial de jornalista ambiental',
  `StatusJornalista` VARCHAR(50) NULL COMMENT 'Status da solicitação: Pendente, Aprovado, Reprovado',
  `RegistroJornalista` VARCHAR(100) NULL COMMENT 'Número do registro de imprensa (ex: MTB)',
  `VeiculoJornalista` VARCHAR(150) NULL COMMENT 'Nome do veículo de imprensa ou mídia independente',
  `LinkPortfolioJornalista` VARCHAR(255) NULL COMMENT 'Link para portfólio de matérias ambientais',
  `JustificativaJornalista` TEXT NULL COMMENT 'Justificativa do candidato a jornalista',
  `MotivoReprovacaoJornalista` TEXT NULL COMMENT 'Motivo em caso de rejeição da credencial',
  `DataSolicitacaoJornalista` DATETIME NULL COMMENT 'Data de envio do pedido de jornalista',
  `DataRespostaJornalista` DATETIME NULL COMMENT 'Data de homologação ou rejeição do pedido',
  `IsBanido` TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Indica se o usuário foi banido da plataforma',
  `DataBanimento` DATETIME NULL COMMENT 'Data do banimento',
  `MotivoBanimento` TEXT NULL COMMENT 'Motivo do banimento',
  `BanidoPorNome` VARCHAR(100) NULL COMMENT 'Nome do administrador que aplicou o banimento',
  `DataSuspensaoForum` DATETIME NULL COMMENT 'Data em que começou a suspensão do fórum',
  `SuspensoForumAte` DATETIME NULL COMMENT 'Data até a qual o usuário está impedido de postar',
  `MotivoSuspensaoForum` TEXT NULL COMMENT 'Motivo da suspensão preventiva no fórum',
  `SuspensoPorNome` VARCHAR(100) NULL COMMENT 'Nome do moderador que suspendeu',
  PRIMARY KEY (`Id`),
  UNIQUE INDEX `IX_Usuarios_Email` (`Email` ASC)
) ENGINE = InnoDB COMMENT = 'Tabela de usuários cadastrados e controle de privilégios';

-- Tabela: Denuncias
-- Registro de infrações e crimes ambientais na Serra da Cantareira reportados por cidadãos.
CREATE TABLE IF NOT EXISTS `Denuncias` (
  `Id` INT NOT NULL AUTO_INCREMENT COMMENT 'Chave primária da denúncia',
  `Tipo` VARCHAR(100) NOT NULL COMMENT 'Tipo de infração (Desmatamento, Lixo, Caça, etc.)',
  `Localizacao` VARCHAR(255) NOT NULL COMMENT 'Ponto de referência ou endereço aproximado',
  `DataOcorrencia` VARCHAR(100) NULL COMMENT 'Data aproximada do ocorrido relatada pelo denunciante',
  `Descricao` TEXT NOT NULL COMMENT 'Relato detalhado da ocorrência ambiental',
  `FotoPath` VARCHAR(255) NULL COMMENT 'Caminho ou URL da foto probatória anexada',
  `NomeContato` VARCHAR(100) NULL COMMENT 'Nome do denunciante (se não anônimo)',
  `EmailContato` VARCHAR(150) NULL COMMENT 'E-mail para retorno do protocolo de fiscalização',
  `Anonima` TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Indica se a denúncia foi submetida sob anonimato',
  `Resolvida` TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Status: 0 para pendente de apuração, 1 para resolvida',
  `CriadoEm` DATETIME NOT NULL COMMENT 'Data e hora do envio da denúncia',
  `ObservacaoAdmin` TEXT NULL COMMENT 'Parecer emitido pela equipe de moderação',
  PRIMARY KEY (`Id`)
) ENGINE = InnoDB COMMENT = 'Denúncias de infrações ambientais na Serra';

-- =============================================================================
-- MÓDULO 2: BIODIVERSIDADE, ESTUDOS E CANTADEX
-- =============================================================================

-- Tabela: Especies
-- Cadastro oficial das espécies da fauna e flora monitoradas na Serra da Cantareira.
CREATE TABLE IF NOT EXISTS `Especies` (
  `Id` INT NOT NULL AUTO_INCREMENT COMMENT 'Identificador único da espécie',
  `Nome` VARCHAR(100) NOT NULL COMMENT 'Nome popular da espécie (ex: Onça-parda)',
  `NomeCientifico` VARCHAR(150) NOT NULL COMMENT 'Nome científico binomial em latim',
  `Categoria` VARCHAR(50) NOT NULL COMMENT 'Classe taxonômica (Mamíferos, Aves, Répteis, etc.)',
  `StatusConservacao` VARCHAR(50) NOT NULL COMMENT 'Classificação IUCN (Vulnerável, Em Perigo, etc.)',
  `Tags` VARCHAR(255) NOT NULL COMMENT 'Tags descritivas (ex: Noturno, Carnívoro, Floresta Densa)',
  `Habitat` VARCHAR(255) NOT NULL COMMENT 'Micro-habitat característico dentro da serra',
  `Alimentacao` VARCHAR(255) NOT NULL COMMENT 'Regime alimentar e hábitos de caça/forrageamento',
  `Ameacas` TEXT NOT NULL COMMENT 'Principais riscos e fatores de degradação',
  `Caracteristicas` TEXT NOT NULL COMMENT 'Morfologia, porte, peso e características biológicas',
  `Comportamento` TEXT NOT NULL COMMENT 'Padrões de sociabilidade e reprodução',
  `StatusDetalhe` VARCHAR(255) NOT NULL COMMENT 'Detalhes sobre a conservação na Mata Atlântica',
  `ImagemUrl` VARCHAR(255) NULL COMMENT 'Caminho da foto da espécie',
  PRIMARY KEY (`Id`)
) ENGINE = InnoDB COMMENT = 'Catálogo de espécies da fauna e flora da Cantareira';

-- Tabela: EspecieQuizPerguntas
-- Perguntas interativas de conhecimento sobre cada espécie para desbloqueio da cartinha.
CREATE TABLE IF NOT EXISTS `EspecieQuizPerguntas` (
  `Id` INT NOT NULL AUTO_INCREMENT COMMENT 'Chave primária da pergunta',
  `EspecieId` INT NOT NULL COMMENT 'Espécie à qual a pergunta pertence',
  `Enunciado` TEXT NOT NULL COMMENT 'Texto da questão ecológica',
  `OpcaoA` VARCHAR(255) NOT NULL COMMENT 'Primeira opção',
  `OpcaoB` VARCHAR(255) NOT NULL COMMENT 'Segunda opção',
  `OpcaoC` VARCHAR(255) NOT NULL COMMENT 'Terceira opção',
  `OpcaoD` VARCHAR(255) NOT NULL COMMENT 'Quarta opção',
  `RespostaCorreta` INT NOT NULL COMMENT 'Índice da alternativa correta (0 a 3)',
  `Ordem` INT NOT NULL DEFAULT 1 COMMENT 'Ordem de exibição no quiz da espécie',
  PRIMARY KEY (`Id`),
  INDEX `IX_EspecieQuizPerguntas_EspecieId` (`EspecieId` ASC),
  CONSTRAINT `fk_EspecieQuizPerguntas_Especies`
    FOREIGN KEY (`EspecieId`)
    REFERENCES `Especies` (`Id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE
) ENGINE = InnoDB COMMENT = 'Perguntas avaliativas das espécies na CantaDex';

-- Tabela: PokedexRegistros
-- Registra o progresso do usuário no domínio da espécie (Cartinha da CantaDex).
CREATE TABLE IF NOT EXISTS `PokedexRegistros` (
  `Id` INT NOT NULL AUTO_INCREMENT COMMENT 'Identificador do registro de domínio',
  `UsuarioId` INT NOT NULL COMMENT 'Usuário que conquistou ou visualizou a espécie',
  `EspecieId` INT NOT NULL COMMENT 'Espécie associada',
  `VistaEm` DATETIME NOT NULL COMMENT 'Data do primeiro contato com a espécie',
  `QuizAprovado` TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Requisito 1: Quiz aprovado com nota >= 70%',
  `EstudoLido` TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Requisito 2: Leitura completa de artigo científico',
  `Dominada` TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Cartinha dourada desbloqueada na CantaDex',
  `DominadaEm` DATETIME NULL COMMENT 'Data e hora da obtenção da cartinha',
  PRIMARY KEY (`Id`),
  INDEX `IX_PokedexRegistros_EspecieId` (`EspecieId` ASC),
  UNIQUE INDEX `IX_PokedexRegistros_Usuario_Especie` (`UsuarioId` ASC, `EspecieId` ASC),
  CONSTRAINT `fk_PokedexRegistros_Usuarios`
    FOREIGN KEY (`UsuarioId`)
    REFERENCES `Usuarios` (`Id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
  CONSTRAINT `fk_PokedexRegistros_Especies`
    FOREIGN KEY (`EspecieId`)
    REFERENCES `Especies` (`Id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE
) ENGINE = InnoDB COMMENT = 'Cartinhas e desbloqueios da CantaDex por usuário';

-- Tabela: ReservasAvistamento
-- Saldo de reservas que autoriza o usuário a cadastrar avistamentos da espécie.
CREATE TABLE IF NOT EXISTS `ReservasAvistamento` (
  `Id` INT NOT NULL AUTO_INCREMENT COMMENT 'Identificador único da reserva',
  `UsuarioId` INT NOT NULL COMMENT 'Usuário detentor do direito de registro',
  `EspecieId` INT NOT NULL COMMENT 'Espécie que foi dominada',
  `QuantidadeDisponivel` INT NOT NULL DEFAULT 1 COMMENT 'Quantidade de registros permitidos',
  PRIMARY KEY (`Id`),
  INDEX `IX_ReservasAvistamento_EspecieId` (`EspecieId` ASC),
  UNIQUE INDEX `IX_ReservasAvistamento_Usuario_Especie` (`UsuarioId` ASC, `EspecieId` ASC),
  CONSTRAINT `fk_ReservasAvistamento_Usuarios`
    FOREIGN KEY (`UsuarioId`)
    REFERENCES `Usuarios` (`Id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
  CONSTRAINT `fk_ReservasAvistamento_Especies`
    FOREIGN KEY (`EspecieId`)
    REFERENCES `Especies` (`Id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE
) ENGINE = InnoDB COMMENT = 'Cotas de avistamento liberadas após dominar espécie';

-- Tabela: EstudosCientificos
-- Repositório de pesquisas, artigos e monografias submetidas por pesquisadores e guardiões.
CREATE TABLE IF NOT EXISTS `EstudosCientificos` (
  `Id` INT NOT NULL AUTO_INCREMENT COMMENT 'Identificador único da publicação científica',
  `UsuarioId` INT NOT NULL COMMENT 'Autor ou guardião que submeteu o artigo',
  `Titulo` VARCHAR(200) NOT NULL COMMENT 'Título da pesquisa científica',
  `Subtitulo` VARCHAR(255) NOT NULL COMMENT 'Subtítulo contextualizador',
  `Categoria` VARCHAR(100) NOT NULL COMMENT 'Área temática da biologia/ecologia',
  `Resumo` TEXT NOT NULL COMMENT 'Abstract ou resumo executivo',
  `ConteudoHtml` LONGTEXT NOT NULL COMMENT 'Corpo textual integral da pesquisa',
  `Slug` VARCHAR(150) NOT NULL COMMENT 'Identificador amigável para URL',
  `EspecieNome` VARCHAR(100) NULL COMMENT 'Nome da espécie abordada',
  `EspecieId` INT NULL COMMENT 'FK opcional vinculando diretamente à espécie',
  `StatusConservacao` VARCHAR(50) NULL COMMENT 'Status conservacionista discutido no trabalho',
  `Autor` VARCHAR(200) NOT NULL COMMENT 'Nome completo dos autores do artigo',
  `Instituicao` VARCHAR(200) NULL COMMENT 'Universidade, ONG ou centro de pesquisa',
  `ImagemUrl` VARCHAR(255) NULL COMMENT 'Imagem de capa do estudo',
  `TempoLeituraMinutos` INT NOT NULL DEFAULT 5 COMMENT 'Estimativa de tempo de leitura',
  `DestaquesTexto` TEXT NULL COMMENT 'Principais achados e conclusões ecológicas',
  `AmeacasTexto` TEXT NULL COMMENT 'Ameaças identificadas no habitat',
  `AcoesTexto` TEXT NULL COMMENT 'Recomendações de manejo e conservação',
  `ReferenciasTexto` TEXT NULL COMMENT 'Referências bibliográficas no formato ABNT',
  `StatusAprovacao` VARCHAR(50) NOT NULL DEFAULT 'Pendente' COMMENT 'Pendente, Aprovado ou Reprovado',
  `DataSubmissao` DATETIME NOT NULL COMMENT 'Data do envio da submissão',
  `DataResposta` DATETIME NULL COMMENT 'Data da moderação científica',
  `MotivoReprovacao` TEXT NULL COMMENT 'Motivo em caso de necessidade de revisão',
  `ObservacoesAdmin` TEXT NULL COMMENT 'Anotações internas dos pareceristas',
  PRIMARY KEY (`Id`),
  UNIQUE INDEX `IX_EstudosCientificos_Slug` (`Slug` ASC),
  INDEX `IX_EstudosCientificos_UsuarioId` (`UsuarioId` ASC),
  INDEX `IX_EstudosCientificos_EspecieId` (`EspecieId` ASC),
  CONSTRAINT `fk_EstudosCientificos_Usuarios`
    FOREIGN KEY (`UsuarioId`)
    REFERENCES `Usuarios` (`Id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
  CONSTRAINT `fk_EstudosCientificos_Especies`
    FOREIGN KEY (`EspecieId`)
    REFERENCES `Especies` (`Id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE
) ENGINE = InnoDB COMMENT = 'Artigos e pesquisas científicas sobre a Serra';

-- Tabela: NotasEstudo
-- Caderno digital pessoal para resumos, fichamentos e hipóteses dos usuários.
CREATE TABLE IF NOT EXISTS `NotasEstudo` (
  `Id` INT NOT NULL AUTO_INCREMENT COMMENT 'Chave primária da anotação',
  `UsuarioId` INT NULL COMMENT 'Autor da anotação (nulo em notas offline)',
  `Titulo` VARCHAR(150) NOT NULL COMMENT 'Título do fichamento ou resumo',
  `ConteudoHtml` LONGTEXT NOT NULL COMMENT 'Texto formatado pelo editor interativo',
  `ResumoTexto` TEXT NULL COMMENT 'Prévia textual limpa',
  `ArtigoSlug` VARCHAR(150) NULL COMMENT 'Slug do artigo de referência vinculado',
  `ArtigoTitulo` VARCHAR(200) NULL COMMENT 'Título do artigo de referência',
  `DataCriacao` DATETIME NOT NULL COMMENT 'Data de início da redação da nota',
  `DataAtualizacao` DATETIME NOT NULL COMMENT 'Data da última alteração salva',
  PRIMARY KEY (`Id`),
  INDEX `IX_NotasEstudo_UsuarioId` (`UsuarioId` ASC),
  CONSTRAINT `fk_NotasEstudo_Usuarios`
    FOREIGN KEY (`UsuarioId`)
    REFERENCES `Usuarios` (`Id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE
) ENGINE = InnoDB COMMENT = 'Caderno digital de estudos e anotações do usuário';

-- =============================================================================
-- MÓDULO 3: MAPEAMENTO COLABORATIVO E AVISTAMENTOS
-- =============================================================================

-- Tabela: Avistamentos
-- Registros de animais e espécimes observados na Serra com geolocalização.
CREATE TABLE IF NOT EXISTS `Avistamentos` (
  `Id` INT NOT NULL AUTO_INCREMENT COMMENT 'Chave primária do avistamento',
  `EspecieId` INT NULL COMMENT 'Espécie identificada (se catalogada)',
  `NomeAnimal` VARCHAR(100) NOT NULL COMMENT 'Nome popular registrado no momento da observação',
  `FotoPath` VARCHAR(255) NULL COMMENT 'Foto principal da observação',
  `Latitude` DECIMAL(10, 7) NOT NULL COMMENT 'Coordenada geográfica Latitude',
  `Longitude` DECIMAL(10, 7) NOT NULL COMMENT 'Coordenada geográfica Longitude',
  `DataAvistamento` VARCHAR(100) NOT NULL COMMENT 'Data e horário em que o animal foi visto',
  `Quantidade` INT NOT NULL DEFAULT 1 COMMENT 'Número de indivíduos observados',
  `Observacoes` TEXT NULL COMMENT 'Comportamento, estado de saúde e clima',
  `UsuarioId` INT NULL COMMENT 'Cidadão ou biólogo que realizou o registro',
  `NomeObservador` VARCHAR(100) NOT NULL COMMENT 'Nome de identificação do observador',
  `CriadoEm` DATETIME NOT NULL COMMENT 'Data e hora do cadastro no banco de dados',
  PRIMARY KEY (`Id`),
  INDEX `IX_Avistamentos_EspecieId` (`EspecieId` ASC),
  INDEX `IX_Avistamentos_UsuarioId` (`UsuarioId` ASC),
  CONSTRAINT `fk_Avistamentos_Especies`
    FOREIGN KEY (`EspecieId`)
    REFERENCES `Especies` (`Id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE,
  CONSTRAINT `fk_Avistamentos_Usuarios`
    FOREIGN KEY (`UsuarioId`)
    REFERENCES `Usuarios` (`Id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE
) ENGINE = InnoDB COMMENT = 'Avistamentos georreferenciados de animais na Cantareira';

-- Tabela: AvistamentoMidias
-- Galerias de fotos e vídeos em alta resolução associadas a um avistamento.
CREATE TABLE IF NOT EXISTS `AvistamentoMidias` (
  `Id` INT NOT NULL AUTO_INCREMENT COMMENT 'Chave primária do arquivo de mídia',
  `AvistamentoId` INT NOT NULL COMMENT 'Avistamento ao qual a mídia se vincula',
  `Caminho` VARCHAR(255) NOT NULL COMMENT 'URL ou caminho do arquivo armazenado',
  `Tipo` VARCHAR(50) NOT NULL COMMENT 'Formato da mídia: Foto ou Video',
  `CriadoEm` DATETIME NOT NULL COMMENT 'Data de upload do anexo',
  PRIMARY KEY (`Id`),
  INDEX `IX_AvistamentoMidias_AvistamentoId` (`AvistamentoId` ASC),
  CONSTRAINT `fk_AvistamentoMidias_Avistamentos`
    FOREIGN KEY (`AvistamentoId`)
    REFERENCES `Avistamentos` (`Id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE
) ENGINE = InnoDB COMMENT = 'Fotos e vídeos complementares de avistamentos';

-- =============================================================================
-- MÓDULO 4: COMUNIDADES E FÓRUM PARTICIPATIVO
-- =============================================================================

-- Tabela: ForumComunidades
-- Comunidades e grupos temáticos criados por usuários e moderadores.
CREATE TABLE IF NOT EXISTS `ForumComunidades` (
  `Id` INT NOT NULL AUTO_INCREMENT COMMENT 'Chave primária da comunidade',
  `Nome` VARCHAR(100) NOT NULL COMMENT 'Nome público da comunidade temática',
  `Slug` VARCHAR(100) NOT NULL COMMENT 'Slug da comunidade para links amigáveis',
  `Descricao` TEXT NOT NULL COMMENT 'Missão e propósito da comunidade',
  `Regras` TEXT NULL COMMENT 'Diretrizes de convivência e moderação',
  `IconePath` VARCHAR(255) NULL COMMENT 'Ícone de avatar da comunidade',
  `BannerPath` VARCHAR(255) NULL COMMENT 'Imagem de cabeçalho da comunidade',
  `CriadorId` INT NULL COMMENT 'Usuário que fundou a comunidade',
  `StatusAprovacao` VARCHAR(50) NOT NULL DEFAULT 'Aprovado' COMMENT 'Pendente ou Aprovado pela equipe',
  `AprovadoEm` DATETIME NULL COMMENT 'Data da aprovação',
  `AprovadoPorId` INT NULL COMMENT 'ID do administrador que aprovou',
  `MotivoRejeicao` TEXT NULL COMMENT 'Motivo em caso de recusa da fundação',
  `IsGeral` TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Indica se é a comunidade matriz oficial',
  `CriadoEm` DATETIME NOT NULL COMMENT 'Data de fundação',
  PRIMARY KEY (`Id`),
  UNIQUE INDEX `IX_ForumComunidades_Slug` (`Slug` ASC),
  INDEX `IX_ForumComunidades_CriadorId` (`CriadorId` ASC),
  CONSTRAINT `fk_ForumComunidades_Usuarios`
    FOREIGN KEY (`CriadorId`)
    REFERENCES `Usuarios` (`Id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE
) ENGINE = InnoDB COMMENT = 'Grupos e comunidades de guardiões no fórum';

-- Tabela: ForumTopicos
-- Postagens, discussões, dúvidas e reportagens publicadas no fórum.
CREATE TABLE IF NOT EXISTS `ForumTopicos` (
  `Id` INT NOT NULL AUTO_INCREMENT COMMENT 'Chave primária do tópico',
  `UsuarioId` INT NOT NULL COMMENT 'Autor da publicação',
  `ComunidadeId` INT NULL COMMENT 'Comunidade na qual o post foi inserido',
  `Titulo` VARCHAR(200) NOT NULL COMMENT 'Título do tópico',
  `Categoria` VARCHAR(50) NOT NULL COMMENT 'Categoria temática (Debate, Notícia, etc.)',
  `Conteudo` LONGTEXT NOT NULL COMMENT 'Texto integral do post',
  `Hashtags` VARCHAR(255) NOT NULL COMMENT 'Tags e palavras-chave para busca',
  `CriadoEm` DATETIME NOT NULL COMMENT 'Data e hora da publicação',
  PRIMARY KEY (`Id`),
  INDEX `IX_ForumTopicos_UsuarioId` (`UsuarioId` ASC),
  INDEX `IX_ForumTopicos_ComunidadeId` (`ComunidadeId` ASC),
  CONSTRAINT `fk_ForumTopicos_Usuarios`
    FOREIGN KEY (`UsuarioId`)
    REFERENCES `Usuarios` (`Id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
  CONSTRAINT `fk_ForumTopicos_ForumComunidades`
    FOREIGN KEY (`ComunidadeId`)
    REFERENCES `ForumComunidades` (`Id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE
) ENGINE = InnoDB COMMENT = 'Tópicos de debate e notícias publicados no fórum';

-- Tabela: ForumRespostas
-- Comentários e interações da comunidade em resposta a um tópico.
CREATE TABLE IF NOT EXISTS `ForumRespostas` (
  `Id` INT NOT NULL AUTO_INCREMENT COMMENT 'Chave primária da resposta',
  `ForumTopicoId` INT NOT NULL COMMENT 'Tópico de discussão ao qual a resposta pertence',
  `UsuarioId` INT NOT NULL COMMENT 'Autor do comentário',
  `Conteudo` TEXT NOT NULL COMMENT 'Texto da resposta',
  `CriadoEm` DATETIME NOT NULL COMMENT 'Data e hora da resposta',
  PRIMARY KEY (`Id`),
  INDEX `IX_ForumRespostas_ForumTopicoId` (`ForumTopicoId` ASC),
  INDEX `IX_ForumRespostas_UsuarioId` (`UsuarioId` ASC),
  CONSTRAINT `fk_ForumRespostas_ForumTopicos`
    FOREIGN KEY (`ForumTopicoId`)
    REFERENCES `ForumTopicos` (`Id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
  CONSTRAINT `fk_ForumRespostas_Usuarios`
    FOREIGN KEY (`UsuarioId`)
    REFERENCES `Usuarios` (`Id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE
) ENGINE = InnoDB COMMENT = 'Respostas e comentários em tópicos do fórum';

-- Tabela: ForumCurtidasTopico
-- Registra o apoio e curtidas de usuários nos tópicos principais.
CREATE TABLE IF NOT EXISTS `ForumCurtidasTopico` (
  `Id` INT NOT NULL AUTO_INCREMENT COMMENT 'Chave primária da curtida',
  `ForumTopicoId` INT NOT NULL COMMENT 'Tópico curtido',
  `UsuarioId` INT NOT NULL COMMENT 'Usuário que curtiu',
  `CriadoEm` DATETIME NOT NULL COMMENT 'Data e hora do like',
  PRIMARY KEY (`Id`),
  INDEX `IX_ForumCurtidasTopico_UsuarioId` (`UsuarioId` ASC),
  UNIQUE INDEX `IX_ForumCurtidasTopico_Topico_Usuario` (`ForumTopicoId` ASC, `UsuarioId` ASC),
  CONSTRAINT `fk_ForumCurtidasTopico_ForumTopicos`
    FOREIGN KEY (`ForumTopicoId`)
    REFERENCES `ForumTopicos` (`Id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
  CONSTRAINT `fk_ForumCurtidasTopico_Usuarios`
    FOREIGN KEY (`UsuarioId`)
    REFERENCES `Usuarios` (`Id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE
) ENGINE = InnoDB COMMENT = 'Curtidas dadas pelos usuários aos tópicos';

-- Tabela: ForumCurtidasResposta
-- Registra as curtidas concedidas pelos membros a comentários individuais.
CREATE TABLE IF NOT EXISTS `ForumCurtidasResposta` (
  `Id` INT NOT NULL AUTO_INCREMENT COMMENT 'Chave primária do like',
  `ForumRespostaId` INT NOT NULL COMMENT 'Resposta curtida',
  `UsuarioId` INT NOT NULL COMMENT 'Usuário autor do like',
  `CriadoEm` DATETIME NOT NULL COMMENT 'Data do voto positivo',
  PRIMARY KEY (`Id`),
  INDEX `IX_ForumCurtidasResposta_UsuarioId` (`UsuarioId` ASC),
  UNIQUE INDEX `IX_ForumCurtidasResposta_Resposta_Usuario` (`ForumRespostaId` ASC, `UsuarioId` ASC),
  CONSTRAINT `fk_ForumCurtidasResposta_ForumRespostas`
    FOREIGN KEY (`ForumRespostaId`)
    REFERENCES `ForumRespostas` (`Id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
  CONSTRAINT `fk_ForumCurtidasResposta_Usuarios`
    FOREIGN KEY (`UsuarioId`)
    REFERENCES `Usuarios` (`Id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE
) ENGINE = InnoDB COMMENT = 'Curtidas dadas às respostas do fórum';

-- =============================================================================
-- MÓDULO 5: CAMPANHAS AMBIENTAIS E VOLUNTARIADO
-- =============================================================================

-- Tabela: Campanhas
-- Mobilizações ecológicas de campo (mutirões de plantio, limpeza e palestras).
CREATE TABLE IF NOT EXISTS `Campanhas` (
  `Id` INT NOT NULL AUTO_INCREMENT COMMENT 'Chave primária da campanha',
  `Titulo` VARCHAR(200) NOT NULL COMMENT 'Título da ação ambiental',
  `Descricao` TEXT NOT NULL COMMENT 'Descrição detalhada do propósito',
  `ObjetivoPrincipal` TEXT NULL COMMENT 'Alvo estratégico da ação',
  `ObjetivosEspecificos` TEXT NULL COMMENT 'Metas pontuais do evento',
  `ImpactoEsperado` TEXT NULL COMMENT 'Resultados mensuráveis para a flora/fauna',
  `PublicoAlvo` VARCHAR(150) NULL COMMENT 'Público convidado (escolas, voluntários, etc.)',
  `LocalEncontro` VARCHAR(255) NULL COMMENT 'Endereço e ponto de encontro na Serra',
  `Horario` VARCHAR(50) NULL COMMENT 'Horário de concentração e início',
  `OQueLevar` TEXT NULL COMMENT 'Itens requeridos (luvas, garrafa d água, etc.)',
  `Cronograma` TEXT NULL COMMENT 'Programação dos horários e atividades',
  `Periodo` VARCHAR(100) NOT NULL COMMENT 'Período de realização',
  `DataInicio` VARCHAR(100) NULL COMMENT 'Data inicial da ação',
  `DataFim` VARCHAR(100) NULL COMMENT 'Data de conclusão',
  `Organizador` VARCHAR(150) NULL COMMENT 'Nome da instituição ou líder organizador',
  `Meta` INT NOT NULL DEFAULT 50 COMMENT 'Meta máxima de voluntários esperados',
  `Participantes` INT NOT NULL DEFAULT 0 COMMENT 'Quantidade de inscritos confirmados',
  `Ativa` TINYINT(1) NOT NULL DEFAULT 1 COMMENT 'Indica se a campanha está aberta a adesões',
  `StatusAprovacao` VARCHAR(50) NOT NULL DEFAULT 'Aprovado' COMMENT 'Pendente ou Aprovada',
  `MotivoReprovacao` TEXT NULL COMMENT 'Parecer em caso de não homologação',
  `DataCriacao` DATETIME NOT NULL COMMENT 'Data da proposta',
  `DataResposta` DATETIME NULL COMMENT 'Data do deferimento por moderador',
  `AprovadoPorNome` VARCHAR(100) NULL COMMENT 'Nome do moderador que validou',
  `ImagemUrl` VARCHAR(255) NULL COMMENT 'Imagem ilustrativa do evento',
  `UsuarioId` INT NULL COMMENT 'Usuário criador da campanha',
  `ComunidadeId` INT NULL COMMENT 'Comunidade vinculada ao evento',
  PRIMARY KEY (`Id`),
  INDEX `IX_Campanhas_UsuarioId` (`UsuarioId` ASC),
  INDEX `IX_Campanhas_ComunidadeId` (`ComunidadeId` ASC),
  CONSTRAINT `fk_Campanhas_Usuarios`
    FOREIGN KEY (`UsuarioId`)
    REFERENCES `Usuarios` (`Id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE,
  CONSTRAINT `fk_Campanhas_ForumComunidades`
    FOREIGN KEY (`ComunidadeId`)
    REFERENCES `ForumComunidades` (`Id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE
) ENGINE = InnoDB COMMENT = 'Campanhas ecológicas e mutirões de preservação';

-- Tabela: CampanhaParticipacoes
-- Relação associativa (N:M) entre voluntários e as campanhas em que se inscreveram.
CREATE TABLE IF NOT EXISTS `CampanhaParticipacoes` (
  `Id` INT NOT NULL AUTO_INCREMENT COMMENT 'Chave primária da inscrição',
  `CampanhaId` INT NOT NULL COMMENT 'Campanha ecológica escolhida',
  `UsuarioId` INT NOT NULL COMMENT 'Voluntário inscrito',
  PRIMARY KEY (`Id`),
  INDEX `IX_CampanhaParticipacoes_UsuarioId` (`UsuarioId` ASC),
  UNIQUE INDEX `IX_CampanhaParticipacoes_Campanha_Usuario` (`CampanhaId` ASC, `UsuarioId` ASC),
  CONSTRAINT `fk_CampanhaParticipacoes_Campanhas`
    FOREIGN KEY (`CampanhaId`)
    REFERENCES `Campanhas` (`Id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
  CONSTRAINT `fk_CampanhaParticipacoes_Usuarios`
    FOREIGN KEY (`UsuarioId`)
    REFERENCES `Usuarios` (`Id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE
) ENGINE = InnoDB COMMENT = 'Inscrições e engajamento de usuários nas campanhas';

-- =============================================================================
-- MÓDULO 6: GAMIFICAÇÃO, NÍVEIS E MISSÃO DIÁRIA
-- =============================================================================

-- Tabela: NivelJogos
-- Fases e mapas do mini-jogo diário de resgate animal e conscientização ecológica.
CREATE TABLE IF NOT EXISTS `NivelJogos` (
  `Id` INT NOT NULL AUTO_INCREMENT COMMENT 'Chave primária da fase',
  `Titulo` VARCHAR(100) NOT NULL COMMENT 'Nome temático do mapa de resgate',
  `Descricao` TEXT NOT NULL COMMENT 'Briefing e objetivos da missão diária',
  `Data` VARCHAR(50) NOT NULL COMMENT 'Data do calendário à qual a fase se destina',
  `Ativo` TINYINT(1) NOT NULL DEFAULT 1 COMMENT 'Flag de disponibilidade da missão',
  `DadosJson` LONGTEXT NOT NULL COMMENT 'Matriz em JSON com tiles, animais e obstáculos',
  `TotalAnimais` INT NOT NULL DEFAULT 3 COMMENT 'Número de animais que necessitam de resgate',
  PRIMARY KEY (`Id`)
) ENGINE = InnoDB COMMENT = 'Fases e labirintos gerados para o jogo educativo';

-- Tabela: JogoTentativas
-- Histórico de execuções das missões diárias com pontuação, tempo e vidas restantes.
CREATE TABLE IF NOT EXISTS `JogoTentativas` (
  `Id` INT NOT NULL AUTO_INCREMENT COMMENT 'Chave primária do score',
  `NivelJogoId` INT NOT NULL COMMENT 'Fase jogada',
  `UsuarioId` INT NOT NULL COMMENT 'Jogador',
  `Dia` VARCHAR(50) NOT NULL COMMENT 'Data do jogo diário (AAAA-MM-DD)',
  `IniciadoEm` DATETIME NOT NULL COMMENT 'Hora de início da partida',
  `ConcluidoEm` DATETIME NULL COMMENT 'Hora da finalização',
  `Concluida` TINYINT(1) NOT NULL DEFAULT 0 COMMENT '1 se completou com sucesso o resgate',
  `AnimaisResgatados` INT NOT NULL DEFAULT 0 COMMENT 'Quantidade resgatada',
  `TotalAnimais` INT NOT NULL DEFAULT 3 COMMENT 'Meta total da fase',
  `TempoSegundos` INT NOT NULL DEFAULT 0 COMMENT 'Cronômetro decorrido',
  `VidasRestantes` INT NOT NULL DEFAULT 3 COMMENT 'Vidas restantes ao fim da partida',
  `AnimaisResgatadosIds` VARCHAR(255) NOT NULL DEFAULT '[]' COMMENT 'IDs das espécies salvas em JSON',
  PRIMARY KEY (`Id`),
  INDEX `IX_JogoTentativas_NivelJogoId` (`NivelJogoId` ASC),
  UNIQUE INDEX `IX_JogoTentativas_Usuario_Nivel_Dia` (`UsuarioId` ASC, `NivelJogoId` ASC, `Dia` ASC),
  CONSTRAINT `fk_JogoTentativas_NivelJogos`
    FOREIGN KEY (`NivelJogoId`)
    REFERENCES `NivelJogos` (`Id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
  CONSTRAINT `fk_JogoTentativas_Usuarios`
    FOREIGN KEY (`UsuarioId`)
    REFERENCES `Usuarios` (`Id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE
) ENGINE = InnoDB COMMENT = 'Tentativas, pontuações e desempenho dos jogadores';

-- =============================================================================
-- MÓDULO 7: FAQ, ATENDIMENTO E SUPORTE INTERATIVO
-- =============================================================================

-- Tabela: FaqTickets
-- Atendimentos abertos por usuários para tirar dúvidas com moderadores e equipe técnica.
CREATE TABLE IF NOT EXISTS `FaqTickets` (
  `Id` INT NOT NULL AUTO_INCREMENT COMMENT 'Chave primária do ticket de suporte',
  `UsuarioId` INT NOT NULL COMMENT 'Cidadão ou guardião que abriu a solicitação',
  `Assunto` VARCHAR(200) NOT NULL COMMENT 'Assunto principal do chamado',
  `Categoria` VARCHAR(100) NOT NULL COMMENT 'Categoria (Credencial, Fórum, Espécies, Geral)',
  `Status` VARCHAR(50) NOT NULL DEFAULT 'Aberto' COMMENT 'Aberto, Em Atendimento, Encerrado',
  `CriadoEm` DATETIME NOT NULL COMMENT 'Data de abertura do protocolo',
  `AtualizadoEm` DATETIME NOT NULL COMMENT 'Data do último despacho/resposta',
  `UltimaRespostaPor` VARCHAR(100) NULL COMMENT 'Nome de quem enviou a última mensagem',
  PRIMARY KEY (`Id`),
  INDEX `IX_FaqTickets_UsuarioId` (`UsuarioId` ASC),
  CONSTRAINT `fk_FaqTickets_Usuarios`
    FOREIGN KEY (`UsuarioId`)
    REFERENCES `Usuarios` (`Id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE
) ENGINE = InnoDB COMMENT = 'Tickets de suporte e esclarecimento de dúvidas';

-- Tabela: FaqMensagens
-- Histórico de mensagens do chat trocadas entre o usuário e a equipe de moderação.
CREATE TABLE IF NOT EXISTS `FaqMensagens` (
  `Id` INT NOT NULL AUTO_INCREMENT COMMENT 'Chave primária da mensagem de chat',
  `FaqTicketId` INT NOT NULL COMMENT 'Ticket de atendimento ao qual a mensagem pertence',
  `UsuarioId` INT NOT NULL COMMENT 'Autor da mensagem',
  `Conteudo` TEXT NOT NULL COMMENT 'Texto da mensagem',
  `CriadoEm` DATETIME NOT NULL COMMENT 'Data e horário de envio',
  `IsEquipeModOuAdmin` TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Flag: 1 se escrita por moderador/admin',
  `CargoAutor` VARCHAR(50) NULL COMMENT 'Cargo exibido no balão de chat',
  PRIMARY KEY (`Id`),
  INDEX `IX_FaqMensagens_FaqTicketId` (`FaqTicketId` ASC),
  INDEX `IX_FaqMensagens_UsuarioId` (`UsuarioId` ASC),
  CONSTRAINT `fk_FaqMensagens_FaqTickets`
    FOREIGN KEY (`FaqTicketId`)
    REFERENCES `FaqTickets` (`Id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
  CONSTRAINT `fk_FaqMensagens_Usuarios`
    FOREIGN KEY (`UsuarioId`)
    REFERENCES `Usuarios` (`Id`)
    ON DELETE RESTRICT
    ON UPDATE CASCADE
) ENGINE = InnoDB COMMENT = 'Mensagens enviadas no chat de suporte do FAQ';

-- -----------------------------------------------------------------------------
-- RESTAURAÇÃO DE CONFIGURAÇÕES GLOBAIS
-- -----------------------------------------------------------------------------
SET SQL_MODE=@OLD_SQL_MODE;
SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS;
SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS;

-- =============================================================================
-- FIM DO SCRIPT DDL
-- =============================================================================
