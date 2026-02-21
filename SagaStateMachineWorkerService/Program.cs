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
	cfg.UsingRabbitMq((_provider, _configurator) => Bus.Factory.CreateUsingRabbitMq(configure =>
	{
		configure.Host(builder.Configuration.GetConnectionString("RabbitMQ"));

		//The part that will trigger the state machine when the OrderCreatedRequestEvent arrives.
		configure.ReceiveEndpoint(RabbitMQSettingsConst.OrderSaga,e =>
		{
			e.ConfigureSaga<OrderStateInstance>(_provider); //Create an order created request record in the OrderStateInstance table.
		});
	}));
});

var host = builder.Build();
host.Run();
