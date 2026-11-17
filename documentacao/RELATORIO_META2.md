# EcoCanta — relatório da reformulação da Meta 2

Data: 13/11/2026. Projeto existente: ASP.NET Core MVC, .NET 10, EF Core e SQLite.

Destaque do mapa retirado em 15/11/2026 a pedido do usuário. Os mapas de consulta e registro foram restaurados à versão anterior, sem o contorno ou preenchimento da Cantareira. O [relatório do destaque](mapa-20261115/RELATORIO.md) permanece apenas como histórico da alteração desfeita.

Correção de 15/11/2026: cartas com duas faces persistentes, animação imediata e coleta com fila de salvamento e margem de soltura. Foram aprovadas 173 verificações, incluindo atraso e interrupção de rede. Consulte o [relatório da correção](correcoes-20261115/RELATORIO.md).

Revisão de interações em 14/11/2026: animações no Detetive e Missão com cenário mais claro, tarefas guiadas e arraste por mouse/toque. Foram aprovadas 154 verificações. Consulte o [relatório das interações](interacoes-20261114/RELATORIO.md). O segundo desafio permaneceu como estava.

Revisão de jogos em 14/11/2026: Detetive com flashcards e associação, Guardião como único quiz da jornada e Missão com cenário interativo. Foram aprovadas 396 verificações nesta etapa, e os dois DOCX foram atualizados. Consulte o [relatório dos jogos](jogos-20261114/RELATORIO.md) para comportamento, arquivos e evidências atuais.

Revisão visual anterior de 14/11/2026: acesso permanente ao mapa, apresentação ilustrada das recompensas e modo escuro implementados, com 210 verificações aprovadas. Consulte o [relatório visual](revisao-20261114/RELATORIO.md). O texto abaixo mantém o registro da entrega inicial de 13/11; a revisão dos jogos substitui suas descrições dos desafios e dos documentos.

## Resultado implementado

A estrutura original foi reaproveitada. A interface e os fluxos prioritários da Meta 2 estão implementados no projeto real. A validação editorial das 14 espécies e de parte do acervo científico continua pendente; a aplicação e os documentos identificam essas lacunas.

- Homepage pública, cabeçalho, rodapé, login, cadastro, apresentação do projeto, catálogo, fichas, jornada, progresso, perfil, ajuda e telas administrativas reformulados.
- Logotipo original extraído do documento de identidade visual, sem redesenho. Paleta do grupo, Sora e Inter locais, componentes reutilizáveis e estados de foco visíveis.
- Menu móvel com abertura, fechamento por Escape e indicação de estado; grades responsivas e tabelas administrativas apresentadas como registros com rótulos no celular.
- Catálogo público com busca e filtros Aves / Animais Terrestres. Fichas com habitat, alimentação, características, ameaças, importância ecológica, preservação e referências. Conservação separada em abrangência global, brasileira e paulista.
- Três desafios: Detetive da Fauna, Guardião da Cantareira e Missão de Preservação. Respostas, pontuação, aproveitamento e datas persistidos por tentativa; explicações após acerto ou erro; revisão, repetição e retomada.
- Aproveitamento mínimo de 70% calculado no servidor, com progressão sequencial. A aprovação histórica permanece válida mesmo quando uma nova tentativa não alcança o mínimo.
- Recompensas visuais de avatar e acessório derivadas das conclusões persistidas, além do desbloqueio final do mapa.
- Questões copiadas para cada tentativa, preservando seu conteúdo durante edições administrativas. Proteção contra reenvio concorrente da mesma resposta e acesso à tentativa de outro usuário.
- Mapa protegido no servidor em suas rotas de consulta e registro. XP não concede acesso nem produz mensagem de desbloqueio. Novos avistamentos dependem de aprovação administrativa; estudantes recebem apenas regiões aproximadas, sem coordenadas precisas ou identificação do observador.
- Novas evidências armazenadas fora de wwwroot e disponibilizadas apenas à administração. Fotos e vídeos são aceitos no formulário de registro.
- Cadastro público limitado a Nome, Email e Senha, com hash BCrypt e normalização do e-mail. Campos de privilégios enviados pelo cliente são ignorados. Perfis ativos: Estudante e Administrador.
- Cookies antigos substituídos; conta, bloqueio e perfil conferidos no servidor. Logout por POST com antifalsificação. A concessão administrativa ocorre somente na área restrita.
- Voz Estudantil adaptada sobre a estrutura existente, com envio de textos, fontes, revisão administrativa e publicação condicionada à aprovação.
- Mural vinculado a cada campanha, reaproveitando tópicos, comentários e apoios. Arquivamento administrativo das publicações e suporte por chamados.
- Gestão administrativa de espécies, usuários, campanhas, quizzes, questões dos três desafios, textos, avistamentos, mural e atendimento.
- Rotas antigas de jogo diário, níveis, denúncias e suas administrações direcionadas aos fluxos correspondentes; POSTs desses módulos são recusados. Comunidades independentes, caderno e credenciamento de Jornalista não têm ações nos controllers reformulados. Seus registros históricos foram preservados.
- Migração aditiva com modelo de destino próprio, snapshot atualizado e backup SQLite consistente antes de migrações pendentes.

