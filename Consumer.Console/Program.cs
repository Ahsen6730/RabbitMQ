using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Consumer.ConsoleApp
{
    public class Program
    {
        private static string connectionString = "amqp://guest:guest@localhost:5672";
        private static string queueName;
        private static IConnection connection;
        private static IModel _channel;
        private static IModel channel => _channel ??= CreateOrGetChannel();
        class Mesaj
        {
            public string id { get; set; }
            public string epc { get; set; }
        }
        static void Main(string[] args)
        {
            queueName = args.Length > 0 ? args[0] : "test2";

            connection = GetConnection();

            channel.QueueDeclare(queueName, exclusive: false);
            channel.QueueBind(queueName, "test2", queueName);

            var consumerEvent = new EventingBasicConsumer(channel);

            consumerEvent.Received += (ch, e) => 
            {
                var byteArr = e.Body.ToArray();
                var bodyStr = Encoding.UTF8.GetString(byteArr);
                var jsonString = JsonConvert.DeserializeObject<Mesaj>(bodyStr);
                Console.WriteLine($"Received Data json: {bodyStr}");

                Console.WriteLine($"Received Data: {jsonString.epc}");

                channel.BasicAck(e.DeliveryTag, false);
            };


            channel.BasicConsume(queueName, false, consumerEvent);

            Console.WriteLine($"{queueName} listening....\n\n\n");



            Console.ReadLine();
        }



        private static IModel CreateOrGetChannel()
        {
            return connection.CreateModel();
        }


        private static IConnection GetConnection()
        {
            ConnectionFactory factory = new ConnectionFactory()
            {
                Uri = new Uri(connectionString, UriKind.RelativeOrAbsolute)
            };

            return factory.CreateConnection();
        }
    }
}
