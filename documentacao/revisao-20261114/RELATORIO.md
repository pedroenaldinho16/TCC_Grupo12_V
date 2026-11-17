# EcoCanta — revisão de 14/11/2026

Esta revisão melhora a localização do mapa, apresenta as recompensas da jornada e acrescenta os modos claro e escuro ao projeto MVC existente.

## Comportamento implementado

- **Mapa:** entrada permanente no menu principal, inclusive no celular, chamada na página inicial, na jornada e nas conquistas, além do rodapé. O endereço `/Mapa` apresenta os requisitos e o próximo desafio enquanto o estudante não conclui a jornada. Depois das três aprovações, abre diretamente o mapa existente. Administradores mantêm o acesso autorizado.
- **Proteção:** tentativas de acessar as rotas de avistamentos sem concluir os desafios são encaminhadas pelo servidor à apresentação do mapa. Essa apresentação não entrega os registros de avistamentos.
- **Recompensas:** o sistema já derivava as conquistas das aprovações persistidas. A apresentação foi ampliada: avatar ilustrado de Guardião após o Detetive, escudo equipado automaticamente após o Guardião e mapa após a Missão. Resultado e “Minhas conquistas” mostram os itens e os requisitos. Aprovações anteriores são reconhecidas, inclusive após novo login ou uma tentativa posterior insuficiente. Não há editor de roupas ou seleção manual de acessórios nesta revisão.
- **Tema:** botão de sol/lua no cabeçalho, acessível também com o menu móvel fechado. O primeiro acesso acompanha o tema do sistema; uma escolha explícita fica salva no navegador. O tema é aplicado antes dos estilos para evitar clarão ao navegar. Se o armazenamento estiver bloqueado, a alternância continua funcionando na página.
- **Identidade:** os verdes, a marca original e as fotografias foram preservados. Fundos, textos, campos, mensagens e controles administrativos recebem cores próprias para cada tema. O fundo cartográfico mantém as cores do provedor; controles e pop-ups acompanham o tema.

As regras, perguntas, pontuação e dinâmica dos desafios não foram alteradas. Não houve alteração do esquema do banco nem migração nesta revisão. Os testes de cadastro, respostas e conquistas utilizaram cópias SQLite isoladas, sem inserir contas ou progresso de teste no banco original.

## Arquivos de implementação

Caminhos relativos a `TCC_Grupo12_III/`:

| Área | Arquivos |
| --- | --- |
| Acesso ao mapa | `Controllers/JornadaController.cs`, `Filters/RequerNivelMapaAttribute.cs`, `Views/Jornada/Mapa.cshtml` |
| Navegação | `Views/Shared/_Layout.cshtml`, `Views/Home/Index.cshtml`, `Views/Jornada/Index.cshtml`, `Views/Shared/_AcessoMapa.cshtml` |
| Recompensas | `Models/ApresentacaoJornadaViewModels.cs`, `Views/Shared/_GuardiaoAvatar.cshtml`, `Views/Shared/_RecompensaDesafio.cshtml`, `Views/Jornada/Progresso.cshtml`, `Views/Jornada/Resultado.cshtml` |
| Aparência | `wwwroot/js/theme.js`, `wwwroot/css/appearance.css`, `wwwroot/css/meta2.css` |

Validação: `tests/EcoCanta.Verificacao/RevisaoInterface.cs` e `TestApplication.cs`, com ajustes de execução em `Program.cs` e `Complementos.cs`. Guia: `COMO_RODAR_O_PROJETO.txt`. O teste permite indicar uma compilação isolada por `ECOCANTA_TEST_APP`, sem disputar os arquivos da instância local.

## Validação executada

- Restore e build do projeto de verificação e da aplicação: concluídos, com **zero avisos e zero erros de compilação**.
- **210 verificações granulares aprovadas**, registradas em [resultado.json](resultado.json).
- Cadastro e login de estudante; acesso administrativo; respostas insuficientes e aprovadas nos três desafios; repetição; persistência das três recompensas e novo login; bloqueio e desbloqueio do mapa no servidor.
- Chrome em 1440, 768 e 390 pixels: navegação, menu móvel, teclado, tema do sistema, preferência explícita, recarregamento, armazenamento indisponível e movimento reduzido.
- **35 capturas reais** em [capturas](capturas), cobrindo páginas públicas, catálogo, ficha, questão ativa, resultados, conquistas, mapa bloqueado/liberado, campanhas, perfil, ajuda e administração.
- Nas páginas capturadas: verificação de transbordamento horizontal, imagens carregadas, erros JavaScript e contraste calculado de títulos, descrições, rótulos e botões principais. A verificação de contraste não representa uma auditoria integral de acessibilidade.
- Inspeção visual de capturas da homepage, conquistas, mapa bloqueado no celular e formulários administrativos escuros.
- Logs da cópia de teste conferidos: sem exceção de execução; permanece um aviso de desempenho do EF Core sobre consulta com múltiplas coleções. O aviso não impediu os fluxos verificados.
- Instância local atualizada no perfil `http`; conferência de endereço e resposta HTTP em [instancia-local.json](instancia-local.json).

Uma falha intermediária de contraste no fundo das perguntas administrativas foi corrigida antes da execução aprovada. Seu registro histórico foi preservado separadamente.

## Execução e conferência

Na pasta `TCC_Grupo12_III`:

```powershell
dotnet restore
dotnet build
dotnet run --launch-profile http
```

Endereço: `http://localhost:5046`. Mapa: `http://localhost:5046/Mapa`. Conquistas: `http://localhost:5046/Jornada/Progresso` após login. O botão Claro/Escuro fica ao lado do menu no cabeçalho; no celular aparece como ícone de sol/lua.

Para repetir a validação desta revisão, após encerrar eventuais instâncias de teste na porta 5057:

```powershell
dotnet run --project tests/EcoCanta.Verificacao/EcoCanta.Verificacao.csproj -- . --revisao-interface
```

## Próxima etapa

As mudanças na dinâmica dos desafios aguardam as próximas orientações do usuário. Os DOCX entregues em 13/11 permanecem preservados; as novas telas desta revisão estão registradas aqui e nas capturas, para atualização documental consolidada após a definição final dos desafios. As pendências editoriais anteriores continuam descritas no relatório da Meta 2. Nenhuma publicação ou envio ao GitHub foi realizado.