## Documentos entregues

- Meta2_Subsecao_4_2_EcoCanta.docx: subseção reescrita a partir do texto original, seguindo login, cadastro, início, Estudante e Administrador; 34 capturas reais incorporadas.
- Meta2_Identidade_Visual_EcoCanta.docx: marca original, paleta, tipografia, padrões de títulos, botões, cards e ícones, justificativas e exemplos reais.
- PDFs de conferência, capturas e conteúdo estruturado em documentacao.
- Gerar-Documentos.ps1 permite regenerar os DOCX a partir de conteudo-meta2.json e das imagens. As versões anteriores geradas foram mantidas em versoes-documentos.

Os fluxos autenticados das capturas utilizam dados de demonstração em cópias isoladas do banco. As imagens não são protótipos e os dados de demonstração não representam pesquisa com estudantes nem comprovação de eventos ou avistamentos reais.

## Preservação do banco

O backup inicial dos arquivos e do conjunto SQLite está em backups/antes-meta2-20261113-155828. Backups consistentes adicionais usam a API BackupDatabase do SQLite antes da migração.

Uma rotina antiga do DataSeeder regravava datas de demonstração a cada inicialização. O seeding foi condicionado à ausência dos dados iniciais. As datas afetadas foram recompostas a partir do backup anterior à migração, com outro backup antes da recomposição.

A comparação das 21 tabelas legadas verificou as colunas originais e confirmou zero registros originais ausentes ou alterados. As novas tabelas e colunas da jornada foram adicionadas sem excluir dados. A evidência detalhada está em validacao/preservacao-banco.json.

Bancos de teste não substituem app.db. Os testes de cadastro, moderação e progresso usam cópias próprias; nenhuma conta de teste criada pela verificação foi inserida no banco original.

## Arquivos alterados

A lista completa está em ARQUIVOS_ALTERADOS.txt, obtida por comparação com o backup. Os grupos principais são:

| Área | Arquivos principais |
| --- | --- |
| Inicialização e banco | Program.cs, Data/AppDbContext.cs, Data/JornadaModelSnapshot.cs, Data/JornadaSeeder.cs, Models/Jornada.cs e migração JornadaEducativa com seu Designer |
| Jornada | Controllers/JornadaController.cs, Services/JornadaService.cs, Controllers/AdminJornadaController.cs, Views/Jornada e Views/AdminJornada |
| Identidade e navegação | Views/Shared/_Layout.cshtml, _AdminTabs.cshtml, wwwroot/css/meta2.css, wwwroot/js/meta2.js, marca original e fontes locais |
| Catálogo | Controllers/EspeciesController.cs, AdminEspeciesController.cs, Models/Especie.cs, Views/Especies e formulários administrativos |
| Contas | Controllers/LoginController.cs, CadastroController.cs, PerfilController.cs, AdminController.cs e suas Views |
| Mapa | Controllers/AvistamentosController.cs, AdminAvistamentosController.cs, Filters/RequerNivelMapaAttribute.cs, Models/Avistamento.cs e Views correspondentes |
| Conteúdos e campanhas | Controllers/EducacaoController.cs, ForumController.cs, CampanhasController.cs, Models/VozForm.cs e Views correspondentes |
| Compatibilidade | Filters/EscopoMeta1Filter.cs, ajustes de autorização, Data/NivelHelper.cs, Data/PokedexHelper.cs e resultados dos quizzes |
| Validação e entrega | tests/EcoCanta.Verificacao, COMO_RODAR_O_PROJETO.txt, DOCX, capturas, JSON e este relatório |

## Testes executados

