using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using SagaStateMachineWorkerService;
using SagaStateMachineWorkerService.Models;
using Shared;
using System.Reflection;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

var connectionString = builder.Configuration.GetConnectionString("OrderStateCon");

//MasTransit Configuration
builder.Services.AddMassTransit(cfg =>
{
	//State Machine Configuration
	cfg.AddSagaStateMachine<OrderStateMachine, OrderStateInstance>().EntityFrameworkRepository(opt =>
	{
		opt.AddDbContext<DbContext, OrderStateDbContext>((provider, builder) =>
		{
			builder.UseSqlServer(connectionString, m =>
			{
				m.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
			});
		});
	});

	//RabbitMQ Configuration and it's connection with state machine
	cfg.UsingRabbitMq((context, rabbit) =>
	{
		rabbit.Host(builder.Configuration.GetConnectionString("RabbitMQ"));

		rabbit.ReceiveEndpoint(RabbitMQSettingsConst.OrderSaga, e =>
		{
			e.ConfigureSaga<OrderStateInstance>(context); //Create an object instance from OrderStateInstance
		});

	});
});

var host = builder.Build();
host.Run();
