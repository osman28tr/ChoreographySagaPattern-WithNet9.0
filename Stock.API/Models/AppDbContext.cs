using Microsoft.EntityFrameworkCore;

namespace Stock.API.Models
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Stock> Stocks { get; set; }
    }
}
