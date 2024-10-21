using Confluent.Kafka;
using Newtonsoft.Json;
using Prometheus;

namespace MyBackgroundService
{
    public class Worker : IHostedLifecycleService
    {
        private const string TopicMailDownload = "mail-download";
        private readonly ILogger<Worker> _logger;
        private readonly IProducer<Null, string> _producer;
        private static readonly Counter MailProducedCounter = Metrics.CreateCounter("mail_producer", "Total number of mails produced.");
        private MetricServer _metricServer;
        private Dictionary<string, List<string>> providers = new();
        private Timer _timer;
        
        public Worker(ILogger<Worker> logger, IProducer<Null, string> producer)
        {
            _logger = logger;
            _producer = producer;
        }
        
        public Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("StartAsync");
            
            _metricServer = new MetricServer(port: 7001);  
            _metricServer.Start();
            
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(20)); // Runs every 30 seconds
            
            return Task.CompletedTask;
        }
        
        private async void DoWork(object state)
        {
            string topic = TopicMailDownload;

            foreach (var provider in providers)
            {
                string providerName = provider.Key; 
                List<string> mailIds = provider.Value; 
                
                foreach (var mailId in mailIds)
                {
                    var messageObject = new { Provider = providerName, MailId = mailId };
                    string message = JsonConvert.SerializeObject(messageObject);
                    await SendMessage(topic, message, providerName);
                    
                    MailProducedCounter.Inc();
                }
            }
        }

        public Task StartedAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
        
        public void Dispose()
        {
            _timer?.Dispose();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _producer.Flush(TimeSpan.FromSeconds(10));
            _producer.Dispose();
            
            _metricServer.Stop();

            return Task.CompletedTask;
        }

        public async Task StartingAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Preparing provider data");
            await PrepareProviders();
            _logger.LogInformation("Provider data is prepared");
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

        private async Task SendMessage(string topic, string message, string key)
        {
            var msg = new Message<Null, string> { Value = message }; 

            var deliveryResult = await _producer.ProduceAsync(topic, msg);

            if (deliveryResult.Status == PersistenceStatus.Persisted)
            {
                Console.WriteLine($"Produced message '{deliveryResult.Value}' to partition {deliveryResult.Partition}");
            }
            else
            {
                Console.WriteLine($"Failed to deliver message: {deliveryResult.Message.Value}");
            }
        }

        private async Task PrepareProviders()
        {
            List<string> guidMicrosofts = new();
            for (int i = 0; i < 20; i++)
            {
                Guid providerGuid = Guid.NewGuid();
                guidMicrosofts.Add($"microsoft-{providerGuid}");
            }

            providers.Add("microsoft", guidMicrosofts);

            List<string> guidGoogles = new();
            for (int i = 0; i < 20; i++)
            {
                Guid providerGuid = Guid.NewGuid();
                guidGoogles.Add($"google-{providerGuid.ToString()}");
            }

            providers.Add("google", guidGoogles);
        }
    }
}