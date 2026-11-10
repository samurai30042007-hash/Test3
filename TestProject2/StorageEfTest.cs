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
    [Fact]
    public async Task EfTaskUpdateIsCompleted_UpdatesTask()
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
        try
        {
            var userId = (int)(await TestDb.CreateTestUserAsync(dataSource, unique, 1) ?? throw new InvalidOperationException("Cannot create user"));
            var taskId = (int)(await TestDb.CreateTestToDoAsync(dataSource, unique, 1, userId) ?? throw new InvalidOperationException("Cannot create task"));
            var originalTask = await context.ToDos.AsNoTracking().SingleAsync(t => t.Id == taskId);

            Assert.True(await storage.EfTaskUpdateIsCompleted(taskId, true, CancellationToken.None));
            await using (var updatedContext = new ToDoEFContext(options))
            {
                var updatedTask = await updatedContext.ToDos.AsNoTracking().SingleAsync(t => t.Id == taskId);
                Assert.True(updatedTask.IsCompleted);
                Assert.Equal(originalTask.Title, updatedTask.Title);
                Assert.Equal(originalTask.OwnerId, updatedTask.OwnerId);
                Assert.Equal(originalTask.CreatedAt, updatedTask.CreatedAt);
            }

            Assert.True(await storage.EfTaskUpdateIsCompleted(taskId, false, CancellationToken.None));
            await using (var updatedContext = new ToDoEFContext(options))
            {
                var updatedTask = await updatedContext.ToDos.AsNoTracking().SingleAsync(t => t.Id == taskId);
                Assert.False(updatedTask.IsCompleted);
            }
            Assert.False(await storage.EfTaskUpdateIsCompleted(-1, true, CancellationToken.None));
        }
        finally
        {
            await TestDb.DeleteTestUserAsync(dataSource, unique, 1);
        }
    }
    [Fact]
    public async Task EfTaskDelete_DeletesTask()
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
            var result1 = await TestDb.CreateTestToDoAsync(dataSource, unique, 1, userId) ?? throw new ArgumentNullException("Cannot create task");
            var taskId = (int)result1;
            var isDeleted = await storage.EfTaskDelete(taskId, CancellationToken.None);
            Assert.True(isDeleted);
            await using var newContext = new ToDoEFContext(options);
            var deletedTask = await newContext.ToDos.AsNoTracking().Where(t => t.Id == taskId).SingleOrDefaultAsync();
            Assert.Null(deletedTask);
        }
        finally
        {
            await TestDb.DeleteTestUserAsync(dataSource, unique, 1);
        }
    }
    [Fact]
    public async Task EfTaskDelete_ReturnsFalse_WhenTaskNotFound()
    {
        string str = Environment.GetEnvironmentVariable("TEST_DB_CONNECTION_STRING") ?? throw new InvalidOperationException("Test database connection string not found.");
        var options = new DbContextOptionsBuilder<ToDoEFContext>()
            .UseNpgsql(str)
            .Options;
        await using ToDoEFContext context = new(options);
        StorageEF storage = new(context);
        var isDeleted = await storage.EfTaskDelete(-1, CancellationToken.None);
        Assert.False(isDeleted);
    }
    [Fact]
    public async Task EfTaskPut_UpdatesTask()
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
        int userId1;
        int userId2;
        try
        {
            var result = await TestDb.CreateTestUserAsync(dataSource, unique, 1) ?? throw new ArgumentNullException("Cannot create user");
            userId1 = (int)result;
            var res = await TestDb.CreateTestUserAsync(dataSource, unique, 2) ?? throw new ArgumentNullException("Cannot create user");
            userId2 = (int)res;
            var result1 = await TestDb.CreateTestToDoAsync(dataSource, unique, 1, userId1) ?? throw new ArgumentNullException("Cannot create task");
            var taskId = (int)result1;
            var task = await context.ToDos.AsNoTracking().Where(t => t.Id == taskId).SingleAsync() ?? throw new ArgumentNullException("Cannot get task");

            var updateResult = await storage.EfTaskPut(taskId, $"Updated task title{unique}", true, userId2, CancellationToken.None);
            Assert.Equal(MessegStoreg.Ok, updateResult);
            await using var newContext = new ToDoEFContext(options);
            var updatedTask = await newContext.ToDos.AsNoTracking().Where(t => t.Id == taskId).SingleAsync() ?? throw new ArgumentNullException("Cannot get task");
            Assert.Equal(task.CreatedAt, updatedTask.CreatedAt);
            Assert.Equal(taskId, updatedTask.Id);
            Assert.Equal($"Updated task title{unique}", updatedTask.Title);
            Assert.True(updatedTask.IsCompleted);
            Assert.Equal(userId2, updatedTask.OwnerId);
            var updateResult2 = await storage.EfTaskPut(taskId, $"Updated task title{unique}", true, userId2, CancellationToken.None);
            Assert.Equal(MessegStoreg.Ok, updateResult2);
        }
        finally
        {
            await TestDb.DeleteTestUserAsync(dataSource, unique, 1);
            await TestDb.DeleteTestUserAsync(dataSource, unique, 2);
        }
    }
    [Fact]
    public async Task EfTaskPut_ReturnsForeignKeyViolation()
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
            var Id = await TestDb.CreateTestToDoAsync(dataSource, unique, 1, userId) ?? throw new ArgumentNullException("Cannot create task");
            var task = await context.ToDos.AsNoTracking().Where(t => t.Id == (int)Id).SingleAsync() ?? throw new ArgumentNullException("Cannot get task");

            result = await TestDb.CreateTestUserAsync(dataSource, unique, 2) ?? throw new ArgumentNullException("Cannot delete user");
            userId = (int)result;
            await TestDb.DeleteTestUserAsync(dataSource, unique, 2);

            var res = await storage.EfTaskPut(task.Id, $"Updated task title{unique}", true, userId, CancellationToken.None);
            Assert.Equal(MessegStoreg.ForeignKeyViolation, res);
            var updatedTask = await context.ToDos.AsNoTracking().Where(t => t.Id == task.Id).SingleOrDefaultAsync();
            Assert.NotNull(updatedTask);
            Assert.Equal(task.Title, updatedTask.Title);
            Assert.Equal(task.IsCompleted, updatedTask.IsCompleted);
            Assert.Equal(task.OwnerId, updatedTask.OwnerId);
            Assert.Equal(task.CreatedAt, updatedTask.CreatedAt);
            Assert.Equal(task.Id, updatedTask.Id);
        }
        finally
        {
            for (int i = 0; i < 2; i++)
            {
                await TestDb.DeleteTestUserAsync(dataSource, unique, i + 1);

            }
        }

    }
    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public async Task EfTaskPut_ReturnsArgumentException_WhenOwnerIdIsInvalid(int value)
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
            var result1 = await TestDb.CreateTestToDoAsync(dataSource, unique, 1, userId) ?? throw new ArgumentNullException("Cannot create task");
            var taskId = (int)result1;
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await storage.EfTaskPut(taskId, $"Updated task title{unique}", true, value, CancellationToken.None);
            });
        }
        finally
        {
            await TestDb.DeleteTestUserAsync(dataSource, unique, 1);
        }
    }
    [Fact]
    public async Task EfTaskPut_ReturnsNotFound_WhenTaskNotFound()
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
            var res = await storage.EfTaskPut(-1, $"Updated task title{unique}", true, userId, CancellationToken.None);
            Assert.Equal(MessegStoreg.NotFound, res);
        }
        finally
        {
            await TestDb.DeleteTestUserAsync(dataSource, unique, 1);
        }
    }
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task EfTaskPut_ReturnsArgumentException_WhenTitleIsInvalid(string? title)
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
            var result1 = await TestDb.CreateTestToDoAsync(dataSource, unique, 1, userId) ?? throw new ArgumentNullException("Cannot create task");
            var taskId = (int)result1;
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await storage.EfTaskPut(taskId, title, true, userId, CancellationToken.None);
            });
        }
        finally
        {
            await TestDb.DeleteTestUserAsync(dataSource, unique, 1);
        }
    }
    [Fact]
    public async Task EfCompletedCloseTask_ReturnsCorrectCount()
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
            var res2 = await TestDb.CreateTestUserAsync(dataSource, unique, 2) ?? throw new ArgumentNullException("Cannot create user");
            userId = (int)result;
            var taskIds = new List<int>();
            for (int i = 0; i < 3; i++)
            {
                var result2 = await TestDb.CreateTestToDoAsync(dataSource, unique, i, userId) ?? throw new ArgumentNullException("Cannot create task");
                taskIds.Add((int)result2);
            }
            var result1 = await TestDb.CreateTestToDoAsync(dataSource, unique, 3, userId, true) ?? throw new ArgumentNullException("Cannot create task");
            taskIds.Add((int)result1);
            var _ = await TestDb.CreateTestToDoAsync(dataSource, unique, 4, (int)res2) ?? throw new ArgumentNullException("Cannot create task");
            var count = await storage.EfCompletedCloseTask(userId, CancellationToken.None);
            Assert.Equal(3, count);
            await using var newContext = new ToDoEFContext(options);
            for (int i = 0; i < 4; i++)
            {
                var tasks = await newContext.ToDos.Where(t => t.Id == taskIds[i]).SingleOrDefaultAsync() ?? throw new InvalidOperationException("Task not found");
                Assert.True(tasks.IsCompleted);
                Assert.Equal($"task{unique}{i}", tasks.Title);
                Assert.Equal(tasks.Id, taskIds[i]);
                Assert.Equal(userId, tasks.OwnerId);
            }
            count = await storage.EfCompletedCloseTask(userId, CancellationToken.None);
            Assert.Equal(0, count);
        }
        finally
        {
            await TestDb.DeleteTestUserAsync(dataSource, unique, 1);
            await TestDb.DeleteTestUserAsync(dataSource, unique, 2);
        }
    }
    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public async Task EfCompletedCloseTask_ThrowsArgumentException_WhenOwnerIdIsInvalid(int value)
    {
        string str = Environment.GetEnvironmentVariable("TEST_DB_CONNECTION_STRING") ?? throw new InvalidOperationException("Test database connection string not found.");
        await using NpgsqlDataSource dataSource = NpgsqlDataSource.Create(str);
        await TestDb.AssertTestDatabaseAsync(dataSource);
        var options = new DbContextOptionsBuilder<ToDoEFContext>()
            .UseNpgsql(str)
            .Options;
        await using ToDoEFContext context = new(options);
        StorageEF storage = new(context);
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await storage.EfCompletedCloseTask(value, CancellationToken.None);
        });
    }
} 

