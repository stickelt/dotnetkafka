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

            // Load configuration from appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var kafkaSettings = configuration.GetSection("KafkaSettings");
            var bootstrapServers = kafkaSettings["BootstrapServers"];
            var topic = kafkaSettings["Topic"];
            var groupId = kafkaSettings["GroupId"] + "-" + Guid.NewGuid(); // force unique group ID
            var username = kafkaSettings["Username"];
            var password = kafkaSettings["Password"];

            Console.WriteLine($"Bootstrap Servers: {bootstrapServers}");
            Console.WriteLine($"Topic: {topic}");
            Console.WriteLine($"Group ID: {groupId}");
            Console.WriteLine();

            // Kafka consumer configuration
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

                // Helpful for debugging connection
                Debug = "all"
            };

            Console.WriteLine("Creating Kafka consumer...");
            using var consumer = new ConsumerBuilder<string, string>(config).Build();

            consumer.Subscribe(topic);

            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            Console.WriteLine("Waiting for messages... (Ctrl+C to exit)\n");

            try
            {
                int maxMessages = 10;
                int count = 0;

                while (!cts.Token.IsCancellationRequested && count < maxMessages)
                {
                    var result = consumer.Consume(cts.Token);

                    Console.WriteLine($"Message received at: {DateTime.Now}");
                    Console.WriteLine($"Key: {result.Message.Key}");
                    Console.WriteLine($"Raw Value: {result.Message.Value}");

                    try
                    {
                        var doc = JsonDocument.Parse(result.Message.Value);
                        var options = new JsonSerializerOptions { WriteIndented = true };
                        string prettyJson = JsonSerializer.Serialize(doc.RootElement, options);
                        Console.WriteLine("Formatted JSON:");
                        Console.WriteLine(prettyJson);
                    }
                    catch
                    {
                        Console.WriteLine("Message is not valid JSON.");
                    }

                    Console.WriteLine(new string('-', 50));
                    count++;
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Consumption stopped.");
            }
            finally
            {
                consumer.Close();
                Console.WriteLine("Consumer closed.");
            }
        }
    }
}
