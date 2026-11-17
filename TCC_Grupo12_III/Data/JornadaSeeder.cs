using TCC_Grupo12_III.Models;
namespace TCC_Grupo12_III.Data;
public static class JornadaSeeder {
 public static void Seed(AppDbContext db) {
  if(db.JornadaPerguntas.Any())return;
  var especies=db.Especies.OrderBy(e=>e.Id).Take(14).ToList();
  foreach(var e in especies) {
   var outras=especies.Where(x=>x.Id!=e.Id).Take(3).Select(x=>x.Nome).ToList();
   if(outras.Count<3)continue;
   var opcoes=outras.Append(e.Nome).OrderBy(_=>Random.Shared.Next()).ToList();
   db.JornadaPerguntas.Add(new JornadaPergunta{Desafio=1,Enunciado=$"Observe a fotografia e as pistas da ficha: {e.Caracteristicas} Qual espécie corresponde a essas características?",ImagemUrl=e.ImagemUrl,
   OpcaoA=opcoes[0],OpcaoB=opcoes[1],OpcaoC=opcoes[2],OpcaoD=opcoes[3],Correta=opcoes.IndexOf(e.Nome)+1,
   Explicacao=$"A espécie é {e.Nome} ({e.NomeCientifico}). Na ficha educativa, seu habitat é descrito assim: {e.Habitat} Conhecer o ambiente em que vive ajuda a reconhecer o que precisa ser preservado.",
   Fonte="Ficha educativa original do Grupo 12. As referências científicas e a seleção final das espécies dependem de revisão editorial."});
  }
  var guia="ICMBio — Guia de conduta consciente em ambientes naturais. https://www.gov.br/icmbio/pt-br/centrais-de-conteudo/guia2-pdf";
  var ibama="Ibama — Apresentar denúncia ambiental. https://www.gov.br/pt-br/servicos/apresentar-denuncia-ambiental";
  var tcc="Grupo 12 — documentação do TCC, seção 2.2 (ameaças à fauna e fragmentação de habitats).";
  var situacoes=new (string Q,string A,string B,string C,string D,string E,string F)[]{
   ("Uma ave se aproxima durante a visita. Qual atitude favorece a observação responsável?","Observar a distância, sem oferecer alimento.","Oferecer um lanche para aproximá-la.","Tentar segurá-la para fotografar.","Persegui-la até seu ninho.","Alimentar ou perseguir interfere no comportamento dos animais. A observação responsável respeita sua distância.",guia),
   ("Você termina um lanche na trilha. O que fazer com a embalagem?","Levá-la de volta para descarte adequado.","Escondê-la sob as folhas.","Deixá-la perto de uma árvore.","Jogá-la em um córrego.","Resíduos devem retornar com o visitante. Retirar o lixo evita sua permanência na floresta.",guia),
   ("Um colega propõe sair da trilha para chegar perto de um animal. Como agir?","Permanecer na trilha e respeitar a orientação da visita.","Abrir uma nova trilha com o grupo.","Cercar o animal para ele não fugir.","Segui-lo em silêncio até seu abrigo.","Permanecer no percurso autorizado reduz a interferência da visita no ambiente e respeita as regras da unidade.",guia),
   ("Uma fotografia ficaria melhor se o animal fosse tocado. Qual decisão tomar?","Fotografar sem tocar nem interferir.","Pegar o animal por alguns segundos.","Usar comida para atraí-lo.","Mover seu abrigo para melhorar a luz.","A fotografia deve respeitar a vida silvestre. A aproximação não justifica tocar ou modificar o abrigo.",guia),
   ("Você encontra um anúncio de venda ilegal de fauna. Qual é a atitude responsável?","Buscar o canal oficial do órgão ambiental para relatar a situação.","Comprar para depois soltar por conta própria.","Compartilhar o anúncio para conseguir compradores.","Marcar um encontro e enfrentar o vendedor.","A comunicação deve ser encaminhada ao órgão competente pelos canais oficiais, com segurança. Comprar animais mantém a demanda pelo comércio ilegal.",ibama),
   ("Durante uma visita, o grupo percebe uma possível infração ambiental. Como proceder?","Informar um responsável e consultar o canal oficial de denúncia.","Confrontar as pessoas envolvidas.","Publicar acusações e dados pessoais.","Investigar sozinho uma área isolada.","A EcoCanta orienta o estudante a procurar responsáveis e autoridades. O canal oficial permite informar fatos para análise pelo órgão competente.",ibama),
   ("Uma área de mata foi dividida por construções. Qual consequência deve ser debatida?","O isolamento de habitats e a dificuldade de deslocamento da fauna.","A garantia de mais alimento para todas as espécies.","A eliminação de todas as ameaças.","A independência dos animais em relação à vegetação.","A fragmentação pode isolar populações e interromper deslocamentos. Preservar áreas conectadas integra a proteção dos habitats.",tcc),
   ("Uma campanha discute atropelamentos de fauna. Qual proposta merece estudo pelos responsáveis?","Medidas de proteção da fauna e de conectividade no planejamento viário.","Atrair animais para a estrada com comida.","Transferir animais sem acompanhamento especializado.","Aumentar a circulação de carros perto dos abrigos.","Atropelamentos aparecem entre as ameaças abordadas pelo grupo. A prevenção exige planejamento técnico e proteção dos locais de passagem.",tcc),
   ("Por que preservar uma floresta ajuda aves e animais terrestres?","Porque abriga recursos e ambientes de que diferentes espécies dependem.","Porque todas as espécies vivem do mesmo modo.","Porque animais não dependem de habitat.","Porque basta proteger somente as espécies maiores.","O habitat reúne condições necessárias à vida. Proteger o ambiente favorece relações entre espécies e a biodiversidade.",tcc),
   ("Uma atividade pede para relacionar desmatamento e fauna. Qual explicação faz sentido?","A perda de vegetação pode reduzir habitats e recursos disponíveis.","O desmatamento garante abrigo para toda a fauna.","As espécies deixam de precisar de alimento.","A perda da floresta não se relaciona com os animais.","A retirada de vegetação altera o habitat. Reconhecer essa relação ajuda o estudante a compreender a finalidade das ações de preservação.",tcc)
  };
  for(var i=0;i<situacoes.Length;i++){
   var s=situacoes[i];var opcoes=new[]{s.A,s.B,s.C,s.D}.OrderBy(_=>Random.Shared.Next()).ToArray();
   db.JornadaPerguntas.Add(new JornadaPergunta{Desafio=2,Enunciado=s.Q,OpcaoA=opcoes[0],OpcaoB=opcoes[1],OpcaoC=opcoes[2],OpcaoD=opcoes[3],Correta=Array.IndexOf(opcoes,s.A)+1,Explicacao=s.E,Fonte=s.F});
  }
  // A missão integra aves e terrestres existentes; não cria registros de espécies.
  foreach(var e in especies.GroupBy(e=>e.Categoria=="Aves"?"Aves":"Terrestres").SelectMany(g=>g.Take(2))) {
   var outras=especies.Where(x=>x.Id!=e.Id).Take(3).Select(x=>x.Nome).ToList();if(outras.Count<3)continue;
   var opcoes=outras.Append(e.Nome).OrderBy(_=>Random.Shared.Next()).ToArray();
   db.JornadaPerguntas.Add(new JornadaPergunta{Desafio=3,Enunciado=$"Na observação de campo, associe a espécie ao habitat descrito na ficha: {e.Habitat} Pista: {e.Caracteristicas}",ImagemUrl=e.ImagemUrl,OpcaoA=opcoes[0],OpcaoB=opcoes[1],OpcaoC=opcoes[2],OpcaoD=opcoes[3],Correta=Array.IndexOf(opcoes,e.Nome)+1,Explicacao=$"Trata-se de {e.Nome}. Ameaças registradas na ficha: {e.Ameacas} Reconhecer a espécie e o habitat permite discutir as ações necessárias à sua preservação.",Fonte="Ficha educativa original do Grupo 12; revisão científica pendente."});
  }
  foreach(var i in new[]{0,1,4,6,8,9}) {
   var s=situacoes[i];var opcoes=new[]{s.A,s.B,s.C,s.D}.OrderBy(_=>Random.Shared.Next()).ToArray();
   db.JornadaPerguntas.Add(new JornadaPergunta{Desafio=3,Enunciado="Missão de campo — "+s.Q,OpcaoA=opcoes[0],OpcaoB=opcoes[1],OpcaoC=opcoes[2],OpcaoD=opcoes[3],Correta=Array.IndexOf(opcoes,s.A)+1,Explicacao=s.E,Fonte=s.F});
  }
  db.SaveChanges();
 }
}
