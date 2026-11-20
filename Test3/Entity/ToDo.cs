namespace Test3.Entity;

public class ToDoEntity
{
    public int Id { get; set; }
    public string Title { get; set; }
    public bool IsCompleted { get; set; }
    public int OwnerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Description { get; set; }
}