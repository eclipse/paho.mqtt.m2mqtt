[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=nanoframework_paho.mqtt.m2mqtt&metric=alert_status)](https://sonarcloud.io/dashboard?id=nanoframework_paho.mqtt.m2mqtt) [![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=nanoframework_paho.mqtt.m2mqtt&metric=reliability_rating)](https://sonarcloud.io/dashboard?id=nanoframework_paho.mqtt.m2mqtt) [![License](https://img.shields.io/badge/License-EPL-blue.svg)](https://github.com/nanoframework/paho.mqtt.m2mqtt/blob/main/LICENSE) [![NuGet](https://img.shields.io/nuget/dt/nanoFramework.M2Mqtt.svg)](https://www.nuget.org/packages/nanoFramework.M2Mqtt) [![#yourfirstpr](https://img.shields.io/badge/first--timers--only-friendly-blue.svg)](https://github.com/nanoframework/Home/blob/main/CONTRIBUTING.md) [![Discord](https://img.shields.io/discord/478725473862549535.svg)](https://discord.gg/gCyBu8T)

![nanoFramework logo](https://raw.githubusercontent.com/nanoframework/Home/main/resources/logo/nanoFramework-repo-logo.png)

---

文档语言: [English](README.md) | [简体中文](README.zh-cn.md)

# .NET **nanoFramework** M2Mqtt

欢迎使用 .NET **nanoFramework**的 MQTT 客户端库。当前版本支持 v3.1、v3.1.1 和 v5.0。

这是 MQTT 客户端库 [M2Mqtt](https://github.com/eclipse/paho.mqtt.m2mqtt) 的初始端口。
原项目有官网[这里](https://m2mqtt.wordpress.com/)。

从那时起，MQTT 客户端发生了相当大的变化，并已适应 .NET nanoFramework。

## 构建状态

| 组件                 | 构建状态                                                                                                                                                                                                                                                                                                                            | NuGet 包                                                                                                                                                    |
| :------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------- |
| nanoFramework.M2Mqtt | [![构建状态](https://dev.azure.com/nanoframework/nanoFramework.m2mqtt/_apis/build/status/nanoframework.m2mqtt?repoName=nanoframework%2FnanoFramework.m2mqtt&branchName=main)](https://dev.azure.com/nanoframework/nanoFramework.m2mqtt/_build/latest?definitionId=56&repoName=nanoframework%2FnanoFramework.m2mqtt&branchName=main) | [![NuGet](https://img.shields.io/nuget/v/nanoFramework.M2Mqtt.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.M2Mqtt/) |

## 项目描述

M2Mqtt 是用于物联网和 M2M 通信的 MQTT 客户端。

[MQTT](https://mqtt.org/),Message Queue Telemetry Transport 的缩写，是一种轻量级消息传递协议，它使资源有限的嵌入式设备能够在受限网络上执行异步通信。

MQTT 协议基于发布/订阅模式，因此客户端可以订阅一个或多个主题并接收其他客户端在这些主题上发布的消息。

该库包含一个示例 MQTT 客户端，您可以使用它连接到任何 MQTT 代理。

这些二进制文件以 [NuGet 包](https://www.nuget.org/packages/nanoframework.M2Mqtt) 的形式提供。

有关 MQTT 协议的所有信息，请访问 MQTT 官方[网站](http://mqtt.org/)。建议您充分了解 MQTT 协议的工作原理以正确使用它。服务质量的机制是一个重要的理解机制。

## 用法

无论使用什么版本，用法都是全局相同的。 v3.1.1 和 v5.0 之间有一些特殊性。 5.0 版带来了更多控制和附加属性。为方便起见，它们都在属性注释中使用 `v5.0 only` 进行了注释。如果您将 v5.0 属性与 v3.1 或 v3.1.1 协议一起使用，它们将被忽略。

这是创建 v3.1.1 服务器并连接到它的基本示例：

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

对于 v5.0，您只需要在连接之前指定版本：

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

注意：在这两个示例中，都需要特定的证书才能连接到 Mosquitto 服务器。您将在 [示例](./TestMqtt/TestAppv5.0). 中找到它

### v5.0 特定的身份验证流程

MQTT 5.0 版支持特定的身份验证流程。在 Connect 之后，Authentication 机制可以像质询请求一样使用。在这种情况下，您必须：

- 确保将 v5 设置为协议
- 将属性`IsAuthenticationFlow`设置为 true
- 注册到 `Authentication` 事件
- 通过发送另一条身份验证消息或有关您的案例所需的任何内容来相应地管理答案。

注意：该协议使用 `AuthenticationMethod` 和 `AuthenticationData` 作为此特定机制的属性。

以下是规范给出的示例：

显示 SCRAM 挑战的非规范示例

- 客户端到服务器：CONNECT Authentication Method="SCRAM-SHA-1" Authentication Data=client-first-data
- 服务器到客户端：AUTH rc=0x18 Authentication Method="SCRAM-SHA-1" Authentication Data=server-first-data
- Client to Server AUTH rc=0x18 Authentication Method="SCRAM-SHA-1" Authentication Data=client-final-data
- 服务器到客户端 CONNACK rc=0 Authentication Method="SCRAM-SHA-1" Authentication Data=server-final-data

显示 Kerberos 挑战的非规范示例

- 客户端到服务器连接认证方法="GS2-KRB5"
- 服务器到客户端 AUTH rc=0x18 身份验证方法="GS2-KRB5"
- Client to Server AUTH rc=0x18 Authentication Method="GS2-KRB5" Authentication Data=initial context token
- Server to Client AUTH rc=0x18 Authentication Method="GS2-KRB5" Authentication Data=reply context token
- 客户端到服务器 AUTH rc=0x18 验证方法="GS2-KRB5"
- Server to Client CONNACK rc=0 Authentication Method="GS2-KRB5" Authentication Data=认证结果

在这些机制中，只有在收到带有成功代码的 Connack 时才会设置“IsConnected”属性。由于这些身份验证机制是特定的并且是用户设置的，因此这个特定的`MqttClient`提供了使用这种机制的能力。

### 订阅事件

MqttClient 提供事件。您可以订阅它们。例如，您可以获得有关何时使用 v5.0 协议打开连接的其他信息。以下示例显示了在启用 MQTT v5.0 协议的情况下连接到 Azure IoT Hub 所需的内容：

```csharp
// 创建客户端
MqttClient mqtt = new MqttClient(IoTHub, 8883, true, new X509Certificate(CertAzure), null, MqttSslProtocols.TLSv1_2);
// 设置版本
mqtt.ProtocolVersion = MqttProtocolVersion.Version_5;
// 注册活动
mqtt.ConnectionOpened += MqttConnectionOpened;
// 您可以添加其他属性
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
// 您将在此处获得更多代码

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

### 例子

M2Mqtt 库提供了一个主类`MqttClient`，它代表 MQTT 客户端连接到代理。您可以连接到提供其 IP 地址或主机名以及一些与 MQTT 协议相关的可选参数的代理。
连接到代理后，您可以使用 `Publish()` 方法将消息发布到主题，并使用 `Subscribe()` 方法订阅主题并接收在其上发布的消息。 `MqttClient` 类是基于事件的，因此当消息发布到您订阅的主题时，您会收到一个事件。您可以在消息发布完成、订阅或取消订阅主题时收到事件。

以下是主题的客户端订阅者示例：

```csharp
string MQTT_BROKER_ADDRESS = "192.168.1.2";
// 创建客户端实例
MqttClient client = new MqttClient(IPAddress.Parse(MQTT_BROKER_ADDRESS));

// 注册到收到的消息
client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;

string clientId = Guid.NewGuid().ToString();
client.Connect(clientId);

// 使用 QoS 2 订阅主题“/home/temperature”
client.Subscribe(new string[] { "/home/temperature" }, new MqttQoSLevel[] { MqttMsgBase.ExactlyOnce });

// 您可以在此处添加一些代码

static void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
{
//处理收到的消息
}
```

以下是一个主题的客户端发布者示例：

```csharp
string MQTT_BROKER_ADDRESS = "192.168.1.2";
// 创建客户端实例
MqttClient client = new MqttClient(IPAddress.Parse(MQTT_BROKER_ADDRESS));

string clientId = Guid.NewGuid().ToString();
client.Connect(clientId);

string strValue = Convert.ToString(value);

// 使用 QoS 2 发布关于“/home/temperature”主题的消息
client.Publish("/home/temperature", Encoding.UTF8.GetBytes(strValue), MqttQoSLevel.ExactlyOnce, false);

// 更多代码在这里
```

### 避免证书检查

在某些情况下，通过 TLS 连接时避免证书检查会很方便。虽然**不**推荐这种情况，但您可以像这样进行调整：

```csharp
//您可以完全不指定证书
MqttClient mqtt = new MqttClient(IoTHub, 8883, true, null, null, MqttSslProtocols.TLSv1_2);
//而且您必须将 ValidateServerCertificate 设置为 false
mqtt.Settings.ValidateServerCertificate = false;
string clientId = Guid.NewGuid().ToString();
client.Connect(clientId);
```

## 反馈和文档

有关文档、提供反馈、问题和了解如何贡献，请参阅 [Home repo](https://github.com/nanoframework/Home)。

加入我们的 Discord 社区 [此处](https://discord.gg/gCyBu8T)。

## 学分

该项目的贡献者列表可以在 [CONTRIBUTORS](https://github.com/nanoframework/Home/blob/main/CONTRIBUTORS.md) 找到。
该库由 [Paolo Patierno](https://github.com/ppatierno) 创建和维护，它是 [Eclipse 项目](https://github.com/eclipse/paho.mqtt.m2mqtt) 的一部分。

## License

**nanoFramework**类库在 [MIT 许可](LICENSE.md) 下获得许可。

## 行为守则

该项目采用了贡献者公约定义的行为准则，以阐明我们社区中的预期行为。
有关详细信息，请参阅 [.NET Foundation 行为准则](https://dotnetfoundation.org/code-of-conduct)。

### .NET Foundation

该项目由 [.NET Foundation](https://dotnetfoundation.org) 支持.
