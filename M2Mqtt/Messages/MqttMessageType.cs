// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace nanoFramework.M2Mqtt.Messages
{
    /// <summary>
    /// MQTT Message Type
    /// </summary>
    public enum  MqttMessageType
    {
        /// <summary>
        /// Connection request
        /// </summary>
        Connect = 0x01,

        /// <summary>
        /// Connect acknowledgment
        /// </summary>
        ConnectAck = 0x02,

        /// <summary>
        /// Publish message
        /// </summary>
        Publish = 0x03,

        /// <summary>
        /// Publish acknowledgment (QoS 1)
        /// </summary>
        PublishAck = 0x04,

        /// <summary>
        /// Publish received (QoS 2 delivery part 1)
        /// </summary>
        PublishReceived = 0x05,

        /// <summary>
        /// Publish release (QoS 2 delivery part 2)
        /// </summary>
        PublishRelease = 0x06,

        /// <summary>
        /// Publish complete (QoS 2 delivery part 3)
        /// </summary>
        PublishComplete = 0x07,

        /// <summary>
        /// Subscribe request
        /// </summary>
        Subscribe = 0x08,

        /// <summary>
        /// Subscribe acknowledgment
        /// </summary>
        SubscribeAck = 0x09,

        /// <summary>
        /// Unsubscribe request
        /// </summary>
        Unsubscribe = 0x0A,

        /// <summary>
        /// Unsubscribe acknowledgment
        /// </summary>
        UnsubscribeAck = 0x0B,

        /// <summary>
        /// PING request
        /// </summary>
        PingRequest = 0x0C,

        /// <summary>
        /// PING response
        /// </summary>
        PingResponse = 0x0D,

        /// <summary>
        /// Disconnect notification
        /// </summary>
        Disconnect = 0x0E,

        /// <summary>
        /// Authentication exchange
        /// </summary>
        Authentication = 0x0F,
    }
}
