using System;
using System.Linq;
using System.Text;
using System.Threading;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Topic;

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {

            var client = new MqttClient("localhost");
            client.MqttMsgPublishReceived += (s, m) =>
            {
                Console.WriteLine($"Publish Recieved: {m.Topic} \t {Encoding.ASCII.GetString(m.Message)}");
            };

            client.MqttMsgPublished += (s, m) =>
            {
                Console.WriteLine("Message Published");
            };

            client.ConnectionClosed += (s, m) =>
            {
                Console.WriteLine("Connection Closed");
            };

            client.MqttMsgSubscribed += (s, m) =>
            {
                Console.WriteLine("Message Subscribed");
            };

            client.MqttMsgUnsubscribed += (s, m) =>
            {
                Console.WriteLine("Message Unsubscribed");
            };

            client.Reconnected += (s, m) =>
            {
                Console.WriteLine("Reconnected");
            };

            client.Connected += (s, m) =>
            {
                Console.WriteLine("Connected");
            };

            client.Connected += (s, m) =>
            {
                Console.WriteLine("I am a fruitcake");
            };

            client.Disconnected += (s, m) =>
            {
                Console.WriteLine("Disconnected");
            };

            client.Event += (s, m) =>
            {
                var t = "{\r\n";
                var q = m.Dict.Select((a) => $"\t\"{a.Key}\" : \"{a.Value}\"");
                t += String.Join(",\r\n", q);
                t += "\r\n}";
                Console.WriteLine(t);

            };

            //client.shouldReconnect = false;
            client.Connect();

            Console.ReadKey();
            Console.WriteLine("Begin attempt publish");
            var do_the_thing = true;
            new Thread(() =>
            {
                while (do_the_thing)
                {
                    client.Publish("/banana", Encoding.ASCII.GetBytes("I am a fish"));
                    Thread.Sleep(1000);
                }
            }).Start();

            Console.ReadKey();
            Console.WriteLine("subscribe to banana");
            client.AddTopic(new Topic("/banana"));
            Console.ReadKey();
            Console.WriteLine("subscribe to fig");
            client.AddTopic(new Topic("/fig"));
            Console.ReadKey();
            Console.WriteLine("unsubscribe from fig");
            client.RemoveTopic("/fig");
            Console.ReadKey();
            do_the_thing = false;
            client.Disconnect();
        }
    }
}
