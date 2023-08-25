// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using nanoFramework.M2Mqtt;
using nanoFramework.M2Mqtt.Messages;
using System;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;


namespace Mosquito.TemperatureGauge
{
    public class Program
    {
        private const string c_SSID = "yourssid";
        private const string c_AP_PASSWORD = "yourpassword";

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

            Debug.WriteLine("Connecting to Mosquito test server...");

            client.Connect(clientId);

            Debug.WriteLine("Connected to Mosquito test server");

            while (running)
            {
                // send random temperature value to Gauge demo in Mosquito test server, see http://test.mosquitto.org/gauge/
                client.Publish("temp/random", GetRandomTemperature(), null, null, MqttQoSLevel.ExactlyOnce, false);

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

            Debug.WriteLine($"Temperature: {temperatureAsString}");

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
                    Debug.WriteLine("Network connection is: Wi-Fi");

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

        private static void SetDateTime()
        {
            Debug.WriteLine("Setting up system clock...");

            // set system date time (needs to be accurate to the day in order to be able to validate a certificate)
            //Rtc.SetSystemTime(new DateTime(2018, 08, 02));

            // if SNTP is available and enabled on target device this can be skipped because we should have a valid date & time
            while (DateTime.UtcNow.Year < 2018)
            {
                Debug.WriteLine("Waiting for valid date time...");
                // wait for valid date & time
                Thread.Sleep(1000);
            }
        }

        static string GetCertificate()
        {
            // Mosquito test server CA certificate
            // from http://test.mosquitto.org/

            // X509 
            string certificate =
@"-----BEGIN CERTIFICATE-----
MIIEAzCCAuugAwIBAgIUBY1hlCGvdj4NhBXkZ/uLUZNILAwwDQYJKoZIhvcNAQEL
BQAwgZAxCzAJBgNVBAYTAkdCMRcwFQYDVQQIDA5Vbml0ZWQgS2luZ2RvbTEOMAwG
A1UEBwwFRGVyYnkxEjAQBgNVBAoMCU1vc3F1aXR0bzELMAkGA1UECwwCQ0ExFjAU
BgNVBAMMDW1vc3F1aXR0by5vcmcxHzAdBgkqhkiG9w0BCQEWEHJvZ2VyQGF0Y2hv
by5vcmcwHhcNMjAwNjA5MTEwNjM5WhcNMzAwNjA3MTEwNjM5WjCBkDELMAkGA1UE
BhMCR0IxFzAVBgNVBAgMDlVuaXRlZCBLaW5nZG9tMQ4wDAYDVQQHDAVEZXJieTES
MBAGA1UECgwJTW9zcXVpdHRvMQswCQYDVQQLDAJDQTEWMBQGA1UEAwwNbW9zcXVp
dHRvLm9yZzEfMB0GCSqGSIb3DQEJARYQcm9nZXJAYXRjaG9vLm9yZzCCASIwDQYJ
KoZIhvcNAQEBBQADggEPADCCAQoCggEBAME0HKmIzfTOwkKLT3THHe+ObdizamPg
UZmD64Tf3zJdNeYGYn4CEXbyP6fy3tWc8S2boW6dzrH8SdFf9uo320GJA9B7U1FW
Te3xda/Lm3JFfaHjkWw7jBwcauQZjpGINHapHRlpiCZsquAthOgxW9SgDgYlGzEA
s06pkEFiMw+qDfLo/sxFKB6vQlFekMeCymjLCbNwPJyqyhFmPWwio/PDMruBTzPH
3cioBnrJWKXc3OjXdLGFJOfj7pP0j/dr2LH72eSvv3PQQFl90CZPFhrCUcRHSSxo
E6yjGOdnz7f6PveLIB574kQORwt8ePn0yidrTC1ictikED3nHYhMUOUCAwEAAaNT
MFEwHQYDVR0OBBYEFPVV6xBUFPiGKDyo5V3+Hbh4N9YSMB8GA1UdIwQYMBaAFPVV
6xBUFPiGKDyo5V3+Hbh4N9YSMA8GA1UdEwEB/wQFMAMBAf8wDQYJKoZIhvcNAQEL
BQADggEBAGa9kS21N70ThM6/Hj9D7mbVxKLBjVWe2TPsGfbl3rEDfZ+OKRZ2j6AC
6r7jb4TZO3dzF2p6dgbrlU71Y/4K0TdzIjRj3cQ3KSm41JvUQ0hZ/c04iGDg/xWf
+pp58nfPAYwuerruPNWmlStWAXf0UTqRtg4hQDWBuUFDJTuWuuBvEXudz74eh/wK
sMwfu1HFvjy5Z0iMDU8PUDepjVolOCue9ashlS4EB5IECdSR2TItnAIiIwimx839
LdUdRudafMu5T5Xma182OC0/u/xRlEm+tvKGGmfFcN0piqVl8OrSPBgIlb+1IKJE
m/XriWr/Cq4h/JfB7NTsezVslgkBaoU=
-----END CERTIFICATE-----
";

            return certificate;
        }
    }
}
