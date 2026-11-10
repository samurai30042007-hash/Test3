using Microsoft.EntityFrameworkCore;
using Npgsql;
using Test3;

var builder = WebApplication.CreateBuilder(args);
var str = builder.Configuration.GetConnectionString("ToDoDb") ?? throw new Exception(message: "Not find db config");
builder.Services.AddDbContext<ToDoEFContext>(options => options.UseNpgsql(str));
builder.Services.AddSingleton<NpgsqlDataSource>(_ => NpgsqlDataSource.Create(str));
builder.Services.AddScoped<StorageEF>();
builder.Services.AddScoped<Storage>();
builder.Services.AddProblemDetails();
//builder.Services.AddDbContext

var app = builder.Build();
app.UseExceptionHandler();

app.MapPut("/EfToDos/{id}", async (int id, UpdateTaskRequest request, StorageEF storage, CancellationToken cancellation) =>
{
    var result = await storage.EfTaskPut(id, request.Title, request.IsCompleted, request.OwnerId, cancellation);
    return result switch
    {
        MessegStoreg.Ok => Results.Ok(),
        MessegStoreg.NotFound => Results.NotFound(),
        MessegStoreg.ForeignKeyViolation => Results.BadRequest("Foreign key violation."),
        _ => Results.Problem("An error occurred while updating the task.") // Что это делает?
    };
});
app.MapDelete("/EFToDos/{id}", async (int id, StorageEF storage, CancellationToken cancellation) =>
{
    bool isDeleted = await storage.EfTaskDelete(id, cancellation);
    if (!isDeleted)
    {
        return Results.NotFound();
    }
    return Results.NoContent();
});
app.MapPatch("/ToDosEF/{id}/Title", async (int id, UpdateTitleTaskRequest request, StorageEF storage, CancellationToken cancellation) =>
{
    if (string.IsNullOrWhiteSpace(request.Title))
    {
        return Results.BadRequest("Title cannot be empty.");
    }
    bool isUpadte = await storage.EfTaskUpdateTitle(id, request.Title, cancellation);
    if (!isUpadte)
    {
        return Results.NotFound();
    }
    return Results.Ok();
});
app.MapPost("/ToDosEF", async (CreateTaskRequest request, StorageEF storage, CancellationToken cancellation) =>
{
    if (string.IsNullOrWhiteSpace(request.Title))
    {
        return Results.BadRequest("Title cannot be empty.");
    }
    if (request.OwnerId <= 0)
    {
        return Results.BadRequest("OwnerId must be greater than zero.");
    }
    return Results.Ok(await storage.EfTaskCreate(request.Title, request.OwnerId, false, cancellation));
});
app.MapGet("/ToDosEF/{id}", async (int id, StorageEF storage, CancellationToken cancellation) =>
{
    var tasks = await storage.EfTaskReader(id , cancellation);
    if (tasks is null)
    {
        return Results.NotFound();
    }
    return Results.Ok(tasks);
});
app.MapGet("/ToDosEF/OwnerId/{ownerId}", async (StorageEF storage, CancellationToken cancellation, int ownerId, bool? isCompleted) =>
{
    List<ToDoEntity> ToDos;
    try
    {
        ToDos = await storage.EfToDosFindByOwner(cancellation, (int)ownerId, isCompleted);
    }
    catch (ArgumentException)
    {
        return Results.BadRequest();
    }
    return Results.Ok(ToDos);
});
app.MapGet("/ToDosEF/open", async (StorageEF storage, CancellationToken cancellation, int? limit, int? ownerId) =>
{
    if (limit == null)
    {
        return Results.BadRequest();
    }
    if (ownerId == null)
    {
        return Results.BadRequest();

    }
    List<ToDoEntity> ToDos;
    try
    {
        ToDos = await storage.FindOpenTodosEf((int)limit, (int)ownerId, cancellation);
    }
    catch (ArgumentException)
    {
        return Results.BadRequest();
    }
    return Results.Ok(ToDos);
});
app.MapPatch("/ToDoEf/Completed/{ownerId}", async (int ownerId, StorageEF storage, CancellationToken cancellation) =>
{
    if (ownerId <= 0)
    {
        return Results.BadRequest();
    }
    var count = await storage.EfCompletedCloseTask(ownerId, cancellation);
    return Results.Ok(count);
});


