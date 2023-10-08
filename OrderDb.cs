using Microsoft.EntityFrameworkCore;

class OrderDb : DbContext
{
    public OrderDb(DbContextOptions<OrderDb> options)
        : base(options) { }

    public DbSet<Order> Orders => Set<Order>();
}
