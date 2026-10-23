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

            var result = await TestDb.CreateTestUserAsync(dataSource, uniqueId, 1);
            if (result is null)
            {
                throw new InvalidOperationException("Failed to insert user into the database.");
            }
            userId = (int)result;


            var result1 = await TestDb.CreateTestToDoAsync(dataSource, uniqueId, 1, userId);
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
            Assert.Equal($"task{uniqueId}1", task.Title);
            Assert.False(task.IsCompleted);
            Assert.Equal(userId, task.OwnerId);
        }
        finally
        {
            await TestDb.DeleteTestUserAsync(dataSource, uniqueId, 1);
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

            for (int i = 0; i < 2; i++)
            {
                var result = await TestDb.CreateTestUserAsync(dataSource, uniqueId, i + 1);
                if (result is null)
                {
                    throw new InvalidOperationException("Failed to insert user into the database.");
                }
                userId.Add((int)result);
            }


            {
                for (int i = 0; i < 4; i++)
                {
                    object? result;
                    if (i == 3)
                    {
                        result = await TestDb.CreateTestToDoAsync(dataSource, uniqueId, i + 1, userId[0], true);
                    }
                    else
                    {
                        result = await TestDb.CreateTestToDoAsync(dataSource, uniqueId, i + 1, userId[0]);
                    }
                    if (result is null)
                    {
                        throw new InvalidOperationException("Failed to insert task into the database.");
                    }
                }
                var result5 = await TestDb.CreateTestToDoAsync(dataSource, uniqueId, 5, userId[1]);
                if (result5 is null)
                {
                    throw new InvalidOperationException("Failed to insert task into the database.");
                }
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
                await TestDb.DeleteTestUserAsync(dataSource, uniqueId, i + 1);
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
    [Fact]
    public async Task EfToDosFindByOwner_ReturnsTasks()
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
        var userId = new List<int>();
        try
        {
            {
                for (int i = 0; i < 2; i++)
                {
                    var result = await TestDb.CreateTestUserAsync(dataSource, uniqueId, i + 1);
                    if (result is null)
                    {
                        throw new InvalidOperationException("Failed to insert user into the database.");
                    }
                    userId.Add((int)result);
                }
            }
            {

                for (int i = 0; i < 3; i++)
                {
                    object? result;
                    if (i == 1)
                    {
                        result = await TestDb.CreateTestToDoAsync(dataSource, uniqueId, i + 1, userId[0], true);
                    }
                    else
                    {
                        result = await TestDb.CreateTestToDoAsync(dataSource, uniqueId, i + 1, userId[0]);
                    }
                    if (result is null)
                    {
                        throw new InvalidOperationException("Failed to insert task into the database.");
                    }
                }
                var result4 = await TestDb.CreateTestToDoAsync(dataSource, uniqueId, 5, userId[1]);
                if (result4 is null)
                {
                    throw new InvalidOperationException("Failed to insert task into the database.");
                }

            }
            // Получение всех задач без указания isCompleted
            {
                // Act
                var Tasks = await storage.EfToDosFindByOwner(CancellationToken.None, userId[0], null);

                // Assert
                Assert.Equal(3, Tasks.Count());
                Assert.Equal($"task{uniqueId}1", Tasks[0].Title);
                Assert.False(Tasks[0].IsCompleted);
                Assert.Equal($"task{uniqueId}2", Tasks[1].Title);
                Assert.True(Tasks[1].IsCompleted);
                Assert.Equal($"task{uniqueId}3", Tasks[2].Title);
                Assert.False(Tasks[2].IsCompleted);
                Assert.All(Tasks, task => Assert.Equal(userId[0], task.OwnerId));
            }
        }
        finally
        {
            for (int i = 0; i < 2; i++)
            {
                await TestDb.DeleteTestUserAsync(dataSource, uniqueId, i + 1);
            }
        }
    }
    [Fact]
    public async Task EfToDosFindByOwner_ReturnsTasks_WhenTaskOpen()
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
        var userId = new List<int>();
        try
        {
            {
                for (int i = 0; i < 2; i++)
                {
                    var result = await TestDb.CreateTestUserAsync(dataSource, uniqueId, i + 1);
                    if (result is null)
                    {
                        throw new InvalidOperationException("Failed to insert user into the database.");
                    }
                    userId.Add((int)result);
                }
            }
            {

                for (int i = 0; i < 3; i++)
                {
                    object? result;
                    if (i == 1)
                    {
                        result = await TestDb.CreateTestToDoAsync(dataSource, uniqueId, i + 1, userId[0], true);
                    }
                    else
                    {
                        result = await TestDb.CreateTestToDoAsync(dataSource, uniqueId, i + 1, userId[0]);
                    }
                    if (result is null)
                    {
                        throw new InvalidOperationException("Failed to insert task into the database.");
                    }
                }
                var result4 = await TestDb.CreateTestToDoAsync(dataSource, uniqueId, 5, userId[1]);
                if (result4 is null)
                {
                    throw new InvalidOperationException("Failed to insert task into the database.");
                }

            }
            // Получение только завершенных задач
            {
                // Act
                var completedTasks = await storage.EfToDosFindByOwner(CancellationToken.None, userId[0], true);
                // Assert
                Assert.Single(completedTasks);
                Assert.Equal($"task{uniqueId}2", completedTasks[0].Title);
                Assert.True(completedTasks[0].IsCompleted);
                Assert.Equal(userId[0], completedTasks[0].OwnerId);
            }
            // Получение только незавершенных задач
            {
                // Act
                var openTasks = await storage.EfToDosFindByOwner(CancellationToken.None, userId[0], false);
                // Assert
                Assert.Equal(2, openTasks.Count());
                Assert.Equal($"task{uniqueId}1", openTasks[0].Title);
                Assert.False(openTasks[0].IsCompleted);
                Assert.Equal($"task{uniqueId}3", openTasks[1].Title);
                Assert.False(openTasks[1].IsCompleted);
                Assert.All(openTasks, task => Assert.Equal(userId[0], task.OwnerId));
            }
        }
        finally
        {
            for (int i = 0; i < 2; i++)
            {
                await TestDb.DeleteTestUserAsync(dataSource, uniqueId, i + 1);
            }
        }
    }
    [Fact]
    public async Task EfToDosFindByOwner_ReturnsTasks_WhenTaskClose()
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
        var userId = new List<int>();
        try
        {
            {
                for (int i = 0; i < 2; i++)
                {
                    var result = await TestDb.CreateTestUserAsync(dataSource, uniqueId, i + 1);
                    if (result is null)
                    {
                        throw new InvalidOperationException("Failed to insert user into the database.");
                    }
                    userId.Add((int)result);
                }
            }
            {

                for (int i = 0; i < 3; i++)
                {
                    object? result;
                    if (i == 1)
                    {
                        result = await TestDb.CreateTestToDoAsync(dataSource, uniqueId, i + 1, userId[0], true);
                    }
                    else
                    {
                        result = await TestDb.CreateTestToDoAsync(dataSource, uniqueId, i + 1, userId[0]);
                    }
                    if (result is null)
                    {
                        throw new InvalidOperationException("Failed to insert task into the database.");
                    }
                }
                var result4 = await TestDb.CreateTestToDoAsync(dataSource, uniqueId, 5, userId[1]);
                if (result4 is null)
                {
                    throw new InvalidOperationException("Failed to insert task into the database.");
                }

            }
            // Получение только незавершенных задач
            {
                // Act
                var openTasks = await storage.EfToDosFindByOwner(CancellationToken.None, userId[0], false);
                // Assert
                Assert.Equal(2, openTasks.Count());
                Assert.Equal($"task{uniqueId}1", openTasks[0].Title);
                Assert.False(openTasks[0].IsCompleted);
                Assert.Equal($"task{uniqueId}3", openTasks[1].Title);
                Assert.False(openTasks[1].IsCompleted);
                Assert.All(openTasks, task => Assert.Equal(userId[0], task.OwnerId));
            }
        }
        finally
        {
            for (int i = 0; i < 2; i++)
            {
                await TestDb.DeleteTestUserAsync(dataSource, uniqueId, i + 1);
            }
        }
    }
    [Fact]
    public async Task EfToDosFindByOwner_ReturnsTasks_WhenNone()
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
        var userId = new List<int>();
        try
        {
            {
                for (int i = 0; i < 2; i++)
                {
                    var result = await TestDb.CreateTestUserAsync(dataSource, uniqueId, i + 1);
                    if (result is null)
                    {
                        throw new InvalidOperationException("Failed to insert user into the database.");
                    }
                    userId.Add((int)result);
                }
            }
            {

                for (int i = 0; i < 3; i++)
                {
                    object? result;
                    if (i == 1)
                    {
                        result = await TestDb.CreateTestToDoAsync(dataSource, uniqueId, i + 1, userId[0], true);
                    }
                    else
                    {
                        result = await TestDb.CreateTestToDoAsync(dataSource, uniqueId, i + 1, userId[0]);
                    }
                    if (result is null)
                    {
                        throw new InvalidOperationException("Failed to insert task into the database.");
                    }
                }
                var result4 = await TestDb.CreateTestToDoAsync(dataSource, uniqueId, 5, userId[1]);
                if (result4 is null)
                {
                    throw new InvalidOperationException("Failed to insert task into the database.");
                }

            }
            // Получение пустого списка задач
            {
                // Act
                var Tasks = await storage.EfToDosFindByOwner(CancellationToken.None, userId[1], true);

                // Assert
                Assert.Empty(Tasks);
            }
        }
        finally
        {
            for (int i = 0; i < 2; i++)
            {
                await TestDb.DeleteTestUserAsync(dataSource, uniqueId, i + 1);
            }
        }
    }
    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public async Task EfToDosFindByOwner_ThrowsArgumentException_WhenOwnerIdIsInvalid(int value)
    {
        // Arrange
        var str = Environment.GetEnvironmentVariable("TEST_DB_CONNECTION_STRING") ?? throw new InvalidOperationException("Test database connection string not found.");
        var options = new DbContextOptionsBuilder<ToDoEFContext>()
            .UseNpgsql(str)
            .Options;
        await using var context = new ToDoEFContext(options);
        StorageEF storage = new(context);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await storage.EfToDosFindByOwner(CancellationToken.None, value, null);
        });
    }
    [Fact]
    public async Task EfTaskCreate_ReturnsTask()
    {

        string str = Environment.GetEnvironmentVariable("TEST_DB_CONNECTION_STRING") ?? throw new InvalidOperationException("Test database connection string not found.");
        await using NpgsqlDataSource dataSource = NpgsqlDataSource.Create(str);
        await TestDb.AssertTestDatabaseAsync(dataSource);
        var options = new DbContextOptionsBuilder<ToDoEFContext>()
            .UseNpgsql(str)
            .Options;
        await using ToDoEFContext context = new(options);
        StorageEF storage = new(context);
        int userId;
        Guid unique = Guid.NewGuid();
        try
        {
            var result = await TestDb.CreateTestUserAsync(dataSource, unique, 1) ?? throw new ArgumentNullException("Cannot create user");
            userId = (int)result;


            var task = await storage.EfTaskCreate("Created task", userId, true, CancellationToken.None);
            Assert.True(task.Id > 0);
            Assert.NotEqual(DateTime.MinValue, task.CreatedAt);
            Assert.Equal("Created task", task.Title);
            Assert.Equal(userId, task.OwnerId);
            Assert.True(task.IsCompleted);
        }
        finally
        {
            await TestDb.DeleteTestUserAsync(dataSource, unique, 1);
        }
    }
    [Fact]
    public async Task EfTaskCreate_SaveTaskPostgres()
    {
        string str = Environment.GetEnvironmentVariable("TEST_DB_CONNECTION_STRING") ?? throw new InvalidOperationException("Test database connection string not found.");
        await using NpgsqlDataSource dataSource = NpgsqlDataSource.Create(str);
        await TestDb.AssertTestDatabaseAsync(dataSource);
        var options = new DbContextOptionsBuilder<ToDoEFContext>()
            .UseNpgsql(str)
            .Options;

        int userId;
        Guid unique = Guid.NewGuid();
        int taskId;
        try
        {
            {
                await using ToDoEFContext context = new(options);
                StorageEF storage = new(context);
                var result = await TestDb.CreateTestUserAsync(dataSource, unique, 1) ?? throw new ArgumentNullException("Cannot create user");
                userId = (int)result;


                var task = await storage.EfTaskCreate("Created task", userId, true, CancellationToken.None);
                Assert.True(task.Id > 0);
                Assert.NotEqual(DateTime.MinValue, task.CreatedAt);
                Assert.Equal("Created task", task.Title);
                Assert.Equal(userId, task.OwnerId);
                Assert.True(task.IsCompleted);
                taskId = task.Id;
            }
            {
                await using ToDoEFContext context = new(options);
                var task = await context.ToDos.AsNoTracking().Where(t => t.Id == taskId).SingleOrDefaultAsync() ?? throw new ArgumentNullException("Cannot get task");
                Assert.Equal(taskId, task.Id);
                Assert.NotEqual(DateTime.MinValue, task.CreatedAt);
                Assert.Equal("Created task", task.Title);
                Assert.Equal(userId, task.OwnerId);
                Assert.True(task.IsCompleted);
            }
        }
        finally
        {
            await TestDb.DeleteTestUserAsync(dataSource, unique, 1);
        }
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task EfTaskUpdateTitle_UpdatesTaskTitle(bool isCompleted)
    {
        string str = Environment.GetEnvironmentVariable("TEST_DB_CONNECTION_STRING") ?? throw new InvalidOperationException("Test database connection string not found.");
        await using NpgsqlDataSource dataSource = NpgsqlDataSource.Create(str);
        await TestDb.AssertTestDatabaseAsync(dataSource);
        var options = new DbContextOptionsBuilder<ToDoEFContext>()
            .UseNpgsql(str)
            .Options;
        await using ToDoEFContext context = new(options);
        StorageEF storage = new(context);
        Guid unique = Guid.NewGuid();
        int userId;
        try
        {
            var result = await TestDb.CreateTestUserAsync(dataSource, unique, 1) ?? throw new ArgumentNullException("Cannot create user");
            userId = (int)result;
            var result1 = await TestDb.CreateTestToDoAsync(dataSource, unique, 1, userId, isCompleted) ?? throw new ArgumentNullException("Cannot create task");
            var taskId = (int)result1;
            var task = await storage.EfTaskReader(taskId, CancellationToken.None) ?? throw new ArgumentNullException("Cannot get task");
            var isUpdated = await storage.EfTaskUpdateTitle(taskId, $"Updated task title{unique}", CancellationToken.None);
            if (!isUpdated)
            {
                throw new InvalidOperationException("Failed to update task title.");
            }
            await using var newContext = new ToDoEFContext(options);
            var updatedTask = await newContext.ToDos.AsNoTracking().Where(t => t.Id == taskId).SingleOrDefaultAsync() ?? throw new ArgumentNullException("Cannot get task");
            Assert.Equal($"Updated task title{unique}", updatedTask.Title);
            Assert.Equal(isCompleted, updatedTask.IsCompleted);
            Assert.Equal(userId, updatedTask.OwnerId);
            Assert.Equal(task.CreatedAt, updatedTask.CreatedAt);
            isUpdated = await storage.EfTaskUpdateTitle(taskId, $"Updated task title{unique}", CancellationToken.None);
            updatedTask = await newContext.ToDos.AsNoTracking().Where(t => t.Id == taskId).SingleOrDefaultAsync() ?? throw new ArgumentNullException("Cannot get task");
            Assert.True(isUpdated);
            Assert.Equal($"Updated task title{unique}", updatedTask.Title);
            Assert.Equal(isCompleted, updatedTask.IsCompleted);
            Assert.Equal(userId, updatedTask.OwnerId);
            Assert.Equal(task.CreatedAt, updatedTask.CreatedAt);
        }
        finally
        {
            await TestDb.DeleteTestUserAsync(dataSource, unique, 1);
        }
    }

    [Fact]
    public async Task EfTaskUpdateTitle_ReturnsFalse_WhenTaskNotFound()
    {
        string str = Environment.GetEnvironmentVariable("TEST_DB_CONNECTION_STRING") ?? throw new InvalidOperationException("Test database connection string not found.");
        var options = new DbContextOptionsBuilder<ToDoEFContext>()
            .UseNpgsql(str)
            .Options;
        await using ToDoEFContext context = new(options);
        StorageEF storage = new(context);
        var isUpdated = await storage.EfTaskUpdateTitle(-1, $"Updated task title{Guid.NewGuid()}", CancellationToken.None);
        Assert.False(isUpdated);
    }
} 

