using nanoFramework.M2Mqtt;
using nanoFramework.M2Mqtt.Messages;
using System;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading;

namespace MemoryLeakTestApp
{
    public class Program
    {
        private const string Brocker = "192.168.1.2";
        private const string Ssid = "yourssid";
        private const string Password = "you_wifi_password";
        private const int NumberOfLoops = 5;
        private static MqttClient client;
        private static uint freeRam = 0;

        public static void Main()
        {
            client = new MqttClient(Brocker);

            string clientId = Guid.NewGuid().ToString();
            client.Connect(clientId);

            // Basic QoS tests
            client.Publish("temp/test", Encoding.UTF8.GetBytes($"Message QoS 0"), MqttQoSLevel.AtMostOnce, false);
            Thread.Sleep(200);
            client.Publish("temp/test", Encoding.UTF8.GetBytes($"Message QoS 1"), MqttQoSLevel.AtLeastOnce, false);
            Thread.Sleep(200);
            client.Publish("temp/test", Encoding.UTF8.GetBytes($"Message QoS 2"), MqttQoSLevel.ExactlyOnce, false);
            Thread.Sleep(200);

            // Advance tests for different QoS
            Publish(MqttQoSLevel.AtMostOnce);
            Thread.Sleep(2000);
            Publish(MqttQoSLevel.AtLeastOnce);
            Thread.Sleep(2000);
            Publish(MqttQoSLevel.ExactlyOnce);
            Thread.Sleep(2000);

            client.Publish("temp/test", Encoding.UTF8.GetBytes($"Memory left after all the test"), MqttQoSLevel.AtMostOnce, false);
            freeRam = nanoFramework.Runtime.Native.GC.Run(true);
            client.Publish("temp/free-ram", Encoding.UTF8.GetBytes(freeRam.ToString("F0")), MqttQoSLevel.AtMostOnce, false);
            // Wait a bit 
            Thread.Sleep(5_000);
            client.Publish("temp/test", Encoding.UTF8.GetBytes($"Memory left after all the test: 5s"), MqttQoSLevel.AtMostOnce, false);
            freeRam = nanoFramework.Runtime.Native.GC.Run(true);
            client.Publish("temp/free-ram", Encoding.UTF8.GetBytes(freeRam.ToString("F0")), MqttQoSLevel.AtMostOnce, false);
            // Wait more
            Thread.Sleep(35_000);
            client.Publish("temp/test", Encoding.UTF8.GetBytes($"Memory left after all the test: 35s"), MqttQoSLevel.AtMostOnce, false);
            freeRam = nanoFramework.Runtime.Native.GC.Run(true);
            client.Publish("temp/free-ram", Encoding.UTF8.GetBytes(freeRam.ToString("F0")), MqttQoSLevel.AtMostOnce, false);
            Thread.Sleep(120_000);
            client.Publish("temp/test", Encoding.UTF8.GetBytes($"Memory left after all the test: 120s"), MqttQoSLevel.AtMostOnce, false);
            freeRam = nanoFramework.Runtime.Native.GC.Run(true);
            client.Publish("temp/free-ram", Encoding.UTF8.GetBytes(freeRam.ToString("F0")), MqttQoSLevel.AtMostOnce, false);
            Thread.Sleep(Timeout.Infinite);

            // Testing dispose
            client.Dispose();
        }

        private static void Publish(MqttQoSLevel level)
        {
            client.Publish("temp/test", Encoding.UTF8.GetBytes($"single message QoS {level}"), level, false);
            for (int i = 0; i < NumberOfLoops; i++)
            {
                freeRam = nanoFramework.Runtime.Native.GC.Run(true);
                client.Publish("temp/free-ram", Encoding.UTF8.GetBytes($"{i}-{freeRam.ToString("F0")}"), level, false);
                Thread.Sleep(1000);
            }

            client.Publish("temp/test", Encoding.UTF8.GetBytes($"two messages without delays QoS {level}"), level, false);
            for (int i = 0; i < NumberOfLoops; i++)
            {
                freeRam = nanoFramework.Runtime.Native.GC.Run(true);
                client.Publish("temp/free-ram", Encoding.UTF8.GetBytes($"0/{i}-{freeRam.ToString("F0")}"), level, false);
                client.Publish("temp/free-ram", Encoding.UTF8.GetBytes($"1/{i}-{freeRam.ToString("F0")}"), level, false);
                Thread.Sleep(1000);
            }

            client.Publish("temp/test", Encoding.UTF8.GetBytes($"two messages with delay QoS {level}"), level, false);
            for (int i = 0; i < NumberOfLoops; i++)
            {
                freeRam = nanoFramework.Runtime.Native.GC.Run(true);
                client.Publish("temp/free-ram", Encoding.UTF8.GetBytes($"0/{i}-{freeRam.ToString("F0")}"), level, false);
                Thread.Sleep(50);
                client.Publish("temp/free-ram", Encoding.UTF8.GetBytes($"1/{i}-{freeRam.ToString("F0")}"), level, false);
                Thread.Sleep(1000);
            }

            client.Publish("temp/test", Encoding.UTF8.GetBytes($"Stress test with no delay QoS {level}"), level, false);
            for (int i = 0; i < NumberOfLoops; i++)
            {
                freeRam = nanoFramework.Runtime.Native.GC.Run(true);
                client.Publish("temp/free-ram", Encoding.UTF8.GetBytes($"{i}-{freeRam.ToString("F0")}"), level, false);
            }
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
                    Debug.WriteLine("Network connection is: Wi-Fi");

                    Wireless80211Configuration wc = Wireless80211Configuration.GetAllWireless80211Configurations()[ni.SpecificConfigId];
                    if (wc.Ssid != Ssid && wc.Password != Password)
                    {
                        // have to update Wi-Fi configuration
                        wc.Ssid = Ssid;
                        wc.Password = Password;
                        wc.SaveConfiguration();
                    }
                    else
                    {   // Wi-Fi configuration matches
                    }
                }
                else
                {
                    // network interface is Ethernet
                    Debug.WriteLine("Network connection is: Ethernet");

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
            Debug.WriteLine("Waiting for IP...");

            while (true)
            {
                NetworkInterface ni = NetworkInterface.GetAllNetworkInterfaces()[0];
                if (ni.IPv4Address != null && ni.IPv4Address.Length > 0)
                {
                    if (ni.IPv4Address[0] != '0')
                    {
                        Debug.WriteLine($"We have an IP: {ni.IPv4Address}");
                        break;
                    }
                }

                Thread.Sleep(500);
            }
        }
    }
}
