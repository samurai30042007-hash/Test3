using Npgsql;
using System.Diagnostics;
using Test3;

namespace Test3.Tests;

public class StorageTests
{
    [Fact]
    public async Task TestDb()
    {
        string connectionString = Environment.GetEnvironmentVariable("TEST_DB_CONNECTION_STRING") ?? throw new InvalidOperationException("Test DB is not configured.");
        await using var dataSource = NpgsqlDataSource.Create(connectionString);
        try
        {
            await using var connection = await dataSource.OpenConnectionAsync();
            await using var command = new NpgsqlCommand("""
            SELECT current_database();
            """, connection);
            var result = await command.ExecuteScalarAsync();
            if (result is null)
            {
                throw new InvalidOperationException("Failed to execute query.");
            }
            else if ((string)result != "todo_api_test")
            {
                throw new InvalidOperationException("Unexpected database.");
            }
            await using var connection2 = await dataSource.OpenConnectionAsync();
            await using var command2 = new NpgsqlCommand("""
            insert into public.users (name, email)
            values ('Alex', 'alex@example.com'), 
            ('Maria', 'maria@example.com'),
            ('Ivan', 'ivan@example.com'),
            ('Oleg', 'oleg@example.com')
            returning id;
            """, connection2);
            await using var result2 = await command2.ExecuteReaderAsync();
            var ownerId = new List<int>();
            while (await result2.ReadAsync())
            {
                ownerId.Add(result2.GetInt32(0));
            }

            Storage storage = new(dataSource);
            int? id = await storage.Add("Insert task", ownerId[0], CancellationToken.None);
            Assert.NotNull(id);
            var task = await storage.FindToDo((int)id, CancellationToken.None);
            Assert.NotNull(task);
            Assert.Equal(task.Title, "Insert task");
            Assert.Equal(task.OwnerId, ownerId[0]);
            Assert.Equal(task.IsCompleted, false);
        }
        finally
        {
            await using var connection3 = await dataSource.OpenConnectionAsync();
            await using var command3 = new NpgsqlCommand("""
            DELETE FROM public.users
            """, connection3);
            await command3.ExecuteNonQueryAsync();
                
        }
    }
}