app.MapGet("/ToDos/open", async (StorageEF storage, CancellationToken cancellation, int? limit, int? ownerId) =>
{
    if (limit == null)
    {
        return Results.BadRequest();
    }
    if (ownerId == null)
    {
        return Results.BadRequest();

    }
    List<ToDoEntity> ToDos;
    try
    {
        ToDos = await storage.FindOpenTodosEf((int)limit, (int)ownerId, cancellation);
    }
    catch (ArgumentException)
    {
        return Results.BadRequest();
    }
    return Results.Ok(ToDos);
});
app.MapGet("/ToDos", async(StorageEF storage, CancellationToken cancellation, int? ownerId) =>
{
    if (ownerId <= 0)
    {
        return Results.Ok(new List<ToDoEntity>());
    }
    if (ownerId == null)
    {
        var tasks = await storage.EfTaskReaderAll(cancellation);
        return Results.Ok(tasks);
    }
    else
    {
        var tasks = await storage.EfToDosFindByOwner(cancellation, (int)ownerId, null);
        return Results.Ok(tasks);
    }

});
app.MapGet("/ToDo/{id}", async (int id, StorageEF storage, CancellationToken cancellation) =>
{
    var task = await storage.EfTaskReader(id, cancellation);
    if (task is null)
    {
        return Results.NotFound();
    }
    return Results.Ok(task);
});
app.MapPost("/ToDo",  async (CreateTaskRequest request, StorageEF storage, CancellationToken cancellation) =>
{
    if (string.IsNullOrWhiteSpace(request.Title))
    {
        return Results.BadRequest("Title cannot be empty.");
    }
    ToDoEntity? task;
    try
    {
        task = await storage.EfTaskCreate(request.Title, request.OwnerId, false, cancellation);
    }
    catch(ArgumentException)
    {
        task = null;
    }
    if (task is null)
    {
        return Results.BadRequest("Failed to create task. Invalid OwnerId or other issue.");
    }
    return Results.Created($"/ToDo/{task.Id}", task); 
});
app.MapPatch("/ToDo/{id}/Title", async (int id, UpdateTitleTaskRequest request, StorageEF storage, CancellationToken cancellation) =>
{
    if (string.IsNullOrWhiteSpace(request.Title))
    {
        return Results.BadRequest("Title cannot be empty.");
    }
    if (!await storage.EfTaskUpdateTitle(id, request.Title, cancellation))
    {
        return Results.NotFound("Invalid id value");
    }
    
    return Results.Ok(await storage.EfTaskReader(id, cancellation));


});
app.MapPatch("/ToDo/{id}/IsCompleted", async (int id, UpdateIsCompletedTaskRequest request, StorageEF storage, CancellationToken cancellation) =>
{

    if (!await storage.EfTaskUpdateIsCompleted(id, request.IsCompleted, cancellation))
    {
        return Results.NotFound("Invalid id value");
    }
    return Results.Ok(await storage.EfTaskReader(id, cancellation));
});
app.MapPut("/ToDo/{id}", async (int id, UpdateTaskRequest request, StorageEF storage, CancellationToken cancellation) =>
{
    if (string.IsNullOrWhiteSpace(request.Title))
    {
        return Results.BadRequest("Title cannot be empty.");
    }
    if (request.OwnerId <= 0)
    {
        return Results.BadRequest("OwnerId must be greater than zero.");
    }
    var result = await storage.EfTaskPut(id, request.Title, request.IsCompleted, request.OwnerId, cancellation);
    switch (result) // вообще я только понял что по хорошему нужно передавать request
    {
        case MessegStoreg.Ok:
        {
                return Results.Ok(await storage.EfTaskReader(id, cancellation));
        }
        case MessegStoreg.ForeignKeyViolation:
        {
                return Results.BadRequest();
        }
        case MessegStoreg.NotFound:
        {
                return Results.NotFound();
        }
        default:
                throw new ArgumentException("Invalid MessegStoreg value");
    }
});
app.MapDelete("/ToDo/{id}", async (int id, StorageEF storage, CancellationToken cancellation) =>
{
    if (!await storage.EfTaskDelete(id, cancellation))
    {
        return Results.NotFound("Invalid id value");
    }
    return Results.NoContent();
});

app.Run();
