using Npgsql;
using Test3;

var builder = WebApplication.CreateBuilder(args);
var str = builder.Configuration.GetConnectionString("ToDoDb");
builder.Services.AddSingleton<NpgsqlDataSource>(_ => str is null ? throw new Exception (message: "Not find db config") : NpgsqlDataSource.Create(str));
builder.Services.AddScoped<Storage>();
builder.Services.AddScoped<LifeTimeProbe>();
builder.Services.AddTransient<ReadProbe>();
builder.Services.AddScoped<UniqueId>();
builder.Services.AddScoped<Log>();
builder.Services.AddScoped<Report>();
//builder.Services.AddDbContext

var app = builder.Build();

app.Use(async (context, next) =>
{
    try // заготовка к логеру
    {
        await next();
    }
    catch (Exception)
    {

        throw;
    }
});


app.MapGet("/ToDo/{id}", async (int id, Storage storage, CancellationToken cancellation) =>
{
    var task = await storage.FindToDo(id, cancellation);
    if (task is null)
    {
        return Results.NotFound();
    }
    return Results.Ok(task);
});

app.MapPost("/ToDo",  async (CreateTaskRequest request, Storage storage, CancellationToken cancellation) =>
{
    if (string.IsNullOrWhiteSpace(request.Title))
    {
        return Results.BadRequest("Title cannot be empty.");
    }
    int? id = await storage.Add(request.Title, request.OwnerId, cancellation);
    if (id is null)
    {
        return Results.BadRequest("Invalid owner id value");
    }
    return Results.Created($"/ToDo/{id}", await storage.FindToDo((int)id, cancellation)); 
});

app.MapPatch("/ToDo/{id}/Title", async (int id, UpdateTitleTaskRequest request, Storage storage, CancellationToken cancellation) =>
{
    if (string.IsNullOrWhiteSpace(request.Title))
    {
        return Results.BadRequest("Title cannot be empty.");
    }
    if (!await storage.TryPatchTitle(id, request.Title, cancellation))
    {
        return Results.BadRequest("Invalid id value");
    }
    
    return Results.Ok(await storage.FindToDo(id, cancellation));


});

app.MapPatch("/ToDo/{id}/IsCompleted", async (int id, UpdateIsCompletedTaskRequest request, Storage storage, CancellationToken cancellation) =>
{
    
    if (!await storage.TryPatchIsComplete(id, request.IsCompleted, cancellation))
    {
        return Results.BadRequest("Invalid id value");
    }
    
    
    return Results.Ok(await storage.FindToDo(id, cancellation));


});

app.MapPut("/ToDo/{id}", async (int id, UpdateTaskRequest request, Storage storage, CancellationToken cancellation) =>
{
    if (string.IsNullOrWhiteSpace(request.Title))
    {
        return Results.BadRequest("Title cannot be empty.");
    }
    if (!await storage.TryPut(id, request.Title, request.IsCompleted, request.OwnerId, cancellation))
    {
        return Results.BadRequest("Invalid id value");
    }

    return Results.Ok(await storage.FindToDo(id, cancellation));
});

app.MapDelete("/ToDo/{id}", async (int id, Storage storage, CancellationToken cancellation) =>
{
    if (!await storage.TryDelete(id, cancellation))
    {
        return Results.NotFound("Invalid id value");
    }
    return Results.NoContent();
});

app.MapGet("di-probe", (LifeTimeProbe first, LifeTimeProbe second) =>
{
    return new { First = first.Id, Second = second.Id };
});
app.MapGet("di-constructor", (ReadProbe probe, LifeTimeProbe lifeProbe) =>
{
    return new { Probe = probe.Read(), LifeProbe = lifeProbe.Id };
});
app.MapGet("Test", (UniqueId uniqueId, Log log, Report report) =>
{
    return new { LogMessage = log.LogMessage(), ReportMessage = report.ReportMessage() };
});

app.Run();
