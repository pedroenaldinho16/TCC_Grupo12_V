internal static class TestApplication
{
    public static string Dll(string root) => Environment.GetEnvironmentVariable("ECOCANTA_TEST_APP")
        ?? Path.Combine(root, "TCC_Grupo12_III", "bin", "Debug", "net10.0", "TCC_Grupo12_III.dll");

    public static void PrepareCopy(string path)
    {
        if (System.IO.Path.GetFileName(path) != "teste.db") throw new ArgumentException("Use somente o banco descartável de testes.");
        using var connection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={path}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE name='__EFMigrationsLock'";
        if ((long)command.ExecuteScalar()! == 0) return;
        // Um snapshot do banco em uso pode conter um lock operacional do processo original.
        // A cópia descartável tem outro servidor e não deve herdar esse lock.
        command.CommandText = "DELETE FROM __EFMigrationsLock";
        command.ExecuteNonQuery();
    }
}
