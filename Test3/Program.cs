using Test3;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
Storage storage = new();
storage.Add("Task 1", false);
storage.Add( "Task 2", false);

app.MapGet("/ToDo", () =>
{
    return storage.ToDos;
});

app.MapGet("/ToDo/{id}", (int id) =>
{
    var task = storage.FindToDo(id);
    if (task is null)
    {
        return Results.NotFound();
    }
    return Results.Ok(task);
});

app.MapPost("/ToDo",(CreateTaskRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Title))
    {
        return Results.BadRequest("Title cannot be empty.");
    }
    int id = storage.Add(request.Title);
    return Results.Created($"/ToDo/{id}", storage.FindToDo(id));
});

app.MapPatch("/ToDo/{id}/Title", (int id, UpdateTitleTaskRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Title))
    {
        return Results.BadRequest("Title cannot be empty.");
    }
    try
    {
        storage.TryPatchTitle(id, request.Title);
    }
    catch (ArgumentNullException)
    {
        return Results.BadRequest("Title cannot be empty.");
    }
    catch (ArgumentException)
    {
         return Results.NotFound();
    }
    return Results.Ok(storage.FindToDo(id));    


});

app.MapPatch("/ToDo/{id}/IsCompleted", (int id, UpdateIsCompletedTaskRequest request) =>
{
    try
    {
        storage.TryPatchIsComplete(id, request.IsCompleted);
    }
    catch (ArgumentException)
    {
        return Results.NotFound();
    }

    return Results.Ok(storage.FindToDo(id));
    

});

app.MapPut("/ToDo/{id}", (int id, UpdateTaskRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Title))
    {
        return Results.BadRequest("Title cannot be empty.");
    }
    try
    {
        storage.TryPut(id, request.Title, request.IsCompleted);
    }
    catch (ArgumentNullException)
    {
        return Results.BadRequest("Title cannot be empty.");
    }
    catch (ArgumentException)
    {
        return Results.NotFound();
    }
    return Results.Ok(storage.FindToDo(id));
});

app.MapDelete("/ToDo/{id}", (int id) =>
{
    var deleted = storage.TryDelete(id);
    if (!deleted)
    {
        return Results.NotFound();
    }
    return Results.NoContent();
});

app.Run();
