using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using MyBackgroundService;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        // This adds the appsettings.json file to the configuration pipeline
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    })
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;
        services.AddHostedService<Worker>();
        
        services.AddDbContext<CheckerDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("PostgresConnection")));

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = "demobroker:9092"
        };

        services.AddSingleton(producerConfig);
        
        services.AddSingleton<IProducer<Null, string>>(sp =>
        {
            var config = sp.GetRequiredService<ProducerConfig>();
            return new ProducerBuilder<Null, string>(config)
                .SetValueSerializer(Serializers.Utf8)
                .Build();
        });
    })
    .Build();

using (var scope = builder.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CheckerDbContext>();
    dbContext.Database.Migrate();  // Automatically apply migrations
}

await builder.RunAsync();
