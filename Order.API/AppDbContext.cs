using Microsoft.EntityFrameworkCore;
using Order.API.Models;

namespace Order.API
{
	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options):base(options)
		{
			
		}
		public DbSet<Order.API.Models.Order> Orders { get; set; }
		public DbSet<OrderItem> OrderItems { get; set; }
	}
}
