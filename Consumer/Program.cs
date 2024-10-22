using Confluent.Kafka;
using Consumer;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        // This adds the appsettings.json file to the configuration pipeline
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    })
    .ConfigureServices((context,services) =>
{
    var configuration = context.Configuration;
    services.AddHostedService<Worker>();
    services.AddDbContext<CheckerDbContext>(options =>
        options.UseNpgsql(configuration.GetConnectionString("PostgresConnection")));
    var producerConfig = new ConsumerConfig
    {
        GroupId = "test-consumer-group",
        BootstrapServers = "demobroker:9092",
        AutoOffsetReset = AutoOffsetReset.Earliest,
        EnableAutoCommit = false,
    };

    services.AddSingleton(producerConfig);
}).Build();
await builder.RunAsync();