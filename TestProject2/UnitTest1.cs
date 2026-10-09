using Npgsql;

namespace Test3.Tests;

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
    [Fact]
    public async Task TestFindOpenTodos()
    {
        string connectionString =
            Environment.GetEnvironmentVariable("TEST_DB_CONNECTION_STRING")
            ?? throw new InvalidOperationException("Test DB is not configured.");
        await using var dataSource = NpgsqlDataSource.Create(connectionString);

        {
            await using var connection = await dataSource.OpenConnectionAsync();
            var command = new NpgsqlCommand("""
                SELECT current_database();
                """, connection);
            var result = await command.ExecuteScalarAsync();
            Assert.NotNull(result);
            if ((string)result != "todo_api_test")
            {
                throw new InvalidOperationException("Unexpected database.");
            }
        }
        List<int> ownerIds = new List<int>(2);
        {
            string emailA = $"integration-{Guid.NewGuid()}@example.com";
            string emailB = $"integration-{Guid.NewGuid()}@example.com";
            await using var connection = await dataSource.OpenConnectionAsync();
            var command = new NpgsqlCommand("""
                INSERT INTO public.users (name, email)
                VALUES ($1, $2), ($3, $4)
                RETURNING id;
                """, connection);
            command.Parameters.Add(new NpgsqlParameter { Value = "Integration Open Test A" });
            command.Parameters.Add(new NpgsqlParameter { Value = emailA });
            command.Parameters.Add(new NpgsqlParameter { Value = "Integration Open Test B" });
            command.Parameters.Add(new NpgsqlParameter { Value = emailB });

            await using var result = await command.ExecuteReaderAsync();

            while (await result.ReadAsync())
            {
                ownerIds.Add(result.GetInt32(0));
            }

            if (ownerIds.Count != 2)
            {
                throw new InvalidOperationException("Failed to create test owners.");
            }
        }

        try
        {
            Storage storage = new(dataSource);
            List<int?> ids = new(5);
            ids.Add(await storage.Add("A1", ownerIds[0], CancellationToken.None));
            ids.Add(await storage.Add("A2", ownerIds[0], CancellationToken.None));
            ids.Add(await storage.Add("A3", ownerIds[0], CancellationToken.None));
            ids.Add(await storage.Add("A4", ownerIds[0], CancellationToken.None, true));
            ids.Add(await storage.Add("B1", ownerIds[1], CancellationToken.None));
            foreach (var id in ids)
            {
                Assert.NotNull(id);
            }
            List<ToDo> allTasks = new(5);
            //Данный блок написан ии так как мне лень писать отдельные тесты для каждого элемента, так как они все одинаковые по сути
            for (int i = 0; i < 4; i++)
            {
                var task = await storage.FindToDo((int)ids[i], CancellationToken.None);

                Assert.NotNull(task);
                Assert.Equal($"A{i + 1}", task.Title);
                Assert.Equal(ownerIds[0], task.OwnerId);


                if (i == 3)
                {
                    Assert.True(task.IsCompleted);
                }
                else
                {
                    Assert.False(task.IsCompleted);
                }
                allTasks.Add(task);
            }


            var lastTask = await storage.FindToDo((int)ids[4], CancellationToken.None);

            Assert.NotNull(lastTask);
            Assert.Equal("B1", lastTask.Title);
            Assert.Equal(ownerIds[1], lastTask.OwnerId);
            Assert.False(lastTask.IsCompleted);
            allTasks.Add(lastTask);


            int limit = 2;
            var openTodos = await storage.FindOpenTodos(limit, ownerIds[0], CancellationToken.None);
            Assert.Equal(2, openTodos.Count);
            List<ToDo> result = allTasks.OrderByDescending((toDo) => toDo.Id).Where((toDo) => toDo.OwnerId == ownerIds[0] && !toDo.IsCompleted).Take(limit).ToList();
            for (int i = 0; i < limit; i++)
            {
                Assert.Equal(result[i].Title, openTodos[i].Title);
                Assert.Equal(result[i].Id, openTodos[i].Id);
                Assert.Equal(result[i].OwnerId, openTodos[i].OwnerId);
                Assert.Equal(result[i].IsCompleted, openTodos[i].IsCompleted);
                Assert.Equal(result[i].CreateAt, openTodos[i].CreateAt);
            }

        }
        finally
        {
            await using var connection = await dataSource.OpenConnectionAsync();
            await using var command = new NpgsqlCommand("""
                DELETE FROM public.users
                WHERE id = $1 OR id = $2;
                """, connection);

            command.Parameters.Add(new NpgsqlParameter { Value = ownerIds[0] });
            command.Parameters.Add(new NpgsqlParameter { Value = ownerIds[1] });
            await command.ExecuteNonQueryAsync();
        }
    }

}
