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
        static public async Task<object?> CreateTestUserAsync(NpgsqlDataSource dataSource, Guid id, int count)
        {
            await using var connection = await dataSource.OpenConnectionAsync();
            await using var command = new NpgsqlCommand("""
                INSERT INTO public.users (name, email)
                VALUES ($1, $2)
                RETURNING id
                """, connection);
            command.Parameters.Add(new NpgsqlParameter { Value = $"user{id}{count}Test" });
            command.Parameters.Add(new NpgsqlParameter { Value = $"user{id}{count}@example.com" });
            var result = await command.ExecuteScalarAsync();
            return result;
        }
        static public async Task<object?> CreateTestToDoAsync(NpgsqlDataSource dataSource, Guid id, int count,int ownerId, bool isCompleted = false)
        {
            await using var connection = await dataSource.OpenConnectionAsync();
            await using var command = new NpgsqlCommand("""
                INSERT INTO public.tasks (title, iscompleted, owner_id)
                VALUES ($1, $2, $3)
                RETURNING id
                """, connection);
            command.Parameters.Add(new NpgsqlParameter { Value = $"task{id}{count}" });
            command.Parameters.Add(new NpgsqlParameter { Value =  isCompleted});
            command.Parameters.Add(new NpgsqlParameter { Value = ownerId });
            var result = await command.ExecuteScalarAsync();
            return result;
        }
        static public async Task DeleteTestUserAsync(NpgsqlDataSource dataSource, Guid id, int count)
        {
            await using var connection = await dataSource.OpenConnectionAsync();
            await using var command = new NpgsqlCommand("""
                DELETE FROM public.users 
                WHERE email = $1
                """, connection);
            command.Parameters.Add(new NpgsqlParameter { Value = $"user{id}{count}@example.com" });
            await command.ExecuteNonQueryAsync();
        }
    }
}
