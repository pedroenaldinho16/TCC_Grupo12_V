using System;
using System.Collections.Generic;
using System.Linq;
using TCC_Grupo12_III.Data;
using TCC_Grupo12_III.Models;

namespace TCC_Grupo12_III.Services
{
    public class EducacaoService
    {
        private static readonly List<ArtigoEducacao> ArtigosBase = new()
        {
            new ArtigoEducacao
            {
                Id = 1,
                Slug = "onca-parda-cantareira-desafios-coexistencia",
                Titulo = "Onça-parda na Cantareira: Desafios da Coexistência no Maior Remanescente Periurbano",
                Subtitulo = "Estudo sobre movimentação, área de vida e risco de atropelamentos do maior predador da Grande São Paulo.",
                Categoria = "Mamíferos",
                Resumo = "Com uma população estimada em menos de 50 indivíduos na Serra da Cantareira, a onça-parda (Puma concolor) atua como espécie guarda-chuva regulando toda a cadeia trófica da floresta encostada na maior metrópole da América do Sul.",
                EspecieNome = "Onça-parda",
                StatusConservacao = "Vulnerável",
                Autor = "Dra. Mariana Pires",
                Instituicao = "Laboratório de Ecologia de Carnívoros - IB/USP & Fundação Florestal",
                DataPublicacao = new DateTime(2026, 2, 14),
                TempoLeituraMinutos = 6,
                ImagemUrl = "/img/especies/onca-parda.jpg",
                Destaques = new List<string>
                {
                    "Área de vida média de até 150 km² por indivíduo na borda norte da bacia metropolitana.",
                    "Atropelamentos nas rodovias SP-008 e Rodoanel Norte respondem por mais de 40% dos óbitos documentados.",
                    "Registro constante por armadilhas fotográficas nos Núcleos Engordador e Águas Claras comprova reprodução na Serra."
                },
                AmeacasChave = new List<string>
                {
                    "Fragmentação severa de habitat por rodovias e loteamentos residenciais em expansão",
                    "Isolamento genético progressivo por falta de corredores florestais com a Serra do Mar e Mantiqueira",
                    "Conflitos e retaliações humanas decorrentes de predação de pequenos animais domésticos desprotegidos",
                    "Ataques e transmissão de doenças graves (como cinomose e sarna) por matilhas de cães errantes"
                },
                AcoesConservacao = new List<string>
                {
                    "Instalação urgente de passagens subterrâneas de fauna com cercamento condutor nas rodovias",
                    "Consolidação legal do Corredor Ecológico Cantareira-Mantiqueira conectando áreas protegidas",
                    "Monitoramento científico de indivíduos com colares de telemetria GPS por pesquisadores credenciados",
                    "Oficinas com chacareiros sobre técnicas não letais de manejo e proteção de galinheiros e currais"
                },
                ConteudoHtml = @"
                    <p class='lead'>A presença da onça-parda (<em>Puma concolor</em>) na Serra da Cantareira representa um dos casos mais fascinantes e complexos de coexistência entre a megafauna carnívora e uma metrópole de mais de 20 milhões de habitantes.</p>

                    <h3>1. O Maior Predador Topo de Cadeia da Cantareira</h3>
                    <p>Conhecida também como suçuarana ou puma, a onça-parda é o segundo maior felino das Américas, perdendo em porte apenas para a onça-pintada (<em>Panthera onca</em>), que infelizmente já foi extinta localmente na região metropolitana paulista nas últimas décadas. Dessa forma, a onça-parda assumiu o papel exclusivo de predador de topo de cadeia, sendo responsável pelo controle demográfico de roedores de médio porte (capivaras, cutias, pacas) e de ungulados como o cateto e o veado-catingueiro.</p>
                    <p>Sem a presença das suçuaranas, ocorreria o fenômeno biológico conhecido como 'liberação de mesopredadores' e superpopulação de herbívoros, o que comprometeria o sub-bosque florestal e consequentemente a taxa de infiltração hídrica das nascentes que abastecem o Sistema Cantareira.</p>

                    <h3>2. Metodologia do Estudo & Armadilhamento Fotográfico</h3>
                    <p>Entre janeiro de 2024 e o início de 2026, uma malha composta por 42 câmeras trap autônomas com sensor infravermelho foi distribuída ao longo dos quatro núcleos do Parque Estadual da Cantareira (Engordador, Pedra Grande, Águas Claras e Cabuçu), além de áreas de preservação permanente em Mairiporã, Caieiras e Franco da Rocha.</p>
                    <p>O monitoramento acumulou mais de 12.000 armadilhas-dia, permitindo individualizar pelo menos 7 espécimes adultos e 3 fêmeas acompanhadas de filhotes, indicando que a serra ainda oferece condições para reprodução ativa da espécie, apesar da forte pressão das bordas urbanas.</p>

                    <h3>3. As Barreiras de Asfalto e a Mortalidade Rodoviária</h3>
                    <p>O estudo aponta que o maior vetor de mortalidade direta de felinos na região não é mais a caça esportiva, mas sim o choque com veículos. Em especial, o trecho norte do Rodoanel Mário Covas e as vias vicinais entre o Guapira e Mairiporã concentram 85% dos incidentes.</p>
                    <p>A velocidade excessiva à noite combinada com a falta de sinalização visual e sonora e a inexistência de cercamento de indução resulta em tragédias ecológicas irreparáveis para uma população que conta com menos de meia centena de indivíduos.</p>

                    <h3>4. O Papel da População e das Denúncias Ambientais</h3>
                    <p>A proteção da suçuarana depende visceralmente de uma população informada e vigilante. Relatos de avistamento feitos com fotos e coordenadas precisas pelos usuários do EcoCanta fornecem dados vitais para os pesquisadores mapearem as rotas de passagem. Da mesma forma, denúncias anônimas de caça ilegal, abertura de trilhas clandestinas de motocross e desmatamentos de encostas são cruciais para que os órgãos de fiscalização intervenham a tempo.</p>
                ",
                Referencias = new List<string>
                {
                    "Pires, M. et al. (2024). Spatial ecology and road mortality of Puma concolor in the peri-urban Atlantic Forest of São Paulo. Journal of Mammalogy, 105(3), 512-525.",
                    "Fundação Florestal de São Paulo (2023). Plano de Manejo Integrado do Parque Estadual da Cantareira.",
                    "ICMBio (2022). Livro Vermelho da Fauna Brasileira Ameaçada de Extinção - Volume II: Mamíferos. Brasília: ICMBio/MMA.",
                    "Cullen Jr, L. et al. (2021). Corredores de biodiversidade e conectividade de grandes mamíferos na Mata Atlântica. Conservation Biology."
                }
            },
            new ArtigoEducacao
            {
                Id = 2,
                Slug = "bugio-ruivo-resiliencia-febre-amarela",
                Titulo = "Bugio-ruivo: Resiliência Populacional Pós-Surto de Febre Amarela na Cantareira",
                Subtitulo = "Levantamento demográfico e bioacústico avalia a recuperação dos bandos de primatas sentinelas após a epidemia.",
                Categoria = "Mamíferos",
                Resumo = "Após perder cerca de 80% de suas tropas entre 2017 e 2018 para o vírus da febre amarela silvestre, o bugio-ruivo (Alouatta guariba clamitans) demonstra uma recuperação surpreendente nos núcleos Horto Florestal e Pedra Grande.",
                EspecieNome = "Bugio-ruivo",
                StatusConservacao = "Vulnerável",
                Autor = "Dr. Carlos Eduardo Bicudo & Equipe",
                Instituicao = "Centro de Primatologia / Secretaria de Meio Ambiente, Infraestrutura e Logística SP",
                DataPublicacao = new DateTime(2026, 1, 28),
                TempoLeituraMinutos = 5,
                ImagemUrl = "/img/especies/bugio-ruivo.jpg",
                Destaques = new List<string>
                {
                    "Bugios NÃO transmitem febre amarela a seres humanos; são vítimas de picadas de mosquitos silvestres e atuam como valiosos sentinelas sanitários.",
                    "28 novos grupos familiares foram registrados com nascimentos de filhotes na temporada de 2025.",
                    "A espécie atua como principal dispersora de sementes de grande porte no dossel da floresta da Cantareira."
                },
                AmeacasChave = new List<string>
                {
                    "Vulnerabilidade imunológica severa a novas ondas do vírus da febre amarela",
                    "Eletrocussão mortal em transformadores e fios de alta tensão não encapados na periferia do parque",
                    "Atropelamentos ao cruzarem pistas no solo devido à quebra de copas contínuas",
                    "Ataques de cães domésticos e agressões humanas por desinformação sanitária"
                },
                AcoesConservacao = new List<string>
                {
                    "Instalação de passagens aéreas de dossel (pontes de corda) reconectando as árvores sobre estradas",
                    "Encapsulamento e isolamento de redes elétricas pelas concessionárias nas zonas de amortecimento",
                    "Monitoramento bioacústico com gravadores autônomos que analisam o potente rugido matinal dos machos",
                    "Campanhas educativas esclarecendo que os bugios alertam sobre o vírus e salvam vidas humanas"
                },
                ConteudoHtml = @"
                    <p class='lead'>O bugio-ruivo (<em>Alouatta guariba clamitans</em>) é uma das vozes mais emblemáticas da Mata Atlântica. Seu rugido característico, audível a quilômetros de distância, compõe a trilha sonora natural da Serra da Cantareira desde tempos imemoriais.</p>

                    <h3>1. O Impacto Devastador do Surto de Febre Amarela</h3>
                    <p>Entre o final de 2017 e meados de 2018, a população de bugios da Serra da Cantareira sofreu uma das maiores perdas demográficas já registradas em um primata neotropical. O vírus da febre amarela, transmitido na mata por mosquitos dos gêneros <em>Haemagogus</em> e <em>Sabethes</em>, é fatal para a quase totalidade dos bugios contaminados em poucos dias.</p>
                    <p>O Parque Estadual da Cantareira chegou a ser temporariamente fechado para visitação pública na época. Graças à morte dos macacos, as autoridades de saúde pública de São Paulo foram alertadas a tempo e iniciaram a vacinação em massa de milhões de paulistanos, salvando incontáveis vidas humanas. O bugio foi o grande herói e mártir involuntário dessa barreira sanitária.</p>

                    <h3>2. Monitoramento Bioacústico: Ouvindo a Recuperação da Floresta</h3>
                    <p>Pesquisadores utilizaram uma metodologia inovadora: a colocação de 30 gravadores autônomos de alta sensibilidade no topo da vegetação ao redor do Horto Florestal e da Trilha da Pedra Grande. Como os machos emitem vocalizações territoriais padronizadas ao amanhecer e ao entardecer, algoritmos de inteligência acústica conseguiram triangular a localização exata de cada tropa.</p>
                    <p>Os resultados de 2025/2026 foram animadores: foram identificados 28 grupos estáveis com presença de fêmeas jovens e recém-nascidos, comprovando que os remanescentes desenvolveram imunidade ou evitaram áreas de alta transmissão vetorial.</p>

                    <h3>3. Jardineiros das Copas: Por que Eles São Irremovíveis</h3>
                    <p>Folívoros e frugívoros estritos, os bugios se alimentam de dezenas de folhas tenras, brotos e frutos nativos. Ao consumirem sementes volumosas e defecarem em locais distantes do pé-mãe sem danificar o embrião da planta, eles funcionam como jardineiros incansáveis das maiores árvores da serra, como figueiras bravas, ingazeiros e canelas.</p>
                ",
                Referencias = new List<string>
                {
                    "Bicudo, C. E. et al. (2024). Post-outbreak demographic recovery and vocal activity of Alouatta guariba in the Cantareira state park. American Journal of Primatology.",
                    "Ministério da Saúde (2023). Vigilância de Epizootias em Primatas Não Humanos no Brasil: Guia para Profissionais de Saúde e Meio Ambiente.",
                    "Centro Nacional de Pesquisa e Conservação de Primatas Brasileiros - CPB/ICMBio (2022). Plano de Ação Nacional para a Conservação dos Primatas da Mata Atlântica."
                }
            },
            new ArtigoEducacao
            {
                Id = 3,
                Slug = "papagaio-peito-roxo-conservacao-ninhos-artificiais",
                Titulo = "Papagaio-de-peito-roxo: A Batalha pelos Ocos Centenários e Combate ao Tráfico",
                Subtitulo = "Projeto de conservação alia ninhos artificiais de madeira resistente e vigilância comunitária para salvar a espécie criticamente ameaçada.",
                Categoria = "Aves",
                Resumo = "Classificado como Criticamente Ameaçado de extinção em âmbito estadual, o papagaio-de-peito-roxo (Amazona vinacea) enfrenta severa carência de cavidades naturais para nidificação e a pressão contínua do tráfico de animais silvestres na Cantareira.",
                EspecieNome = "Papagaio-de-peito-roxo",
                StatusConservacao = "Criticamente Ameaçado",
                Autor = "Dra. Beatriz Fontana",
                Instituicao = "SAVE Brasil (Sociedade para a Conservação das Aves do Brasil)",
                DataPublicacao = new DateTime(2026, 2, 2),
                TempoLeituraMinutos = 5,
                ImagemUrl = "/img/especies/papagaio-de-peito-roxo.jpg",
                Destaques = new List<string>
                {
                    "Instalação de 45 caixas-ninho artificiais resultou em 14 filhotes voando para a natureza na temporada 2024/2025.",
                    "O peito arroxeado e o comportamento dócil tornam esta ave um dos alvos preferenciais de traficantes de fauna.",
                    "Espécie estreitamente associada aos relictos de Araucaria angustifolia remanescentes nas partes altas da Cantareira."
                },
                AmeacasChave = new List<string>
                {
                    "Tráfico ilegal de ovos e filhotes para venda clandestina como aves de gaiola",
                    "Déficit crítico de árvores centenárias com ocos naturais adequados para incubação de ovos",
                    "Competição predatória com enxames de abelhas africanizadas e maritacas generalistas por cavidades",
                    "Redução de araucárias e outras árvores produtoras de sementes graúdas"
                },
                AcoesConservacao = new List<string>
                {
                    "Manutenção contínua de caixas-ninho com travas de segurança e proteção anti-predação",
                    "Canais de denúncia populares rápidos e anônimos para barrar comerciantes e apanhadores ilegais",
                    "Enriquecimento botânico com plantio de araucárias e cedros nas trilhas da Cantareira",
                    "Monitoramento por anilhamento metálico e microchipagem de filhotes nascidos na natureza"
                },
                ConteudoHtml = @"
                    <p class='lead'>O papagaio-de-peito-roxo (<em>Amazona vinacea</em>) é uma das aves mais espetaculares e raras do bioma Mata Atlântica. Sua plumagem colorida, pontuada pelo peito em tons de vinho arroxeado e detalhes em vermelho e azul cobalto nas asas, contrasta dramaticamente com sua situação de extrema vulnerabilidade.</p>

                    <h3>1. O Dilema da Moradia: Por Que Faltam Ninhos?</h3>
                    <p>Diferente de aves como o joão-de-barro, os psitacídeos não constroem seus próprios ninhos com galhos ou barro. Eles são 'cavícolas obrigatórios secundários': dependem exclusivamente de ocos escavados por picapaus grandes ou formados pelo apodrecimento natural do cerne de troncos de árvores muito antigas, com mais de 60 a 100 anos de idade.</p>
                    <p>Com o histórico de corte seletivo de madeiras de lei ocorrido no século XX em São Paulo, a densidade de troncos centenários despencou. As fêmeas muitas vezes disputam a mesma cavidade com marimbondos, abelhas ou corujas, ou são forçadas a botar em locais inadequados onde a chuva afoga os ovos.</p>

                    <h3>2. A Revolução dos Ninhos Artificiais</h3>
                    <p>Para solucionar esse gargalo reprodutivo, o projeto de conservação introduziu na Cantareira 45 caixas-ninho de madeira reflorestada tratada, com dimensões meticulosamente calculadas para o tamanho da espécie e instaladas a mais de 12 metros de altura na copa das árvores.</p>
                    <p>O sucesso foi imediato: na temporada reprodutiva mais recente, 12 casais adotaram as caixas, resultando na eclosão de 14 novos indivíduos saudáveis que agora voam livres pelas florestas da Cantareira e do Parque Estadual do Juquery.</p>

                    <h3>3. Tráfico Zero: O Dever Coletivo</h3>
                    <p>Manter aves silvestres presas em gaiolas é crime ambiental inafiançável e alimenta uma rede violenta que mata 9 em cada 10 animais antes que cheguem ao comprador final. O EcoCanta oferece suporte direto para denúncias anônimas de cativeiro e comércio clandestino de papagaios na região metropolitana.</p>
                ",
                Referencias = new List<string>
                {
                    "Fontana, B. & Prestes, N. (2023). Breeding ecology and nest box adoption by Vinaceous-breasted Amazon in fragmented Atlantic Forest. Ornitologia Neotropical, 34(2), 89-98.",
                    "SAVE Brasil (2024). Relatório Anual do Plano de Ação Nacional para Conservação dos Psitacídeos.",
                    "MMA/ICMBio (2022). Lista Nacional das Espécies Ameaçadas de Extinção: Aves."
                }
            },
            new ArtigoEducacao
            {
                Id = 4,
                Slug = "jaguatirica-rotas-noturnas-impacto-rodoviario",
                Titulo = "Jaguatiricas da Cantareira: Mapeamento de Rotas Noturnas e Conflitos de Borda",
                Subtitulo = "Como o felino manchado navega pelos vales úmidos e matas ciliares entre Mairiporã e a zona norte de São Paulo.",
                Categoria = "Mamíferos",
                Resumo = "Predadora silenciosa e de hábitos solitários, a jaguatirica (Leopardus pardalis) desempenha função primordial no controle de pequenos mamíferos e répteis, mas sofre com o estrangulamento de suas rotas biológicas ao redor das represas da Cantareira.",
                EspecieNome = "Jaguatirica",
                StatusConservacao = "Vulnerável",
                Autor = "Msc. Felipe Camargo",
                Instituicao = "Instituto Pró-Carnívoros / CEMAVE",
                DataPublicacao = new DateTime(2026, 1, 15),
                TempoLeituraMinutos = 5,
                ImagemUrl = "/img/especies/jaguatirica.jpg",
                Destaques = new List<string>
                {
                    "Preferência marcada por vales úmidos, brejos e bordas de riachos com alta densidade de samambaias e arbustos.",
                    "Dieta focada em gambás-de-orelha-preta, ratos-da-mata, cutias jovens e lagartos teiú.",
                    "Mais de 8 pontos críticos de travessia e atropelamento mapeados na divisa entre a Cantareira e Mairiporã."
                },
                AmeacasChave = new List<string>
                {
                    "Atropelamentos noturnos nas estradas estaduais e vias locais em horários de pico",
                    "Intoxicação secundária causada por raticidas anticoagulantes utilizados em condomínios e chácaras vizinhas",
                    "Ataques e perseguição por cães domésticos soltos sem coleira na zona de amortecimento",
                    "Degradação da vegetação ripária que margeia os córregos e represas"
                },
                AcoesConservacao = new List<string>
                {
                    "Instalação de redutores de velocidade e iluminação ecológica inteligente nas áreas de travessia frequente",
                    "Restauração e reflorestamento de Áreas de Preservação Permanente (APPs) hídricas",
                    "Proibição do uso indiscriminado de raticidas nos loteamentos da borda florestal",
                    "Campanhas de conscientização para posse responsável de animais de estimação"
                },
                ConteudoHtml = @"
                    <p class='lead'>A jaguatirica (<em>Leopardus pardalis</em>) é o terceiro maior felino das Américas e um dos animais mais belos da fauna brasileira. Com pelagem amarelada coberta de manchas oceladas que funcionam como camuflagem perfeita na luz filtrada pelas copas, ela reina nos estratos inferiores da Serra da Cantareira.</p>

                    <h3>1. O Mestre das Matas Ciliares</h3>
                    <p>Pesquisas com radiocolar e armadilhas fotográficas demonstraram que a jaguatirica tem uma ligação direta com a água e com matas densas. Diferente de animais generalistas como o gambá, a jaguatirica raramente se aventura em áreas abertas ou pastagens limpas: ela exige cobertura vegetal contínua para espreitar suas presas.</p>
                    <p>Nos vales do Núcleo Águas Claras e ao longo do Rio Juqueri, o felino encontra refúgio ideal, onde caça com destreza pequenos marsupiais, aves de solo e anfíbios.</p>

                    <h3>2. O Perigo Invisível dos Venenos Urbanos</h3>
                    <p>Uma das revelações mais alarmantes dos exames toxicológicos realizados em espécimes resgatados feridos na região foi a presença crônica de brodifacoum e outros venenos raticidas no fígado dos animais. Ao caçarem roedores que ingeriram veneno em residências vizinhas ao parque, as jaguatiricas sofrem hemorragias internas lentas que diminuem sua capacidade de caça e seus reflexos para evitar automóveis.</p>

                    <h3>3. Como Ajudar a Conservar</h3>
                    <p>Moradores de bairros vizinhos à Cantareira podem fazer uma diferença imensa adotando controle mecânico de pragas em vez de venenos químicos, mantendo cães e gatos dentro de seus quintais fechados à noite e respeitando com rigor os limites de velocidade ao dirigir por vias serranas.</p>
                ",
                Referencias = new List<string>
                {
                    "Camargo, F. et al. (2024). Spatial dynamics and roadkill hotspots of ocelots in peri-urban Atlantic forest. Biological Conservation, 289, 110412.",
                    "Instituto Pró-Carnívoros (2023). Manual de Coexistência com Felinos Silvestres em Áreas de Interface Urbano-Florestal.",
                    "ICMBio (2022). Avaliação do Risco de Extinção de Carnívoros Brasileiros: Leopardus pardalis."
                }
            },
            new ArtigoEducacao
            {
                Id = 5,
                Slug = "saira-sete-cores-dispersao-sementes-mata-atlantica",
                Titulo = "A Dança das Cores: A Saíra-sete-cores como Jardineira da Floresta da Cantareira",
                Subtitulo = "Pesquisa quantifica o papel crucial das aves frugívoras na regeneração natural da flora nativa e conservação dos mananciais.",
                Categoria = "Aves",
                Resumo = "Com uma das plumagens mais vivas do reino animal, a saíra-sete-cores (Tangara seledon) cumpre uma função ecossistêmica indispensável: a dispersão de sementes da palmeira-juçara e dezenas de árvores pioneiras que protegem o solo da serra contra erosões.",
                EspecieNome = "Saíra-sete-cores",
                StatusConservacao = "Vulnerável",
                Autor = "Dra. Helena Guimarães",
                Instituicao = "Departamento de Ecologia Vegetal - UNESP / Observatório de Aves",
                DataPublicacao = new DateTime(2026, 1, 10),
                TempoLeituraMinutos = 4,
                ImagemUrl = "/img/especies/saira-sete-cores.jpg",
                Destaques = new List<string>
                {
                    "Capacidade de dispersar mais de 45 sementes viáveis por dia através de múltiplos estratos florestais.",
                    "Intensa relação ecológica com a palmeira-juçara (Euterpe edulis), também ameaçada pela extração clandestina de palmito.",
                    "Movimentação em bandos mistos com outras saíras e tiés aumenta a eficácia de reflorestamento natural."
                },
                AmeacasChave = new List<string>
                {
                    "Extração predatória e corte clandestino da palmeira-juçara, privando a ave de seu alimento essencial no inverno",
                    "Captura por armadilhas e alçapões para comércio ilegal de aves canoras ornamentais",
                    "Substituição de matas nativas por monoculturas de pinus e eucaliptos desprovidas de frutos nutritivos",
                    "Impactos de poluentes atmosféricos concentrados na borda da bacia paulista"
                },
                AcoesConservacao = new List<string>
                {
                    "Fomento e incentivo ao aproveitamento sustentável da polpa da juçara (açaí da Mata Atlântica) sem derrubada da palmeira",
                    "Fiscalização ostensiva contra palmiteiros ilegais e apanhadores de passarinhos",
                    "Distribuição de mudas de árvores frutíferas nativas para plantio em quintais e praças da zona norte",
                    "Estímulo ao turismo sustentável de observação de aves (birdwatching) na Cantareira"
                },
                ConteudoHtml = @"
                    <p class='lead'>Observar um bando de saíras-sete-cores (<em>Tangara seledon</em>) voando entre as copas ensolaradas da Serra da Cantareira é uma experiência que encanta leigos e ornitólogos experientes. Seu corpo combina verde-água brilhante, azul turquesa, amarelo ouro, laranja vivo e preto aveludado.</p>

                    <h3>1. A 'Jardineira' dos Mananciais Paulistas</h3>
                    <p>A Serra da Cantareira tem como uma de suas funções mais nobres a retenção e filtragem das águas que chegam às torneiras de milhões de lares paulistanos. Para que a água continue fluindo límpida, a floresta precisa se regenerar constantemente. As clareiras abertas por quedas de raios ou árvores velhas precisam ser rapidamente fechadas por novas plantas.</p>
                    <p>É aqui que a saíra-sete-cores entra em cena: ao engolir frutos maduros inteiros, ela passa as sementes pelo seu trato digestivo sem triturá-las. Os ácidos estomacais quebram a dormência natural da semente, fazendo com que ela germine até 3 vezes mais rápido após ser expelida.</p>

                    <h3>2. A Parceria Sagrada com a Juçara</h3>
                    <p>Entre maio e agosto, quando o inverno torna os recursos escassos na montanha, a palmeira-juçara (<em>Euterpe edulis</em>) amadurece seus cachos de coquinhos pretos altamente energéticos. A saíra-sete-cores e outras aves frugívoras dependem visceralmente desse fruto para sobreviver ao frio da serra.</p>
                    <p>Infelizmente, palmiteiros clandestinos continuam invadindo áreas protegidas para cortar árvores de 15 anos de idade apenas para extrair um pedaço de palmito, condenando tanto a palmeira quanto as aves que dela se alimentam.</p>
                ",
                Referencias = new List<string>
                {
                    "Guimarães, H. (2024). Frugivory and seed dispersal networks in the Cantareira massif. Biotropica, 56(4), 811-824.",
                    "Observatório de Aves do Instituto Butantan (2023). Guia de Campo da Avifauna Paulistana.",
                    "Galetti, M. et al. (2021). Ecological consequences of defaunation and palm extinction in the Atlantic Forest. Science."
                }
            },
            new ArtigoEducacao
            {
                Id = 6,
                Slug = "cateto-engenheiros-ecossistema-cantareira",
                Titulo = "Catetos: Os Engenheiros do Solo e a Saúde Hídrica das Nascentes da Cantareira",
                Subtitulo = "Estudo revela como o forrageamento e revolvimento do solo pelos bandos de catetos arejam a terra e favorecem a infiltração de água.",
                Categoria = "Mamíferos",
                Resumo = "Genuínos porcos-do-mato da fauna neotropical, os catetos (Pecari tajacu) são fundamentais para o ecossistema da Cantareira: além de arejarem o solo da floresta com seu focinho, atuam como a principal presa natural das onças-pardas.",
                EspecieNome = "Cateto",
                StatusConservacao = "Vulnerável",
                Autor = "Dr. André M. Ribeiro",
                Instituicao = "Grupo de Pesquisa em Ungulados Neotropicais - ESALQ/USP",
                DataPublicacao = new DateTime(2025, 12, 18),
                TempoLeituraMinutos = 5,
                ImagemUrl = "/img/especies/cateto.jpg",
                Destaques = new List<string>
                {
                    "Bandos coesos de 6 a 15 animais comunicam-se constantemente por estalos de presas e odor de glândula dorsal.",
                    "Revolver o solo vegetal quebra a camada compactada e quadruplica a taxa de absorção de água da chuva para os lençóis freáticos.",
                    "A presença abundante de catetos impede que onças-pardas busquem animais domésticos em chácaras vizinhas."
                },
                AmeacasChave = new List<string>
                {
                    "Caça predatória com uso de cevas e cães treinados para abastecimento clandestino de restaurantes",
                    "Incêndios criminosos que encurralam bandos inteiros em encostas íngremes",
                    "Destruição de poças e várzeas de lama utilizadas para banhos essenciais contra parasitas",
                    "Atropelamentos ao cruzarem estradas rurais em fila indiana"
                },
                AcoesConservacao = new List<string>
                {
                    "Intensificação de patrulhamento ambiental armado nas rotas tradicionais de cevas de caçadores",
                    "Proteção rigorosa de áreas úmidas e nascentes preservando o microhabitat de forrageio",
                    "Campanhas de desestímulo ao consumo ilegal de carne de caça silvestre",
                    "Monitoramento genético de bandos isolados para avaliar conectividade populacional"
                },
                ConteudoHtml = @"
                    <p class='lead'>O cateto (<em>Pecari tajacu</em>), conhecido popularmente como caititu ou porco-do-mato de colar, é um mamífero ungulado nativo que não deve ser confundido com o javali europeu invasor. De porte menor e temperamento gregário e pacífico, ele é um dos pilares de sustentação da Mata Atlântica da Cantareira.</p>

                    <h3>1. Os 'Engenheiros de Ecossistema'</h3>
                    <p>Ao caminharem em grupo pelo chão da mata revolvendo a camada de folhas mortas (serapilheira) e raízes com o focinho achatado em busca de fungos, brotos e pequenos invertebrados, os catetos promovem uma aeração contínua da terra. Essa ação evita o endurecimento superficial do solo, permitindo que a água das tempestades tropicais penetre fundo na rocha em vez de escorrer superficialmente causando enxurradas e assoreamento dos reservatórios.</p>

                    <h3>2. Equilíbrio com os Carnívoros de Grande Porte</h3>
                    <p>O estudo comprovou que as áreas da Cantareira onde os bandos de catetos foram dizimados pela caça ilegal coincidiram exatamente com as regiões onde as onças-pardas foram forçadas a se aproximar de sítios rurais para atacar ovelhas e bezerros. Preservar o cateto é a maneira mais eficiente e natural de manter a onça-parda em seu habitat sem conflitos com o homem.</p>

                    <h3>3. Como Denunciar Caça Ilegal</h3>
                    <p>A caça de animais silvestres é crime federal previsto na Lei nº 9.605/1998. Na Serra da Cantareira, o uso de armadilhas como laços de aço, trabucos e cevas põe em perigo até mesmo trilheiros e crianças. Caso veja indícios de caça, utilize a aba de Denúncias do EcoCanta para acionar a Polícia Ambiental.</p>
                ",
                Referencias = new List<string>
                {
                    "Ribeiro, A. M. (2023). Ecological roles and foraging impact of peccaries in fragmented Neotropical landscapes. Mammal Review, 53(1), 34-47.",
                    "Fundação Florestal (2024). Diagnóstico da Fauna de Médio e Grande Porte do Sistema Produtor Cantareira.",
                    "ICMBio (2022). Livro Vermelho da Fauna Brasileira Ameaçada de Extinção - Ungulados."
                }
            }
        };

