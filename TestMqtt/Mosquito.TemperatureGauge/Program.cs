using System;
using System.Threading;
using System.Text;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Net.NetworkInformation;
using System.Security.Cryptography.X509Certificates;


namespace Mosquito.TemperatureGauge
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

            X509Certificate caCert = new X509Certificate(GetCertificate());

            // Wait for network to connect (temp)
            SetupAndConnectNetwork();

            SetDateTime();

            // connect to the Mosquito test server
            client = new MqttClient("test.mosquitto.org", 8883, true, caCert, null, MqttSslProtocols.TLSv1_2);

            // use a unique id as client id, each time we start the application
            clientId = Guid.NewGuid().ToString();

            Console.WriteLine("Connecting to Mosquito test server...");

            client.Connect(clientId);

            Console.WriteLine("Connected to Mosquito test server");

            while (running)
            {
                // send random temperature value to Gauge demo in Mosquito test server, see http://test.mosquitto.org/gauge/
                client.Publish("temp/random", GetRandomTemperature(), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);

                // the Mosquito test server has a local process that updates every 15 seconds, so we just follow that
                Thread.Sleep(15000);
            }

            client.Disconnect();

            // prevent app from returning, ever
            Thread.Sleep(Timeout.Infinite);
        }

        private static byte[] GetRandomTemperature()
        {
            // generate random value
            Random randomProvider = new Random();
            var randomTemperature = randomProvider.NextDouble()*10;

            // convert to string formatted NN.NN
            var temperatureAsString = randomTemperature.ToString("N2");

            Console.WriteLine($"Temperature: {temperatureAsString}");

            return Encoding.UTF8.GetBytes(temperatureAsString);
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

        private static void SetDateTime()
        {
            Console.WriteLine("Setting up system clock...");

            // set system date time (needs to be accurate to the day in order to be able to validate a certificate)
            //Rtc.SetSystemTime(new DateTime(2018, 08, 02));

            // if SNTP is available and enabled on target device this can be skipped because we should have a valid date & time
            while (DateTime.UtcNow.Year < 2018)
            {
                Console.WriteLine("Waiting for valid date time...");
                // wait for valid date & time
                Thread.Sleep(1000);
            }
        }

        static byte[] GetCertificate()
        {
            // Mosquito test server CA certificate
            // from http://test.mosquitto.org/

            // X509 
            string certificate =
@"-----BEGIN CERTIFICATE-----
MIIC8DCCAlmgAwIBAgIJAOD63PlXjJi8MA0GCSqGSIb3DQEBBQUAMIGQMQswCQYD
VQQGEwJHQjEXMBUGA1UECAwOVW5pdGVkIEtpbmdkb20xDjAMBgNVBAcMBURlcmJ5
MRIwEAYDVQQKDAlNb3NxdWl0dG8xCzAJBgNVBAsMAkNBMRYwFAYDVQQDDA1tb3Nx
dWl0dG8ub3JnMR8wHQYJKoZIhvcNAQkBFhByb2dlckBhdGNob28ub3JnMB4XDTEy
MDYyOTIyMTE1OVoXDTIyMDYyNzIyMTE1OVowgZAxCzAJBgNVBAYTAkdCMRcwFQYD
VQQIDA5Vbml0ZWQgS2luZ2RvbTEOMAwGA1UEBwwFRGVyYnkxEjAQBgNVBAoMCU1v
c3F1aXR0bzELMAkGA1UECwwCQ0ExFjAUBgNVBAMMDW1vc3F1aXR0by5vcmcxHzAd
BgkqhkiG9w0BCQEWEHJvZ2VyQGF0Y2hvby5vcmcwgZ8wDQYJKoZIhvcNAQEBBQAD
gY0AMIGJAoGBAMYkLmX7SqOT/jJCZoQ1NWdCrr/pq47m3xxyXcI+FLEmwbE3R9vM
rE6sRbP2S89pfrCt7iuITXPKycpUcIU0mtcT1OqxGBV2lb6RaOT2gC5pxyGaFJ+h
A+GIbdYKO3JprPxSBoRponZJvDGEZuM3N7p3S/lRoi7G5wG5mvUmaE5RAgMBAAGj
UDBOMB0GA1UdDgQWBBTad2QneVztIPQzRRGj6ZHKqJTv5jAfBgNVHSMEGDAWgBTa
d2QneVztIPQzRRGj6ZHKqJTv5jAMBgNVHRMEBTADAQH/MA0GCSqGSIb3DQEBBQUA
A4GBAAqw1rK4NlRUCUBLhEFUQasjP7xfFqlVbE2cRy0Rs4o3KS0JwzQVBwG85xge
REyPOFdGdhBY2P1FNRy0MDr6xr+D2ZOwxs63dG1nnAnWZg7qwoLgpZ4fESPD3PkA
1ZgKJc2zbSQ9fCPxt2W3mdVav66c6fsb7els2W2Iz7gERJSX
-----END CERTIFICATE-----";

            return Encoding.UTF8.GetBytes(certificate);
        }
    }
}
