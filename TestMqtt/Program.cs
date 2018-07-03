using System;
using System.Threading;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;


namespace TestMqtt
{
    public class Program
    {
        public static void Main()
        {
            MqttClient client = null;
            string clientId;
            bool running = true;

            // Wait for Wifi to connect (temp)
            Thread.Sleep(10000);

            // Loop forever
            while (true)
            {
                try
                {
                    string BrokerAddress = "192.168.2.129";
                    client = new MqttClient(BrokerAddress);

                    //X509Certificate caCert = new X509Certificate(MyCaCert);
                    //X509Certificate clCert = new X509Certificate(MyClCert);
                    //client = new MqttClient(BrokerAddress, 8883, true, caCert, clCert, MqttSslProtocols.TLSv1_2);


                    // register a callback-function (we have to implement, see below) which is called by the library when a message was received
                    client.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;

                    // use a unique id as client id, each time we start the application
                    clientId = Guid.NewGuid().ToString();

                    client.Connect(clientId);

                    // Subscribe topics
                    //     client.Subscribe(new string[] { "Test1", "Test2" }, new byte[] { 2, 2 });

                    byte[] message = Encoding.UTF8.GetBytes("Test message");
                    client.Publish("/Esp32/Test1", message, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);

                    while (running)
                    {
                        Thread.Sleep(1000);
                    }

                    client.Disconnect();
                }
                catch (Exception ex)
                {
                    // Do whatever please you with the exception caught
                    Console.WriteLine("Main exception " + ex.Message);
                }
                    
                // Wait before retry
                Thread.Sleep(10000);
            }
        }

        



        private static void Client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            string message = "";
            // string message = Encoding.UTF8.GetString(e.message);
            Console.WriteLine("Publish Received " + message);
        }
    }
}
