using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Text.Json;

namespace KafkaConsumerDotNet
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Kafka Consumer .NET Application");
            Console.WriteLine("-------------------------------");

            // Load configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var kafkaSettings = configuration.GetSection("KafkaSettings");
            var bootstrapServers = kafkaSettings["BootstrapServers"];
            var topic = kafkaSettings["Topic"];
            var groupId = kafkaSettings["GroupId"];
            var username = kafkaSettings["Username"];
            var password = kafkaSettings["Password"];

            Console.WriteLine($"Bootstrap Servers: {bootstrapServers}");
            Console.WriteLine($"Topic: {topic}");
            Console.WriteLine($"Group ID: {groupId}");
            Console.WriteLine();

            // Configure Kafka consumer
            var config = new ConsumerConfig
            {
                BootstrapServers = bootstrapServers,
                GroupId = groupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = true,
                SecurityProtocol = SecurityProtocol.SaslSsl,
                SaslMechanism = SaslMechanism.Plain,
                SaslUsername = username,
                SaslPassword = password,
            };

            Console.WriteLine("Creating consumer instance...");
            using (var consumer = new ConsumerBuilder<string, string>(config).Build())
            {
                Console.WriteLine("Subscribing to topic...");
                consumer.Subscribe(topic);

                var cts = new CancellationTokenSource();
                Console.CancelKeyPress += (_, e) => {
                    e.Cancel = true;
                    cts.Cancel();
                };

                Console.WriteLine("Starting consumption loop. Press Ctrl+C to exit.");
                Console.WriteLine();

                try
                {
                    while (!cts.Token.IsCancellationRequested)
                    {
                        try
                        {
                            var consumeResult = consumer.Consume(cts.Token);
                            
                            Console.WriteLine($"Message received at: {DateTime.Now}");
                            Console.WriteLine($"Key: {consumeResult.Message.Key}");
                            
                            var value = consumeResult.Message.Value;
                            Console.WriteLine($"Value: {value}");
                            
                            // Try to parse JSON for pretty printing
                            try
                            {
                                using (JsonDocument doc = JsonDocument.Parse(value))
                                {
                                    var options = new JsonSerializerOptions { WriteIndented = true };
                                    var json = JsonSerializer.Serialize(doc, options);
                                    Console.WriteLine("Formatted JSON:");
                                    Console.WriteLine(json);
                                }
                            }
                            catch (JsonException)
                            {
                                // If not valid JSON, just show as-is
                                Console.WriteLine("Message is not valid JSON");
                            }
                            
                            Console.WriteLine(new string('-', 50));
                        }
                        catch (ConsumeException e)
                        {
                            Console.WriteLine($"Error consuming message: {e.Error.Reason}");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // This is normal when cancellation is requested
                }
                finally
                {
                    consumer.Close();
                    Console.WriteLine("Consumer closed.");
                }
            }
        }
    }
}
