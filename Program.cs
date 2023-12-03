using System.Text.Json.Serialization;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

var app = builder.Build();

var todoTasks = new List<TodoTask>
{
    new(1, "Walk the dog", "Take the dog for a walk around the block"),
    new(2, "Do the dishes", "Make sure to do all the dishes in the sink", DateTime.Now.AddDays(1)),
    new(3, "Do the laundry", "Make sure to do all the laundry in the basket", DateTime.Now.AddDays(3)),
    new(4, "Clean the bathroom", "Make sure to clean the bathroom", DateTime.Now.AddDays(5)),
    new(5, "Clean the car", "Make sure to clean the car", DateTime.Now.AddDays(7))
};

var todosApi = app.MapGroup("/tasks");

todosApi.MapGet("/", () => todoTasks);

todosApi.MapGet("/{id}", (int id) =>
    todoTasks.FirstOrDefault(a => a.Id == id) is { } todo
        ? Results.Ok(todo)
        : Results.NotFound());

todosApi.MapPost("/", (TodoTaskDto todoDto) =>
{
    var newId = todoTasks.Max(a => a.Id) + 1;
    var task = new TodoTask(newId, todoDto.Title, todoDto.Description, todoDto.DueBy);
    
    todoTasks.Add(task);
    return Results.Created($"/tasks/{task.Id}", task);
});

todosApi.MapDelete("/{id}", (int id) =>
{
    var todo = todoTasks.FirstOrDefault(a => a.Id == id);
    if (todo is null)
    {
        return Results.NotFound();
    }

    todoTasks.Remove(todo);
    return Results.NoContent();
});

todosApi.MapPut("/{id}", (int id, UpdateTaskDto updateTaskDto) =>
{
    var task = todoTasks.FirstOrDefault(a => a.Id == id);

    if (task is null)
    {
        var newId = todoTasks.Max(a => a.Id) + 1;
        var newTask = new TodoTask(newId, updateTaskDto.Title, updateTaskDto.Description, updateTaskDto.DueBy);
    
        todoTasks.Add(newTask);
        
        return Results.Created($"/tasks/{newTask.Id}", newTask);
    }
    
    task = task with
    {
        Title = updateTaskDto.Title,
        Description = updateTaskDto.Description,
        DueBy = updateTaskDto.DueBy,
        Completed = updateTaskDto.Completed
    };
    
    return Results.Ok(task);
});

todosApi.MapPost("/{id}/complete", (int id) =>
{
    var task = todoTasks.FirstOrDefault(a => a.Id == id);

    if (task is null)
    {
        return Results.NotFound();
    }

    task = task with { Completed = true };
    
    return Results.Ok(task);
});

app.Run();

public record TodoTask (int Id, string? Title, string? Description, DateTime? DueBy = null, bool Completed = false);

public record TodoTaskDto (string? Title, string? Description, DateTime? DueBy = null);

public record UpdateTaskDto (string? Title, string? Description, DateTime? DueBy = null, bool Completed = false);

[JsonSerializable(typeof(TodoTask[]))]
[JsonSerializable(typeof(List<TodoTask>))]
[JsonSerializable(typeof(TodoTaskDto))]
[JsonSerializable(typeof(UpdateTaskDto))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}