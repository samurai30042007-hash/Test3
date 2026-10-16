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
        return await _context.ToDos.AsNoTracking().Where(t => t.Id == id).FirstOrDefaultAsync(cancellation);
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
            .Where(t => t.OwnerId == ownerId && !t.IsCompleted)
            .OrderBy(t => t.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellation);
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
            entity.Property(t => t.CreatedAt).HasColumnName("created_at");
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
}
