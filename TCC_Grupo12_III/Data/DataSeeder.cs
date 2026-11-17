using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TCC_Grupo12_III.Models;

namespace TCC_Grupo12_III.Data
{
    public static class DataSeeder
    {
        private static readonly Dictionary<string, string> ImagensPadrao = new()
        {
            ["Onça-parda"] = "/img/especies/onca-parda.jpg",
            ["Papagaio-de-peito-roxo"] = "/img/especies/papagaio-de-peito-roxo.jpg",
            ["Jaguatirica"] = "/img/especies/jaguatirica.jpg",
            ["Cateto"] = "/img/especies/cateto.jpg",
            ["Saíra-sete-cores"] = "/img/especies/saira-sete-cores.jpg",
            ["Bugio-ruivo"] = "/img/especies/bugio-ruivo.jpg"
        };

        private static readonly Dictionary<string, string> ImagensCampanhasPadrao = new()
        {
            ["Reflorestamento da Cantareira 2026"] = "/img/campanhas/reflorestamento.jpg",
            ["Monitoramento de Fauna"] = "/img/campanhas/monitoramento.jpg",
            ["Limpeza de Trilhas"] = "/img/campanhas/limpeza-trilhas.jpg",
            ["Educação nas Escolas"] = "/img/campanhas/educacao-escolas.jpg"
        };

        public static void Seed(AppDbContext context)
        {
            SemearEspecies(context);

            SemearNivelJogo(context);

            SemearAvistamentos(context);

            var usuarios = SemearUsuarios(context);

            var comunidades = SemearComunidades(context, usuarios);

            SemearCampanhas(context, usuarios, comunidades);

            SemearForumTopicos(context, usuarios, comunidades);

            SemearForumRespostas(context, usuarios);

            SemearEstudosCientificos(context, usuarios);

            SemearDenuncias(context);

            SemearFaqTickets(context, usuarios);
        }

        private static void SemearEspecies(AppDbContext context)
        {
            if (!context.Especies.Any())
            {
                context.Especies.AddRange(
                    new Especie
                    {
                        Nome = "Onça-parda",
                        NomeCientifico = "Puma concolor",
                        Categoria = "Mamíferos",
                        StatusConservacao = "Vulnerável",
                        Tags = "Floresta Densa, Mamífero, Carnívoro, Noturno",
                        Habitat = "Florestas densas, áreas montanhosas e regiões de vegetação nativa preservada, como os núcleos da Serra da Cantareira.",
                        Alimentacao = "Carnívoro. Alimenta-se de veados, capivaras, tatus, pacas e outros mamíferos de médio porte.",
                        Ameacas = "Desmatamento e fragmentação de habitat\nCaça ilegal e conflitos com humanos\nAtropelamentos em rodovias\nRedução de presas naturais",
                        Caracteristicas = "Maior felino das Américas depois da onça-pintada, de hábitos noturnos e solitários.",
                        Comportamento = "Extremamente territorial, pode ter áreas de caça de até 200 km². Excelente saltadora, alcança 5 metros em um único salto.",
                        StatusDetalhe = "Classificada como \"Vulnerável\" em SP. Estimativa de menos de 50 indivíduos na Serra da Cantareira.",
                        ImagemUrl = "/img/especies/onca-parda.jpg"
                    },
                    new Especie
                    {
                        Nome = "Papagaio-de-peito-roxo",
                        NomeCientifico = "Amazona vinacea",
                        Categoria = "Aves",
                        StatusConservacao = "Criticamente Ameaçado",
                        Tags = "Mata Atlântica, Ave, Frugívoro, Diurno",
                        Habitat = "Remanescentes de Mata Atlântica com araucárias e árvores de grande porte para nidificação.",
                        Alimentacao = "Frugívoro. Alimenta-se de frutos, sementes e pinhões de araucária.",
                        Ameacas = "Perda de habitat na Mata Atlântica\nCaptura ilegal para o tráfico de animais\nCompetição por cavidades para nidificação\nFragmentação de corredores florestais",
                        Caracteristicas = "Papagaio de porte médio, com peito arroxeado característico e voo em bandos ruidosos.",
                        Comportamento = "Vive em bandos e costuma se deslocar longas distâncias em busca de árvores frutíferas.",
                        StatusDetalhe = "Classificado como \"Criticamente Ameaçado\", uma das espécies de papagaio mais ameaçadas do Brasil.",
                        ImagemUrl = "/img/especies/papagaio-de-peito-roxo.jpg"
                    },
                    new Especie
                    {
                        Nome = "Jaguatirica",
                        NomeCientifico = "Leopardus pardalis",
                        Categoria = "Mamíferos",
                        StatusConservacao = "Vulnerável",
                        Tags = "Floresta Densa, Mamífero, Carnívoro, Noturno",
                        Habitat = "Florestas densas e áreas de mata ciliar próximas a corpos d'água.",
                        Alimentacao = "Carnívoro. Caça pequenos mamíferos, aves, répteis e anfíbios.",
                        Ameacas = "Fragmentação de habitat\nAtropelamentos em rodovias\nCaça ilegal por sua pelagem\nConflito com animais domésticos",
                        Caracteristicas = "Felino de médio porte, com pelagem manchada usada como camuflagem na mata.",
                        Comportamento = "Solitária e de hábitos predominantemente noturnos, marca território com urina e arranhões em árvores.",
                        StatusDetalhe = "Classificada como \"Vulnerável\", com população em declínio devido à perda de habitat.",
                        ImagemUrl = "/img/especies/jaguatirica.jpg"
                    },
                    new Especie
                    {
                        Nome = "Cateto",
                        NomeCientifico = "Pecari tajacu",
                        Categoria = "Mamíferos",
                        StatusConservacao = "Vulnerável",
                        Tags = "Floresta Densa, Mamífero, Onívoro, Diurno",
                        Habitat = "Florestas densas e áreas de mata secundária com fontes de água próximas.",
                        Alimentacao = "Onívoro. Come raízes, frutos, sementes e pequenos invertebrados.",
                        Ameacas = "Caça para consumo de carne\nDesmatamento de áreas de forrageio\nAtropelamentos em estradas\nFragmentação de habitat",
                        Caracteristicas = "Porco-do-mato de pequeno porte, vive em grupos familiares e possui glândula de odor característico.",
                        Comportamento = "Vive em grupos de até uma dezena de indivíduos, se deslocando juntos em busca de alimento.",
                        StatusDetalhe = "Classificado como \"Vulnerável\" em áreas urbanas de Mata Atlântica, como a Serra da Cantareira.",
                        ImagemUrl = "/img/especies/cateto.jpg"
                    },
                    new Especie
                    {
                        Nome = "Saíra-sete-cores",
                        NomeCientifico = "Tangara seledon",
                        Categoria = "Aves",
                        StatusConservacao = "Vulnerável",
                        Tags = "Mata Atlântica, Ave, Frugívoro, Diurno",
                        Habitat = "Bordas de floresta e áreas de mata secundária da Mata Atlântica.",
                        Alimentacao = "Frugívoro e insetívoro ocasional. Alimenta-se de pequenos frutos e insetos.",
                        Ameacas = "Perda de habitat na Mata Atlântica\nFragmentação florestal\nCaptura para o comércio ilegal de aves",
                        Caracteristicas = "Uma das aves mais coloridas da Mata Atlântica, com plumagem multicolorida característica.",
                        Comportamento = "Costuma se deslocar em bandos mistos com outras espécies de saíras pela copa das árvores.",
                        StatusDetalhe = "Classificada como \"Vulnerável\", dependente da conservação de remanescentes de Mata Atlântica.",
                        ImagemUrl = "/img/especies/saira-sete-cores.jpg"
                    },
                    new Especie
                    {
                        Nome = "Bugio-ruivo",
                        NomeCientifico = "Alouatta guariba",
                        Categoria = "Mamíferos",
                        StatusConservacao = "Vulnerável",
                        Tags = "Floresta Densa, Mamífero, Folívoro, Diurno",
                        Habitat = "Copas de florestas densas e preservadas da Mata Atlântica.",
                        Alimentacao = "Folívoro e frugívoro. Alimenta-se principalmente de folhas, brotos e frutos.",
                        Ameacas = "Desmatamento e fragmentação de habitat\nSurtos de febre amarela\nAtropelamentos e eletrocussão em fiação\nCaça ilegal",
                        Caracteristicas = "Primata de grande porte conhecido pelo rugido característico, ouvido a longas distâncias.",
                        Comportamento = "Vive em grupos familiares e é conhecido pelas vocalizações potentes usadas para marcar território.",
                        StatusDetalhe = "Classificado como \"Vulnerável\", com populações sensíveis a surtos de febre amarela silvestre.",
                        ImagemUrl = "/img/especies/bugio-ruivo.jpg"
                    }
                );
                context.SaveChanges();
            }

            foreach (var especie in context.Especies.Where(e => e.ImagemUrl == null || e.ImagemUrl == "").ToList())
            {
                if (ImagensPadrao.TryGetValue(especie.Nome, out var imagem))
                {
                    especie.ImagemUrl = imagem;
                }
            }
            context.SaveChanges();
        }

