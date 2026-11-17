# EcoCanta — destaque da Cantareira no mapa, 15/11/2026

**Alteração desfeita em 15/11/2026 a pedido do usuário.** O contorno e o preenchimento foram retirados dos dois mapas. As Views foram restauradas ao conteúdo anterior e o script foi arquivado em `backups/retirada-destaque-*`. O conteúdo e as capturas abaixo são registros históricos, não a apresentação atual do sistema.

Foi preservado um arquivo de compatibilidade sem desenho para as Views compiladas anteriores à retirada. A causa e a validação do mapa em branco estão no [relatório da correção](../mapa-correcao-20261115/RELATORIO.md).

Foi acrescentado um contorno verde discreto com preenchimento transparente nos mapas existentes de consulta e registro de avistamentos. Não foram acrescentados botões, pop-ups, legendas ou seções.

O destaque utiliza o recorte **aproximado já existente** em `LimitesSerraCantareira.Contorno`, sem alterar suas coordenadas. Esse recorte é uma referência do projeto, não um limite oficial de todo o relevo ou de uma unidade de conservação. A [Fundação Florestal disponibiliza limites oficiais de unidades de conservação](https://servicos.sp.gov.br/fcarta/6C3B93ED-AD25-45C1-93CF-50994AEC464B); esses limites representam outro tipo de delimitação e não foram apresentados como se fossem o contorno da serra.

## Implementação

- Traço verde `#2B8171`, espessura de 2,5 pixels e preenchimento com opacidade de 10%.
- Camada posicionada acima do fundo cartográfico e abaixo dos registros de animais.
- Contorno sem interação, preservando cliques, navegação e seleção de coordenadas.
- Enquadramento inicial ajustado para mostrar toda a área, sem botão adicional ou movimento de aproximação.
- Mesma referência nos mapas de consulta e de registro.
- Descrição acessível que identifica o contorno como aproximado.

As permissões, a jornada, a proteção das localizações e a validação territorial dos registros permaneceram como estavam. A ampliação dos registros para todo o Brasil não fez parte desta autorização, que tratou apenas do destaque visual.

## Arquivos

- `TCC_Grupo12_III/wwwroot/js/cantareira-mapa.js`: desenho compartilhado da camada.
- `TCC_Grupo12_III/Views/Avistamentos/Index.cshtml`: integração na consulta.
- `TCC_Grupo12_III/Views/Avistamentos/Registrar.cshtml`: integração no registro.
- `TCC_Grupo12_III/Models/LimitesSerraCantareira.cs`: comentário de documentação corrigido para explicitar a natureza aproximada; coordenadas e validação não foram modificadas.

Backup consistente e cópias dos arquivos anteriores em `backups/antes-jogos-20261115-015801`. Não houve migração nem edição do banco original.

## Conferência executada

Restore e compilação da aplicação concluídos, com zero avisos e zero erros. A conferência de navegador utilizou Chrome e uma cópia isolada do SQLite. Os avistamentos foram ocultados somente nessa cópia, para que as capturas não revelassem posições precisas de registros originais.

Foram verificadas quatro apresentações: consulta em desktop, consulta em celular com tema escuro, registro em desktop e registro em celular. Em todas houve uma camada de contorno, fundo cartográfico carregado, ausência de erros JavaScript e ausência de transbordamento horizontal.

O filtro de espécies foi utilizado sem remover o contorno. Nos dois tamanhos da página de registro, um clique dentro da área desenhada preencheu as coordenadas e colocou o marcador normalmente. As capturas da consulta foram inspecionadas visualmente.

Evidências: [conferencia.json](conferencia.json), [mapa desktop](mapa-desktop.png), [mapa no celular](mapa-celular-escuro.png), [registro desktop](registro-desktop.png) e [registro no celular](registro-celular.png).

O servidor de conferência usou a porta 5057 e foi encerrado. Não foi iniciada outra instância na porta 5046.

Para carregar as novas Razor Views no servidor utilizado pelo grupo, encerre a execução anterior com Ctrl+C, execute novamente `dotnet run --launch-profile http` na pasta `TCC_Grupo12_III` e atualize o navegador com Ctrl+F5.
