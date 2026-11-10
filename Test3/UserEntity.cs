using Microsoft.EntityFrameworkCore;


namespace Test3
{

    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions options) : base(options) { }
        public DbSet<UserEntity> UserDb => Set<UserEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<UserEntity>();
            entity.ToTable("users", "public").ToTable(u => u.HasCheckConstraint("users_name_check", "length(trim(name)) > 0"));
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Id).UseIdentityAlwaysColumn().HasColumnName("id");
            entity.Property(u => u.Name).IsRequired().HasColumnName("name");
            entity.Property(u => u.Email).IsRequired().HasColumnName("email");
            entity.HasAlternateKey(u => u.Email).HasName("users_email_key");
        }
    }
    public class UserEntity
    {
        public int Id;
        public string Name;
        public string Email;
    }
}
