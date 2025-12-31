using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Infra.Data.SqlServer
{
    public static class DatabaseInitializer
    {
        public static bool EnsureDatabaseExists(string connectionString, string dbName)
        {
            bool dbExists = false;

            var builder = new SqlConnectionStringBuilder(connectionString)
            {
                InitialCatalog = "master"
            };

            using (var connection = new SqlConnection(builder.ToString()))
            {
                connection.Open();

                var checkDbCmd = connection.CreateCommand();
                checkDbCmd.CommandText = $@"
                    SELECT COUNT(*) FROM sys.databases WHERE name = N'{dbName}'";
                dbExists = (int)checkDbCmd.ExecuteScalar() > 0;

                if (!dbExists)
                {
                    string assemblyDir = Path.GetDirectoryName(typeof(DatabaseInitializer).Assembly.Location)!;
                    string scriptPath = Path.Combine(assemblyDir, "schema.sql");
                    string script = File.ReadAllText(scriptPath);
                    var command = connection.CreateCommand();

                    var batches = script.Split(
                        new[] { "\r\nGO\r\n", "\nGO\n", "\rGO\r" },
                        StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                    foreach (var batch in batches)
                    {
                        if (!string.IsNullOrWhiteSpace(batch))
                        {
                            var batchCommand = connection.CreateCommand();
                            batchCommand.CommandText = batch;
                            batchCommand.ExecuteNonQuery();
                        }
                    }
                }
            }

            return dbExists;
        }
    }
}
