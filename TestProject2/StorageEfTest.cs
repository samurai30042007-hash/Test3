using Microsoft.EntityFrameworkCore;
using Npgsql;
using TestProject2.Helpers;
namespace Test3.Tests;
// C:\Program Files (x86)\R.G. Mechanics\Command and Conquer - Generals
public class StorageEfTest
{
    [Fact]
    public async Task EfTaskReader_ReturnsTask()
    {
        // Arrange
        var str = Environment.GetEnvironmentVariable("TEST_DB_CONNECTION_STRING") ?? throw new InvalidOperationException("Test database connection string not found.");
        var options = new DbContextOptionsBuilder<ToDoEFContext>()
            .UseNpgsql(str)
            .Options;
        await using var context = new ToDoEFContext(options);
        StorageEF storage = new(context);
        await using var dataSource = NpgsqlDataSource.Create(str);
        await TestDb.AssertTestDatabaseAsync(dataSource);
        Guid uniqueId = Guid.NewGuid();
        int userId;
        try
        {
            await using var connection1 = await dataSource.OpenConnectionAsync();
            await using var command1 = new NpgsqlCommand("""
            INSERT INTO public.users (name, email)
            VALUES ($1, $2)
            returning id;
            """, connection1);
            command1.Parameters.Add(new NpgsqlParameter { Value = $"user{uniqueId}Test1" });
            command1.Parameters.Add(new NpgsqlParameter { Value = $"user{uniqueId}@example.com" });
            var result = await command1.ExecuteScalarAsync();
            if (result is null)
            {
                throw new InvalidOperationException("Failed to insert user into the database.");
            }
            userId = (int)result;
        
            
            await using var connection = await dataSource.OpenConnectionAsync();
            await using var command = new NpgsqlCommand("""
                INSERT INTO public.tasks (title, iscompleted, owner_id)
                VALUES ($1, $2, $3)
                RETURNING id
                """, connection);
            command.Parameters.Add(new NpgsqlParameter { Value = $"task{uniqueId}" });
            command.Parameters.Add(new NpgsqlParameter { Value = false });
            command.Parameters.Add(new NpgsqlParameter { Value = userId });
            var result1 = await command.ExecuteScalarAsync();
            if (result1 is null)
            {
                throw new InvalidOperationException("Failed to insert task into the database.");
            }
            var taskId = (int)result1;

            // Act
            var task = await storage.EfTaskReader(taskId, CancellationToken.None);

            // Assert
            Assert.NotNull(task);
            Assert.Equal(taskId, task!.Id);
            Assert.Equal($"task{uniqueId}", task.Title);
            Assert.False(task.IsCompleted);
            Assert.Equal(userId, task.OwnerId);
        }
        finally
        {
            await using var connection2 = await dataSource.OpenConnectionAsync();
            await using var command2 = new NpgsqlCommand("""
                DELETE FROM public.users
                WHERE email = $1;
                """, connection2);
            command2.Parameters.Add(new NpgsqlParameter { Value = $"user{uniqueId}@example.com" });
            await command2.ExecuteNonQueryAsync();
        }


    }
    [Fact]
    public async Task FindOpenTodosEf_ReturnsOpenTasks() 
    {
        var str = Environment.GetEnvironmentVariable("TEST_DB_CONNECTION_STRING") ?? throw new InvalidOperationException("Test database connection string not found.");
        var options = new DbContextOptionsBuilder<ToDoEFContext>()
            .UseNpgsql(str)
            .Options;
        await using var context = new ToDoEFContext(options);
        StorageEF storage = new(context);
        await using var dataSource = NpgsqlDataSource.Create(str);
        await TestDb.AssertTestDatabaseAsync(dataSource);
        Guid uniqueId = Guid.NewGuid();
        int[] userId = new int[2];
        try
        {
            {
            await using var connection = await dataSource.OpenConnectionAsync();
            await using var command = new NpgsqlCommand("""
                INSERT INTO public.users (name, email)
                VALUES ($1, $2), ($3, $4)
                returning id;
                """, connection);
            command.Parameters.Add(new NpgsqlParameter { Value = $"user{uniqueId}Test2" });
            command.Parameters.Add(new NpgsqlParameter { Value = $"user{uniqueId}@example.com" });
            command.Parameters.Add(new NpgsqlParameter { Value = $"user{uniqueId}2Test2" });
            command.Parameters.Add(new NpgsqlParameter { Value = $"user{uniqueId}2@example.com" });
            await using var reader = await command.ExecuteReaderAsync();
            int index = 0;
                while (await reader.ReadAsync())
                {
                    userId[index++] = reader.GetInt32(0);
                }
            }

            {
                await using var connection = await dataSource.OpenConnectionAsync();
                await using var command = new NpgsqlCommand("""
                    INSERT INTO public.tasks (title, iscompleted, owner_id)
                    VALUES ($1, $2, $3), ($4, $5, $6), ($7, $8, $9), ($10, $11, $12)
                    """, connection);
                command.Parameters.Add(new NpgsqlParameter { Value = $"task{uniqueId}1" });
                command.Parameters.Add(new NpgsqlParameter { Value = false });
                command.Parameters.Add(new NpgsqlParameter { Value = userId[0] });
                command.Parameters.Add(new NpgsqlParameter { Value = $"task{uniqueId}2" });
                command.Parameters.Add(new NpgsqlParameter { Value = true });
                command.Parameters.Add(new NpgsqlParameter { Value = userId[0] });
                command.Parameters.Add(new NpgsqlParameter { Value = $"task{uniqueId}3" });
                command.Parameters.Add(new NpgsqlParameter { Value = false });
                command.Parameters.Add(new NpgsqlParameter { Value = userId[0] });
                command.Parameters.Add(new NpgsqlParameter { Value = $"task{uniqueId}4" });
                command.Parameters.Add(new NpgsqlParameter { Value = false });
                command.Parameters.Add(new NpgsqlParameter { Value = userId[1] });
                await command.ExecuteNonQueryAsync();
            }

            { 
                // Act
                var openTasks = await storage.FindOpenTodosEf(2, userId[0], CancellationToken.None);
                // Assert
                Assert.NotNull(openTasks);
                Assert.Equal(2, openTasks.Count());
                Assert.Equal(openTasks[0].Title, $"task{uniqueId}3");
                Assert.Equal(openTasks[1].Title, $"task{uniqueId}1");
                Assert.All(openTasks, task => Assert.False(task.IsCompleted));
                Assert.All(openTasks, task => Assert.Equal(userId[0], task.OwnerId));
            }



        }
        finally
        {
            foreach (var id in userId)
            {
                await using var connection = await dataSource.OpenConnectionAsync();
                await using var command = new NpgsqlCommand("""
                    DELETE FROM public.users
                    WHERE id = $1;
                    """, connection);
                command.Parameters.Add(new NpgsqlParameter { Value = id });
                await command.ExecuteNonQueryAsync();
            }
        }
    }

}