- dotnet restore: concluído.
- dotnet build: compilação final sem avisos e sem erros.
- Inicialização pelo perfil http e conferência do endereço real em log; resposta HTTP da homepage.
- 290 verificações na execução principal e 24 na complementar: 314 verificações automatizadas aprovadas. São verificações granulares, incluindo páginas, respostas e estados; não representam pesquisas de usabilidade independentes.
- Integridade SQLite, chaves estrangeiras, migrações aplicadas e correspondência do modelo EF com o snapshot.
- Cadastro válido, inválido e duplicado; senha com hash; login correto e incorreto; logout; antifalsificação e rejeição de privilégios no cadastro público.
- Permissões de Estudante, Administrador e marcadores históricos; revogação de permissão e novo login.
- Consulta pública, categorias, fichas, quiz da espécie e conquista na CantaDex.
- Três desafios com acertos e erros, feedback, mínimo de 70%, reprovação, repetição, retomada, histórico e persistência.
- Bloqueio de URL direta e salto de etapas, XP alto sem desbloqueio e liberação após os três desafios.
- Avistamento inválido e válido com fotografia real, revisão, privacidade de evidências e coordenadas.
- Texto pendente e aprovado, escape de HTML, mural, comentário, apoio e arquivamento.
- Cadastro e edição administrativa de espécies e contas, criação de campanha, questão com fonte e resposta a chamado.
- Chrome em 1440, 768 e 390 pixels; carregamento de fontes e imagens, ausência de transbordamento, menu móvel e tabelas adaptadas.
- Seleção de alternativa e envio de resposta por teclado, com confirmação de persistência.
- Pacotes DOCX e XML validados; abertura dos documentos no Word. Detalhes em validacao/documentos-word.json.

Evidências: validacao/resultado.json, resultado-complementar.json, preservacao-banco.json, documentos-word.json e inicializacao.json. Os registros de falhas de tentativas anteriores foram mantidos para rastreabilidade; os resultados finais aprovados são os JSON citados.

## Pendências que dependem do grupo e das orientadoras

1. Fornecer o link do GitHub. Conforme combinado, o documento mantém esse campo explicitamente pendente; ele precisa ser preenchido antes da entrega.
2. Validar a composição final de 14 espécies: existem 2 aves e 4 espécies terrestres originais. Faltam definir 5 aves e 3 espécies terrestres. Não foram criados animais fictícios nem nomes científicos presumidos.
3. Completar e conferir referências científicas, importância ecológica e avaliações de conservação por abrangência, com fonte e ano. O acervo original e as questões que utilizam suas fichas precisam dessa revisão editorial.
4. Ao concluir a seleção das espécies, revisar também as perguntas do Detetive e da Missão no editor administrativo.
5. Confirmar datas, organização, imagens e informações dos registros de campanhas antes de divulgá-los como ações efetivas.
6. Conferir a paginação e as referências cruzadas ao incorporar a subseção 4.2 à documentação completa do TCC.
7. Avaliar o sistema com estudantes e leitores de tela. Os testes executados não comprovam eficácia pedagógica nem conformidade integral de acessibilidade.
8. Os limites geográficos foram herdados do projeto e não foram auditados contra cartografia oficial. O fundo de mapa requer internet. As evidências legadas mantêm seus arquivos e caminhos originais, com acesso restrito à administração por middleware. Novos envios ficam fora de wwwroot.

## Como executar

Na raiz do workspace:

    cd TCC_Grupo12_III
    dotnet restore
    dotnet build
    dotnet run --launch-profile http

Endereço esperado e verificado: http://localhost:5046. A instância utilizada na conferência foi encerrada ao fim do teste.

Para atualizar durante o desenvolvimento:

    dotnet watch run --launch-profile http

As contas existentes foram preservadas; o cadastro público cria Estudantes. Senhas não são publicadas neste relatório.

Não houve publicação da aplicação, criação de repositório ou envio de alterações ao GitHub.

## Materiais consultados

Os cinco materiais em referencias foram lidos: Meta 1, Meta 2, documentação original, identidade visual e protótipo. A organização de trilhas de https://www.cursoemvideo.com/ foi consultada como referência, preservando a marca EcoCanta.

As orientações gerais de visitação e encaminhamento aos órgãos competentes utilizadas nos desafios apontam para o Guia de Conduta Consciente do ICMBio e o serviço oficial do Ibama:
- https://www.gov.br/icmbio/pt-br/centrais-de-conteudo/guia2-pdf
- https://www.gov.br/pt-br/servicos/apresentar-denuncia-ambiental
