using Microsoft.EntityFrameworkCore;
namespace Test3;



public class StorageEF
{
    private readonly ToDoEFContext _context;
    public StorageEF(ToDoEFContext context)
    {
        _context = context;
    }
    public async Task<ToDoEntity?> EfTaskReader(int id, CancellationToken cancellation)
    {
        return await _context.ToDos.AsNoTracking().Where(t => t.Id == id ).FirstOrDefaultAsync(cancellation);
    }
    public async Task<List<ToDoEntity>> EfToDosFindByOwner(CancellationToken cancellation, int ownerId, bool? isCompleted)
    {
        if (ownerId <= 0)
        {
            throw new ArgumentException("OwnerId must be greater than zero.", nameof(ownerId));
        }
        return await _context.ToDos.AsNoTracking()
            .Where(t => t.OwnerId == ownerId && (isCompleted == null || t.IsCompleted == isCompleted))
            .OrderBy(t => t.Id)
            .ToListAsync(cancellation);
    }
    public async Task<List<ToDoEntity>> FindOpenTodosEf(int limit, int ownerId, CancellationToken cancellation)
    {
        if (limit <= 0)
        {
            throw new ArgumentException("Limit must be greater than zero.", nameof(limit));
        }
        if (ownerId <= 0)
        {
            throw new ArgumentException("OwnerId must be greater than zero.", nameof(ownerId));
        }
        return await _context.ToDos.AsNoTracking()
            .Where(t => t.OwnerId == ownerId && !t.IsCompleted).
            OrderByDescending(t => t.Id)
            .Take(limit)
            .ToListAsync(cancellation);
    }
    public async Task<ToDoEntity> EfTaskCreate(string title, int ownerId, bool isCompleted, CancellationToken cancellation)
    {
        var Entity = new ToDoEntity
        {
            Title = title,
            OwnerId = ownerId,
            IsCompleted = isCompleted
        };
        _context.ToDos.Add(Entity);
        int result = await _context.SaveChangesAsync(cancellation);
        if (result <= 0)
        {
            throw new Exception("Failed to create the task.");
        }
        var task = await _context.ToDos.AsNoTracking().Where(t => t.Id == result).SingleOrDefaultAsync();

        return task ?? throw new Exception("Failed to retrieve the created task.");

    }
}
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
        entity.Property(t => t.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}


public class ToDoEntity
{
    public int Id { get; set; }
    public string Title { get; set; }
    public bool IsCompleted { get; set; }
    public int OwnerId { get; set; }
    public DateTime CreatedAt { get; set; }
}

