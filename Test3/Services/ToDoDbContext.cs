using Microsoft.EntityFrameworkCore;
using Test3.Entity;

namespace Test3.Services;

public class ToDoEFContext(DbContextOptions<ToDoEFContext> options) : DbContext(options)
{

    public DbSet<ToDoEntity> ToDos => Set<ToDoEntity>();
    public DbSet<User> UserDb => Set<User>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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
        {
            var entity = modelBuilder.Entity<User>();
            entity.ToTable("users", "public").ToTable(u => u.HasCheckConstraint("users_name_check", "length(trim(name)) > 0"));
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Id).UseIdentityAlwaysColumn().HasColumnName("id");
            entity.Property(u => u.Name).IsRequired().HasColumnName("name");
            entity.Property(u => u.Email).IsRequired().HasColumnName("email");
            entity.HasAlternateKey(u => u.Email).HasName("users_email_key");
        }

    }
}