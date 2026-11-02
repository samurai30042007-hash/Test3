using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Npgsql;
using System.ComponentModel;
using System.Reflection.Metadata.Ecma335;
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
        return await _context.ToDos.AsNoTracking().Where(t => t.Id == id).FirstOrDefaultAsync(cancellation);
    }
    public async Task<List<ToDoEntity>> EfTaskReaderAll(CancellationToken cancellation)
    {
        return await _context.ToDos.AsNoTracking().OrderBy(t => t.Id).ToListAsync(cancellation);
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
    public async Task<ToDoEntity?> EfTaskCreate(string title, int ownerId, bool isCompleted, CancellationToken cancellation)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Wrong value. Title is null or whitespace");
        }
        else if (ownerId <= 0)
        {
            throw new ArgumentException("Wrong value. OwnerId should be graeter than 0");
        }
        var Entity = new ToDoEntity
        {
            Title = title,
            OwnerId = ownerId,
            IsCompleted = isCompleted
        };
        _context.ToDos.Add(Entity);
        int result;
        try
        {
            result = await _context.SaveChangesAsync(cancellation);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException exPg && exPg.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            return null;
        }
        if (result <= 0)
        {
            throw new Exception("Failed to create the task.");
        }

        return Entity;

    }
    public async Task<bool> EfTaskUpdateTitle(int id, string title, CancellationToken cancellation)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title cannot be null or whitespace.", nameof(title));
        }
        var task = await _context.ToDos.Where(t => t.Id == id).SingleOrDefaultAsync(cancellation);
        if (task is null)
        {
            return false;
        }
        task.Title = title;
        await _context.SaveChangesAsync(cancellation);
        return true;
    }
    public async Task<bool> EfTaskUpdateIsCompleted(int id, bool isCompleted, CancellationToken cancellation)
    {
        var task = await _context.ToDos.Where(t => t.Id == id).SingleOrDefaultAsync(cancellation);
        if (task is null)
        {
            return false;
        }
        task.IsCompleted = isCompleted;
        await _context.SaveChangesAsync(cancellation);
        return true;
    }
    public async Task<bool> EfTaskDelete(int id,CancellationToken cancellation)
    {
        var task = await _context.ToDos.Where(t => t.Id == id).SingleOrDefaultAsync(cancellation);
        if (task is null)
        {
            return false;
        }
        _context.ToDos.Remove(task);
        await _context.SaveChangesAsync(cancellation);
        return true;
    }
    public async Task<MessegStoreg> EfTaskPut(int id, string? title, bool isCompleted, int ownerId, CancellationToken cancellation)
    {
        try
        {
            
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException("Title cannot be null or whitespace.", nameof(title));
            }
            if (ownerId <= 0)
            {
                throw new ArgumentException("OwnerId cannot be less than or equal to 0.", nameof(ownerId));
            }
            var task = await _context.ToDos.Where(t => t.Id == id).SingleOrDefaultAsync(cancellation);
            if (task is null)
            {
                return MessegStoreg.NotFound;
            }
            task.Title = title;
            task.IsCompleted = isCompleted;
            task.OwnerId = ownerId;
            await _context.SaveChangesAsync(cancellation);
            return MessegStoreg.Ok;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException exPg && exPg.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            return MessegStoreg.ForeignKeyViolation;
        }
        catch
        {
            throw;
        }
    }
    public async Task<int> EfCompletedCloseTask(int ownerId,  CancellationToken cancellation)
    {
        if (!(ownerId > 0))
        {
            throw new ArgumentException();
        }
        
        var tasks = await _context.ToDos.Where(t => t.OwnerId == ownerId && !t.IsCompleted).ToListAsync(cancellation);

        foreach (var task in tasks)
        {
            task.IsCompleted = true;
        }
        var count = tasks.Count;
        await _context.SaveChangesAsync(cancellation);

        //var count = await _context.ToDos.Where(t => t.OwnerId == ownerId && !t.IsCompleted).ExecuteUpdateAsync(s => s.SetProperty(t => t.IsCompleted, true), cancellation);
        return count;
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

