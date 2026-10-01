using Microsoft.EntityFrameworkCore;

namespace Test3;

public class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
{
}
