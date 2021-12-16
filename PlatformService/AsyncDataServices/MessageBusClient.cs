using System;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using PlatformService.Dtos;
using RabbitMQ.Client;

namespace PlatformService.AsyncDataServices
{
    public class MessageBusClient : IMessageBusClient
    {
        private readonly IConfiguration _configuration;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public MessageBusClient(IConfiguration configuration)
        {
            _configuration = configuration;

            var factory = new ConnectionFactory 
            {
                HostName = _configuration["RabbitMqHost"], 
                Port = int.Parse(_configuration["RabbitMqPort"])
            };
            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _channel.ExchangeDeclare(exchange: "trigger", type: ExchangeType.Fanout);
                _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;

                System.Console.WriteLine("--> Connected to Message bus");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"--> Couldn't connect to the message bus {ex.Message}");
            }
        }

        public void PublishNewPlatform(PlatformPublishedDto platformPublishedDto)
        {
            var message = JsonSerializer.Serialize(platformPublishedDto);

            if(_connection.IsOpen)
            {
                System.Console.WriteLine("RabbitQM connection open, sending message...");
                SendMessage(message);
            }
            
            System.Console.WriteLine("RabbitQM connection is closed, not sending...");
        }

        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            System.Console.WriteLine("--> Rabbit MQ connection has been shutdown");
        }

        private void SendMessage(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(exchange: "trigger",routingKey: "", basicProperties: null, body: body);
            System.Console.WriteLine($"--> We have sent message {message}");
        }

        public void Dispose()
        {
            System.Console.WriteLine("Disposing Message bus...");
            if(_connection.IsOpen)
            {
                _connection.Dispose();
            }
            System.Console.WriteLine("Message bus disposed");
        }
    }
}