namespace Test3.Contracts;
public class UpdateTaskRequest
{
    public string? Title { get; set; }
    public bool IsCompleted { get; set; }
    public int OwnerId { get; set; }
    public string? Description { get; set; }
}