        private static void SemearAvistamentos(AppDbContext context)
        {
            if (context.Avistamentos.Any())
            {
                return;
            }

            var especies = context.Especies.ToDictionary(e => e.Nome, e => e.Id);

            int? IdDe(string nome) => especies.TryGetValue(nome, out var id) ? id : null;

            context.Avistamentos.AddRange(
                new Avistamento
                {
                    EspecieId = IdDe("Onça-parda"),
                    NomeAnimal = "Onça-parda",
                    FotoPath = "/img/especies/onca-parda.jpg",
                    Latitude = -23.4526,
                    Longitude = -46.6106,
                    DataAvistamento = DateTime.Today.AddDays(-9),
                    Quantidade = 1,
                    Observacoes = "Indivíduo adulto cruzando a trilha do Núcleo Engordador no início da manhã.",
                    NomeObservador = "Marina Alves",
                    CriadoEm = DateTime.Now.AddDays(-9)
                },
                new Avistamento
                {
                    EspecieId = IdDe("Papagaio-de-peito-roxo"),
                    NomeAnimal = "Papagaio-de-peito-roxo",
                    FotoPath = "/img/especies/papagaio-de-peito-roxo.jpg",
                    Latitude = -23.4438,
                    Longitude = -46.5997,
                    DataAvistamento = DateTime.Today.AddDays(-6),
                    Quantidade = 4,
                    Observacoes = "Bando pousado em araucárias próximo ao mirante da Pedra Grande.",
                    NomeObservador = "Rafael Souza",
                    CriadoEm = DateTime.Now.AddDays(-6)
                },
                new Avistamento
                {
                    EspecieId = IdDe("Jaguatirica"),
                    NomeAnimal = "Jaguatirica",
                    FotoPath = "/img/especies/jaguatirica.jpg",
                    Latitude = -23.3979,
                    Longitude = -46.6280,
                    DataAvistamento = DateTime.Today.AddDays(-13),
                    Quantidade = 1,
                    Observacoes = "Registrada por câmera trap no Núcleo Águas Claras.",
                    NomeObservador = "Juliana Lima",
                    CriadoEm = DateTime.Now.AddDays(-13)
                },
                new Avistamento
                {
                    EspecieId = IdDe("Cateto"),
                    NomeAnimal = "Cateto",
                    FotoPath = "/img/especies/cateto.jpg",
                    Latitude = -23.3953,
                    Longitude = -46.6876,
                    DataAvistamento = DateTime.Today.AddDays(-4),
                    Quantidade = 3,
                    Observacoes = "Grupo de três indivíduos forrageando na borda da mata do Núcleo Cabuçu.",
                    NomeObservador = "Carlos Mendes",
                    CriadoEm = DateTime.Now.AddDays(-4)
                },
                new Avistamento
                {
                    EspecieId = IdDe("Saíra-sete-cores"),
                    NomeAnimal = "Saíra-sete-cores",
                    FotoPath = "/img/especies/saira-sete-cores.jpg",
                    Latitude = -23.4594,
                    Longitude = -46.6196,
                    DataAvistamento = DateTime.Today.AddDays(-2),
                    Quantidade = 2,
                    Observacoes = "Casal alimentando-se em arbustos frutíferos junto ao Horto Florestal.",
                    NomeObservador = "Beatriz Rocha",
                    CriadoEm = DateTime.Now.AddDays(-2)
                },
                new Avistamento
                {
                    EspecieId = IdDe("Bugio-ruivo"),
                    NomeAnimal = "Bugio-ruivo",
                    FotoPath = "/img/especies/bugio-ruivo.jpg",
                    Latitude = -23.4419,
                    Longitude = -46.6233,
                    DataAvistamento = DateTime.Today.AddDays(-1),
                    Quantidade = 5,
                    Observacoes = "Grupo de cinco indivíduos vocalizando no dossel da Trilha do Bugio.",
                    NomeObservador = "Felipe Nogueira",
                    CriadoEm = DateTime.Now.AddDays(-1)
                }
            );

            context.SaveChanges();
        }

        private static void SemearNivelJogo(AppDbContext context)
        {
            var especies = context.Especies.OrderBy(e => e.Id).ToList();
            if (especies.Count == 0)
            {
                return;
            }

            int Especie(int i) => especies[i % especies.Count].Id;

            var dados = new NivelJogoDados { Largura = 120 };

            dados.Plataformas.Add(new NivelPlataforma { X = 0, Y = 15, W = 56, H = 2, Tipo = "terra" });
            dados.Plataformas.Add(new NivelPlataforma { X = 66, Y = 15, W = 54, H = 2, Tipo = "terra" });

            dados.Plataformas.Add(new NivelPlataforma { X = 20, Y = 11, W = 8, H = 1, Tipo = "plataforma" });
            dados.Plataformas.Add(new NivelPlataforma { X = 45, Y = 9, W = 6, H = 1, Tipo = "plataforma" });
            dados.Plataformas.Add(new NivelPlataforma { X = 59, Y = 12, W = 4, H = 1, Tipo = "madeira" });
            dados.Plataformas.Add(new NivelPlataforma { X = 72, Y = 11, W = 8, H = 1, Tipo = "plataforma" });
            dados.Plataformas.Add(new NivelPlataforma { X = 95, Y = 9, W = 6, H = 1, Tipo = "plataforma" });

            dados.Plataformas.Add(new NivelPlataforma { X = 54, Y = 14, W = 1, H = 1, Tipo = "mola" });
            dados.Plataformas.Add(new NivelPlataforma { X = 90, Y = 14, W = 1, H = 1, Tipo = "mola" });

            dados.Perigos.Add(new NivelPerigo { X = 30, Y = 14, W = 3, H = 1, Tipo = "fogo" });
            dados.Perigos.Add(new NivelPerigo { X = 56, Y = 15, W = 10, H = 2, Tipo = "agua" });
            dados.Perigos.Add(new NivelPerigo { X = 82, Y = 14, W = 3, H = 1, Tipo = "espinho" });

            dados.Animais.Add(new NivelAnimal { X = 12, Y = 14, EspecieId = Especie(0) });
            dados.Animais.Add(new NivelAnimal { X = 23, Y = 10, EspecieId = Especie(1) });
            dados.Animais.Add(new NivelAnimal { X = 48, Y = 8, EspecieId = Especie(2) });
            dados.Animais.Add(new NivelAnimal { X = 75, Y = 14, EspecieId = Especie(3) });
            dados.Animais.Add(new NivelAnimal { X = 98, Y = 8, EspecieId = Especie(4) });

            dados.Inicio = new NivelPonto { X = 2, Y = 14 };
            dados.Objetivo = new NivelPonto { X = 116, Y = 14 };

            var nivelExistente = context.NivelJogos.FirstOrDefault();
            if (nivelExistente == null)
            {
                context.NivelJogos.Add(new NivelJogo
                {
                    Titulo = "Resgate na Trilha do Bugio",
                    Descricao = "Corra pela Serra, use as molas, desvie dos perigos e resgate a espécie escolhida na base final.",
                    Data = DateTime.Today,
                    Ativo = true,
                    DadosJson = dados.Serializar(),
                    TotalAnimais = dados.Animais.Count
                });
            }
            else
            {
                var dadosAntigos = NivelJogoDados.Desserializar(nivelExistente.DadosJson);
                var temAguaSobreTerra = dadosAntigos != null && dadosAntigos.Perigos.Any(p => p.Tipo == "agua" &&
                    dadosAntigos.Plataformas.Any(plat => plat.Tipo == "terra" && plat.Y == p.Y + 1 && plat.X <= p.X && plat.X + plat.W >= p.X + p.W));
                if (temAguaSobreTerra)
                {
                    nivelExistente.DadosJson = dados.Serializar();
                    nivelExistente.TotalAnimais = dados.Animais.Count;
                }
            }

            context.SaveChanges();
        }

