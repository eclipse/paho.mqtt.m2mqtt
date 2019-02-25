using System;
using System.Threading;
using System.Text;
using System.Net.NetworkInformation;
using System.Security.Cryptography.X509Certificates;

using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace TestMqtt
{
    public class Program
    {
        private const string c_SSID = "myssid";
        private const string c_AP_PASSWORD = "mypassword";

        public static void Main()
        {
 
            MqttClient client = null;
            string clientId;
            bool running = true;

            // Wait for Wifi/network to connect (temp)
            SetupAndConnectNetwork();

            // Loop forever
            while (true)
            {
                try
                {
                    string BrokerAddress = "192.168.2.129";
                    client = new MqttClient(BrokerAddress);

                    // register a callback-function (we have to implement, see below) which is called by the library when a message was received
                    client.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;
                    client.MqttMsgSubscribed += Client_MqttMsgSubscribed;

                    // use a unique id as client id, each time we start the application
                    //clientId = Guid.NewGuid().ToString();
                    clientId = new Guid(1, 23, 44, 32, 45, 33, 22, 11, 1, 2, 3).ToString();

                    Console.WriteLine("Connecting MQTT");

                    client.Connect(clientId);

                    Console.WriteLine("Connected MQTT");
                    // Subscribe topics
                    //     client.Subscribe(new string[] { "Test1", "Test2" }, new byte[] { 2, 2 });

                    byte[] message = Encoding.UTF8.GetBytes("Test message");
                    client.Publish("/Esp32/Test1", message, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);

                    string[] SubTopics = new string[]
                    {
                        "/Automation/Lights/#"
                    };

                    Console.WriteLine("Subscribe /Automation/Lights/#");
                    client.Subscribe(SubTopics, new byte[] { 2 });

                    Console.WriteLine("Enter wait loop");
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
                    Console.WriteLine("Main exception " + ex.Message);
                }
                    
                // Wait before retry
                Thread.Sleep(10000);
            }
        }

        private static void Client_MqttMsgSubscribed(object sender, MqttMsgSubscribedEventArgs e)
        {
            Console.WriteLine("Client_MqttMsgSubscribed ");
        }

        private static void Client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            string topic  = e.Topic;

            string message = Encoding.UTF8.GetString(e.Message,0,e.Message.Length);

            Console.WriteLine("Publish Received Topic:" + topic + " Message:" + message);

        }
        public static void SetupAndConnectNetwork()
        {
            NetworkInterface[] nis = NetworkInterface.GetAllNetworkInterfaces();
            if (nis.Length > 0)
            {
                // get the first interface
                NetworkInterface ni = nis[0];

                if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                {
                    // network interface is Wi-Fi
                    Console.WriteLine("Network connection is: Wi-Fi");

                    Wireless80211Configuration wc = Wireless80211Configuration.GetAllWireless80211Configurations()[ni.SpecificConfigId];
                    if (wc.Ssid != c_SSID && wc.Password != c_AP_PASSWORD)
                    {
                        // have to update Wi-Fi configuration
                        wc.Ssid = c_SSID;
                        wc.Password = c_AP_PASSWORD;
                        wc.SaveConfiguration();
                    }
                    else
                    {   // Wi-Fi configuration matches
                    }
                }
                else
                {
                    // network interface is Ethernet
                    Console.WriteLine("Network connection is: Ethernet");

                    ni.EnableDhcp();
                }

                // wait for DHCP to complete
                WaitIP();
            }
            else
            {
                throw new NotSupportedException("ERROR: there is no network interface configured.\r\nOpen the 'Edit Network Configuration' in Device Explorer and configure one.");
            }
        }

        static void WaitIP()
        {
            Console.WriteLine("Waiting for IP...");

            while (true)
            {
                NetworkInterface ni = NetworkInterface.GetAllNetworkInterfaces()[0];
                if (ni.IPv4Address != null && ni.IPv4Address.Length > 0)
                {
                    if (ni.IPv4Address[0] != '0')
                    {
                        Console.WriteLine($"We have an IP: {ni.IPv4Address}");
                        break;
                    }
                }

                Thread.Sleep(500);
            }
        }


    }
}
