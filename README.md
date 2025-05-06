# Kafka Consumer .NET Console Application

A simple .NET Core console application that connects to a Kafka topic and displays messages as they are received.

## Features

- Connects to Kafka cluster using SASL/SSL authentication
- Reads messages from a specified topic
- Automatically formats and displays JSON messages
- Configuration via appsettings.json

## Configuration

Edit the `appsettings.json` file to configure your Kafka connection:

```json
{
  "KafkaSettings": {
    "BootstrapServers": "YOUR_BOOTSTRAP_SERVER",
    "Topic": "YOUR_TOPIC",
    "GroupId": "YOUR_CONSUMER_GROUP",
    "Username": "YOUR_API_KEY",
    "Password": "YOUR_API_SECRET",
    "SecurityProtocol": "SaslSsl",
    "SaslMechanism": "Plain"
  }
}
```

Replace:
- `YOUR_BOOTSTRAP_SERVER` with your Kafka bootstrap server address (e.g., `lkc-2jr292.dom8wldz4w7.us-east4.gcp.confluent.cloud:9092`)
- `YOUR_TOPIC` with the topic name (e.g., `gcp.retail.rxdw.rtds.dev.asembia-txn`)
- `YOUR_CONSUMER_GROUP` with your consumer group ID (e.g., `asembia-ekp-dev-asembia-txn-cg`)
- `YOUR_API_KEY` with your Confluent Cloud API key
- `YOUR_API_SECRET` with your Confluent Cloud API secret

## Running the Application

```bash
dotnet run
```

Or, to build and run the application:

```bash
dotnet build
dotnet bin/Debug/net8.0/KafkaConsumerDotNet.dll
```

## Example Output

```
Kafka Consumer .NET Application
-------------------------------
Bootstrap Servers: lkc-2jr292.dom8wldz4w7.us-east4.gcp.confluent.cloud:9092
Topic: gcp.retail.rxdw.rtds.dev.asembia-txn
Group ID: asembia-ekp-dev-asembia-txn-cg

Creating consumer instance...
Subscribing to topic...
Starting consumption loop. Press Ctrl+C to exit.

Message received at: 5/5/2025 11:30:45 AM
Key: sample-key
Value: {"UUID":"079dcf95-6907-4e5c-b080-979d7c8b2f55","Response":{"code":202,"RxDataIDList":["58460"]}}
Formatted JSON:
{
  "UUID": "079dcf95-6907-4e5c-b080-979d7c8b2f55",
  "Response": {
    "code": 202,
    "RxDataIDList": [
      "58460"
    ]
  }
}
--------------------------------------------------
```

## Requirements

- .NET 8.0 or later
- Confluent Kafka client library
- Access to a Kafka cluster (like Confluent Cloud) 