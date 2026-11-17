# EcoCanta — jogos educativos de 14/11/2026

O Detetive da Fauna e a Missão de Preservação foram adaptados dentro da jornada existente. O Guardião da Cantareira é o único quiz entre as três etapas. Os quizzes complementares das fichas da CantaDex continuam disponíveis, pois não fazem parte dessa substituição.

## Experiência implementada

**Detetive da Fauna:** flashcards com fotografias e dados das espécies já cadastradas. O estudante vira as cartas para estudar habitat e alimentação, além da importância ecológica quando preenchida. Depois, seleciona uma fotografia e associa a carta com o nome correspondente. Uma associação incorreta oferece uma pista e permite tentar novamente. A fotografia também possui uma descrição textual acessível, baseada na ficha. Todos os pares devem ser completados para conquistar o avatar.

**Guardião da Cantareira:** quiz com situações-problema, explicações após cada resposta e aproveitamento mínimo de 70%, calculado no servidor. A aprovação concede o escudo. As questões continuam editáveis pela administração.

**Missão de Preservação:** cenário vetorial original, em duas dimensões, com as ferramentas Observar, Recolher, Sinalizar e Rádio. As três missões são:

1. Recolher três resíduos, um por vez, e levar cada um ao ponto de descarte da entrada.
2. Observar a ave presa, sinalizar à distância e chamar a equipe de proteção. Um profissional aparece no cenário e assume o atendimento na narrativa.
3. Investigar as pegadas de um atalho e sinalizar o percurso autorizado, representando a orientação da equipe do parque.

Os resíduos desaparecem quando recolhidos; a bolsa informa o objeto transportado; a ave recebe o estado de atendimento; e o atalho recebe uma placa. O diário identifica missões concluídas e apresenta o aprendizado de cada ação. Ao concluir os oito objetivos, o estudante pode rever o cenário e abrir o resultado com a recompensa do mapa.

A Missão usa objetivos próprios e não carrega questões do Detetive ou do Guardião. Isso corrige a repetição de perguntas apontada pelo usuário. As antigas questões permanecem como acervo de consulta, sem entrar em novas partidas.

