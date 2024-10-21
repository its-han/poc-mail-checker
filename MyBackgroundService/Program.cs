using Confluent.Kafka;
using MyBackgroundService;

var builder = Host.CreateDefaultBuilder(args).ConfigureServices(services =>
    {
        services.AddHostedService<MyBackgroundService.Worker>();

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

await builder.RunAsync();
