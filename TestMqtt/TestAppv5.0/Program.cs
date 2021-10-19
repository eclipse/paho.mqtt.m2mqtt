// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using nanoFramework.M2Mqtt;
using nanoFramework.M2Mqtt.Messages;
using nanoFramework.Networking;
using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace TestAppv5._0
{
    public class Program
    {
        private const string Ssid = "ssid";
        private const string Paswword = "pswd";

        private const string IoTHub = "youriothub.azure-devices.net";
        private const string DeviceID = "nanoTestDevice";
        private const string Sas = "asaskeybase64encoded";

        public static void Main()
        {
            var success = NetworkHelper.ConnectWifiDhcp(Ssid, Paswword, token: new CancellationTokenSource(60000).Token);
            if (!success)
            {
                Debug.WriteLine("Can't connect to wifi");
                return;
            }

            Debug.WriteLine("Hello from nanoFramework MQTT 5.0 real v5.0 server test!");
            // You can change the server to test
            // Mosquitto supports MQTT5.0 but send some of the messages as v3.1.1 (forgetting the specific MQTT v5.0 property size)
            // MqttClient mqtt = new MqttClient("test.mosquitto.org", 8883, true, new X509Certificate(CertMosquitto), null, MqttSslProtocols.TLSv1_2);
            // EMQX fully supports v5.0 and sends properly the v.5 property size
            // MqttClient mqtt = new MqttClient("broker.emqx.io", 8883, true, new X509Certificate(CertEMQX), null, MqttSslProtocols.TLSv1_2);
            // Azure IoT Edge
            MqttClient mqtt = new MqttClient(IoTHub, 8883, true, new X509Certificate(CertAzure), null, MqttSslProtocols.TLSv1_2);

            mqtt.ProtocolVersion = MqttProtocolVersion.Version_5;
            mqtt.ConnectionOpened += MqttConnectionOpened;
            mqtt.ConnectionClosed += MqttConnectionClosed;
            mqtt.MqttMsgPublished += MqttMsgPublished;
            mqtt.MqttMsgPublishReceived += MqttMqttMsgPublishReceived;
            mqtt.MqttMsgSubscribed += MqttMqttMsgSubscribed;
            mqtt.MqttMsgUnsubscribed += MqttMqttMsgUnsubscribed;
            mqtt.ConnectionClosedRequest += MqttConnectionClosedRequest;

            mqtt.RequestResponseInformation = true;
            // Seems like no need of password or user name for Mosquitto or EMQX
            // var ret = mqtt.Connect("nanoTestDevice", true);
            // Need device ID and Sas token for Azure

            var at = DateTime.UtcNow;
            var atString = (at.ToUnixTimeSeconds() * 1000).ToString();
            var expiry = at.AddMinutes(40);
            var expiryString = (expiry.ToUnixTimeSeconds() * 1000).ToString();
            string toSign = $"{IoTHub}\n{DeviceID}\n\n{atString}\n{expiryString}\n";
            var hmac = new HMACSHA256(Convert.FromBase64String(Sas));
            var sas = hmac.ComputeHash(Encoding.UTF8.GetBytes(toSign));
            mqtt.AuthenticationMethod = "SAS";
            mqtt.AuthenticationData = sas;
            mqtt.UserProperties.Add(new UserProperty("sas-at", atString));
            mqtt.UserProperties.Add(new UserProperty("sas-expiry", expiryString));
            mqtt.UserProperties.Add(new UserProperty("api-version", "2020-10-01-preview"));
            mqtt.UserProperties.Add(new UserProperty("host", IoTHub));
            var ret = mqtt.Connect(DeviceID,
                null,
                null,
                false,
                MqttQoSLevel.AtLeastOnce,
                false, null,
                null,
                true,
                60
                );

            if (ret != MqttReasonCode.Success)
            {
                Debug.WriteLine($"ERROR connecting: {ret}");
                mqtt.Disconnect();
                return;
            }

            // 30 seconds max waiting
            var token = new CancellationTokenSource(30000).Token;
            while (!mqtt.IsConnected && !token.IsCancellationRequested)
            {
                Thread.Sleep(200);
            }

            if (token.IsCancellationRequested)
            {
                Debug.WriteLine($"Too long time to connect, connection closed");
                mqtt.Disconnect();
                return;
            }

            // Uncomment to get flooded with topics
            // mqtt.Subscribe(new string[] { "$SYS/#" }, new MqttQoSLevel[] { MqttQoSLevel.AtLeastOnce });
            // mqtt.Subscribe(new string[] { "nanoTestDevice/#" }, new MqttQoSLevel[] { MqttQoSLevel.AtLeastOnce });
            // For Azure IoT, if you have a subscription that allow cloud to device, you can subscribe to it:
            // mqtt.Subscribe(new string[] { "$iothub/commands" }, new MqttQoSLevel[] { MqttQoSLevel.AtLeastOnce });

            //mqtt.UserProperties.Add(new UserProperty("my beautiful", "property"));
            for (int i = 0; (i < 100) && mqtt.IsConnected; i++)
            {
                // For normal servers
                // var number = mqtt.Publish("nanoTestDevice/publish", Encoding.UTF8.GetBytes($"{{\"Number\":{i}}}"), MqttQoSLevel.ExactlyOnce, false);
                // For Azure IoT
                var number = mqtt.Publish("$iothub/telemetry", Encoding.UTF8.GetBytes($"{{\"Number\":{i}}}"), MqttQoSLevel.AtLeastOnce, false);
                Debug.WriteLine($"Posted message: {number}");
                Thread.Sleep(10000);
            }

            if (mqtt.IsConnected)
            {
                mqtt.Disconnect();
            }

            Thread.Sleep(Timeout.Infinite);
        }

        private static void MqttConnectionClosedRequest(object sender, ConnectionClosedRequestEventArgs e)
        {
            Debug.WriteLine("Disconnection requested by server");
            Debug.WriteLine($"  Return code: {e.Message.Reason}");
            Debug.WriteLine($"  Return code: {e.Message.ResonCode}");
            Debug.WriteLine($"  Return code: {e.Message.ServerReference}");
            Debug.WriteLine($"  Num user props: {e.Message.UserProperties.Count}");
            foreach (UserProperty prop in e.Message.UserProperties)
            {
                Debug.WriteLine($"    Key  : {prop.Name}");
                Debug.WriteLine($"    Value: {prop.Value}");
            }
        }

        private static void MqttMqttMsgUnsubscribed(object sender, MqttMsgUnsubscribedEventArgs e)
        {
            Debug.WriteLine($"Unsubscribed {e.MessageId}");
        }

        private static void MqttMqttMsgSubscribed(object sender, MqttMsgSubscribedEventArgs e)
        {
            Debug.WriteLine($"Subscribed {e.MessageId}, Levels:");
            foreach (MqttQoSLevel level in e.GrantedQoSLevels)
            {
                Debug.WriteLine($"  level");
            }
        }

        private static void MqttMqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            Debug.WriteLine($"Publish received Topic: {e.Topic}");
            Debug.WriteLine($"  Level: {e.QosLevel}");
            Debug.WriteLine($"  Retain: {e.Retain}");
            Debug.WriteLine($"  Dup flag: {e.DupFlag}");
            Debug.WriteLine($"  Msg length: {e.Message.Length}");
        }

        private static void MqttMsgPublished(object sender, MqttMsgPublishedEventArgs e)
        {
            Debug.WriteLine($"Publish published {e.MessageId}");
            Debug.WriteLine($"  IsPublished {e.IsPublished}");
        }

        private static void MqttConnectionClosed(object sender, System.EventArgs e)
        {
            Debug.WriteLine("Connection closed");
        }

        private static void MqttConnectionOpened(object sender, ConnectionOpenedEventArgs e)
        {
            Debug.WriteLine($"Connection open");
            Debug.WriteLine($"  ClientID: {((MqttClient)sender).ClientId}");
            Debug.WriteLine($"  Assigned client id: {e.Message.AssignedClientIdentifier}");
            if (e.Message.AuthenticationData != null) Debug.WriteLine($"  Auth data length: {e.Message.AuthenticationData.Length}");
            Debug.WriteLine($"  Auth method: {e.Message.AuthenticationMethod}");
            Debug.WriteLine($"  Dup flag: {e.Message.DupFlag}");
            Debug.WriteLine($"  Max packet size: {e.Message.MaximumPacketSize}");
            Debug.WriteLine($"  Max QoS: {e.Message.MaximumQoS}");
            Debug.WriteLine($"  Msg ID: {e.Message.MessageId}");
            Debug.WriteLine($"  Qos level: {e.Message.QosLevel}");
            Debug.WriteLine($"  Reason: {e.Message.Reason}");
            Debug.WriteLine($"  Receive max: {e.Message.ReceiveMaximum}");
            Debug.WriteLine($"  Rep info: {e.Message.ResponseInformation}");
            Debug.WriteLine($"  Retain: {e.Message.Retain}");
            Debug.WriteLine($"  Retain available: {e.Message.RetainAvailable}");
            Debug.WriteLine($"  Return code: {e.Message.ReturnCode}");
            Debug.WriteLine($"  Server keep alive: {e.Message.ServerKeepAlive}");
            Debug.WriteLine($"  Server ref: {e.Message.ServerReference}");
            Debug.WriteLine($"  Session exp inter: {e.Message.SessionExpiryInterval}");
            Debug.WriteLine($"  Session present: {e.Message.SessionPresent}");
            Debug.WriteLine($"  Shared subs available: {e.Message.SharedSubscriptionAvailable}");
            Debug.WriteLine($"  Shared identifier available: {e.Message.SubscriptionIdentifiersAvailable}");
            Debug.WriteLine($"  Topic alias max: {e.Message.TopicAliasMaximum}");
            Debug.WriteLine($"  Num user props: {e.Message.UserProperties.Count}");
            foreach (UserProperty prop in e.Message.UserProperties)
            {
                Debug.WriteLine($"    Key  : {prop.Name}");
                Debug.WriteLine($"    Value: {prop.Value}");
            }

            Debug.WriteLine($"  Wildcard available: {e.Message.WildcardSubscriptionAvailable}");
        }

        private const string CertMosquitto = @"-----BEGIN CERTIFICATE-----
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

        private const string CertEMQX = @"-----BEGIN CERTIFICATE-----
MIIF3jCCA8agAwIBAgIQAf1tMPyjylGoG7xkDjUDLTANBgkqhkiG9w0BAQwFADCB
iDELMAkGA1UEBhMCVVMxEzARBgNVBAgTCk5ldyBKZXJzZXkxFDASBgNVBAcTC0pl
cnNleSBDaXR5MR4wHAYDVQQKExVUaGUgVVNFUlRSVVNUIE5ldHdvcmsxLjAsBgNV
BAMTJVVTRVJUcnVzdCBSU0EgQ2VydGlmaWNhdGlvbiBBdXRob3JpdHkwHhcNMTAw
MjAxMDAwMDAwWhcNMzgwMTE4MjM1OTU5WjCBiDELMAkGA1UEBhMCVVMxEzARBgNV
BAgTCk5ldyBKZXJzZXkxFDASBgNVBAcTC0plcnNleSBDaXR5MR4wHAYDVQQKExVU
aGUgVVNFUlRSVVNUIE5ldHdvcmsxLjAsBgNVBAMTJVVTRVJUcnVzdCBSU0EgQ2Vy
dGlmaWNhdGlvbiBBdXRob3JpdHkwggIiMA0GCSqGSIb3DQEBAQUAA4ICDwAwggIK
AoICAQCAEmUXNg7D2wiz0KxXDXbtzSfTTK1Qg2HiqiBNCS1kCdzOiZ/MPans9s/B
3PHTsdZ7NygRK0faOca8Ohm0X6a9fZ2jY0K2dvKpOyuR+OJv0OwWIJAJPuLodMkY
tJHUYmTbf6MG8YgYapAiPLz+E/CHFHv25B+O1ORRxhFnRghRy4YUVD+8M/5+bJz/
Fp0YvVGONaanZshyZ9shZrHUm3gDwFA66Mzw3LyeTP6vBZY1H1dat//O+T23LLb2
VN3I5xI6Ta5MirdcmrS3ID3KfyI0rn47aGYBROcBTkZTmzNg95S+UzeQc0PzMsNT
79uq/nROacdrjGCT3sTHDN/hMq7MkztReJVni+49Vv4M0GkPGw/zJSZrM233bkf6
c0Plfg6lZrEpfDKEY1WJxA3Bk1QwGROs0303p+tdOmw1XNtB1xLaqUkL39iAigmT
Yo61Zs8liM2EuLE/pDkP2QKe6xJMlXzzawWpXhaDzLhn4ugTncxbgtNMs+1b/97l
c6wjOy0AvzVVdAlJ2ElYGn+SNuZRkg7zJn0cTRe8yexDJtC/QV9AqURE9JnnV4ee
UB9XVKg+/XRjL7FQZQnmWEIuQxpMtPAlR1n6BB6T1CZGSlCBst6+eLf8ZxXhyVeE
Hg9j1uliutZfVS7qXMYoCAQlObgOK6nyTJccBz8NUvXt7y+CDwIDAQABo0IwQDAd
BgNVHQ4EFgQUU3m/WqorSs9UgOHYm8Cd8rIDZsswDgYDVR0PAQH/BAQDAgEGMA8G
A1UdEwEB/wQFMAMBAf8wDQYJKoZIhvcNAQEMBQADggIBAFzUfA3P9wF9QZllDHPF
Up/L+M+ZBn8b2kMVn54CVVeWFPFSPCeHlCjtHzoBN6J2/FNQwISbxmtOuowhT6KO
VWKR82kV2LyI48SqC/3vqOlLVSoGIG1VeCkZ7l8wXEskEVX/JJpuXior7gtNn3/3
ATiUFJVDBwn7YKnuHKsSjKCaXqeYalltiz8I+8jRRa8YFWSQEg9zKC7F4iRO/Fjs
8PRF/iKz6y+O0tlFYQXBl2+odnKPi4w2r78NBc5xjeambx9spnFixdjQg3IM8WcR
iQycE0xyNN+81XHfqnHd4blsjDwSXWXavVcStkNr/+XeTWYRUc+ZruwXtuhxkYze
Sf7dNXGiFSeUHM9h4ya7b6NnJSFd5t0dCy5oGzuCr+yDZ4XUmFF0sbmZgIn/f3gZ
XHlKYC6SQK5MNyosycdiyA5d9zZbyuAlJQG03RoHnHcAP9Dc1ew91Pq7P8yF1m9/
qS3fuQL39ZeatTXaw2ewh0qpKJ4jjv9cJ2vhsE/zB+4ALtRZh8tSQZXq9EfX7mRB
VXyNWQKV3WKdwrnuWih0hKWbt5DHDAff9Yk2dDLWKMGwsAvgnEzDHNb842m1R0aB
L6KCq9NjRHDEjf8tM7qtj3u1cIiuPhnPQCjY/MiQu12ZIvVS5ljFH4gxQ+6IHdfG
jjxDah2nGN59PRbxYvnKkKj9
-----END CERTIFICATE-----
";

        private const string CertAzure = @"-----BEGIN CERTIFICATE-----
MIIDdzCCAl+gAwIBAgIEAgAAuTANBgkqhkiG9w0BAQUFADBaMQswCQYDVQQGEwJJ
RTESMBAGA1UEChMJQmFsdGltb3JlMRMwEQYDVQQLEwpDeWJlclRydXN0MSIwIAYD
VQQDExlCYWx0aW1vcmUgQ3liZXJUcnVzdCBSb290MB4XDTAwMDUxMjE4NDYwMFoX
DTI1MDUxMjIzNTkwMFowWjELMAkGA1UEBhMCSUUxEjAQBgNVBAoTCUJhbHRpbW9y
ZTETMBEGA1UECxMKQ3liZXJUcnVzdDEiMCAGA1UEAxMZQmFsdGltb3JlIEN5YmVy
VHJ1c3QgUm9vdDCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBAKMEuyKr
mD1X6CZymrV51Cni4eiVgLGw41uOKymaZN+hXe2wCQVt2yguzmKiYv60iNoS6zjr
IZ3AQSsBUnuId9Mcj8e6uYi1agnnc+gRQKfRzMpijS3ljwumUNKoUMMo6vWrJYeK
mpYcqWe4PwzV9/lSEy/CG9VwcPCPwBLKBsua4dnKM3p31vjsufFoREJIE9LAwqSu
XmD+tqYF/LTdB1kC1FkYmGP1pWPgkAx9XbIGevOF6uvUA65ehD5f/xXtabz5OTZy
dc93Uk3zyZAsuT3lySNTPx8kmCFcB5kpvcY67Oduhjprl3RjM71oGDHweI12v/ye
jl0qhqdNkNwnGjkCAwEAAaNFMEMwHQYDVR0OBBYEFOWdWTCCR1jMrPoIVDaGezq1
BE3wMBIGA1UdEwEB/wQIMAYBAf8CAQMwDgYDVR0PAQH/BAQDAgEGMA0GCSqGSIb3
DQEBBQUAA4IBAQCFDF2O5G9RaEIFoN27TyclhAO992T9Ldcw46QQF+vaKSm2eT92
9hkTI7gQCvlYpNRhcL0EYWoSihfVCr3FvDB81ukMJY2GQE/szKN+OMY3EU/t3Wgx
jkzSswF07r51XgdIGn9w/xZchMB5hbgF/X++ZRGjD8ACtPhSNzkE1akxehi/oCr0
Epn3o0WC4zxe9Z2etciefC7IpJ5OCBRLbf1wbWsaY71k5h+3zvDyny67G7fyUIhz
ksLi4xaNmjICq44Y3ekQEe5+NauQrz4wlHrQMz2nZQ/1/I6eYs9HRCwBXbsdtTLS
R9I4LtD+gdwyah617jzV/OeBHRnDJELqYzmp
-----END CERTIFICATE-----
";
    }
}
