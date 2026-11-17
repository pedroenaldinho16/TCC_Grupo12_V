# EcoCanta — correções das cartas e da coleta, 15/11/2026

O usuário confirmou que a carta mudava de fotografia para texto sem mostrar a virada e que a coleta apresentava falhas ao arrastar com o mouse.

## Diagnóstico e correções

A versão anterior aguardava a resposta e o HTML do servidor antes de animar a carta e substituía seu elemento no DOM. Um teste com 350 ms de latência reproduziu a ausência de movimento antes da resposta. A evidência anterior foi preservada em `resultado-antes-da-correcao.json` e `falha-reproduzida-antes-da-correcao.txt`.

As cartas agora mantêm frente e verso no mesmo elemento, com uma transição CSS tridimensional iniciada no clique. O salvamento recebe metadados JSON e atualiza apenas estudo, revisão e feedback, preservando o elemento que está girando. Cliques rápidos são processados em sequência, com a intenção explícita de abrir ou fechar cada carta. A interface apresenta um indicador enquanto existem ações a salvar.

O botão “Animações: ligadas/reduzidas” permite escolher a preferência no navegador. O padrão respeita a redução de movimento do sistema. A ativação explícita no jogo supera a regra global de redução somente na transição das cartas; nenhuma configuração do Windows é alterada.

Na coleta, a versão anterior verificava um ponto de soltura muito restrito, bloqueava novos gestos durante o salvamento e suprimia cliques por um intervalo fixo após o arraste. Foram implementadas:

- Detecção do destino com margem nas bordas e consideração da figura arrastada.
- Fila de ações, usando a revisão atual do servidor em cada envio.
- Preservação do objeto segurado durante a chegada de uma resposta; a interface aguarda o encerramento do gesto antes de substituir o cenário.
- Ocultação apenas dos itens com destino pendente, mantendo os demais disponíveis.
- Supressão somente do clique sintético do arraste anterior, liberando um novo gesto real imediatamente.
- Tratamento de cancelamento, perda de captura, troca de janela e falha de conexão.
- Recuperação explícita da atividade quando não é possível confirmar o salvamento.

Os objetivos, as recompensas, o desbloqueio do mapa e o segundo desafio foram preservados. Não houve alteração de esquema ou migração.

## Validação executada

**173 verificações aprovadas** na execução final: [resultado.json](resultado.json).

Foram executados no Chrome:

- Virada visível antes da resposta em rede lenta, com captura durante o movimento.
- Preservação do mesmo botão no DOM após salvar o estudo.
- Cliques rápidos em cartas diferentes, com confirmação do estado final persistido.
- Redução de movimento e ativação explícita das animações pelo usuário.
- Arrastes consecutivos com mouse enquanto o salvamento anterior permanece pendente.
- Soltura dez pixels além da borda da lixeira, aceita pela margem de interação.
- Soltura fora do destino, Escape e destino incompatível enviado ao servidor.
- Toque emulado nas missões da ave e da sinalização, alternativa pelo teclado e formulário sem JavaScript.
- Interrupção de conexão, restauração do objeto, recuperação da atividade e nova coleta concluída.
- Persistência, aprovações, recompensas, bloqueio e liberação do mapa e histórico anterior.

Desktop, tablet e celular foram conferidos em 1440, 768 e 390 pixels, nos temas claro e escuro. Capturas reais estão em [capturas](capturas). A captura durante a virada e as fichas foram inspecionadas visualmente. O toque foi emulado no navegador; não foi usado aparelho físico nesta revisão.

Restore e builds da aplicação e do verificador concluíram com **zero avisos e zero erros**. Os testes usaram servidor temporário na porta 5057 e uma cópia SQLite; o servidor foi encerrado ao final. Nenhuma instância foi deixada na porta 5046.

## Preservação e arquivos

Backup anterior às mudanças: `backups/antes-jogos-20261115-010107`, com SQLite consistente e arquivos de código. A comparação das 24 tabelas, excluindo apenas o lock operacional de migração, confirmou os registros originais preservados: [preservacao-banco.json](preservacao-banco.json).

Principais arquivos alterados em `TCC_Grupo12_III/`:

- `wwwroot/js/jornada-jogos.js`
- `wwwroot/css/jornada-jogos.css`
- `Views/Jornada/_Flashcards.cshtml`
- `Views/Jornada/Cartas.cshtml`
- `Views/Jornada/_EstadoJogo.cshtml`
- `Controllers/JornadaController.cs`
- `Services/JornadaJogosService.cs`

O verificador foi ampliado em `tests/EcoCanta.Verificacao/JogosEducativos.cs` e `Program.cs`. O conteúdo científico e os documentos acadêmicos não foram reescritos nesta correção.

## Como carregar a correção

No terminal que executa a aplicação, encerre a instância anterior com Ctrl+C e, dentro de `TCC_Grupo12_III`, execute:

```powershell
dotnet run --launch-profile http
```

No navegador, pressione Ctrl+F5. Se o Detetive indicar “Animações: reduzidas”, ative o botão para mostrar “Animações: ligadas”.

Para repetir a validação, na raiz do workspace:

```powershell
dotnet run --project tests/EcoCanta.Verificacao/EcoCanta.Verificacao.csproj -- . --confiabilidade
```

Não houve publicação nem envio ao GitHub. As evidências de falhas intermediárias foram preservadas separadamente; o resultado final aprovado está em `resultado.json`.
