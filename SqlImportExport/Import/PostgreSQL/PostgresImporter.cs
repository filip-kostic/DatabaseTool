using Npgsql;

namespace SqlImportExport.Import.PostgreSQL
{
    class PostgresImporter : IImporter
    {
        public PostgresImporter(string connectionString)
        {
            ConnectionString = connectionString;
        }

        string ConnectionString { get; }

        public void Execute(string query)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