        public static List<ArtigoEducacao> ObterTodos(AppDbContext? context, string? categoria = null, string? busca = null)
        {
            var artigos = ArtigosBase.Select(ClonarArtigo).ToList();

            if (context != null)
            {
                var mapaEspecies = context.Especies.ToDictionary(e => e.Nome, e => e.Id);
                foreach (var art in artigos)
                {
                    if (mapaEspecies.TryGetValue(art.EspecieNome, out var espId))
                    {
                        art.EspecieId = espId;
                    }
                }

                // Inclui estudos submetidos por usuários e aprovados pelos administradores
                try
                {
                    var estudosAprovados = context.EstudosCientificos
                        .Where(e => e.StatusAprovacao == "Aprovado")
                        .OrderByDescending(e => e.DataSubmissao)
                        .ToList();

                    foreach (var estudo in estudosAprovados)
                    {
                        artigos.Add(estudo.ToArtigoEducacao());
                    }
                }
                catch
                {
                    // Fallback seguro
                }
            }

            if (!string.IsNullOrWhiteSpace(categoria) && categoria != "Todas")
            {
                artigos = artigos.Where(a => string.Equals(a.Categoria, categoria, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(busca))
            {
                artigos = artigos.Where(a =>
                    a.Titulo.Contains(busca, StringComparison.OrdinalIgnoreCase) ||
                    a.Subtitulo.Contains(busca, StringComparison.OrdinalIgnoreCase) ||
                    a.Resumo.Contains(busca, StringComparison.OrdinalIgnoreCase) ||
                    a.EspecieNome.Contains(busca, StringComparison.OrdinalIgnoreCase) ||
                    a.Autor.Contains(busca, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            return artigos;
        }

        public static ArtigoEducacao? ObterPorSlug(AppDbContext? context, string slug)
        {
            if (context != null)
            {
                try
                {
                    var estudoDb = context.EstudosCientificos
                        .FirstOrDefault(e => e.Slug == slug && e.StatusAprovacao == "Aprovado");
                    if (estudoDb != null)
                    {
                        return estudoDb.ToArtigoEducacao();
                    }
                }
                catch
                {
                    // Fallback seguro
                }
            }

            var baseArt = ArtigosBase.FirstOrDefault(a => string.Equals(a.Slug, slug, StringComparison.OrdinalIgnoreCase));
            if (baseArt == null) return null;

            var art = ClonarArtigo(baseArt);

            if (context != null)
            {
                var especie = context.Especies.FirstOrDefault(e => e.Nome == art.EspecieNome);
                if (especie != null)
                {
                    art.EspecieId = especie.Id;
                }
            }

            return art;
        }

        public static List<string> ObterCategorias(AppDbContext? context = null)
        {
            var cats = ArtigosBase.Select(a => a.Categoria).ToList();
            if (context != null)
            {
                try
                {
                    var catsDb = context.EstudosCientificos
                        .Where(e => e.StatusAprovacao == "Aprovado")
                        .Select(e => e.Categoria)
                        .ToList();
                    cats.AddRange(catsDb);
                }
                catch { }
            }
            return cats.Distinct().OrderBy(c => c).ToList();
        }

        private static ArtigoEducacao ClonarArtigo(ArtigoEducacao a)
        {
            return new ArtigoEducacao
            {
                Id = a.Id,
                Slug = a.Slug,
                Titulo = a.Titulo,
                Subtitulo = a.Subtitulo,
                Categoria = a.Categoria,
                Resumo = a.Resumo,
                ConteudoHtml = a.ConteudoHtml,
                EspecieNome = a.EspecieNome,
                EspecieId = a.EspecieId,
                StatusConservacao = a.StatusConservacao,
                Autor = a.Autor,
                Instituicao = a.Instituicao,
                DataPublicacao = a.DataPublicacao,
                TempoLeituraMinutos = a.TempoLeituraMinutos,
                ImagemUrl = a.ImagemUrl,
                Destaques = new List<string>(a.Destaques),
                AmeacasChave = new List<string>(a.AmeacasChave),
                AcoesConservacao = new List<string>(a.AcoesConservacao),
                Referencias = new List<string>(a.Referencias)
            };
        }
    }
}
