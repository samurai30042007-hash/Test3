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
            var entity = modelBuilder.Entity<User>();
            entity.ToTable("users", "public").ToTable(u => u.HasCheckConstraint("users_name_check", "length(trim(name)) > 0"));
            entity.HasKey(u => u.Id).HasName("users_pkey");
            entity.Property(u => u.Id).UseIdentityAlwaysColumn().HasColumnName("id");
            entity.Property(u => u.Name).IsRequired().HasColumnName("name");
            entity.Property(u => u.Email).IsRequired().HasColumnName("email");
            entity.HasAlternateKey(u => u.Email).HasName("users_email_key");
        }
        {
            var entity = modelBuilder.Entity<ToDoEntity>();
            entity.ToTable("tasks", "public").ToTable(t => t.HasCheckConstraint("tasks_title_check", "length(trim(title)) > 0"));
            entity.HasKey(t => t.Id).HasName("tasks_pkey");
            entity.Property(t => t.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(t => t.Title).HasColumnName("title").IsRequired();
            entity.Property(t => t.IsCompleted).HasColumnName("iscompleted").IsRequired().HasDefaultValue(false);
            entity.Property(t => t.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
            entity.Property(t => t.Description).HasColumnName("description").HasMaxLength(500);
            entity.Property(t => t.OwnerId).HasColumnName("owner_id").IsRequired();
            entity.HasOne<User>().WithMany()
                .HasForeignKey(t => t.OwnerId)
                .HasConstraintName("tasks_owner_id_fkey")
                .OnDelete(DeleteBehavior.Cascade);
        }
        

    }
}