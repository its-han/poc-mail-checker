using Confluent.Kafka;
using Consumer;

var builder = Host.CreateDefaultBuilder(args).ConfigureServices(services =>
{
    services.AddHostedService<Worker>();
    var producerConfig = new ConsumerConfig
    {
        GroupId = "test-consumer-group",
        BootstrapServers = "demobroker:9092",
        AutoOffsetReset = AutoOffsetReset.Earliest
    };

    services.AddSingleton(producerConfig);
}).Build();
await builder.RunAsync();