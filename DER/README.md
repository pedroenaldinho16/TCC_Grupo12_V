# 🌿 EcoCanta — Modelo Entidade-Relacionamento (DER)
**Projeto de Conclusão de Curso (TCC) — Grupo 12 (3º Módulo de Informática)**  
**Banco de Dados:** MySQL 8.0+ / MariaDB • **Engine:** InnoDB • **Charset:** `utf8mb4`

---

## 📁 Arquivos Inclusos nesta Pasta

| Arquivo | Descrição |
| :--- | :--- |
| **`DER_EcoCanta.mwb`** | **Arquivo de Projeto do MySQL Workbench.** Abre diretamente o modelo com as 21 tabelas dispostas visualmente no *EER Diagram*, incluindo campos, tipos, chaves primárias, estrangeiras e linhas de relacionamento. |
| **`DER_EcoCanta.sql`** | **Script DDL SQL Completo.** Script executável compatível com MySQL 8.0 e MariaDB para recriação integral do banco de dados `ecocanta_db`, com `ENGINE = InnoDB`, índices únicos e regras de integridade referencial (`ON DELETE CASCADE` / `SET NULL`). |
| **`DER_EcoCanta_Diagrama.png`** | **Imagem Panorâmica em Alta Definição.** Renderização visual de todo o modelo relacional dividida em 5 clusters temáticos com identificação de PKs, FKs e tipos de dados. |
| **`DER_EcoCanta_Visual.html`** | **Visualizador Web Interativo.** Página HTML autocontida com estilização moderna para apresentação em slides, relatórios ou consulta rápida. |

---

## 🗂️ Arquitetura das 21 Tabelas por Módulo

### 1. Usuários, Autenticação e Moderação
* **`Usuarios`**: Gerenciamento de credenciais, pontuação de experiência (XP), privilégios de moderação (`IsModerador`), administração (`IsAdmin`), homologação de jornalistas ambientais (`IsJornalista`, `RegistroJornalista`, `StatusJornalista`) e histórico de suspensões/banimentos.
* **`Denuncias`**: Canal de fiscalização cidadã para relatos de crimes ambientais (descarte irregular de lixo, desmatamento, caça ou invasão de área de manancial) com suporte a anexos probatórios e parecer administrativo.
* **`FaqTickets`**: Protocolos de suporte abertos pela comunidade para esclarecimento de dúvidas e solicitações.
* **`FaqMensagens`**: Mensagens bidirecionais trocadas entre o usuário e a equipe de moderação dentro de cada chamado.

### 2. Biodiversidade, Estudos e CantaDex
* **`Especies`**: Fichas taxonômicas oficiais da fauna e flora monitoradas na Serra da Cantareira (nome popular, binomial científico, bioma, dieta, hábitos e status de conservação IUCN).
* **`EspecieQuizPerguntas`**: Banco de questões ecológicas interativas por espécie (exigência de $\ge 70\%$ de acertos para desbloqueio da cartinha).
* **`PokedexRegistros`**: Registro de domínio do usuário sobre a espécie na CantaDex (exige Quiz Aprovado + Leitura de Artigo Científico).
* **`ReservasAvistamento`**: Cotas individuais liberadas que autorizam o guardião a registrar avistamentos de determinada espécie.
* **`EstudosCientificos`**: Repositório de pesquisas, artigos e monografias científicas submetidas por pesquisadores e homologadas pela administração.
* **`NotasEstudo`**: Caderno digital pessoal para resumos, fichamentos acadêmicos e hipóteses vinculadas a artigos científicos.

### 3. Mapeamento Colaborativo e Avistamentos
* **`Avistamentos`**: Registros georreferenciados (Latitude e Longitude em `DECIMAL(10,7)`) de observações de animais silvestres na Serra da Cantareira.
* **`AvistamentoMidias`**: Galeria de fotos e vídeos probatórios vinculados a cada avistamento reportado.

### 4. Comunidades e Fórum Participativo
* **`ForumComunidades`**: Grupos temáticos fundados pela comunidade e aprovados por moderadores (ex: Guardiões Felinos, Observadores de Aves).
* **`ForumTopicos`**: Publicações, debates, dúvidas e reportagens ambientais publicadas nos canais do fórum.
* **`ForumRespostas`**: Comentários e discussões em réplica às postagens principais.
* **`ForumCurtidasTopico`**: Curtidas e engajamento concedidos aos tópicos.
* **`ForumCurtidasResposta`**: Votos positivos aplicados a comentários individuais.

### 5. Campanhas Ambientais e Voluntariado
* **`Campanhas`**: Mutirões e mobilizações ecológicas de campo (reflorestamento, limpeza de trilhas e palestras comunitárias).
* **`CampanhaParticipacoes`**: Relação associativa (N:M) que armazena a inscrição e confirmação de voluntários em cada campanha.

### 6. Gamificação Diária
* **`NivelJogos`**: Matrizes em JSON com mapas diários do mini-jogo de resgate animal e conscientização.
* **`JogoTentativas`**: Histórico de desempenho dos jogadores (pontuação, tempo gasto, vidas restantes e espécies resgatadas).

---

## 🛠️ Como Abrir e Editar no MySQL Workbench

1. Abra o software **MySQL Workbench** instalado em seu computador (`C:\Program Files\MySQL\MySQL Workbench 8.0 CE\MySQLWorkbench.exe`).
2. No menu superior, clique em **File** $\rightarrow$ **Open Model...** (ou pressione `Ctrl + O`).
3. Navegue até a pasta `TCC_Grupo12_V\DER\` e selecione o arquivo **`DER_EcoCanta.mwb`**.
4. O diagrama **EER Diagram** será aberto automaticamente, exibindo todas as 21 tabelas com suas cores, chaves primárias (`PK`), estrangeiras (`FK`) e conectores de integridade relacional.
5. Caso deseje exportar para PDF ou imagem diretamente pelo Workbench:
   * Vá em **File** $\rightarrow$ **Export** $\rightarrow$ **Export as PNG...** ou **Export as PDF...**.

---

## 💻 Como Importar via Script SQL (`DER_EcoCanta.sql`)

Caso deseje recriar a estrutura do banco via código ou conectar em um servidor MySQL/MariaDB:
1. Abra a conexão desejada no **MySQL Workbench**.
2. Vá em **File** $\rightarrow$ **Open SQL Script...** (ou pressione `Ctrl + Shift + O`).
3. Selecione o arquivo **`DER_EcoCanta.sql`**.
4. Clique no ícone de raio (**Execute**) para criar a base `ecocanta_db` com todas as 21 tabelas, chaves primárias, estrangeiras e índices.
