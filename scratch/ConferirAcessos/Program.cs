using Microsoft.Data.Sqlite;
using System.Text.RegularExpressions;
var root=Path.GetFullPath(args[0]);
var source=File.ReadAllText(Path.Combine(root,"TCC_Grupo12_III","Data","DataSeeder.cs"));
using var db=new SqliteConnection($"Data Source={Path.Combine(root,"TCC_Grupo12_III","app.db")};Mode=ReadOnly");
db.Open();
foreach(var email in new[]{"admin@ecocanta.org","guardiao@ecocanta.org"}){
 var pattern="Email = \""+Regex.Escape(email)+"\"[\\s\\S]*?HashPassword\\(\"([^\"]+)\"\\)";
 var candidate=Regex.Match(source,pattern).Groups[1].Value;
 using var query=db.CreateCommand();query.CommandText="SELECT IsAdmin, IsBanido, Senha FROM Usuarios WHERE lower(Email) = $email";query.Parameters.AddWithValue("$email",email);
 using var reader=query.ExecuteReader();
 if(!reader.Read()){Console.WriteLine(email+": conta não encontrada");continue;}
 var verified=candidate.Length>0&&BCrypt.Net.BCrypt.Verify(candidate,reader.GetString(2));
 Console.WriteLine($"{email}: perfil={(reader.GetBoolean(0)?"Administrador":"Estudante")}; ativa={!reader.GetBoolean(1)}; senha de demonstração confere={verified}");
}