        private static Dictionary<string, Usuario> SemearUsuarios(AppDbContext context)
        {
            var adminUser = context.Usuarios.FirstOrDefault(u => u.Email == "admin@ecocanta.org");
            if (adminUser == null)
            {
                adminUser = new Usuario
                {
                    Nome = "EcoCanta Admin",
                    Email = "admin@ecocanta.org",
                    Senha = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                    IsAdmin = true,
                    IsModerador = true,
                    Xp = 1500
                };
                context.Usuarios.Add(adminUser);
            }
            else
            {
                adminUser.IsAdmin = true;
                adminUser.IsModerador = true;
            }

            var biologiaUser = context.Usuarios.FirstOrDefault(u => u.Email == "pesquisa@ecocanta.org");
            if (biologiaUser == null)
            {
                biologiaUser = new Usuario
                {
                    Nome = "Dr. André Biólogo",
                    Email = "pesquisa@ecocanta.org",
                    Senha = BCrypt.Net.BCrypt.HashPassword("Bio123!"),
                    IsAdmin = true,
                    IsModerador = true,
                    Xp = 850
                };
                context.Usuarios.Add(biologiaUser);
            }
            else
            {
                biologiaUser.IsAdmin = true;
                biologiaUser.IsModerador = true;
            }

            var moderadorUser = context.Usuarios.FirstOrDefault(u => u.Email == "moderador@ecocanta.org");
            if (moderadorUser == null)
            {
                moderadorUser = new Usuario
                {
                    Nome = "Carlos Eduardo (Moderador)",
                    Email = "moderador@ecocanta.org",
                    Senha = BCrypt.Net.BCrypt.HashPassword("123456"),
                    IsAdmin = false,
                    IsModerador = true,
                    Xp = 450
                };
                context.Usuarios.Add(moderadorUser);
            }
            else
            {
                moderadorUser.IsModerador = true;
            }

            var fernandaMod = context.Usuarios.FirstOrDefault(u => u.Email == "fernanda.mod@ecocanta.org");
            if (fernandaMod == null)
            {
                fernandaMod = new Usuario
                {
                    Nome = "Fernanda Souza (Moderadora)",
                    Email = "fernanda.mod@ecocanta.org",
                    Senha = BCrypt.Net.BCrypt.HashPassword("123456"),
                    IsAdmin = false,
                    IsModerador = true,
                    Xp = 380
                };
                context.Usuarios.Add(fernandaMod);
            }
            else
            {
                fernandaMod.IsModerador = true;
            }

            var beatrizJornalista = context.Usuarios.FirstOrDefault(u => u.Email == "beatriz.jornalista@ecocanta.org");
            if (beatrizJornalista == null)
            {
                beatrizJornalista = new Usuario
                {
                    Nome = "Beatriz Lima (Jornalista)",
                    Email = "beatriz.jornalista@ecocanta.org",
                    Senha = BCrypt.Net.BCrypt.HashPassword("123456"),
                    IsAdmin = false,
                    IsModerador = false,
                    IsJornalista = true,
                    StatusJornalista = "Aprovado",
                    RegistroJornalista = "MTB 54.321/SP",
                    VeiculoJornalista = "EcoJornalismo Metropolitano",
                    LinkPortfolioJornalista = "https://ecojornal.exemplo.com/beatriz",
                    JustificativaJornalista = "Cobertura diária de pautas socioambientais, avifauna e projetos de restauração ecológica da Serra da Cantareira.",
                    DataSolicitacaoJornalista = DateTime.Now.AddDays(-15),
                    DataRespostaJornalista = DateTime.Now.AddDays(-12),
                    Xp = 420
                };
                context.Usuarios.Add(beatrizJornalista);
            }

            var thiagoReporter = context.Usuarios.FirstOrDefault(u => u.Email == "thiago.reporter@ecocanta.org");
            if (thiagoReporter == null)
            {
                thiagoReporter = new Usuario
                {
                    Nome = "Thiago Mendes (Candidato a Jornalista)",
                    Email = "thiago.reporter@ecocanta.org",
                    Senha = BCrypt.Net.BCrypt.HashPassword("123456"),
                    IsAdmin = false,
                    IsModerador = false,
                    IsJornalista = false,
                    StatusJornalista = "Pendente",
                    RegistroJornalista = "MTB 98.765/SP",
                    VeiculoJornalista = "Portal Verde SP",
                    LinkPortfolioJornalista = "https://portalverdesp.exemplo.com/thiago",
                    JustificativaJornalista = "Produção de série investigativa sobre conservação de mananciais, combate ao loteamento clandestino e monitoramento de fauna na bacia da Cantareira.",
                    DataSolicitacaoJornalista = DateTime.Now.AddDays(-2),
                    Xp = 210
                };
                context.Usuarios.Add(thiagoReporter);
            }

            var marianaJornalista = context.Usuarios.FirstOrDefault(u => u.Email == "mariana.eco@ecocanta.org");
            if (marianaJornalista == null)
            {
                marianaJornalista = new Usuario
                {
                    Nome = "Mariana Vasconcelos (Candidata a Jornalista)",
                    Email = "mariana.eco@ecocanta.org",
                    Senha = BCrypt.Net.BCrypt.HashPassword("123456"),
                    IsAdmin = false,
                    IsModerador = false,
                    IsJornalista = false,
                    StatusJornalista = "Pendente",
                    RegistroJornalista = "MTB 11.223/SP",
                    VeiculoJornalista = "Revista Bioma Paulista",
                    LinkPortfolioJornalista = "https://biomapaulista.exemplo.com/mariana",
                    JustificativaJornalista = "Divulgação científica, reportagens de educação ambiental nas escolas e cobertura de eventos de reflorestamento comunitário.",
                    DataSolicitacaoJornalista = DateTime.Now.AddHours(-16),
                    Xp = 180
                };
                context.Usuarios.Add(marianaJornalista);
            }

            var guardiaoUser = context.Usuarios.FirstOrDefault(u => u.Email == "guardiao@ecocanta.org");
            if (guardiaoUser == null)
            {
                guardiaoUser = new Usuario
                {
                    Nome = "Guardião da Trilha",
                    Email = "guardiao@ecocanta.org",
                    Senha = BCrypt.Net.BCrypt.HashPassword("123456"),
                    IsAdmin = false,
                    IsModerador = false,
                    Xp = 190
                };
                context.Usuarios.Add(guardiaoUser);
            }

            var usuarioSuspenso = context.Usuarios.FirstOrDefault(u => u.Email == "lucas.spam@ecocanta.org");
            if (usuarioSuspenso == null)
            {
                usuarioSuspenso = new Usuario
                {
                    Nome = "Lucas Pereira (Suspenso)",
                    Email = "lucas.spam@ecocanta.org",
                    Senha = BCrypt.Net.BCrypt.HashPassword("123456"),
                    IsAdmin = false,
                    IsModerador = false,
                    SuspensoForumAte = DateTime.Now.AddDays(14),
                    MotivoSuspensaoForum = "Divulgação repetitiva de links comerciais e desrespeito às regras de convivência da comunidade.",
                    DataSuspensaoForum = DateTime.Now.AddDays(-2),
                    SuspensoPorNome = "Carlos Eduardo (Moderador)",
                    Xp = 30
                };
                context.Usuarios.Add(usuarioSuspenso);
            }

            var usuarioBanido = context.Usuarios.FirstOrDefault(u => u.Email == "infrator@ecocanta.org");
            if (usuarioBanido == null)
            {
                usuarioBanido = new Usuario
                {
                    Nome = "Marcos Vandalismo (Banido)",
                    Email = "infrator@ecocanta.org",
                    Senha = BCrypt.Net.BCrypt.HashPassword("123456"),
                    IsAdmin = false,
                    IsModerador = false,
                    IsBanido = true,
                    MotivoBanimento = "Tentativa de coordenar caça predatória e envio de ameaças a voluntários do projeto.",
                    DataBanimento = DateTime.Now.AddDays(-7),
                    BanidoPorNome = "EcoCanta Admin",
                    Xp = 0
                };
                context.Usuarios.Add(usuarioBanido);
            }

            context.SaveChanges();

            return context.Usuarios.ToDictionary(u => u.Email, u => u);
        }

        private static Dictionary<string, ForumComunidade> SemearComunidades(AppDbContext context, Dictionary<string, Usuario> usuarios)
        {
            var adminUser = usuarios["admin@ecocanta.org"];
            var biologiaUser = usuarios["pesquisa@ecocanta.org"];
            var beatrizUser = usuarios["beatriz.jornalista@ecocanta.org"];
            var thiagoUser = usuarios["thiago.reporter@ecocanta.org"];
            var guardiaoUser = usuarios["guardiao@ecocanta.org"];

            void GarantirComunidade(string slug, Action<ForumComunidade> config)
            {
                var com = context.ForumComunidades.FirstOrDefault(c => c.Slug == slug);
                if (com == null)
                {
                    com = new ForumComunidade { Slug = slug };
                    config(com);
                    context.ForumComunidades.Add(com);
                }
                else
                {
                    config(com);
                }
            }

            // 1. Assuntos Gerais (Oficial, Aprovada)
            GarantirComunidade("assuntos-gerais", c =>
            {
                c.Nome = "Assuntos Gerais";
                c.Descricao = "Comunidade geral e oficial do EcoCanta mantida pela moderação para discussões abertas, novidades, dúvidas gerais e avisos sobre a Serra da Cantareira.";
                c.Regras = "Respeito mútuo entre todos os membros; postagens pertinentes à conservação, fauna, flora e ecoturismo consciente da Serra da Cantareira; proibido anúncios comerciais não autorizados ou spam.";
                c.StatusAprovacao = "Aprovada";
                c.IsGeral = true;
                c.CriadorId = adminUser.Id;
                c.CriadoEm = DateTime.Now.AddMonths(-1);
                c.AprovadoEm = DateTime.Now.AddMonths(-1);
                c.AprovadoPorId = adminUser.Id;
            });

            // 2. Observadores de Aves Cantareira (Aprovada)
            GarantirComunidade("aves-cantareira", c =>
            {
                c.Nome = "Observadores de Aves Cantareira";
                c.Descricao = "Comunidade dedicada a entusiastas do birdwatching, troca de registros sonoros, fotos de psitacídeos, passeriformes e rotas de observação nos quatro núcleos do parque.";
                c.Regras = "Não utilize playback excessivo em épocas de acasalamento; jamais divulgue coordenadas geográficas exatas de ninhos de espécies vulneráveis; fotos sempre obtidas sem afugentar a ave.";
                c.StatusAprovacao = "Aprovada";
                c.IsGeral = false;
                c.CriadorId = beatrizUser.Id;
                c.CriadoEm = DateTime.Now.AddDays(-20);
                c.AprovadoEm = DateTime.Now.AddDays(-19);
                c.AprovadoPorId = adminUser.Id;
            });

            // 3. Guardiões dos Felinos & Fauna Rara (Aprovada)
            GarantirComunidade("guardioes-felinos", c =>
            {
                c.Nome = "Guardiões dos Felinos & Fauna Rara";
                c.Descricao = "Grupo voltado ao monitoramento colaborativo e ético de mamíferos topo de cadeia, análise de pegadas, registros de armadilhas fotográficas e conscientização sobre atropelamentos.";
                c.Regras = "Compartilhamento estritamente ético e conservacionista; proibido encorajar buscas ativas noturnas fora das trilhas demarcadas; sigilo em avistamentos de onças-pardas com filhotes.";
                c.StatusAprovacao = "Aprovada";
                c.IsGeral = false;
                c.CriadorId = biologiaUser.Id;
                c.CriadoEm = DateTime.Now.AddDays(-15);
                c.AprovadoEm = DateTime.Now.AddDays(-14);
                c.AprovadoPorId = adminUser.Id;
            });

            // 4. Trilheiros & Conservação Noturna (PENDENTE para teste de moderação)
            GarantirComunidade("trilhas-noturnas", c =>
            {
                c.Nome = "Trilheiros & Conservação Noturna";
                c.Descricao = "Comunidade proposta para praticantes de caminhadas ao entardecer, inventários noturnos de anfíbios anuros, corujas e monitoramento do impacto humano em trilhas periféricas.";
                c.Regras = "Somente caminhar em rotas permitidas pelos órgãos ambientais; lanternas com filtro vermelho ao avistar fauna noturna; recolhimento integral de resíduos.";
                c.StatusAprovacao = "Pendente";
                c.IsGeral = false;
                c.CriadorId = thiagoUser.Id;
                c.CriadoEm = DateTime.Now.AddDays(-3);
                c.AprovadoEm = null;
                c.AprovadoPorId = null;
            });

            // 5. Fotografia de Natureza e Macro (PENDENTE para teste de moderação)
            GarantirComunidade("fotografia-macro", c =>
            {
                c.Nome = "Fotografia de Natureza e Macro";
                c.Descricao = "Proposta de comunidade para fotógrafos de macrofotografia botânica, fungos bioluminescentes, insetos polinizadores e répteis da Serra da Cantareira.";
                c.Regras = "Proibido manipular ou remover espécimes de seu habitat natural; postagens devem conter dados sobre iluminação e equipamento para aprendizado coletivo.";
                c.StatusAprovacao = "Pendente";
                c.IsGeral = false;
                c.CriadorId = guardiaoUser.Id;
                c.CriadoEm = DateTime.Now.AddDays(-1);
                c.AprovadoEm = null;
                c.AprovadoPorId = null;
            });

            context.SaveChanges();

            return context.ForumComunidades.ToDictionary(c => c.Slug, c => c);
        }

