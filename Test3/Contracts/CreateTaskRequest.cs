namespace Test3.Contracts;

public class CreateTaskRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int OwnerId { get; set; }
}