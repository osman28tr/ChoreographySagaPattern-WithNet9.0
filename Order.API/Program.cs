using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Order.API;
using Order.API.Consumers;
using Shared;
using Shared.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("OrderDb")));

builder.Services.AddMassTransit(x =>
{
	x.AddConsumer<PaymentCompletedEventConsumer>();
	x.AddConsumer<PaymentFailedEventConsumer>();
	x.UsingRabbitMq((_context, _configurator) =>
	{
		_configurator.Host(builder.Configuration.GetConnectionString("RabbitMQ"));
		_configurator.ReceiveEndpoint(RabbitMQSettingsConst.OrderPaymentCompletedEventQueue, e =>
		{
			e.ConfigureConsumer<PaymentCompletedEventConsumer>(_context);
		});
		_configurator.ReceiveEndpoint(RabbitMQSettingsConst.OrderPaymentFailedEventQueue, e =>
		{
			e.ConfigureConsumer<PaymentFailedEventConsumer>(_context);
		});
	});
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
    pattern: "{controller=Orders}/{action=Create}/{id?}")
    .WithStaticAssets();

app.UseSwagger();
app.UseSwaggerUI();

app.Run();
