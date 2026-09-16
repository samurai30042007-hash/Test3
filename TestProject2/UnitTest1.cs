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

    [Fact]
    public void FindToDo_ShouldDiferentValues()
    {
        // Arrange

        var storage = new Storage();
        storage.Add("Dota 3");
        storage.Add("Minecraft");
        storage.Add("Roblox");
        // Act
        ToDo toDo = storage.FindToDo(0);
        //toDo.Title = "Dota 2";
        // toDo.IsCompleted = true;

        // Assert
        Assert.Equal(toDo, storage.FindToDo(0));
    }
    [Fact]
    public void FindToDos_ShouldDiferentValues()
    {
        var storage = new Storage();
        storage.Add("Dota 3");
        storage.Add("Minecraft");
        storage.Add("Roblox");

        var toDos = storage.ToDos;
        toDos[0].Title = "Dota 2";
        toDos[0].IsCompleted = true;

        Assert.NotEqual(toDos, storage.ToDos);
    }
}