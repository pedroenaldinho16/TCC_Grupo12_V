using Microsoft.Data.Sqlite;

internal static class BackupJogos
{
    public static void Verificar(string root,string pasta="jogos-20261114")
    {
        var backup = Directory.GetDirectories(Path.Combine(root,"backups"),"antes-jogos-*").OrderByDescending(p=>p).First();
        Dictionary<string,object> Hashes(string path)
        {
            using var db=new SqliteConnection($"Data Source={path};Mode=ReadOnly");db.Open();
            using var names=db.CreateCommand();names.CommandText="SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' AND name<>'__EFMigrationsLock' ORDER BY name";
            var tables=new List<string>();using(var r=names.ExecuteReader())while(r.Read())tables.Add(r.GetString(0));
            var result=new Dictionary<string,object>();
            foreach(var table in tables){
                using var command=db.CreateCommand();command.CommandText="SELECT * FROM \""+table.Replace("\"","\"\"")+"\" ORDER BY rowid";
                using var rows=command.ExecuteReader();using var hash=System.Security.Cryptography.IncrementalHash.CreateHash(System.Security.Cryptography.HashAlgorithmName.SHA256);var count=0;
                while(rows.Read()){
                    var values=new string?[rows.FieldCount];for(var i=0;i<values.Length;i++){var value=rows.GetValue(i);values[i]=value is DBNull?null:value.GetType().Name+":"+(value is byte[] bytes?Convert.ToBase64String(bytes):Convert.ToString(value,System.Globalization.CultureInfo.InvariantCulture));}
                    hash.AppendData(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(values));count++;
                }
                result[table]=new{registros=count,sha256=Convert.ToHexString(hash.GetHashAndReset())};
            }
            return result;
        }
        var before=Hashes(Path.Combine(backup,"app.db"));var after=Hashes(Path.Combine(root,"TCC_Grupo12_III","app.db"));
        var same=System.Text.Json.JsonSerializer.Serialize(before)==System.Text.Json.JsonSerializer.Serialize(after);
        var output=Path.Combine(root,"documentacao",pasta,"preservacao-banco.json");
        File.WriteAllText(output,System.Text.Json.JsonSerializer.Serialize(new{preservado=same,backup,tabelas=after.Count,antes=before,depois=after},new System.Text.Json.JsonSerializerOptions{WriteIndented=true}));
        Console.WriteLine($"Comparação com backup: {after.Count} tabelas; preservado={same}");
        if(!same)Environment.ExitCode=1;
    }

    public static void Run(string root)
    {
        var folder = Path.Combine(root, "backups", "antes-jogos-" + DateTime.Now.ToString("yyyyMMdd-HHmmss"));
        Directory.CreateDirectory(folder);
        using var source = new SqliteConnection($"Data Source={Path.Combine(root, "TCC_Grupo12_III", "app.db")};Mode=ReadOnly");
        using var target = new SqliteConnection($"Data Source={Path.Combine(folder, "app.db")}");
        source.Open(); target.Open(); source.BackupDatabase(target);
        foreach (var relative in new[] { "Models/Jornada.cs", "Controllers/JornadaController.cs", "Controllers/AdminJornadaController.cs", "Views/Jornada", "Views/AdminJornada", "Views/Home/Index.cshtml", "Views/Shared/_Layout.cshtml", "Views/Shared/_AcessoMapa.cshtml", "Program.cs" })
        {
            var path = Path.Combine(root, "TCC_Grupo12_III", relative);
            var files = Directory.Exists(path) ? Directory.GetFiles(path, "*", SearchOption.AllDirectories) : new[] { path };
            foreach (var file in files)
            {
                var destination = Path.Combine(folder, "codigo", Path.GetRelativePath(Path.Combine(root, "TCC_Grupo12_III"), file));
                Directory.CreateDirectory(Path.GetDirectoryName(destination)!); File.Copy(file, destination);
            }
        }
        Console.WriteLine(folder);
    }
}
