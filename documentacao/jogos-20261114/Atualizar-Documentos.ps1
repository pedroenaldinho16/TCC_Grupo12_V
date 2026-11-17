$ErrorActionPreference='Stop'
$docsRoot=Split-Path -Parent $PSScriptRoot
$source=Join-Path $docsRoot 'conteudo-meta2.json'
$archive=Join-Path $docsRoot 'versoes-documentos'
[IO.Directory]::CreateDirectory($archive)|Out-Null
$backup=Join-Path $archive '20261114-antes-jogos-conteudo-meta2.json'
if(!(Test-Path -LiteralPath $backup)){Copy-Item -LiteralPath $source -Destination $backup}
$docs=Get-Content -LiteralPath $source -Raw -Encoding UTF8|ConvertFrom-Json
if($docs.PSObject.Properties.Name -contains 'value'){$docs=$docs.value}
function SetSection($doc,[string]$heading,[string]$text){
    $found=$false
    for($i=0;$i -lt $doc.blocks.Count-1;$i++){
        if($doc.blocks[$i].kind -eq 'p' -and $doc.blocks[$i].text -eq $heading){$doc.blocks[$i+1].text=$text;$found=$true;break}
    }
    if(!$found){throw "Subseção ausente: $heading"}
}
SetSection $docs[0] '4.2.2.4 Jornada de aprendizagem' 'A jornada apresenta a sequência Detetive da Fauna → Guardião da Cantareira → Missão de Preservação → Mapa. Cada etapa informa seu propósito e a recompensa correspondente. O estudante explora as cartas e completa as associações no Detetive, alcança pelo menos 70% no quiz do Guardião e cumpre todos os objetivos do cenário da Missão. O sistema permite continuar uma tentativa em andamento e praticar novamente; os pontos de experiência não substituem a conclusão dos desafios.'
SetSection $docs[0] '4.2.2.5 Detetive da Fauna' 'O primeiro desafio utiliza flashcards com fotografias e informações das fichas para estimular o reconhecimento das espécies. O estudante vira cada carta para consultar habitat e alimentação e, quando disponível no acervo, importância ecológica. Após explorar as cartas, associa as fotografias aos nomes populares e científicos. Uma associação incorreta apresenta uma pista e permite nova tentativa. As cartas podem ser consultadas durante a atividade. O estudo e as associações são registrados no servidor, e a conclusão de todos os pares permite conquistar o avatar de Guardião da Cantareira. As cartas utilizam os registros existentes, preservando as pendências de revisão científica indicadas nas fichas.'
SetSection $docs[0] '4.2.2.6 Guardião da Cantareira' 'O segundo desafio mantém o formato de quiz e apresenta situações-problema relativas à observação da fauna, descarte de resíduos, tráfico de animais e alterações dos habitats. As alternativas exigem escolhas e são acompanhadas de explicações sobre suas consequências. A atividade busca aproximar o conhecimento sobre ameaças de atitudes ambientalmente responsáveis. O aproveitamento mínimo de 70% é calculado no servidor. Com a conclusão, o estudante recebe o escudo que acompanha seu avatar.'
SetSection $docs[0] '4.2.2.7 Missão de Preservação' 'O terceiro desafio apresenta um cenário ilustrado e interativo de uma visita educativa à Cantareira, sem utilizar perguntas de quiz. O estudante seleciona uma ferramenta e interage com os elementos da cena. As três missões consistem em recolher e destinar três resíduos, observar e sinalizar uma ave presa e chamar a equipe de proteção, além de investigar um atalho e orientar o percurso autorizado com a equipe do jogo. O cenário muda conforme as ações são realizadas. A ave é atendida por profissionais na narrativa; a atividade não ensina o visitante a fazer uma soltura por conta própria. Os oito objetivos e o diário de ações ficam registrados. A conclusão de todas as missões libera o mapa, desde que os desafios anteriores também estejam concluídos. O cenário é fictício e as orientações educativas apresentam referências oficiais do ICMBio e do Ibama.'
SetSection $docs[0] '4.2.2.8 Progresso e conquistas' 'A tela de progresso apresenta a quantidade de desafios concluídos, as tentativas iniciadas, o avatar, o escudo conquistado e a situação do mapa. O histórico informa o início de cada tentativa, os objetivos ou acertos, o aproveitamento e a situação. As tentativas concluídas permitem revisão, enquanto as tentativas em andamento podem ser retomadas. As aprovações anteriores continuam válidas. Quando uma tentativa de quiz das etapas reformuladas ainda está em andamento, a interface solicita a confirmação para encerrar essa versão e iniciar o novo jogo, mantendo as questões e respostas anteriores no histórico. Esses registros permitem ao estudante acompanhar sua evolução e identificar os conteúdos que precisa estudar novamente.'
SetSection $docs[0] '4.2.2.9 Mapa de avistamentos' 'O mapa permite consultar registros aprovados de fauna por região e filtrar por espécie. Sua entrada permanece no menu principal e na página inicial. Antes da liberação, o endereço /Mapa apresenta os requisitos, a quantidade de desafios concluídos e o próximo passo. O acesso aos avistamentos é validado no servidor. Para estudantes, o sistema apresenta regiões aproximadas e preserva a posição precisa para a administração. As fotografias e os vídeos dos novos registros são evidências restritas à equipe administrativa. O fundo cartográfico utiliza OpenStreetMap e depende de conexão com a internet.'
SetSection $docs[0] '4.2.3.3 Gerenciamento de questões e desafios' 'A administração distingue os três formatos de atividade. As cartas do Detetive utilizam os dados e fotografias mantidos no gerenciamento de espécies; a Missão possui um cenário com objetivos ambientais próprios; e o editor de questões atende ao quiz do Guardião. O administrador pode cadastrar e revisar enunciados, alternativas, resposta esperada, explicação educativa, referência e imagem local do quiz. Uma questão pode ser desativada para novas tentativas. Os snapshots preservam o conteúdo de tentativas já iniciadas. As questões das versões anteriores do Detetive e da Missão permanecem disponíveis em um acervo de consulta e não entram nos novos jogos. Os quizzes específicos das espécies continuam acessíveis em ferramenta administrativa própria.'
SetSection $docs[0] '4.2.3.3.1 Editor de questão' 'O formulário identifica o quiz do Guardião da Cantareira e solicita o enunciado, as quatro alternativas, a resposta esperada, a explicação e a fonte. A imagem é opcional e deve utilizar um arquivo local do projeto. O controle de disponibilidade permite retirar uma questão de novas tentativas enquanto ela passa por revisão. O formulário não cria questões para os jogos de cartas ou exploração.'
foreach($block in $docs[0].blocks){
    if($block.kind -ne 'image'){continue}
    if($block.caption -like 'Detetive da Fauna*'){$block.file='jogos-20261114/capturas/01-flashcards-desktop-claro.png';$block.caption='Detetive da Fauna · Flashcards'}
    if($block.caption -eq 'Missão de Preservação'){$block.file='jogos-20261114/capturas/08-missao-desktop-claro.png'}
}
function AddImageAfter($doc,[string]$caption,[string]$file,[string]$newCaption){
    if(@($doc.blocks|Where-Object {$_.kind -eq 'image' -and $_.file -eq $file}).Count){return}
    $list=[Collections.Generic.List[object]]::new();$list.AddRange([object[]]$doc.blocks)
    for($i=0;$i -lt $list.Count;$i++){if($list[$i].kind -eq 'image' -and $list[$i].caption -eq $caption){$list.Insert($i+1,[pscustomobject]@{kind='image';file=$file;caption=$newCaption});$doc.blocks=$list.ToArray();return}}
    throw "Legenda ausente: $caption"
}
AddImageAfter $docs[0] 'Detetive da Fauna · Flashcards' 'jogos-20261114/capturas/02-flashcard-verso.png' 'Verso de uma carta educativa'
AddImageAfter $docs[0] 'Verso de uma carta educativa' 'jogos-20261114/capturas/05-associacao-desktop-escuro.png' 'Associação entre fotografias e nomes das espécies'
AddImageAfter $docs[0] 'Missão de Preservação' 'jogos-20261114/capturas/13-missao-concluida.png' 'Cenário após a conclusão das missões'