        private static void SemearCampanhas(AppDbContext context, Dictionary<string, Usuario> usuarios, Dictionary<string, ForumComunidade> comunidades)
        {
            var adminUser = usuarios["admin@ecocanta.org"];
            var beatrizUser = usuarios["beatriz.jornalista@ecocanta.org"];
            var guardiaoUser = usuarios["guardiao@ecocanta.org"];

            var comGeral = comunidades["assuntos-gerais"];
            var comAves = comunidades["aves-cantareira"];
            var comFelinos = comunidades["guardioes-felinos"];

            void GarantirCampanha(string titulo, Action<Campanha> config)
            {
                var camp = context.Campanhas.FirstOrDefault(c => c.Titulo == titulo);
                if (camp == null)
                {
                    camp = new Campanha { Titulo = titulo };
                    config(camp);
                    context.Campanhas.Add(camp);
                }
                else
                {
                    config(camp);
                }
            }

            GarantirCampanha("Reflorestamento da Cantareira 2026", c =>
            {
                c.Descricao = "Mutirões de plantio de mudas nativas para recompor áreas degradadas da Serra da Cantareira.";
                c.Periodo = "Jan – Dez 2026";
                c.Participantes = 214;
                c.Meta = 1000;
                c.Ativa = true;
                c.StatusAprovacao = "Aprovado";
                c.ComunidadeId = comGeral.Id;
                c.UsuarioId = adminUser.Id;
                c.ImagemUrl = "/img/campanhas/reflorestamento.jpg";
                c.ObjetivoPrincipal = "Recompor 10 hectares de encostas com mais de 2.000 mudas de árvores nativas da Mata Atlântica.";
                c.ObjetivosEspecificos = "Promover 6 mutirões semestrais com voluntários;\nProduzir mudas em viveiro comunitário credenciado;\nMonitorar o índice de sobrevivência das plântulas.";
                c.LocalEncontro = "Núcleo Pedra Grande - Horto Florestal";
                c.Horario = "Sábados das 08h30 às 13h";
                c.PublicoAlvo = "Voluntários de todas as idades, estudantes e famílias";
                c.OQueLevar = "Roupas confortáveis, bota ou tênis aderente, luva de jardinagem e protetor solar";
                c.Cronograma = "Fase 1: Preparo do solo e abertura de covas;\nFase 2: Plantio coletivo com adubação;\nFase 3: Coroamento e rega no primeiro mês.";
                c.ImpactoEsperado = "Criação de novos corredores de dispersão para aves e mamíferos locais.";
                c.Organizador = "EcoCanta & Fundação Florestal";
            });

            GarantirCampanha("Monitoramento de Fauna", c =>
            {
                c.Descricao = "Contribua com registro fotográfico e bioacústico de animais silvestres nos 4 núcleos do parque.";
                c.Periodo = "Mai – Ago 2026";
                c.Participantes = 89;
                c.Meta = 500;
                c.Ativa = true;
                c.StatusAprovacao = "Aprovado";
                c.ComunidadeId = comFelinos.Id;
                c.UsuarioId = adminUser.Id;
                c.ImagemUrl = "/img/campanhas/monitoramento.jpg";
                c.ObjetivoPrincipal = "Gerar inventário colaborativo de médios e grandes mamíferos utilizando armadilhas fotográficas e registros cidadãos.";
                c.ObjetivosEspecificos = "Mapear pontos críticos de travessia em rodovias;\nTreinar 50 guardiões em identificação de vestígios e pegadas;\nAlimentar a base aberta de dados científicos do EcoCanta.";
                c.LocalEncontro = "Núcleo Engordador - Estrada do Engordador, s/n";
                c.Horario = "Atividade contínua via plataforma e rondas quinzenais";
                c.PublicoAlvo = "Observadores de fauna, biólogos, fotógrafos e frequentadores assíduos";
                c.OQueLevar = "Câmera ou smartphone, binóculos, GPS ou app EcoCanta instalado";
                c.Cronograma = "Mês 1-2: Coleta de avistamentos;\nMês 3: Análise taxonômica por especialistas;\nMês 4: Publicação do relatório consolidado.";
                c.ImpactoEsperado = "Subsidiar instalação de novas passagens de fauna e sinalização redutora de velocidade.";
                c.Organizador = "Laboratório de Conservação da Biodiversidade & EcoCanta";
            });

            GarantirCampanha("Limpeza de Trilhas", c =>
            {
                c.Descricao = "Mutirão mensal para limpeza, remoção de microplásticos e manutenção de trilhas ecológicas.";
                c.Periodo = "Todo 2º sábado";
                c.Participantes = 156;
                c.Meta = 500;
                c.Ativa = true;
                c.StatusAprovacao = "Aprovado";
                c.ComunidadeId = comGeral.Id;
                c.UsuarioId = adminUser.Id;
                c.ImagemUrl = "/img/campanhas/limpeza-trilhas.jpg";
                c.ObjetivoPrincipal = "Remover 100% dos resíduos descartados irregularmente ao longo das principais trilhas turísticas.";
                c.ObjetivosEspecificos = "Instalar sinalização educativa de \"Não Deixe Rastro\";\nRealizar triagem e destinação correta de recicláveis recolhidos;\nSensibilizar mais de 300 visitantes a cada edição.";
                c.LocalEncontro = "Portaria Principal - Núcleo Águas Claras";
                c.Horario = "09h00 às 13h00";
                c.PublicoAlvo = "Grupos de caminhada, ecoturistas e cidadãos engajados";
                c.OQueLevar = "Saco de lixo reforçado (fornecido no local), luvas grossas e garrafa d'água";
                c.Cronograma = "09h00: Briefing de segurança;\n09h30: Divisão por trechos da trilha;\n12h30: Pesagem dos resíduos e encerramento.";
                c.ImpactoEsperado = "Redução imediata do risco de ingestão de plástico por animais silvestres.";
                c.Organizador = "Coletivo Cantareira Limpa";
            });

            GarantirCampanha("Educação nas Escolas", c =>
            {
                c.Descricao = "Palestras, rodas de conversa e oficinas ambientais em escolas públicas do entorno da Serra da Cantareira.";
                c.Periodo = "Encerrada em 2025";
                c.Participantes = 412;
                c.Meta = 412;
                c.Ativa = false;
                c.StatusAprovacao = "Aprovado";
                c.ComunidadeId = comGeral.Id;
                c.UsuarioId = adminUser.Id;
                c.ImagemUrl = "/img/campanhas/educacao-escolas.jpg";
                c.ObjetivoPrincipal = "Sensibilizar 400 crianças do ensino fundamental sobre a riqueza da Mata Atlântica e a fauna da Serra.";
                c.Organizador = "EcoCanta Educação";
            });

            // 5. PENDENTE DE APROVAÇÃO 1: Proposta por Jornalista Aprovada (Beatriz) vinculada à comunidade de Aves
            GarantirCampanha("Censo Comunitário de Aves e Ninhos da Cantareira", c =>
            {
                c.Descricao = "Mapeamento participativo de ninhos ativos e pontos de alimentação de aves ameaçadas, especialmente o papagaio-de-peito-roxo e a saíra-sete-cores.";
                c.Periodo = "Nov – Dez 2026";
                c.Participantes = 0;
                c.Meta = 120;
                c.Ativa = true;
                c.StatusAprovacao = "Pendente";
                c.ComunidadeId = comAves.Id;
                c.UsuarioId = beatrizUser.Id;
                c.DataInicio = DateTime.Today.AddDays(25);
                c.DataFim = DateTime.Today.AddDays(60);
                c.Horario = "Sábados e domingos das 06h30 às 11h30";
                c.LocalEncontro = "Núcleo Águas Claras - Centro de Apoio ao Visitante";
                c.PublicoAlvo = "Observadores de aves, fotógrafos, ornitólogos e estudantes";
                c.OQueLevar = "Binóculo (recomendado 8x42 ou 10x42), caderneta de campo impermeável, capa de chuva e bota de trilha";
                c.ObjetivoPrincipal = "Identificar e georreferenciar com sigilo ético ao menos 20 árvores-ninho ativas na porção central do parque.";
                c.ObjetivosEspecificos = "Capacitar 40 voluntários em gravação de vocalizações de psitacídeos;\nMonitorar a predação de ovos por espécies invasoras;\nSubsidiar plano de conservação de avifauna 2027.";
                c.Cronograma = "Semana 1: Oficina de ornitologia prática;\nSemanas 2-4: Varreduras matinais em campo;\nSemana 5: Consolidação dos dados.";
                c.ImpactoEsperado = "Proteção direta de ninhos durante a temporada reprodutiva de primavera/verão.";
                c.Organizador = "Clube de Observadores de Aves de SP & Beatriz Lima";
                c.ImagemUrl = "/img/especies/papagaio-de-peito-roxo.jpg";
            });

            // 6. PENDENTE DE APROVAÇÃO 2: Proposta por Guardião voluntário
            GarantirCampanha("Mutirão de Restauração de Nascentes do Córrego Água Fria", c =>
            {
                c.Descricao = "Plantio comunitário de espécies ripárias e cercamento preventivo para proteção das nascentes que abastecem comunidades locais.";
                c.Periodo = "Dezembro 2026";
                c.Participantes = 0;
                c.Meta = 80;
                c.Ativa = true;
                c.StatusAprovacao = "Pendente";
                c.ComunidadeId = comGeral.Id;
                c.UsuarioId = guardiaoUser.Id;
                c.DataInicio = DateTime.Today.AddDays(40);
                c.DataFim = DateTime.Today.AddDays(41);
                c.Horario = "08h00 às 14h00";
                c.LocalEncontro = "Entrada do Córrego Água Fria - Mairiporã";
                c.PublicoAlvo = "Moradores do entorno, voluntários e escoteiros";
                c.OQueLevar = "Botas de borracha, luvas de proteção, garrafa d'água e lanche para piquenique coletivo";
                c.ObjetivoPrincipal = "Plantar 600 mudas de espécies nativas ciliares (ingás, embaúbas e guanandi) na cabeceira da bacia.";
                c.ObjetivosEspecificos = "Remover 2 hectares de capim-braquiária invasor;\nInstalar estacas e mourões de delimitação de APP;\nCriar placa educativa de nascente preservada.";
                c.Cronograma = "Dia 1 (08h): Boas-vindas e adubação orgânica;\nDia 1 (10h): Plantio tutorado;\nDia 2: Irrigação e vistoria.";
                c.ImpactoEsperado = "Redução do assoreamento do leito do córrego e aumento da vazão hídrica em 25%.";
                c.Organizador = "Brigada Voluntária da Cantareira";
                c.ImagemUrl = "/img/campanhas/reflorestamento.jpg";
            });

            context.SaveChanges();
        }

