# EcoCanta — animações e arraste, 14/11/2026

## Alterações solicitadas

O Detetive mantém suas cartas, associações e critérios de conclusão. Agora a frente e o verso fazem uma virada em 3D após a ação ser salva. A seleção de uma fotografia tem uma resposta visual discreta; um par correto recebe destaque e uma linha animada liga as cartas. Uma associação incorreta produz um movimento breve de revisão. A preferência `prefers-reduced-motion` desativa essas animações.

A Missão de Preservação foi reorganizada em três momentos guiados: limpar a trilha, ajudar a ave e proteger o caminho. O cenário e os desenhos foram refeitos com ilustrações maiores, nomes explícitos e indicação de destino. A interface apresenta somente o item necessário para a etapa atual, em lugar da antiga escolha entre quatro ferramentas.

- Os resíduos podem ser arrastados diretamente para a lixeira, com mouse ou toque.
- Nas etapas seguintes, binóculos, sinalização, rádio e placa são arrastados para o ponto indicado.
- O objeto acompanha o gesto; o destino compatível recebe destaque. Ao soltar corretamente, o item se desloca para o destino e o cenário é atualizado.
- Soltar fora do destino devolve o item sem pontuar. Escape cancela o arraste.
- Clique ou teclado permitem selecionar o objeto e depois ativar o destino. O fluxo de formulários também continua disponível sem JavaScript.
- O painel de itens foi dimensionado para preservar a identificação da ave no celular.

O segundo desafio não recebeu alterações de conteúdo, regras ou interface.

## Integração

As ações de arraste são verificadas no servidor: combinações incompatíveis são rejeitadas, a sequência de atendimento continua validada e os objetivos não são duplicados. Pontos, conquistas e desbloqueio do mapa continuam integrados à jornada existente. A alteração não adiciona tabelas nem migrações.

O estado anterior de coleta por clique continua sendo reconhecido, inclusive quando existe um resíduo na bolsa. Os registros de tentativas e as aprovações anteriores permanecem válidos.

Uma cópia consistente do banco e dos arquivos centrais foi criada em `backups/antes-jogos-20261114-192152`. A comparação posterior confirmou igualdade dos registros e hashes das **24 tabelas**, excluindo apenas o lock operacional de migração. Evidência: [preservacao-banco.json](preservacao-banco.json).

## Validação

- Restore e compilação da aplicação e do projeto de verificação: concluídos, com zero avisos e zero erros.
- **154 verificações aprovadas** na execução final, usando cópia isolada do SQLite. Evidência: [resultado.json](resultado.json).
- Animação de virada e conexão acionadas pela API nativa do navegador; movimento reduzido sem animações.
- Arraste real de mouse e toque emulado pelo protocolo do Chrome: resíduo para lixeira, binóculos, sinalização, rádio e placa.
- Destino incorreto, cancelamento por Escape, destino incompatível enviado ao servidor, alternativa por teclado e ação com JavaScript desativado.
- Associações corretas e incorretas, quiz reprovado e aprovado, persistência, retomada, recompensas e bloqueio/liberação do mapa.
- Chrome em 1440, 768 e 390 pixels, com modos claro e escuro. Verificados transbordamento horizontal, imagens, erros JavaScript e tamanho dos alvos no celular.
- **19 capturas reais** em [capturas](capturas), incluindo gestos de arraste em andamento. As versões de desktop e celular e o painel da ave foram inspecionados visualmente.
- Servidor do teste encerrado após a execução; a instância local do usuário foi atualizada em `http://localhost:5046`. Jornada, CSS e JavaScript responderam HTTP 200. Evidência: [instancia-local.json](instancia-local.json).

O toque foi testado por emulação no Chrome; não houve teste em aparelho físico ou com leitores de tela nesta revisão. A execução técnica não substitui a avaliação pedagógica planejada com as professoras.

## Arquivos novos ou modificados

Caminhos relativos a `TCC_Grupo12_III/`:

- `Controllers/JornadaController.cs`
- `Services/JornadaJogosService.cs`
- `Models/JornadaJogos.cs`
- `Views/Jornada/Missao.cshtml`
- `Views/Jornada/_CenarioMissao.cshtml`
- `Views/Jornada/_IconeMissao.cshtml`
- `Views/Jornada/_EstadoJogo.cshtml`
- `wwwroot/css/jornada-jogos.css`
- `wwwroot/js/jornada-jogos.js`
- `wwwroot/img/jogos/cantareira-cenario.svg`

Verificação e registro: `tests/EcoCanta.Verificacao/JogosEducativos.cs`, `BackupJogos.cs`, `Program.cs`, `.gitignore`, guia de execução e relatórios. Os DOCX da etapa anterior foram preservados; esta pasta registra as telas e interações da revisão atual para a próxima consolidação documental.

## Como experimentar

Abra `http://localhost:5046/Jornada`, atualize com Ctrl+F5 e selecione “Praticar novamente” caso já tenha concluído o desafio. No Detetive, vire uma carta ou associe um par. Na Missão, segure um resíduo e arraste até a lixeira; siga a orientação apresentada em cada etapa seguinte.

Para iniciar a aplicação, na pasta `TCC_Grupo12_III`:

```powershell
dotnet restore
dotnet build
dotnet run --launch-profile http
```

Para repetir esta validação, na raiz:

```powershell
dotnet run --project tests/EcoCanta.Verificacao/EcoCanta.Verificacao.csproj -- . --interacoes
```

As falhas intermediárias foram preservadas separadamente. A evidência aprovada é `resultado.json`, com 154 verificações e zero falhas. Não houve publicação nem envio ao GitHub.
