using Microsoft.EntityFrameworkCore;
namespace Test3;

public class ToDoEFContext : DbContext
{
    public ToDoEFContext(DbContextOptions<ToDoEFContext> options) : base(options)
    {
    }

    public DbSet<ToDoEntity> ToDos => Set<ToDoEntity>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<ToDoEntity>();
        entity.ToTable("tasks", "public");
        entity.HasKey(t => t.Id);
        entity.Property(t => t.Id).HasColumnName("id");
        entity.Property(t => t.Title).HasColumnName("title");
        entity.Property(t => t.IsCompleted).HasColumnName("iscompleted");
        entity.Property(t => t.OwnerId).HasColumnName("owner_id");
        entity.Property(t => t.createdAt).HasColumnName("created_at");
    }
}


public class ToDoEntity
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public bool IsCompleted { get; set; }
    public int OwnerId { get; set; }
    public DateTime createdAt { get; set; }
}
