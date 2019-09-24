[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=nanoframework_paho.mqtt.m2mqtt&metric=alert_status)](https://sonarcloud.io/dashboard?id=nanoframework_paho.mqtt.m2mqtt) [![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=nanoframework_paho.mqtt.m2mqtt&metric=reliability_rating)](https://sonarcloud.io/dashboard?id=nanoframework_paho.mqtt.m2mqtt) [![License](https://img.shields.io/badge/License-EPL-blue.svg)](https://github.com/nanoframework/paho.mqtt.m2mqtt/blob/master/LICENSE) [![NuGet](https://img.shields.io/nuget/dt/nanoFramework.M2Mqtt.svg)](https://www.nuget.org/packages/nanoFramework.M2Mqtt) [![#yourfirstpr](https://img.shields.io/badge/first--timers--only-friendly-blue.svg)](https://github.com/nanoframework/Home/blob/master/CONTRIBUTING.md) [![Discord](https://img.shields.io/discord/478725473862549535.svg)](https://discord.gg/gCyBu8T)

# nanoFramework M2Mqtt

![](images/M2Mqtt_Short_Logo.png)

Welcome to the MQTT Client Library for nanoFramework.

This is a port of the MQTT Client Library [M2Mqtt](https://github.com/eclipse/paho.mqtt.m2mqtt).
The orignal project has an official website [here](https://m2mqtt.wordpress.com/).

## Build status

| Component | Build Status | NuGet Package |
|:-|---|---|
| nanoFramework.M2Mqtt | [![Build Status](https://dev.azure.com/nanoframework/m2mqtt/_apis/build/status/nanoframework.paho.mqtt.m2mqtt?branchName=master)](https://dev.azure.com/nanoframework/m2mqtt/_build/latest?definitionId=46&branchName=master) | [![NuGet](https://img.shields.io/nuget/v/nanoFramework.M2Mqtt.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.M2Mqtt/)  |
| nanoFramework.M2Mqtt (preview) | [![Build Status](https://dev.azure.com/nanoframework/m2mqtt/_apis/build/status/nanoframework.paho.mqtt.m2mqtt?branchName=develop)](https://dev.azure.com/nanoframework/m2mqtt/_build/latest?definitionId=46&branchName=develop) | [![](https://badgen.net/badge/NuGet/preview/D7B023?icon=https://simpleicons.now.sh/azuredevops/fff)](https://dev.azure.com/nanoframework/feed/_packaging?_a=package&feed=sandbox&package=nanoFramework.M2Mqtt&protocolType=NuGet&view=overview) |

## Project Description

M2Mqtt is a MQTT client for Internet of Things and M2M communication.

MQTT, short for Message Queue Telemetry Transport, is a light weight messaging protocol that enables embedded devices with limited resources to perform asynchronous communication on a constrained network.

MQTT protocol is based on publish/subscribe pattern so that a client can subscribe to one or more topics and receive messages that other clients publish on these topics.

This library contains an sample MQTT client that you can use to connect to any MQTT broker.

The binaries are available as a [NuGet package](https://www.nuget.org/packages/nanoframework.M2Mqtt).

For all information about MQTT protocol, please visit official web site  http://mqtt.org/.

## Example

The M2Mqtt library provides a main class MqttClient that represents the MQTT client to connect to a broker. You can connect to the broker providing its IP address or host name and optionally some parameters related to MQTT protocol.

After connecting to the broker you can use Publish() method to publish a message to a topic and Subscribe() method to subscribe to a topic and receive message published on it. The MqttClient class is events based so that you receive an event when a message is published to a topic you subscribed to. You can receive event when a message publishing is complete, you have subscribed or unsubscribed to a topic.

Following an example of client subscriber to a topic :

```csharp
...

// create client instance
MqttClient client = new MqttClient(IPAddress.Parse(MQTT_BROKER_ADDRESS));

// register to message received
client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;

string clientId = Guid.NewGuid().ToString();
client.Connect(clientId);

// subscribe to the topic "/home/temperature" with QoS 2
client.Subscribe(new string[] { "/home/temperature" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });

...

static void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
{
// handle message received 
}
```

Following an example of client publisher to a topic :

```csharp
...

// create client instance
MqttClient client = new MqttClient(IPAddress.Parse(MQTT_BROKER_ADDRESS));

string clientId = Guid.NewGuid().ToString();
client.Connect(clientId);

string strValue = Convert.ToString(value);

// publish a message on "/home/temperature" topic with QoS 2
client.Publish("/home/temperature", Encoding.UTF8.GetBytes(strValue), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);

...
```

## Feedback and documentation

For documentation, providing feedback, issues and finding out how to contribute please refer to the [Home repo](https://github.com/nanoframework/Home).

Join our Discord community [here](https://discord.gg/gCyBu8T).