As orientações educativas foram conferidas no [Guia de Conduta Consciente em Ambientes Naturais do ICMBio](https://www.gov.br/icmbio/pt-br/centrais-de-conteudo/guia2-pdf) e no [serviço de atendimento e destinação de fauna do Ibama](https://www.gov.br/pt-br/servicos/realizar-a-entrega-de-animal-silvestre-no-centro-de-triagem-de-animais-silvestres-cetas). O cenário é fictício, e o rádio representa uma interação interna ao jogo. O visitante não faz a soltura da ave por conta própria.

## Progresso e compatibilidade

- O servidor registra cada ação, objetivos, cartas exploradas, pares associados e diário da partida nos campos JSON existentes de `JornadaTentativas`.
- O conteúdo das cartas é copiado no início da tentativa. Uma edição posterior da ficha não muda uma partida iniciada.
- Não houve migração nem alteração de esquema. Tentativas, respostas e aprovações anteriores foram preservadas.
- Uma tentativa antiga de quiz ainda em andamento apresenta uma tela de transição. O estudante confirma o encerramento dessa versão e o início do novo jogo. O histórico mantém as questões e respostas anteriores.
- Aprovações anteriores continuam válidas para as conquistas e o mapa. Jogar novamente cria uma tentativa própria; uma partida incompleta não revoga a conquista.
- O desbloqueio do mapa continua exigindo as três etapas no servidor. XP e pontuação enviados pelo cliente não concedem acesso.
- As ações exigem autenticação, pertencimento da tentativa e antifalsificação. A comparação da revisão e do estado salvo impede que um reenvio aplique a mesma alteração duas vezes.
- As interações funcionam por clique, toque e teclado. Os formulários também executam as ações com JavaScript desativado. Com JavaScript, o conteúdo do jogo é atualizado sem recarregar a página inteira, com aviso de salvamento, recuperação de estado e feedback acessível.

## Banco preservado

Backup consistente realizado antes das alterações em `backups/antes-jogos-20261114-151846`, acompanhado de cópias dos arquivos centrais da jornada.

A comparação de registros e hashes das **24 tabelas** confirmou que os dados do banco original permaneceram iguais ao backup. O lock operacional de migração foi excluído dessa comparação. Evidência: [preservacao-banco.json](preservacao-banco.json).

Todas as contas, respostas e alterações administrativas de teste foram executadas em cópias isoladas do SQLite, armazenadas em `scratch`, sem substituir `app.db`.

## Validação executada

**396 verificações granulares aprovadas:**

| Execução | Quantidade | Evidência |
| --- | ---: | --- |
| Dois jogos, persistência e compatibilidade | 106 | [resultado.json](resultado.json) |
| Regressão de contas, catálogo, administração, conteúdos e avistamentos | 266 | [resultado da regressão](../validacao/resultado.json) |
| Formulários administrativos e quiz por teclado | 24 | [resultado complementar](../validacao/resultado-complementar.json) |

As verificações incluem cartas pelo teclado, associação incorreta, pistas, todas as associações, quiz reprovado e aprovado, coleta e descarte separados, repetição sem duplicar pontos, sequência de atendimento da ave, conclusão das missões, novas partidas, outro usuário tentando acessar a tentativa, antifalsificação, retomada após login, transição de tentativas antigas e bloqueio/liberação do mapa.

O Chrome foi utilizado em **1440, 768 e 390 pixels**, com temas claro e escuro. Foram geradas **16 capturas específicas dos jogos** em [capturas](capturas), além das capturas da regressão. Foram conferidos carregamento de imagens, erros JavaScript, transbordamento horizontal e tamanho dos alvos do cenário no celular. Capturas de cartas e do cenário foram inspecionadas visualmente.

Restore e build concluídos. A compilação final da aplicação apresentou **zero avisos e zero erros**. A instância foi iniciada pelo perfil `http`; servidor, jornada, CSS e JavaScript responderam em `http://localhost:5046`. Evidência: [instancia-local.json](instancia-local.json). O log do teste específico dos jogos não apresentou exceções.

Os números representam verificações técnicas e funcionais, sem equivaler a testes de aprendizagem com estudantes ou validação pedagógica pelas professoras.

## Arquivos principais

Caminhos de implementação relativos a `TCC_Grupo12_III/`:

| Área | Arquivos novos ou modificados |
| --- | --- |
| Modelos e persistência | `Models/JornadaJogos.cs`, `Models/Jornada.cs` |
| Regras e integração | `Services/JornadaJogosService.cs`, `Controllers/JornadaController.cs`, `Program.cs` |
| Cartas | `Views/Jornada/Cartas.cshtml`, `_Flashcards.cshtml` |
| Cenário | `Views/Jornada/Missao.cshtml`, `_CenarioMissao.cshtml` |
| Estados, histórico e resultados | `Views/Jornada/_EstadoJogo.cshtml`, `AtualizacaoJogo.cshtml`, `Resultado.cshtml`, `Progresso.cshtml` |
| Navegação e orientações | `Views/Jornada/Index.cshtml`, `Mapa.cshtml`, `Views/Home/Index.cshtml`, `Sobre.cshtml`, `Views/Faq/Index.cshtml`, `Views/Shared/_Layout.cshtml` |
| Administração | `Controllers/AdminJornadaController.cs`, `Views/AdminJornada/Index.cshtml`, `Edit.cshtml` |
| Apresentação e interação | `wwwroot/css/jornada-jogos.css`, `wwwroot/js/jornada-jogos.js` |

Validação: `tests/EcoCanta.Verificacao/JogosEducativos.cs`, `JogarAtividade.cs`, `BackupJogos.cs`, `Program.cs`, `Complementos.cs` e `RevisaoInterface.cs`. Documentação: conteúdo JSON, dois DOCX e scripts de atualização/conferência nesta pasta. As versões anteriores dos documentos ficam em `documentacao/versoes-documentos`.

## Documentação e execução

Os dois DOCX da Meta 2 foram atualizados a partir do conteúdo acadêmico existente. A subseção 4.2 descreve os três formatos, os critérios de conclusão, o histórico e a administração; inclui **37 capturas reais**. O documento de identidade visual incorpora os temas e a apresentação dos jogos, com **8 imagens**. Os pacotes e seus XML foram validados. O campo de GitHub continua pendente, sem endereço inventado.

Ambos foram abertos e paginados no Word: **44 páginas** na subseção 4.2 e **12 páginas** na identidade visual. Evidência: [documentos-word.json](documentos-word.json). A exportação opcional de PDF ficou bloqueada no Word e foi interrompida; os PDFs anteriores foram preservados em `versoes-documentos`. Os DOCX são os arquivos atuais desta entrega.

Na pasta `TCC_Grupo12_III`:

```powershell
dotnet restore
dotnet build
dotnet run --launch-profile http
```

Abra `http://localhost:5046/Jornada`, entre na conta e selecione “Começar ou continuar” ou “Praticar novamente”. Para repetir os testes específicos, na raiz:

```powershell
dotnet run --project tests/EcoCanta.Verificacao/EcoCanta.Verificacao.csproj -- . --jogos
```

## Pontos para a reunião com as professoras

O usuário pode experimentar os dois jogos e solicitar mudanças. A quantidade de missões, as pistas, a dificuldade e o critério de conclusão por domínio dos objetivos deverão ser avaliados com as orientadoras. A lista final das 14 espécies e a revisão científica continuam pendentes; as cartas utilizam os seis registros originais disponíveis. Não foram acrescentadas espécies ou informações de conservação presumidas. Nenhuma publicação ou envio ao GitHub foi realizado.
