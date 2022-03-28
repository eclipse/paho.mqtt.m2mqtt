// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace nanoFramework.M2Mqtt.Messages
{
    internal enum MqttProperty
    {
        /// <summary>Byte, PUBLISH, Will Properties</summary>
        PayloadFormatIndicator = 0x01,
        /// <summary>Four Byte Integer, PUBLISH, Will Properties</summary>
        MessageExpiryInterval = 0x02,
        /// <summary>UTF-8 Encoded String, PUBLISH, Will Properties</summary>
        ContentType = 0x03,
        /// <summary>UTF-8 Encoded String, PUBLISH, Will Properties</summary>
        ResponseTopic = 0x08,
        /// <summary>Binary Data, PUBLISH, Will Properties</summary>
        CorrelationData = 0x09,
        /// <summary>Variable Byte Integer, PUBLISH, SUBSCRIBE</summary>
        SubscriptionIdentifier = 0x0B,
        /// <summary>Four Byte Integer, CONNECT, CONNACK, DISCONNECT</summary>
        SessionExpiryInterval = 0x11,
        /// <summary>UTF-8 Encoded String, CONNACK</summary>
        AssignedClientIdentifier = 0x12,
        /// <summary>Two Byte Integer, CONNACK</summary>
        ServerKeepAlive = 0x13,
        /// <summary>UTF-8 Encoded String, CONNECT, CONNACK, AUTH</summary>
        AuthenticationMethod = 0x15,
        /// <summary>Binary Data, CONNECT, CONNACK, AUTH</summary>
        AuthenticationData = 0x16,
        /// <summary>Byte, CONNECT</summary>
        RequestProblemInformation = 0x17,
        /// <summary>Four Byte Integer, Will Properties</summary>
        WillDelayInterval = 0x18,
        /// <summary>Byte, CONNECT</summary>
        RequestResponseInformation = 0x19,
        /// <summary>UTF-8 Encoded String, CONNACK</summary>
        ResponseInformation = 0x1A,
        /// <summary>UTF-8 Encoded String, CONNACK, DISCONNECT</summary>
        ServerReference = 0x1C,
        /// <summary>UTF-8 Encoded String, CONNACK, PUBACK, PUBREC, PUBREL, PUBCOMP, SUBACK, UNSUBACK, DISCONNECT, AUTH</summary>
        ReasonString = 0x1F,
        /// <summary>Two Byte Integer, CONNECT, CONNACK</summary>
        ReceiveMaximum = 0x21,
        /// <summary>Two Byte Integer, CONNECT, CONNACK</summary>
        TopicAliasMaximum = 0x22,
        /// <summary>Two Byte Integer, PUBLISH</summary>
        TopicAlias = 0x23,
        /// <summary>Byte, CONNACK</summary>
        MaximumQoS = 0x24,
        /// <summary>Byte, CONNACK</summary>
        RetainAvailable = 0x25,
        /// <summary>UTF-8 String Pair, CONNECT, CONNACK, PUBLISH, Will Properties, PUBACK, PUBREC, PUBREL, PUBCOMP, SUBSCRIBE, SUBACK, UNSUBSCRIBE, UNSUBACK, DISCONNECT, AUTH</summary>
        UserProperty = 0x26,
        /// <summary>Four Byte Integer, CONNECT, CONNACK</summary>
        MaximumPacketSize = 0x27,
        /// <summary>Byte, CONNACK</summary>
        WildcardSubscriptionAvailable = 0x28,
        /// <summary>Byte, CONNACK</summary>
        SubscriptionIdentifierAvailable = 0x29,
        /// <summary>Byte, CONNACK</summary>
        SharedSubscriptionAvailable = 0x2A,

    }
}