        private static void SemearForumTopicos(AppDbContext context, Dictionary<string, Usuario> usuarios, Dictionary<string, ForumComunidade> comunidades)
        {
            var adminUser = usuarios["admin@ecocanta.org"];
            var biologiaUser = usuarios["pesquisa@ecocanta.org"];
            var beatrizUser = usuarios["beatriz.jornalista@ecocanta.org"];
            var guardiaoUser = usuarios["guardiao@ecocanta.org"];

            var comGeral = comunidades["assuntos-gerais"];
            var comAves = comunidades["aves-cantareira"];
            var comFelinos = comunidades["guardioes-felinos"];

            void GarantirTopico(string titulo, Action<ForumTopico> config)
            {
                var topico = context.ForumTopicos.FirstOrDefault(t => t.Titulo == titulo);
                if (topico == null)
                {
                    topico = new ForumTopico { Titulo = titulo };
                    config(topico);
                    context.ForumTopicos.Add(topico);
                }
                else
                {
                    config(topico);
                }
            }

            // Tópicos em Assuntos Gerais
            GarantirTopico("Fundação Florestal amplia monitoramento por câmeras trap no Núcleo Engordador", t =>
            {
                t.UsuarioId = adminUser.Id;
                t.ComunidadeId = comGeral.Id;
                t.Categoria = "noticia";
                t.Conteudo = "Nesta semana, 16 novas armadilhas fotográficas digitais foram instaladas ao longo dos afluentes do Núcleo Engordador. O objetivo do projeto é monitorar a movimentação de carnívoros topo de cadeia, como a onça-parda e a jaguatirica, além de avaliar o fluxo de fauna em direção aos corredores com a Serra do Mar.\n\nOs dados preliminares apontam para um aumento na atividade noturna e presença de indivíduos jovens, um indicativo animador de regeneração populacional na borda norte da bacia.";
                t.Hashtags = "monitoramento, oncaparda, engordador, fauna, conservacao";
                t.CriadoEm = DateTime.Now.AddDays(-6);
            });

            GarantirTopico("Operação 'Corta-Fogo 2026' entra em vigor para prevenção de queimadas de inverno", t =>
            {
                t.UsuarioId = biologiaUser.Id;
                t.ComunidadeId = comGeral.Id;
                t.Categoria = "noticia";
                t.Conteudo = "Com a aproximação do período de estiagem, a administração integrada do Parque Estadual da Cantareira lançou a Operação Corta-Fogo 2026. A ação engloba a limpeza e manutenção preventiva de aceiros nos quatro núcleos, sobrevoos com drones térmicos e rondas conjuntas com o Corpo de Bombeiros e a Polícia Militar Ambiental.\n\nLembramos a todos os visitantes e moradores do entorno que soltar balões e acender fogueiras em áreas de amortecimento é crime ambiental com penalidades severas. Se avistar fumaça ou focos suspeitos, use a aba de Denúncias do EcoCanta imediatamente.";
                t.Hashtags = "prevencao, queimadas, cortafogo, seguranca, cantareira";
                t.CriadoEm = DateTime.Now.AddDays(-4);
            });

            GarantirTopico("Nova passagem aérea de fauna é instalada para proteção dos bugios-ruivos", t =>
            {
                t.UsuarioId = biologiaUser.Id;
                t.ComunidadeId = comGeral.Id;
                t.Categoria = "noticia";
                t.Conteudo = "Em parceria com a concessionária viária e a SAVE Brasil, foi concluída a instalação de uma nova passarela aérea de dossel sobre a Estrada da Roseira. A estrutura de cordas reforçadas permite que tropas de bugios-ruivos e pequenos marsupiais atravessem a via sem descerem ao asfalto, zerando o risco de atropelamentos e eletrocussão naquele trecho crítico.\n\nNas primeiras 48 horas de ativação, armadilhas fotográficas já registraram uma família de bugios inspecionando a nova ponte.";
                t.Hashtags = "bugioruivo, passagensdefauna, corredorecologico, estrada";
                t.CriadoEm = DateTime.Now.AddDays(-2);
            });

            GarantirTopico("Avistamento de pegadas frescas de felino na Trilha da Pedra Grande!", t =>
            {
                t.UsuarioId = guardiaoUser.Id;
                t.ComunidadeId = comGeral.Id;
                t.Categoria = "comunidade";
                t.Conteudo = "Pessoal, hoje cedo (por volta das 6h30) subi a trilha da Pedra Grande e notei pegadas bem nítidas no barro perto da bifurcação da bica d'água. Pelo formato arredondado e tamanho das almofadas (cerca de 8 cm de largura, sem marcas de unhas aparentes), parecia muito ser de uma jaguatirica ou até uma onça-parda jovem!\n\nAlguém mais passou por lá recentemente e viu algo? Já cadastrei o avistamento no mapa aqui do aplicativo com foto das pegadas ao lado da minha pegada de tênis para escala.";
                t.Hashtags = "pedragrande, pegadas, avistamento, jaguatirica, trilhas";
                t.CriadoEm = DateTime.Now.AddHours(-20);
            });

            GarantirTopico("Qual o melhor núcleo para observação de aves (birdwatching) aos finais de semana?", t =>
            {
                t.UsuarioId = guardiaoUser.Id;
                t.ComunidadeId = comGeral.Id;
                t.Categoria = "comunidade";
                t.Conteudo = "Estou começando a praticar birdwatching com meus filhos e queríamos muito conseguir avistar a saíra-sete-cores e quem sabe registrar um papagaio-de-peito-roxo. Qual núcleo vocês recomendam como o mais tranquilo e com boa diversidade nas primeiras horas da manhã? O Núcleo Águas Claras costuma ter trilhas mais silenciosas?";
                t.Hashtags = "birdwatching, sairas, aves, dicas, familias";
                t.CriadoEm = DateTime.Now.AddHours(-10);
            });

            // Tópicos em Observadores de Aves Cantareira
            GarantirTopico("Primeiro registro de ninho ativo de Papagaio-de-peito-roxo na temporada 2026", t =>
            {
                t.UsuarioId = beatrizUser.Id;
                t.ComunidadeId = comAves.Id;
                t.Categoria = "comunidade";
                t.Conteudo = "Colegas observadores, com imensa alegria compartilho que localizamos uma cavidade reprodutiva ativa de Amazona vinacea em tronco oco de canela-preta na vertente oeste. O casal foi flagrado revezando visitas com frutos de palmito e pinhões.\n\nConforme as diretrizes éticas da nossa comunidade e da moderação do EcoCanta, as coordenadas GPS exatas não serão publicadas, mas o relato oficial já foi enviado para a equipe de monitoramento da Fundação Florestal!";
                t.Hashtags = "papagaiodepeitoroxo, ninhos, conservacao, aves, reproducao";
                t.CriadoEm = DateTime.Now.AddDays(-1);
            });

            GarantirTopico("Guia de escuta e identificação de cantos de aves da Cantareira", t =>
            {
                t.UsuarioId = guardiaoUser.Id;
                t.ComunidadeId = comAves.Id;
                t.Categoria = "comunidade";
                t.Conteudo = "Preparei uma lista com as 10 vocalizações mais comuns matinais na mata: Tangará (o estalo característico do salto), Saíra-sete-cores (o trinado agudo no topo de embaúba), Pica-pau-de-banda-branca e outros. Quem tiver gravações em áudio ou vídeos gravados nas trilhas, suba aqui no tópico ou vincule aos avistamentos!";
                t.Hashtags = "cantos, bioacustica, aves, identificacao, cantareira";
                t.CriadoEm = DateTime.Now.AddHours(-12);
            });

            // Tópicos em Guardiões dos Felinos & Fauna Rara
            GarantirTopico("Como diferenciar pegadas de jaguatirica e gato-do-mato nas trilhas lamacentas", t =>
            {
                t.UsuarioId = biologiaUser.Id;
                t.ComunidadeId = comFelinos.Id;
                t.Categoria = "comunidade";
                t.Conteudo = "Uma dúvida muito recorrente entre trilheiros é como separar rastros de Leopardus pardalis (jaguatirica) e Leopardus guttulus (gato-do-mato-pequeno).\n\n1. Diâmetro: pegadas de jaguatirica medem entre 4,5 cm e 6 cm de diâmetro (podendo chegar a 7-8 cm em pumas jovens), enquanto gatos-do-mato raramente ultrapassam 3,5 cm;\n2. Almofada plantar: felinos deixam 3 lóbulos bem demarcados na base posterior da almofada e NUNCA mostram marcas de unhas, ao contrário de canídeos silvestres (como o cachorro-do-mato);\n3. Passo e sobreposição: felinos tendem a pisar com as patas traseiras exatamente no mesmo local das dianteiras ao caminhar furtivamente.";
                t.Hashtags = "rastreamento, pegadas, jaguatirica, gatodomato, biologia";
                t.CriadoEm = DateTime.Now.AddDays(-2);
            });

            GarantirTopico("Registro de onça-parda em corredor ecológico no Núcleo Águas Claras", t =>
            {
                t.UsuarioId = biologiaUser.Id;
                t.ComunidadeId = comFelinos.Id;
                t.Categoria = "noticia";
                t.Conteudo = "Nossa armadilha fotográfica número 07 registrou às 03h14 da última terça-feira um macho adulto de Puma concolor em excelente condição física, transitando pela crista montanhosa que conecta Águas Claras ao limite de Mairiporã. Isso comprova a eficácia dos corredores florestais recuperados nos últimos anos!";
                t.Hashtags = "oncaparda, cameratrap, aguasclaras, corredorflorestal";
                t.CriadoEm = DateTime.Now.AddDays(-3);
            });

            var semComunidade = context.ForumTopicos.Where(t => t.ComunidadeId == null).ToList();
            foreach (var t in semComunidade)
            {
                t.ComunidadeId = comGeral.Id;
            }

            context.SaveChanges();
        }

