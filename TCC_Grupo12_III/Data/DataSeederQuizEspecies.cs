using System.Collections.Generic;
using System.Linq;
using TCC_Grupo12_III.Models;

namespace TCC_Grupo12_III.Data
{
    public static class DataSeederQuizEspecies
    {
        public static void Seed(AppDbContext context)
        {
            if (context.EspecieQuizPerguntas.Any())
            {
                return;
            }

            var ids = context.Especies.ToDictionary(e => e.Nome, e => e.Id);
            var lista = new List<EspecieQuizPergunta>();

            void Add(string especie, string enunciado, string a, string b, string c, string d, int correta)
            {
                if (!ids.TryGetValue(especie, out var id))
                {
                    return;
                }

                var ordem = lista.Count(p => p.EspecieId == id) + 1;
                lista.Add(new EspecieQuizPergunta
                {
                    EspecieId = id,
                    Enunciado = enunciado,
                    OpcaoA = a,
                    OpcaoB = b,
                    OpcaoC = c,
                    OpcaoD = d,
                    RespostaCorreta = correta,
                    Ordem = ordem
                });
            }

            Add("Onça-parda", "Qual é o nome científico da onça-parda?", "Panthera onca", "Puma concolor", "Leopardus pardalis", "Pecari tajacu", 2);
            Add("Onça-parda", "Do que a onça-parda se alimenta?", "Apenas frutos", "Folhas e brotos", "Veados, capivaras, tatus e pacas", "Somente insetos", 3);
            Add("Onça-parda", "Qual é uma das principais ameaças à espécie?", "Excesso de chuva", "Desmatamento e fragmentação do habitat", "Falta de sol", "Poluição sonora apenas", 2);

            Add("Papagaio-de-peito-roxo", "Qual é o status de conservação do papagaio-de-peito-roxo?", "Pouco preocupante", "Quase ameaçado", "Criticamente ameaçado", "Extinto", 3);
            Add("Papagaio-de-peito-roxo", "Do que ele se alimenta?", "Frutos, sementes e pinhões de araucária", "Peixes", "Pequenos mamíferos", "Néctar apenas", 1);
            Add("Papagaio-de-peito-roxo", "Qual ameaça está ligada ao tráfico de animais silvestres?", "Chuva ácida", "Captura ilegal de aves", "Excesso de predadores", "Falta de água", 2);

            Add("Jaguatirica", "Qual é o nome científico da jaguatirica?", "Leopardus pardalis", "Puma concolor", "Alouatta guariba", "Tangara seledon", 1);
            Add("Jaguatirica", "Qual é o hábito predominante da jaguatirica?", "Diurno", "Noturno", "Aquático", "Vive em bandos", 2);
            Add("Jaguatirica", "Para que serve a pelagem manchada?", "Atrair presas", "Camuflagem na mata", "Suportar o frio", "Marcar território", 2);

            Add("Cateto", "O cateto costuma viver...", "Sozinho o tempo todo", "Em grupos familiares", "Somente em árvores", "Em cavernas", 2);
            Add("Cateto", "Qual é a alimentação do cateto?", "Onívora: raízes, frutos, sementes e pequenos invertebrados", "Somente carne", "Somente folhas", "Somente peixes", 1);
            Add("Cateto", "Qual é o nome científico do cateto?", "Pecari tajacu", "Puma concolor", "Amazona vinacea", "Leopardus pardalis", 1);

            Add("Saíra-sete-cores", "A saíra-sete-cores é uma...", "Mamífero", "Ave", "Réptil", "Anfíbio", 2);
            Add("Saíra-sete-cores", "Em qual bioma ela vive?", "Mata Atlântica", "Deserto", "Pampa", "Tundra", 1);
            Add("Saíra-sete-cores", "O que a torna tão conhecida?", "Seu rugido", "Sua plumagem multicolorida", "Seu tamanho enorme", "Seu hábito noturno", 2);

            Add("Bugio-ruivo", "Pelo que o bugio-ruivo é conhecido?", "Nadar longas distâncias", "Rugido potente que ecoa pela mata", "Construir ninhos no chão", "Voar", 2);
            Add("Bugio-ruivo", "Qual é a base da alimentação do bugio?", "Folhas, brotos e frutos", "Carne", "Peixes", "Insetos apenas", 1);
            Add("Bugio-ruivo", "Qual doença representa grande ameaça às suas populações?", "Febre amarela silvestre", "Gripe aviária", "Raiva bovina", "Dengue", 1);

            context.EspecieQuizPerguntas.AddRange(lista);
            context.SaveChanges();
        }
    }
}
