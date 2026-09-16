using Test3;

namespace Test3.Tests;

public class StorageTests
{
    [Fact]
    public void Add_ShouldCreateTodo()
    {
        // Arrange
        var storage = new Storage();

        // Act
        int id = storage.Add("Learn ASP.NET");
        var todo = storage.FindToDo(id);

        // Assert
        Assert.NotNull(todo);
        Assert.Equal("Learn ASP.NET", todo.Title);
        Assert.False(todo.IsCompleted);
    }
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Add_ShouldException(string title)
    {
        var storage = new Storage();

        Assert.Throws<ArgumentNullException>(() => storage.Add(title));
    }

}