        private static void SemearForumRespostas(AppDbContext context, Dictionary<string, Usuario> usuarios)
        {
            var adminUser = usuarios["admin@ecocanta.org"];
            var biologiaUser = usuarios["pesquisa@ecocanta.org"];
            var moderadorUser = usuarios["moderador@ecocanta.org"];
            var fernandaMod = usuarios["fernanda.mod@ecocanta.org"];
            var beatrizUser = usuarios["beatriz.jornalista@ecocanta.org"];
            var thiagoUser = usuarios["thiago.reporter@ecocanta.org"];
            var guardiaoUser = usuarios["guardiao@ecocanta.org"];

            void AdicionarRespostaSeNaoExistir(string tituloTopico, int usuarioId, string conteudo, DateTime criadoEm)
            {
                var topico = context.ForumTopicos.FirstOrDefault(t => t.Titulo == tituloTopico);
                if (topico == null) return;

                var trecho = conteudo.Length > 30 ? conteudo.Substring(0, 30) : conteudo;
                var existe = context.ForumRespostas.Any(r => r.ForumTopicoId == topico.Id && r.UsuarioId == usuarioId && r.Conteudo.Contains(trecho));
                if (!existe)
                {
                    context.ForumRespostas.Add(new ForumResposta
                    {
                        ForumTopicoId = topico.Id,
                        UsuarioId = usuarioId,
                        Conteudo = conteudo,
                        CriadoEm = criadoEm
                    });
                }
            }

            // Respostas em: "Operação 'Corta-Fogo 2026' entra em vigor..."
            AdicionarRespostaSeNaoExistir(
                "Operação 'Corta-Fogo 2026' entra em vigor para prevenção de queimadas de inverno",
                guardiaoUser.Id,
                "Excelente iniciativa! No ano passado as brigadas voluntárias foram fundamentais para conter focos perto do Cabuçu. Gostaria de saber quando começam os treinamentos presenciais para novos voluntários neste ano.",
                DateTime.Now.AddDays(-3).AddHours(2)
            );
            AdicionarRespostaSeNaoExistir(
                "Operação 'Corta-Fogo 2026' entra em vigor para prevenção de queimadas de inverno",
                biologiaUser.Id,
                "Olá Guardião! Os treinamentos práticos de combate a princípios de incêndio e abertura de aceiros estão programados para o próximo mês. Assim que o calendário oficial for homologado, abriremos uma Campanha aqui no site para inscrições!",
                DateTime.Now.AddDays(-3).AddHours(5)
            );
            AdicionarRespostaSeNaoExistir(
                "Operação 'Corta-Fogo 2026' entra em vigor para prevenção de queimadas de inverno",
                moderadorUser.Id,
                "Reforço de moderação: caso qualquer usuário identifique colunas de fumaça ou pessoas soltando balões na borda da serra, por favor façam o registro imediato na aba de Denúncias com coordenadas ou ponto de referência.",
                DateTime.Now.AddDays(-2)
            );

            // Respostas em: "Avistamento de pegadas frescas de felino na Trilha da Pedra Grande!"
            AdicionarRespostaSeNaoExistir(
                "Avistamento de pegadas frescas de felino na Trilha da Pedra Grande!",
                biologiaUser.Id,
                "Pela medida de aproximadamente 8 cm e pela nítida ausência de unhas na lama, a probabilidade de se tratar de uma jaguatirica (Leopardus pardalis) de porte avantajado ou uma onça-parda juvenil é altíssima! Já repassei as coordenadas fornecidas para a equipe de campo checar as armadilhas fotográficas da bifurcação.",
                DateTime.Now.AddHours(-16)
            );
            AdicionarRespostaSeNaoExistir(
                "Avistamento de pegadas frescas de felino na Trilha da Pedra Grande!",
                fernandaMod.Id,
                "Aviso da moderação aos visitantes: os felinos da Mata Atlântica são esquivos e costumam se afastar com o som de conversas e passos. Porém, lembrem-se de nunca subir trilhas desacompanhados de madrugada e não saiam das áreas sinalizadas.",
                DateTime.Now.AddHours(-14)
            );
            AdicionarRespostaSeNaoExistir(
                "Avistamento de pegadas frescas de felino na Trilha da Pedra Grande!",
                thiagoUser.Id,
                "Que relato fantástico! Estou finalizando uma matéria investigativa sobre a recuperação dos corredores florestais da Cantareira e gostaria muito de conversar com o autor do avistamento para incluir os detalhes do encontro no texto.",
                DateTime.Now.AddHours(-11)
            );

            // Respostas em: "Qual o melhor núcleo para observação de aves (birdwatching)..."
            AdicionarRespostaSeNaoExistir(
                "Qual o melhor núcleo para observação de aves (birdwatching) aos finais de semana?",
                beatrizUser.Id,
                "Olá! Recomendo de olhos fechados o Núcleo Águas Claras bem cedinho (por volta das 6h30 / 7h). O silêncio na Trilha da Suçuarana é maravilhoso e nas embaúbas e ingás você consegue avistar bandos de saíra-sete-cores, tangarás e pica-paus com muita facilidade!",
                DateTime.Now.AddHours(-8)
            );
            AdicionarRespostaSeNaoExistir(
                "Qual o melhor núcleo para observação de aves (birdwatching) aos finais de semana?",
                moderadorUser.Id,
                "Dica complementar: no Núcleo Engordador, próximo ao lago da antiga represa de abastecimento, é muito comum observar garças, martins-pescadores e pequenos maçaricos nas primeiras horas do amanhecer.",
                DateTime.Now.AddHours(-5)
            );
            AdicionarRespostaSeNaoExistir(
                "Qual o melhor núcleo para observação de aves (birdwatching) aos finais de semana?",
                guardiaoUser.Id,
                "Muito obrigado pelas orientações, Beatriz e Carlos! Já preparei o binóculo e o repelente e vou levar a família no sábado cedo para Águas Claras.",
                DateTime.Now.AddHours(-2)
            );

            // Respostas em: "Primeiro registro de ninho ativo de Papagaio-de-peito-roxo..."
            AdicionarRespostaSeNaoExistir(
                "Primeiro registro de ninho ativo de Papagaio-de-peito-roxo na temporada 2026",
                biologiaUser.Id,
                "Registro monumental, Beatriz! A equipe do laboratório de ornitologia da USP vai acompanhar essa cavidade quinzenalmente para registrar a eclosão dos filhotes. Parabéns pelo cuidado em manter o sigilo de geolocalização.",
                DateTime.Now.AddHours(-18)
            );
            AdicionarRespostaSeNaoExistir(
                "Primeiro registro de ninho ativo de Papagaio-de-peito-roxo na temporada 2026",
                moderadorUser.Id,
                "Nota da Moderação: postagem fixada na comunidade de observadores! Solicitamos a todos os membros que continuem preservando locais de reprodução para mantermos nossa fauna protegida contra o tráfico de aves silvestres.",
                DateTime.Now.AddHours(-12)
            );
            AdicionarRespostaSeNaoExistir(
                "Primeiro registro de ninho ativo de Papagaio-de-peito-roxo na temporada 2026",
                guardiaoUser.Id,
                "Que foto linda e emocionante! É um privilégio ver uma espécie criticamente ameaçada encontrando refúgio seguro nas árvores centenárias da Cantareira.",
                DateTime.Now.AddHours(-4)
            );

            // Respostas em: "Como diferenciar pegadas de jaguatirica e gato-do-mato..."
            AdicionarRespostaSeNaoExistir(
                "Como diferenciar pegadas de jaguatirica e gato-do-mato nas trilhas lamacentas",
                guardiaoUser.Id,
                "Explicação claríssima, Dr. André! Eu sempre tinha dúvida quanto às unhas, agora entendi perfeitamente que se tiver marca de garras na terra é cachorro-do-mato ou cão doméstico.",
                DateTime.Now.AddDays(-1)
            );
            AdicionarRespostaSeNaoExistir(
                "Como diferenciar pegadas de jaguatirica e gato-do-mato nas trilhas lamacentas",
                beatrizUser.Id,
                "Salvo na minha pasta de referências de campo! Material riquíssimo para quem cobre o meio ambiente.",
                DateTime.Now.AddHours(-8)
            );

            context.SaveChanges();
        }

