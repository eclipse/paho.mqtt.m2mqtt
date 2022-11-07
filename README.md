[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=nanoframework_paho.mqtt.m2mqtt&metric=alert_status)](https://sonarcloud.io/dashboard?id=nanoframework_paho.mqtt.m2mqtt) [![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=nanoframework_paho.mqtt.m2mqtt&metric=reliability_rating)](https://sonarcloud.io/dashboard?id=nanoframework_paho.mqtt.m2mqtt) [![License](https://img.shields.io/badge/License-EPL-blue.svg)](https://github.com/nanoframework/paho.mqtt.m2mqtt/blob/main/LICENSE) [![NuGet](https://img.shields.io/nuget/dt/nanoFramework.M2Mqtt.svg)](https://www.nuget.org/packages/nanoFramework.M2Mqtt) [![#yourfirstpr](https://img.shields.io/badge/first--timers--only-friendly-blue.svg)](https://github.com/nanoframework/Home/blob/main/CONTRIBUTING.md) [![Discord](https://img.shields.io/discord/478725473862549535.svg)](https://discord.gg/gCyBu8T)

![nanoFramework logo](https://raw.githubusercontent.com/nanoframework/Home/main/resources/logo/nanoFramework-repo-logo.png)

---

Document Language: [English](README.md) | [简体中文](README.zh-cn.md)

# .NET **nanoFramework** M2Mqtt

Welcome to the MQTT Client Library for .NET **nanoFramework**. The current version supports v3.1, v3.1.1 and v5.0.

This is an initial port of the MQTT Client Library [M2Mqtt](https://github.com/eclipse/paho.mqtt.m2mqtt).
The original project has an official website [here](https://m2mqtt.wordpress.com/).

Since that time, the MQTT Client had quite some changes and has been adapted to .NET nanoFramework.

## Build status

| Component            | Build Status                                                                                                                                                                                                                                                                                                                            | NuGet Package                                                                                                                                               |
| :------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------- |
| nanoFramework.M2Mqtt | [![Build Status](https://dev.azure.com/nanoframework/nanoFramework.m2mqtt/_apis/build/status/nanoframework.m2mqtt?repoName=nanoframework%2FnanoFramework.m2mqtt&branchName=main)](https://dev.azure.com/nanoframework/nanoFramework.m2mqtt/_build/latest?definitionId=56&repoName=nanoframework%2FnanoFramework.m2mqtt&branchName=main) | [![NuGet](https://img.shields.io/nuget/v/nanoFramework.M2Mqtt.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.M2Mqtt/) |

## Project Description

M2Mqtt is a MQTT client for Internet of Things and M2M communication.

[MQTT](https://mqtt.org/), short for Message Queue Telemetry Transport, is a light weight messaging protocol that enables embedded devices with limited resources to perform asynchronous communication on a constrained network.

MQTT protocol is based on publish/subscribe pattern so that a client can subscribe to one or more topics and receive messages that other clients publish on these topics.

This library contains an sample MQTT client that you can use to connect to any MQTT broker.

The binaries are available as a [NuGet package](https://www.nuget.org/packages/nanoframework.M2Mqtt).

For all information about MQTT protocol, please visit MQTT official [web site](http://mqtt.org/). It is recommended to have a good understanding of how MQTT protocol is working to properly use it. The mechanism of Quality of Service is an important one to understand.

## Usage

The usage is globally the same whatever version is used. There are some specificities between v3.1.1 and v5.0. The version 5.0 brings more control and additional properties. For convenience, they are all commented with `v5.0 only` in the properties comments. If you're using a v5.0 property with the v3.1 or v3.1.1 protocol, they'll just be ignored.

Here is a basic example of creating a v3.1.1 server and connecting to it:

```csharp
MqttClient mqtt = new MqttClient("test.mosquitto.org", 8883, true, new X509Certificate(CertMosquitto), null, MqttSslProtocols.TLSv1_2);
var ret = mqtt.Connect("nanoTestDevice", true);
if (ret != MqttReasonCode.Success)
{
    Debug.WriteLine($"ERROR connecting: {ret}");
    mqtt.Disconnect();
    return;
}
```

For the v5.0, you just need to specify the version before the connection:

```csharp
MqttClient mqtt = new MqttClient("test.mosquitto.org", 8883, true, new X509Certificate(CertMosquitto), null, MqttSslProtocols.TLSv1_2);
mqtt.ProtocolVersion = MqttProtocolVersion.Version_5;
var ret = mqtt.Connect("nanoTestDevice", true);
if (ret != MqttReasonCode.Success)
{
    Debug.WriteLine($"ERROR connecting: {ret}");
    mqtt.Disconnect();
    return;
}
```

Note: in both example, a specific certificate is needed to connect to the Mosquitto server. You will find it in the [sample](./TestMqtt/TestAppv5.0).

### v5.0 specific Authentication flow

The MQTT version 5.0 supports specific Authentication flow. After a Connect, the Authentication mechanism can be used like a challenge request. In this case, you'll have to:

- Make sure you setup v5 as a protocol
- Place the property `IsAuthenticationFlow` to true
- Register to the `Authentication` event
- Manage the answers accordingly by sending another Authentication message or anything that is needed regarding your case.

Note: the protocol is using the `AuthenticationMethod` and `AuthenticationData` as properties for this specific mechanism.

Here are examples given by the specifications:

Non-normative example showing a SCRAM challenge

- Client to Server: CONNECT Authentication Method="SCRAM-SHA-1" Authentication Data=client-first-data
- Server to Client: AUTH rc=0x18 Authentication Method="SCRAM-SHA-1" Authentication Data=server-first-data
- Client to Server AUTH rc=0x18 Authentication Method="SCRAM-SHA-1" Authentication Data=client-final-data
- Server to Client CONNACK rc=0 Authentication Method="SCRAM-SHA-1" Authentication Data=server-final-data

Non-normative example showing a Kerberos challenge

- Client to Server CONNECT Authentication Method="GS2-KRB5"
- Server to Client AUTH rc=0x18 Authentication Method="GS2-KRB5"
- Client to Server AUTH rc=0x18 Authentication Method="GS2-KRB5" Authentication Data=initial context token
- Server to Client AUTH rc=0x18 Authentication Method="GS2-KRB5" Authentication Data=reply context token
- Client to Server AUTH rc=0x18 Authentication Method="GS2-KRB5"
- Server to Client CONNACK rc=0 Authentication Method="GS2-KRB5" Authentication Data=outcome of authentication

In those mechanism, the `IsConnected` property will be setup only once a Connack with a success code will be received. As those authentication mechanism are specific and user setup, this specific `MqttClient` offers the ability to use this mechanism.

### Subscribing to events

The MqttClient offers events. You can subscribe to them. As an example, you can get additional information on when the connection is opened with the v5.0 protocol. The below example show what is required to connect to Azure IoT Hub with the MQTT v5.0 protocol enabled:

```csharp
// Create the client
MqttClient mqtt = new MqttClient(IoTHub, 8883, true, new X509Certificate(CertAzure), null, MqttSslProtocols.TLSv1_2);
// Setup the version
mqtt.ProtocolVersion = MqttProtocolVersion.Version_5;
// Register to events
mqtt.ConnectionOpened += MqttConnectionOpened;
// You can add additional properties
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
var ret = mqtt.Connect(DeviceID, null, null, false, MqttQoSLevel.AtLeastOnce, false, null, null, true, 60);
// You will have more code here

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
```

### Example

The M2Mqtt library provides a main class `MqttClient` that represents the MQTT client to connect to a broker. You can connect to the broker providing its IP address or host name and optionally some parameters related to MQTT protocol.

After connecting to the broker you can use `Publish()` method to publish a message to a topic and `Subscribe()` method to subscribe to a topic and receive message published on it. The `MqttClient` class is events based so that you receive an event when a message is published to a topic you subscribed to. You can receive event when a message publishing is complete, you have subscribed or unsubscribed to a topic.

Following an example of client subscriber to a topic :

```csharp
string MQTT_BROKER_ADDRESS = "192.168.1.2";
// create client instance
MqttClient client = new MqttClient(IPAddress.Parse(MQTT_BROKER_ADDRESS));

// register to message received
client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;

string clientId = Guid.NewGuid().ToString();
client.Connect(clientId);

// subscribe to the topic "/home/temperature" with QoS 2
client.Subscribe(new string[] { "/home/temperature" }, new MqttQoSLevel[] { MqttMsgBase.ExactlyOnce });

// You can add some code here

static void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
{
// handle message received
}
```

Following an example of client publisher to a topic :

```csharp
string MQTT_BROKER_ADDRESS = "192.168.1.2";
// create client instance
MqttClient client = new MqttClient(IPAddress.Parse(MQTT_BROKER_ADDRESS));

string clientId = Guid.NewGuid().ToString();
client.Connect(clientId);

string strValue = Convert.ToString(value);

// publish a message on "/home/temperature" topic with QoS 2
client.Publish("/home/temperature", Encoding.UTF8.GetBytes(strValue), MqttQoSLevel.ExactlyOnce, false);

// More code goes here
```

### Avoiding certificate check

In some cases, it can be handy to avoid the certificate checks when connecting through TLS connection. While this scenario is **not** recommended, you can adjust for it like this:

```csharp
// You can specify no certificate at all
MqttClient mqtt = new MqttClient(IoTHub, 8883, true, null, null, MqttSslProtocols.TLSv1_2);
// And you have to setup the ValidateServerCertificate to false
mqtt.Settings.ValidateServerCertificate = false;
string clientId = Guid.NewGuid().ToString();
client.Connect(clientId);
```

## Feedback and documentation

For documentation, providing feedback, issues and finding out how to contribute please refer to the [Home repo](https://github.com/nanoframework/Home).

Join our Discord community [here](https://discord.gg/gCyBu8T).

## Credits

The list of contributors to this project can be found at [CONTRIBUTORS](https://github.com/nanoframework/Home/blob/main/CONTRIBUTORS.md).
This library was created and maintained by [Paolo Patierno](https://github.com/ppatierno) and it's part of the [Eclipse Project](https://github.com/eclipse/paho.mqtt.m2mqtt).

## License

The **nanoFramework** Class Libraries are licensed under the [MIT license](LICENSE.md).

## Code of Conduct

This project has adopted the code of conduct defined by the Contributor Covenant to clarify expected behaviour in our community.
For more information see the [.NET Foundation Code of Conduct](https://dotnetfoundation.org/code-of-conduct).

### .NET Foundation

This project is supported by the [.NET Foundation](https://dotnetfoundation.org).
