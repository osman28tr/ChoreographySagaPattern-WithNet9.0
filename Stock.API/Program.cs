using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared;
using Stock.API.Consumers;
using Stock.API.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
{
	options.UseInMemoryDatabase("StockDb");
});

builder.Services.AddMassTransit(x =>
{
	x.AddConsumer<OrderCreatedEventConsumer>();
	x.UsingRabbitMq((_context, _configurator) =>
	{
		_configurator.Host(builder.Configuration.GetConnectionString("RabbitMQ"));
		_configurator.ReceiveEndpoint(RabbitMQSettingsConst.StockOrderCreatedEventQueue, e =>
		{
			e.ConfigureConsumer<OrderCreatedEventConsumer>(_context);
		});
	});
});

using (var scope = builder.Services.BuildServiceProvider().CreateScope())
{
	var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
	dbContext.Stocks.AddRange(
		new Stock.API.Models.Stock { ProductId = 1, Count = 100 },
		new Stock.API.Models.Stock { ProductId = 2, Count = 150 }
	);
	dbContext.SaveChanges();
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
}
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}")
	.WithStaticAssets();


app.Run();