        private static void SemearEstudosCientificos(AppDbContext context, Dictionary<string, Usuario> usuarios)
        {
            var adminUser = usuarios["admin@ecocanta.org"];
            var biologiaUser = usuarios["pesquisa@ecocanta.org"];

            void GarantirEstudo(string slug, Action<EstudoCientifico> config)
            {
                var estudo = context.EstudosCientificos.FirstOrDefault(e => e.Slug == slug);
                if (estudo == null)
                {
                    estudo = new EstudoCientifico { Slug = slug };
                    config(estudo);
                    context.EstudosCientificos.Add(estudo);
                }
                else
                {
                    config(estudo);
                }
            }

            // 1. Estudo Pendente 1 (Para teste de aprovação de estudos)
            GarantirEstudo("monitoramento-acustico-primatas-engordador", e =>
            {
                e.Titulo = "Monitoramento Acústico Passivo de Primatas no Núcleo Engordador";
                e.Subtitulo = "Uso de gravadores autônomos para avaliar densidade e ritmos diários de vocalização de Alouatta guariba clamitans";
                e.Categoria = "Mamíferos";
                e.EspecieNome = "Bugio-ruivo";
                e.StatusConservacao = "Vulnerável";
                e.Autor = "Dr. Rodrigo Silveira e Equipe LAB-ECO";
                e.Instituicao = "Instituto de Biociências - Universidade de São Paulo (USP)";
                e.TempoLeituraMinutos = 8;
                e.StatusAprovacao = "Pendente";
                e.UsuarioId = biologiaUser.Id;
                e.DataSubmissao = DateTime.Now.AddDays(-3);
                e.ImagemUrl = "/img/especies/bugio-ruivo.jpg";
                e.Resumo = "Este estudo empregou 12 sensores acústicos autônomos distribuídos em gradiente altitudinal no Núcleo Engordador durante 6 meses para registrar e analisar a densidade e o ritmo bioacústico de tropas de bugios-ruivos após os históricos surtos de febre amarela.";
                e.ConteudoHtml = "<p>A bioacústica passiva provou ser indispensável para monitorar primatas neotropicais de dossel em relevos acidentados como a Serra da Cantareira...</p><p>Foram detectados 8 grupos territoriais distintos e picos de vocalização entre 05h45 e 07h15, demonstrando expressiva recuperação populacional na vertente leste.</p>";
                e.DestaquesTexto = "Detecção contínua de 8 tropas residentes com presença de filhotes de primeiro ano\nPicos sonoros concentrados nas primeiras duas horas pós-alvorecer\nSem isolamento genético nos corredores de mata ciliar mapeados";
                e.AmeacasTexto = "Eletrocussão em fiações elétricas desprotegidas no entorno urbano\nAtropelamentos na Estrada da Roseira e rodovias limítrofes\nFragmentação florestal gerada por parcelamento irregular de solo";
                e.AcoesTexto = "Manutenção periódica e instalação de novas pontes de dossel aéreas\nPrograma permanente de vigilância epidemiológica e imunização\nCriação de novos corredores ecológicos conectando os núcleos da serra";
                e.ReferenciasTexto = "Silveira, R. et al. (2025). Bioacoustics of Howler Monkeys in Atlantic Forest remnants. J. Primatol.\nFundação Florestal (2024). Relatório de Monitoramento de Mastofauna da Cantareira.";
            });

            // 2. Estudo Pendente 2 (Para teste de aprovação de estudos)
            GarantirEstudo("inventario-fitossanidade-arborea-cantareira", e =>
            {
                e.Titulo = "Inventário e Fitossanidade de Espécies Arbóreas Raras na Cantareira";
                e.Subtitulo = "Levantamento florístico de cedro-rosa, canela-preta e palmito-juçara em áreas sob restauração ativa";
                e.Categoria = "Flora & Ecossistema";
                e.EspecieNome = "Cedrela fissilis (Cedro-rosa)";
                e.StatusConservacao = "Vulnerável";
                e.Autor = "MSc. Camila Duarte";
                e.Instituicao = "Instituto Florestal / Fundação Florestal de SP";
                e.TempoLeituraMinutos = 6;
                e.StatusAprovacao = "Pendente";
                e.UsuarioId = adminUser.Id;
                e.DataSubmissao = DateTime.Now.AddDays(-1);
                e.ImagemUrl = "/img/campanhas/reflorestamento.jpg";
                e.Resumo = "Avaliação da regeneração natural e taxas de sobrevivência de plântulas de espécies arbóreas clímax em 40 parcelas experimentais após erradicação de gramíneas invasoras nos núcleos Cabuçu e Águas Claras.";
                e.ConteudoHtml = "<p>O estudo avaliou 40 parcelas permanentes de 10x10m ao longo de dois anos após o manejo adaptativo de espécies invasoras...</p><p>Constatou-se uma taxa de regeneração 4 vezes superior nas áreas com sombreamento rápido proporcionado por pioneiras nativas.</p>";
                e.DestaquesTexto = "Mais de 1.200 indivíduos juvenis catalogados com georreferenciamento\nTaxa de sobrevivência de 88% para Euterpe edulis sob dossel florestal preservado\nRedução drástica de gramíneas exóticas competidoras pós-enriquecimento";
                e.AmeacasTexto = "Pressão de coleta clandestina de palmito-juçara nativo\nIncêndios rasteiros durante estiagens severas de inverno\nEscassez de dispersores de sementes de grande porte";
                e.AcoesTexto = "Fiscalização preventiva ostensiva com sobrevoos de drones térmicos\nEnriquecimento de clareiras com sementes de árvores zoocóricas nativas\nEnvolvimento comunitário no cultivo de mudas em viveiros locais";
                e.ReferenciasTexto = "Duarte, C. (2025). Sucessão Florestal e Manejo de Espécies Ameaçadas na Mata Atlântica. Ed. Bioma.";
            });

            // 3. Estudo Aprovado (Referência)
            GarantirEstudo("dinamica-populacional-onca-parda-cantareira", e =>
            {
                e.Titulo = "Dinâmica Populacional e Padrões de Movimentação da Onça-Parda na Serra da Cantareira";
                e.Subtitulo = "Estimativa de densidade, uso de habitat e conectividade da paisagem para Puma concolor na maior floresta urbana do mundo";
                e.Categoria = "Mamíferos";
                e.EspecieNome = "Onça-parda";
                e.StatusConservacao = "Vulnerável";
                e.Autor = "Dr. André Biólogo & Equipe EcoCanta";
                e.Instituicao = "Instituto de Pesquisas Ambientais (IPA) & Parque Estadual da Cantareira";
                e.TempoLeituraMinutos = 10;
                e.StatusAprovacao = "Aprovado";
                e.UsuarioId = biologiaUser.Id;
                e.DataSubmissao = DateTime.Now.AddMonths(-2);
                e.DataResposta = DateTime.Now.AddMonths(-2).AddDays(4);
                e.ObservacoesAdmin = "Metodologia exemplar e conformidade total com os parâmetros de publicação científica do portal.";
                e.ImagemUrl = "/img/especies/onca-parda.jpg";
                e.Resumo = "Análise integrada de 48 armadilhas fotográficas e 120 registros de vestígios ao longo de 2 anos, revelando uma população residente estimada entre 14 e 22 indivíduos adultos de onça-parda em toda a extensão do maciço da Cantareira.";
                e.ConteudoHtml = "<p>A presença de um predador de topo como a onça-parda (Puma concolor) na borda da maior metrópole da América do Sul é um indicativo de integridade ecossistêmica...</p>";
                e.DestaquesTexto = "População residente estimada entre 14 e 22 espécimes adultos na serra\nUso preferencial de grotas preservadas e florestas secundárias tardias\nConectividade funcional confirmada em direção à bacia de Nazaré Paulista";
                e.AmeacasTexto = "Atropelamentos na Rodovia Fernão Dias e Estrada da Roseira\nConflito com criadores de animais no entorno rural\nRedução de presas devido à caça de catetos e pacas";
                e.AcoesTexto = "Implantação de cercamento condutor e passagens inferiores de fauna\nOficinas educativas com proprietários rurais do entorno\nIntensificação de patrulhas integradas da Polícia Militar Ambiental";
                e.ReferenciasTexto = "Biólogo, A. et al. (2024). Ecology and Movement of Pumas in Peri-Urban Atlantic Forest. Conservation Science.";
            });

            context.SaveChanges();
        }

