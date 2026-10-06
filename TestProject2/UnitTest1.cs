using Npgsql;

namespace Test3.Tests;

// Example for PowerShell:
// $env:TEST_DB_CONNECTION_STRING = 'Host=localhost;Port=5432;Database=todo_api_test;Username=postgres;Password=ТВОЙ_ПАРОЛЬ'
public class StorageTests
{
    [Fact]
    public async Task TestDb()
    {
        string connectionString =
            Environment.GetEnvironmentVariable("TEST_DB_CONNECTION_STRING")
            ?? throw new InvalidOperationException("Test DB is not configured.");

        await using var dataSource = NpgsqlDataSource.Create(connectionString);

        // Safety check: no test data is created before the target DB is verified.
        {
            await using var connection = await dataSource.OpenConnectionAsync();
            await using var command = new NpgsqlCommand("""
                SELECT current_database();
                """, connection);

            var result = await command.ExecuteScalarAsync();

            if (result is null)
            {
                throw new InvalidOperationException("Failed to execute database check.");
            }

            if ((string)result != "todo_api_test")
            {
                throw new InvalidOperationException("Unexpected database.");
            }
        }

        string email = $"integration-{Guid.NewGuid()}@example.com";
        int ownerId;

        // Setup: create only the owner needed by this test.
        {
            await using var connection = await dataSource.OpenConnectionAsync();
            await using var command = new NpgsqlCommand("""
                INSERT INTO public.users (name, email)
                VALUES ($1, $2)
                RETURNING id;
                """, connection);

            command.Parameters.Add(new NpgsqlParameter { Value = "Integration Test" });
            command.Parameters.Add(new NpgsqlParameter { Value = email });

            var result = await command.ExecuteScalarAsync()
                ?? throw new InvalidOperationException("Failed to create test owner.");

            ownerId = (int)result;
        }

        // From this point the test owns DB data, so cleanup is guaranteed by finally.
        try
        {
            Storage storage = new(dataSource);

            int? id = await storage.Add(
                "Insert task",
                ownerId,
                CancellationToken.None);

            Assert.NotNull(id);

            var task = await storage.FindToDo(
                (int)id,
                CancellationToken.None);

            Assert.NotNull(task);
            Assert.Equal("Insert task", task.Title);
            Assert.Equal(ownerId, task.OwnerId);
            Assert.False(task.IsCompleted);
        }
        finally
        {
            await using var connection = await dataSource.OpenConnectionAsync();
            await using var command = new NpgsqlCommand("""
                DELETE FROM public.users
                WHERE id = $1;
                """, connection);

            command.Parameters.Add(new NpgsqlParameter { Value = ownerId });
            await command.ExecuteNonQueryAsync();
        }
    }

    [Fact]
    public async Task PutTestStorage()
    {
        string connectionString =
            Environment.GetEnvironmentVariable("TEST_DB_CONNECTION_STRING")
            ?? throw new InvalidOperationException("Test DB is not configured.");

        await using var dataSource = NpgsqlDataSource.Create(connectionString);

        // Safety check: no test data is created before the target DB is verified.
        {
            await using var connection = await dataSource.OpenConnectionAsync();
            await using var command = new NpgsqlCommand("""
                SELECT current_database();
                """, connection);

            var result = await command.ExecuteScalarAsync();

            if (result is null)
            {
                throw new InvalidOperationException("Failed to execute database check.");
            }

            if ((string)result != "todo_api_test")
            {
                throw new InvalidOperationException("Unexpected database.");
            }
        }

        string email = $"integration-{Guid.NewGuid()}@example.com";
        int ownerId;

        // Setup: create only the owner needed by this test.
        {
            await using var connection = await dataSource.OpenConnectionAsync();
            await using var command = new NpgsqlCommand("""
                INSERT INTO public.users (name, email)
                VALUES ($1, $2)
                RETURNING id;
                """, connection);

            command.Parameters.Add(new NpgsqlParameter { Value = "Integration Put Test" });
            command.Parameters.Add(new NpgsqlParameter { Value = email });

            var result = await command.ExecuteScalarAsync()
                ?? throw new InvalidOperationException("Failed to create test owner.");

            ownerId = (int)result;
        }

        // From this point the test owns DB data, so cleanup is guaranteed by finally.
        try
        {
            Storage storage = new(dataSource);

            int? id = await storage.Add(
                "Insert task",
                ownerId,
                CancellationToken.None);

            Assert.NotNull(id);

            var task = await storage.FindToDo(
                (int)id,
                CancellationToken.None);

            Assert.NotNull(task);
            Assert.Equal("Insert task", task.Title);
            Assert.Equal(ownerId, task.OwnerId);
            Assert.False(task.IsCompleted);

            var failedUpdate = await storage.TryPut(
                (int)id,
                "Updated task",
                true,
                -1,
                CancellationToken.None);

            task = await storage.FindToDo(
                (int)id,
                CancellationToken.None);

            Assert.NotNull(task);
            Assert.Equal("Insert task", task.Title);
            Assert.Equal(ownerId, task.OwnerId);
            Assert.False(task.IsCompleted);
            Assert.Equal(MessegStoreg.ForeignKeyViolation, failedUpdate);

            var successfulUpdate = await storage.TryPut(
                (int)id,
                "Updated task",
                true,
                ownerId,
                CancellationToken.None);

            var updatedTask = await storage.FindToDo(
                (int)id,
                CancellationToken.None);

            Assert.NotNull(updatedTask);
            Assert.Equal("Updated task", updatedTask.Title);
            Assert.Equal(ownerId, updatedTask.OwnerId);
            Assert.True(updatedTask.IsCompleted);
            Assert.Equal(MessegStoreg.Ok, successfulUpdate);
        }
        finally
        {
            await using var connection = await dataSource.OpenConnectionAsync();
            await using var command = new NpgsqlCommand("""
                DELETE FROM public.users
                WHERE id = $1;
                """, connection);

            command.Parameters.Add(new NpgsqlParameter { Value = ownerId });
            await command.ExecuteNonQueryAsync();
        }
    }
}
