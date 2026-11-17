using System.Text.Json;
using Microsoft.Data.Sqlite;
class Diagnostico {
 public static void Run(string root,bool restaurar=false){
 var atual=Path.Combine(root,"TCC_Grupo12_III","app.db");
 var backup=Directory.GetFiles(Path.Combine(root,"backups"),"antes-migracao-*.db").Order().FirstOrDefault()??throw new Exception("Backup consistente não encontrado.");
 using var db=new SqliteConnection($"Data Source={atual};Mode={(restaurar ? "ReadWrite" : "ReadOnly")}");db.Open();using(var attach=db.CreateCommand()){attach.CommandText="ATTACH DATABASE $arquivo AS anterior";attach.Parameters.AddWithValue("$arquivo",backup);attach.ExecuteNonQuery();}
 if(restaurar){using var dest=new SqliteConnection($"Data Source={Path.Combine(root,"backups","antes-recompor-historico-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss")+".db")}");dest.Open();db.BackupDatabase(dest);}
 var tables=new[]{"Usuarios","Especies","Campanhas","Avistamentos","PokedexRegistros","EstudosCientificos","ForumTopicos","ForumRespostas","JogoTentativas","NotasEstudo","Denuncias","EspecieQuizPerguntas","CampanhaParticipacoes","ForumComunidades","FaqTickets","FaqMensagens","AvistamentoMidias","ReservasAvistamento","NivelJogos","ForumCurtidasTopico","ForumCurtidasResposta"};
 var resultados=new List<object>();
 foreach(var table in tables){
  using var query=db.CreateCommand();query.CommandText=$"PRAGMA anterior.table_info(\"{table}\")";var columns=new List<string>();using(var read=query.ExecuteReader())while(read.Read())columns.Add("\""+read.GetString(1).Replace("\"","\"\"")+"\"");
  if(columns.Count==0)continue;
  var alteradas=new List<string>();
  foreach(var col in columns.Where(c=>c!="\"Id\"")){using var diff=db.CreateCommand();diff.CommandText=$"SELECT COUNT(*) FROM main.\"{table}\" a JOIN anterior.\"{table}\" b ON a.Id=b.Id WHERE a.{col} IS NOT b.{col}";if((long)diff.ExecuteScalar()!>0)alteradas.Add(col);}
  if(alteradas.Count>0)Console.WriteLine(table+" — colunas alteradas: "+string.Join(", ",alteradas));
  if(restaurar&&alteradas.Count>0){using var transaction=db.BeginTransaction();using var update=db.CreateCommand();update.Transaction=transaction;update.CommandText=$"UPDATE main.\"{table}\" SET "+string.Join(",",alteradas.Select(col=>$"{col}=(SELECT b.{col} FROM anterior.\"{table}\" b WHERE b.Id=main.\"{table}\".Id)"))+$" WHERE Id IN (SELECT Id FROM anterior.\"{table}\")";update.ExecuteNonQuery();transaction.Commit();}
  var cols=string.Join(",",columns);
  long Scalar(string sql){using var q=db.CreateCommand();q.CommandText=sql;return (long)q.ExecuteScalar()!;}
  var antes=Scalar($"SELECT COUNT(*) FROM anterior.\"{table}\"");var depois=Scalar($"SELECT COUNT(*) FROM main.\"{table}\"");
  var perdidos=Scalar($"SELECT COUNT(*) FROM (SELECT {cols} FROM anterior.\"{table}\" EXCEPT SELECT {cols} FROM main.\"{table}\")");
  var novos=Scalar($"SELECT COUNT(*) FROM (SELECT {cols} FROM main.\"{table}\" EXCEPT SELECT {cols} FROM anterior.\"{table}\")");
  resultados.Add(new{tabela=table,antes,depois,registrosOriginaisAusentesOuAlterados=perdidos,registrosNovosOuAlterados=novos});
  Console.WriteLine($"{table}: {antes} → {depois}; originais ausentes/alterados: {perdidos}");
 }
 using var check=db.CreateCommand();check.CommandText="PRAGMA integrity_check";var integrity=check.ExecuteScalar()?.ToString();
 File.WriteAllText(Path.Combine(root,"documentacao","validacao","preservacao-banco.json"),JsonSerializer.Serialize(new{backupComparado=Path.GetFileName(backup),integridade=integrity,tabelas=resultados},new JsonSerializerOptions{WriteIndented=true}));
 }
}