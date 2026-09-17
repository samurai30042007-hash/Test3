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
        int id = storage.Add("Dota 3");
        storage.Add("Minecraft");
        storage.Add("Roblox");
        // Act
        ToDo toDo = storage.FindToDo(id);
        toDo.Title = "Dota 2";
        toDo.IsCompleted = true;

        // Assert
        Assert.Equal("Dota 3", storage.FindToDo(id)?.Title); // НАсколько я помню ? спасет и проверит  null значение
        Assert.Equal(false, storage.FindToDo(id)?.IsCompleted);


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
        int id = toDos[0].Id;

        Assert.Equal("Dota 3", storage.FindToDo(id)?.Title); 
        Assert.Equal(false, storage.FindToDo(id)?.IsCompleted);
    }
    [Fact]
    public void FindToDos_ShouldDeleteValue()
    {
        var storage = new Storage();
        int id = storage.Add("Dota 3");
        storage.Add("Minecraft");
        storage.Add("Roblox");


        var toDos = storage.ToDos;
        toDos.Clear();
        Assert.Equal(0, toDos.Count);
        Assert.NotNull(storage.ToDos[0]);
    }
}