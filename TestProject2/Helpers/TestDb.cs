using Npgsql;

namespace TestProject2.Helpers
{
    internal class TestDb
    {
        static public async Task AssertTestDatabaseAsync(NpgsqlDataSource dataSource)
        {
            await using var connection = await dataSource.OpenConnectionAsync();
            await using var command = new NpgsqlCommand("""
                SELECT current_database();
                """, connection);

            var result = await command.ExecuteScalarAsync();

            if (result is null)
            {
                throw new InvalidOperationException("Test database does not exist.");
            }

            if ((string)result != "todo_api_test")
            {
                throw new InvalidOperationException("Not test database");

            }
        }
    }
}