        private static void SemearDenuncias(AppDbContext context)
        {
            void GarantirDenuncia(string localizacao, Action<Denuncia> config)
            {
                var den = context.Denuncias.FirstOrDefault(d => d.Localizacao == localizacao);
                if (den == null)
                {
                    den = new Denuncia { Localizacao = localizacao };
                    config(den);
                    context.Denuncias.Add(den);
                }
                else
                {
                    config(den);
                }
            }

            // 1. Denúncia Pendente 1: Descarte irregular de lixo
            GarantirDenuncia("Estrada da Roseira, km 8 - Próximo à ponte do Córrego da Roseira", d =>
            {
                d.Tipo = "Descarte Irregular de Lixo";
                d.DataOcorrencia = DateTime.Today.AddDays(-2);
                d.Descricao = "Caminhão basculante descarregou grande quantidade de entulho de construção civil, gesso, restos de alvenaria e galões plásticos na margem da estrada, deslizando pela encosta de mata ciliar de preservação permanente.";
                d.NomeContato = "Marcos Vinicius";
                d.EmailContato = "marcos.v@email.com";
                d.Anonima = false;
                d.Resolvida = false;
                d.FotoPath = "/img/campanhas/limpeza-trilhas.jpg";
                d.CriadoEm = DateTime.Now.AddDays(-2);
            });

            // 2. Denúncia Pendente 2: Supressão vegetal e motosserra
            GarantirDenuncia("Núcleo Cabuçu, limite perimetral com Jardim Monte Alegre", d =>
            {
                d.Tipo = "Desmatamento / Degradação";
                d.DataOcorrencia = DateTime.Today.AddDays(-4);
                d.Descricao = "Motosserra e maquinário pesado em operação aos finais de semana promovendo abertura de clareira não autorizada, com corte de árvores nativas de grande porte e palmito-juçara em área de amortecimento florestal.";
                d.NomeContato = "Morador Anônimo";
                d.EmailContato = null;
                d.Anonima = true;
                d.Resolvida = false;
                d.FotoPath = "/img/campanhas/reflorestamento.jpg";
                d.CriadoEm = DateTime.Now.AddDays(-4);
            });

            // 3. Denúncia Pendente 3: Acampamento com fogueira e risco de incêndio
            GarantirDenuncia("Área de amortecimento da Pedra Grande - Trilha secundária não sinalizada", d =>
            {
                d.Tipo = "Incêndio / Queimada";
                d.DataOcorrencia = DateTime.Today.AddHours(-10);
                d.Descricao = "Restos de acampamento clandestino com fogueira mal apagada e brasas ativas encontradas sob espessa camada de folhas secas ao lado da trilha. Vento moderado no local gerando alto risco de alastramento de chamas para a mata fechada.";
                d.NomeContato = "Guardião da Trilha";
                d.EmailContato = "guardiao@ecocanta.org";
                d.Anonima = false;
                d.Resolvida = false;
                d.FotoPath = null;
                d.CriadoEm = DateTime.Now.AddHours(-10);
            });

            // 4. Denúncia Resolvida (Para histórico e contraste de filtros)
            GarantirDenuncia("Córrego do Bispo - Zona Norte de São Paulo", d =>
            {
                d.Tipo = "Contaminação de Água";
                d.DataOcorrencia = DateTime.Today.AddDays(-20);
                d.Descricao = "Despejo contínuo de efluentes líquidos com coloração avermelhada e forte odor químico diretamente em afluente do Córrego do Bispo que deságua na bacia de captação.";
                d.NomeContato = "Associação de Moradores da Cantareira";
                d.EmailContato = "contato@moradorescantareira.org";
                d.Anonima = false;
                d.Resolvida = true;
                d.ObservacaoAdmin = "Fiscalização realizada conjuntamente com fiscais da CETESB e DAVE. Ponto de descarte clandestino autuado, lacrado e processo sancionador instaurado.";
                d.CriadoEm = DateTime.Now.AddDays(-20);
            });

            context.SaveChanges();
        }

        private static void SemearFaqTickets(AppDbContext context, Dictionary<string, Usuario> usuarios)
        {
            var moderadorUser = usuarios["moderador@ecocanta.org"];
            var thiagoUser = usuarios["thiago.reporter@ecocanta.org"];
            var guardiaoUser = usuarios["guardiao@ecocanta.org"];

            void GarantirTicket(string assunto, Action<FaqTicket> config, Action<FaqTicket> configMensagens)
            {
                var ticket = context.FaqTickets.FirstOrDefault(t => t.Assunto == assunto);
                if (ticket == null)
                {
                    ticket = new FaqTicket { Assunto = assunto };
                    config(ticket);
                    context.FaqTickets.Add(ticket);
                    context.SaveChanges();
                    configMensagens(ticket);
                    context.SaveChanges();
                }
            }

            // Ticket 1: Aberto (Dúvida sobre jornalista)
            GarantirTicket(
                "Dúvida sobre homologação de credencial de jornalista",
                t =>
                {
                    t.UsuarioId = thiagoUser.Id;
                    t.Categoria = "Geral";
                    t.Status = "Aberto";
                    t.CriadoEm = DateTime.Now.AddDays(-2);
                    t.AtualizadoEm = DateTime.Now.AddDays(-2);
                },
                ticket =>
                {
                    context.FaqMensagens.Add(new FaqMensagem
                    {
                        FaqTicketId = ticket.Id,
                        UsuarioId = thiagoUser.Id,
                        Conteudo = "Olá moderação e administração do EcoCanta! Submeti meus dados, registro profissional de imprensa e portfólio de matérias ambientais para a credencial de jornalista há dois dias. Poderiam me orientar sobre o prazo médio de homologação para que eu possa começar a cobrir as ações comunitárias e publicar no fórum? Muito obrigado!",
                        CriadoEm = DateTime.Now.AddDays(-2),
                        IsEquipeModOuAdmin = false,
                        CargoAutor = "Candidato a Jornalista"
                    });
                }
            );

            // Ticket 2: Aberto (Dúvida sobre avistamentos com mídia)
            GarantirTicket(
                "Como anexar múltiplos vídeos nos avistamentos de fauna?",
                t =>
                {
                    t.UsuarioId = guardiaoUser.Id;
                    t.Categoria = "Avistamentos";
                    t.Status = "Aberto";
                    t.CriadoEm = DateTime.Now.AddHours(-14);
                    t.AtualizadoEm = DateTime.Now.AddHours(-14);
                },
                ticket =>
                {
                    context.FaqMensagens.Add(new FaqMensagem
                    {
                        FaqTicketId = ticket.Id,
                        UsuarioId = guardiaoUser.Id,
                        Conteudo = "Boa noite equipe! Consegui filmar um bando de bugios-ruivos vocalizando no dossel da Trilha do Engordador em três takes curtos de cerca de 15 segundos cada. Consigo anexar os três vídeos no mesmo avistamento ou existe limite de tamanho e quantidade de mídias por envio?",
                        CriadoEm = DateTime.Now.AddHours(-14),
                        IsEquipeModOuAdmin = false,
                        CargoAutor = "Guardião"
                    });
                }
            );

            // Ticket 3: Respondido / Atendido (Histórico)
            GarantirTicket(
                "Como propor uma nova campanha comunitária de preservação?",
                t =>
                {
                    t.UsuarioId = guardiaoUser.Id;
                    t.Categoria = "Campanhas";
                    t.Status = "Respondido";
                    t.CriadoEm = DateTime.Now.AddDays(-7);
                    t.AtualizadoEm = DateTime.Now.AddDays(-6);
                    t.UltimaRespostaPor = "Carlos Eduardo (Moderador)";
                },
                ticket =>
                {
                    context.FaqMensagens.AddRange(
                        new FaqMensagem
                        {
                            FaqTicketId = ticket.Id,
                            UsuarioId = guardiaoUser.Id,
                            Conteudo = "Olá! Tenho interesse em propor um mutirão voluntário de plantio e limpeza junto à comunidade escolar do nosso bairro. Quais são os critérios necessários para submeter uma nova proposta de campanha?",
                            CriadoEm = DateTime.Now.AddDays(-7),
                            IsEquipeModOuAdmin = false,
                            CargoAutor = "Guardião"
                        },
                        new FaqMensagem
                        {
                            FaqTicketId = ticket.Id,
                            UsuarioId = moderadorUser.Id,
                            Conteudo = "Olá Guardião! Para propor novas campanhas comunitárias você precisa ser jornalista aprovado, ter ao menos 15 estudos aprovados no site ou ter alcançado o Nível 4 na sua jornada. Além disso, criadores de comunidades aprovadas no fórum também podem vincular campanhas às suas comunidades. Uma vez submetida, nossa equipe de moderação e administração avalia a proposta na fila de aprovações. Conte conosco para o que precisar!",
                            CriadoEm = DateTime.Now.AddDays(-6),
                            IsEquipeModOuAdmin = true,
                            CargoAutor = "Moderador"
                        }
                    );
                }
            );

            context.SaveChanges();

            GarantirMidiasComVideoNosAvistamentos(context);
        }

        private static void GarantirMidiasComVideoNosAvistamentos(AppDbContext context)
        {
            var avistamentos = context.Avistamentos.Include(a => a.Midias).ToList();
            if (avistamentos.Count == 0) return;

            var onca = avistamentos.FirstOrDefault(a => a.NomeAnimal.Contains("Onça") || a.NomeAnimal.Contains("Puma"));
            if (onca != null && !onca.Midias.Any(m => m.Tipo == "Video"))
            {
                if (!onca.Midias.Any(m => m.Tipo == "Foto" && m.Caminho == onca.FotoPath) && !string.IsNullOrEmpty(onca.FotoPath))
                {
                    onca.Midias.Add(new AvistamentoMidia { Caminho = onca.FotoPath, Tipo = "Foto", CriadoEm = onca.CriadoEm });
                }
                onca.Midias.Add(new AvistamentoMidia
                {
                    Caminho = "/videos/onca-parda-trilha.ogv",
                    Tipo = "Video",
                    CriadoEm = onca.CriadoEm.AddMinutes(5)
                });
            }

            var bugio = avistamentos.FirstOrDefault(a => a.NomeAnimal.Contains("Bugio"));
            if (bugio != null && !bugio.Midias.Any(m => m.Tipo == "Video"))
            {
                if (!bugio.Midias.Any(m => m.Tipo == "Foto" && m.Caminho == bugio.FotoPath) && !string.IsNullOrEmpty(bugio.FotoPath))
                {
                    bugio.Midias.Add(new AvistamentoMidia { Caminho = bugio.FotoPath, Tipo = "Foto", CriadoEm = bugio.CriadoEm });
                }
                bugio.Midias.Add(new AvistamentoMidia
                {
                    Caminho = "/videos/bugio-vocalizacao.ogv",
                    Tipo = "Video",
                    CriadoEm = bugio.CriadoEm.AddMinutes(3)
                });
            }

            var ave = avistamentos.FirstOrDefault(a => a.NomeAnimal.Contains("Saíra") || a.NomeAnimal.Contains("Papagaio"));
            if (ave != null && !ave.Midias.Any(m => m.Tipo == "Video"))
            {
                if (!ave.Midias.Any(m => m.Tipo == "Foto" && m.Caminho == ave.FotoPath) && !string.IsNullOrEmpty(ave.FotoPath))
                {
                    ave.Midias.Add(new AvistamentoMidia { Caminho = ave.FotoPath, Tipo = "Foto", CriadoEm = ave.CriadoEm });
                }
                ave.Midias.Add(new AvistamentoMidia
                {
                    Caminho = "/videos/tangara-avistamento.webm",
                    Tipo = "Video",
                    CriadoEm = ave.CriadoEm.AddMinutes(2)
                });
            }

            context.SaveChanges();
        }
    }
}