if(!@($docs[1].blocks|Where-Object {$_.kind -eq 'p' -and $_.text -eq 'Modo escuro e jogos educativos'}).Count){
    $list=[Collections.Generic.List[object]]::new();$list.AddRange([object[]]$docs[1].blocks)
    $index=0;while($index -lt $list.Count -and $list[$index].text -ne '10 Justificativa das escolhas visuais'){$index++}
    $extra=@(
        [pscustomobject]@{kind='p';style='Heading2';text='Modo escuro e jogos educativos'},
        [pscustomobject]@{kind='p';style='Normal';text='O modo escuro utiliza fundos verdes profundos e textos claros, mantendo o logotipo original, a tipografia e as fotografias sem filtros de cor. O botão Claro/Escuro permanece no cabeçalho, inclusive no celular. O primeiro acesso acompanha a preferência do sistema; uma escolha explícita é salva no navegador. Os estilos compartilhados de aparência complementam a paleta original, sem substituí-la.'},
        [pscustomobject]@{kind='p';style='Normal';text='Os flashcards combinam fotografias reais do acervo e um verso textual. No jogo de exploração, a ilustração vetorial original organiza os pontos de interação em um cenário fictício; ferramentas e missões recebem rótulos. Ações por clique, toque e teclado evitam depender de arrastar objetos. As fotografias do catálogo continuam sendo utilizadas para o estudo das espécies; a ave ilustrada da missão é simbólica e não apresenta uma identificação científica presumida.'},
        [pscustomobject]@{kind='image';file='jogos-20261114/capturas/10-missao-celular-escuro.png';caption='Missão de Preservação no celular e em modo escuro'},
        [pscustomobject]@{kind='image';file='jogos-20261114/capturas/02-flashcard-verso.png';caption='Padrão visual do verso do flashcard'}
    )
    $list.InsertRange($index,[object[]]$extra);$docs[1].blocks=$list.ToArray()
}
foreach($block in $docs[1].blocks){if($block.kind -eq 'p' -and $block.text -like 'Os estilos da reformulação estão centralizados*'){$block.text='Os estilos da reformulação estão centralizados em wwwroot/css/meta2.css, complementados por appearance.css para os temas e jornada-jogos.css para as atividades. Cabeçalho, rodapé, mensagens e navegação administrativa são compartilhados pelas Razor Views. Os comportamentos de menu e interação ficam em arquivos JavaScript reutilizáveis, preservando a tecnologia ASP.NET Core MVC do projeto.'}}
$json=$docs|ConvertTo-Json -Depth 15
[IO.File]::WriteAllText($source,$json,[Text.UTF8Encoding]::new($false))
& (Join-Path $docsRoot 'Gerar-Documentos.ps1')
