using Confluent.Kafka;
using Prometheus;

namespace Consumer;

public class Worker : IHostedLifecycleService
{
    private const string TopicMailDownload = "mail-download";
    
    private readonly ConsumerConfig _consumerConfig;
    private IConsumer<Ignore, string> _consumer;
    private CancellationTokenSource _cts;
    private static readonly Counter MailConsumedCounter = Metrics.CreateCounter("mail_consumer", "Total number of mails consumed.");
    private MetricServer _metricServer;

    public Worker(ConsumerConfig consumerConfig)
    {
        _consumerConfig = consumerConfig;
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Starting Kafka Consumer...");
        
        _metricServer = new MetricServer(port: 7002);  
        _metricServer.Start();
        
        // _consumer = new ConsumerBuilder<Ignore, string>(_consumerConfig).Build();
        // _consumer.Subscribe(TopicMailDownload);

        _cts = new CancellationTokenSource();
        
        // for (int i = 0; i < 1; i++)
        // {
        //     // Each thread gets its own consumer
        //     Thread thread = new Thread(() => 
        //     {
        //         var consumer = new ConsumerBuilder<Ignore, string>(_consumerConfig).Build();
        //         consumer.Subscribe(TopicMailDownload);
        //         StartConsuming(consumer, _cts.Token);
        //     });
        //     thread.Start();
        // }
        var consumer = new ConsumerBuilder<Ignore, string>(_consumerConfig).Build();
        consumer.Subscribe(TopicMailDownload);
        StartConsuming(consumer, _cts.Token);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("StopAsync");
        _cts.Cancel();
        _metricServer.Stop();
        return Task.CompletedTask;  
    }

    public Task StartingAsync(CancellationToken cancellationToken)
    {        
        Console.WriteLine("StartingAsync");
        return Task.CompletedTask; 
    }

    public Task StartedAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
        // var config = new ConsumerConfig
        // {
        //     GroupId = "test-consumer-group", // Consumer group ID
        //     BootstrapServers = "demobroker:9092", // Kafka broker address
        //     AutoOffsetReset = AutoOffsetReset.Earliest // Start reading from the earliest message
        // };
        //
        // using (var consumer = new ConsumerBuilder<Ignore, string>(config).Build())
        // {
        //     // Subscribe to the topic
        //     consumer.Subscribe(TopicMailDownload);
        //
        //     CancellationTokenSource cts = new CancellationTokenSource();
        //     Console.CancelKeyPress += (_, e) =>
        //     {
        //         e.Cancel = true;
        //         cts.Cancel();
        //     };
        //
        //     try
        //     {
        //         while (!cts.Token.IsCancellationRequested)
        //         {
        //             try
        //             {
        //                 // Consume the message from the topic
        //                 var consumeResult = consumer.Consume(cts.Token);
        //                 MailConsumedCounter.Inc();
        //                 Console.WriteLine($"Consumed message '{consumeResult.Message.Value}' at: '{consumeResult.TopicPartitionOffset}'.");
        //             }
        //             catch (ConsumeException e)
        //             {
        //                 Console.WriteLine($"Error occurred: {e.Error.Reason}");
        //             }
        //         }
        //     }
        //     catch (OperationCanceledException)
        //     {
        //         // Ensure the consumer leaves the group cleanly and final offsets are committed.
        //         consumer.Close();
        //     }
        // }
    }

    public Task StoppingAsync(CancellationToken cancellationToken)
    {        
        Console.WriteLine("StoppingAsync");
        return Task.CompletedTask;  
    }

    public Task StoppedAsync(CancellationToken cancellationToken)
    {        
        Console.WriteLine("StoppedAsync");
        return Task.CompletedTask;  
    }
    
    private void StartConsuming(IConsumer<Ignore, string> consumer, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} is starting.");
        Random random = new Random();
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // int[] sleep = { 1000, 3000, 2000, 5000 };
                    // int randomIndex = random.Next(sleep.Length);
                    // int randomValue = sleep[randomIndex];
                    // Thread.Sleep(randomValue);
                    Thread.Sleep(1000);

                    Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} is consuming a message... | Sleep time: {0}");
                    
                    var consumeResult = consumer.Consume(cancellationToken);
                    MailConsumedCounter.Inc();
                    Console.WriteLine($"Consumed message '{consumeResult.Message.Value}' from topic '{consumeResult.Topic}'.");
                }
                catch (ConsumeException ex)
                {
                    Console.WriteLine($"Error while consuming: {ex.Error.Reason}");
                }
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Kafka consumption canceled.");
        }
        finally
        {
            // Ensure the consumer leaves the group cleanly and final offsets are committed
            consumer.Close();
        }
    }
}