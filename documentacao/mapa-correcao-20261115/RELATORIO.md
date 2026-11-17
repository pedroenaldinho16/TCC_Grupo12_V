# Correção do mapa em branco — 15/11/2026

A versão em execução ainda utilizava as Views compiladas com a chamada a `EcoCantaMapa.destacarCantareira`. A retirada do arquivo JavaScript provocava `ReferenceError: EcoCantaMapa is not defined` antes da criação da camada de imagens do mapa. A falha foi reproduzida em Chrome headless, com a versão compilada anterior e banco descartável: nenhum tile era criado, e a seleção de coordenadas também ficava interrompida.

Foi mantido `wwwroot/js/cantareira-mapa.js` como compatibilidade: a função antiga existe, mas não executa nenhuma ação. Assim, as Views antigas voltam a funcionar sem desenhar contornos, preenchimentos ou alterar o enquadramento. As Views atuais continuam sem referências a esse arquivo. O arquivo que desenhava o destaque permanece arquivado em `backups/retirada-destaque-20261115-021253`.

Conferências executadas no navegador com banco descartável, em porta separada:

- Versão recompilada: consulta e registro, desktop e celular; imagens do mapa carregadas, nenhum erro JavaScript e nenhum contorno; seleção de coordenadas por clique funcionando nas duas larguras. Evidências em `conferencia.json` e nas quatro capturas desta pasta.
- Versão compilada anterior com o arquivo de compatibilidade: as mesmas quatro telas carregaram imagens do mapa sem erros JavaScript nem contorno; seleção de coordenadas funcionando. Evidências em `compatibilidade/conferencia.json` e nas quatro capturas dessa pasta.
- O servidor em uso na porta 5046 respondeu HTTP 200 ao arquivo corrigido, contendo a função de compatibilidade e sem código para desenhar polígonos.

Nenhuma alteração no banco original, nos requisitos de desbloqueio ou nas permissões do mapa. Os processos de servidor e navegador criados pela conferência foram encerrados; o servidor do usuário foi preservado.

Para carregar o arquivo corrigido na página já aberta, utilizar Ctrl + F5. O próximo `dotnet run --launch-profile http` recompila as Views atuais, que já não chamam o script retirado.
