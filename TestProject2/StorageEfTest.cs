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
        var userId = new List<int>();
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
            command.Parameters.Add(new NpgsqlParameter { Value = $"user{uniqueId}1@example.com" });
            command.Parameters.Add(new NpgsqlParameter { Value = $"user{uniqueId}2Test2" });
            command.Parameters.Add(new NpgsqlParameter { Value = $"user{uniqueId}2@example.com" });
            await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    userId.Add(reader.GetInt32(0));
                }
            Assert.Equal(2, userId.Count);
            }

            {
                await using var connection = await dataSource.OpenConnectionAsync();
                await using var command = new NpgsqlCommand("""
                    INSERT INTO public.tasks (title, iscompleted, owner_id)
                    VALUES ($1, $2, $3), ($4, $5, $6), ($7, $8, $9), ($10, $11, $12), ($13, $14, $15)
                    """, connection);
                command.Parameters.Add(new NpgsqlParameter { Value = $"task{uniqueId}1" });
                command.Parameters.Add(new NpgsqlParameter { Value = false });
                command.Parameters.Add(new NpgsqlParameter { Value = userId[0] });
                command.Parameters.Add(new NpgsqlParameter { Value = $"task{uniqueId}2" });
                command.Parameters.Add(new NpgsqlParameter { Value = false });
                command.Parameters.Add(new NpgsqlParameter { Value = userId[0] });
                command.Parameters.Add(new NpgsqlParameter { Value = $"task{uniqueId}3" });
                command.Parameters.Add(new NpgsqlParameter { Value = false });
                command.Parameters.Add(new NpgsqlParameter { Value = userId[0] });
                command.Parameters.Add(new NpgsqlParameter { Value = $"task{uniqueId}4" });
                command.Parameters.Add(new NpgsqlParameter { Value = true });
                command.Parameters.Add(new NpgsqlParameter { Value = userId[0] });
                command.Parameters.Add(new NpgsqlParameter { Value = $"task{uniqueId}5" });
                command.Parameters.Add(new NpgsqlParameter { Value = false });
                command.Parameters.Add(new NpgsqlParameter { Value = userId[1] });
                await command.ExecuteNonQueryAsync();
            }

            { 
                // Act
                var openTasks = await storage.FindOpenTodosEf(2, userId[0], CancellationToken.None);
                // Assert
                Assert.Equal(2, openTasks.Count());
                Assert.Equal($"task{uniqueId}3", openTasks[0].Title);
                Assert.Equal($"task{uniqueId}2", openTasks[1].Title);
                Assert.All(openTasks, task => Assert.False(task.IsCompleted));
                Assert.All(openTasks, task => Assert.Equal(userId[0], task.OwnerId));
            }



        }
        finally
        {
            for (int i = 0; i < 2; i++)
            {
                await using var connection = await dataSource.OpenConnectionAsync();
                await using var command = new NpgsqlCommand("""
                    DELETE FROM public.users
                    WHERE email = $1;
                    """, connection);
                command.Parameters.Add(new NpgsqlParameter { Value = $"user{uniqueId}{i + 1}@example.com" });
                await command.ExecuteNonQueryAsync();
            }
        }
    }
    [Fact]
    public async Task EfTaskReader_ReturnsNull_WhenNoTasksFound()
    {
        // Arrange
        var str = Environment.GetEnvironmentVariable("TEST_DB_CONNECTION_STRING") ?? throw new InvalidOperationException("Test database connection string not found.");
        var options = new DbContextOptionsBuilder<ToDoEFContext>()
            .UseNpgsql(str)
            .Options;
        await using var context = new ToDoEFContext(options);
        StorageEF storage = new(context);

        // Act
        var task = await storage.EfTaskReader(-1, CancellationToken.None); // По сути наше бд не может на данный момент иметь отрицательный id, но она может его иметь если будетпоставлен диапазон для вроде Primary Key. Незнаю правильно ли я сделал. Хотя мой ToDo запрещает отрицательные id. Поэтому вероятно ошибка в логике бд
        // Assert
        Assert.Null(task);
    }
}
