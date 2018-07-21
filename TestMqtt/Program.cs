using System;
using System.Threading;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using NetHelper;

namespace TestMqtt
{
    public class Program
    {
        public static void Main()
        {
 
            MqttClient client = null;
            string clientId;
            bool running = true;

            // Wait for Wifi/network to connect (temp)
            Net.SetupAndConnectNetwork();

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
                    client.MqttMsgSubscribed += Client_MqttMsgSubscribed;
                    // use a unique id as client id, each time we start the application
                    //clientId = Guid.NewGuid().ToString();
                    clientId = new Guid(1, 23, 44, 32, 45, 33, 22, 11, 1, 2, 3).ToString();

                    Log.WriteLine("Connecting MQTT");

                    client.Connect(clientId);

                    Log.WriteLine("Connected MQTT");
                    // Subscribe topics
                    //     client.Subscribe(new string[] { "Test1", "Test2" }, new byte[] { 2, 2 });

                    byte[] message = Encoding.UTF8.GetBytes("Test message");
                    client.Publish("/Esp32/Test1", message, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);

                    string[] SubTopics = new string[]
                    {
                        "/Automation/Lights/#"
                    };

                    Log.WriteLine("Subscribe /Automation/Lights/#");
                    client.Subscribe(SubTopics, new byte[] { 2 });

                    Log.WriteLine("Enter wait loop");
                    while (running)
                    {
                        Thread.Sleep(10000);
                        client.Publish("/Esp32/Test1", message, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
                    }

                    client.Disconnect();
                }
                catch (Exception ex)
                {
                    // Do whatever please you with the exception caught
                    Log.WriteLine("Main exception " + ex.Message);
                }
                    
                // Wait before retry
                Thread.Sleep(10000);
            }
        }

        private static void Client_MqttMsgSubscribed(object sender, MqttMsgSubscribedEventArgs e)
        {
            Log.WriteLine("Client_MqttMsgSubscribed ");
        }

        private static void Client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            string topic  = e.Topic;
            byte[] bmes = e.Message;

            //string message = Encoding.UTF8.GetString(e.message);


            Log.WriteLine("Publish Received Topic:" + topic + " Message:" + bmes[0].ToString() );

        }
    }
}
