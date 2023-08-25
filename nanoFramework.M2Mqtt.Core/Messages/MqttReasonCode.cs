// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace nanoFramework.M2Mqtt.Messages
{
    /// <summary>
    /// A Reason Code is a one byte unsigned value that indicates the result of an operation.
    /// Reason Codes less than 0x80 indicate successful completion of an operation.
    /// The normal Reason Code for success is 0.
    /// Reason Code values of 0x80 or greater indicate failure.
    /// </summary>
    public enum MqttReasonCode
    {
        /// <summary>Success</summary>
        Success = 0x00,

        /// <summary>Normal disconnection</summary>
        NormalDisconnection = 0x00,

        /// <summary>Granted QoS 0</summary>
        GrantedQoS0 = 0x00,

        /// <summary>Granted QoS 1</summary>
        GrantedQoS1 = 0x01,

        /// <summary>Granted QoS 2</summary>
        GrantedQoS2 = 0x02,

        /// <summary>Disconnect with Will Message</summary>
        DisconnectWithWillMessage = 0x04,

        /// <summary>No matching subscribers</summary>
        NoMatchingSubscribers = 0x10,

        /// <summary>No subscription existed</summary>
        NoSubscriptionExisted = 0x11,

        /// <summary>Continue authentication</summary>
        ContinueAuthentication = 0x18,

        /// <summary>Re-authenticate</summary>
        ReAuthenticate = 0x19,

        /// <summary>Unspecified error</summary>
        UnspecifiedError = 0x80,

        /// <summary>Malformed Packet</summary>
        MalformedPacket = 0x81,

        /// <summary>Protocol Error</summary>
        ProtocolError = 0x82,

        /// <summary>Implementation specific error</summary>
        ImplementationSpecificError = 0x83,

        /// <summary>Unsupported Protocol Version</summary>
        UnsupportedProtocolVersion = 0x84,

        /// <summary>Client Identifier not valid</summary>
        ClientIdentifierNotValid = 0x85,

        /// <summary>Bad User Name or Password</summary>
        BadUserNameOrPassword = 0x86,

        /// <summary>Not authorized</summary>
        NotAuthorized = 0x87,

        /// <summary>Server unavailable</summary>
        ServerUnavailable = 0x88,

        /// <summary>Server busy</summary>
        ServerBusy = 0x89,

        /// <summary>Banned</summary>
        Banned = 0x8A,

        /// <summary>Server shutting down</summary>
        ServeShuttingDown = 0x8B,

        /// <summary>Bad authentication method</summary>
        BadAuthenticationMethod = 0x8C,

        /// <summary>Keep Alive timeout</summary>
        KeepAliveTimeout = 0x8D,
        /// <summary>Session taken over</summary>
        /// 
        SessionTakenOver = 0x8E,
        /// <summary>Topic Filter invalid</summary>
        TopicFilterInvalid = 0x8F,

        /// <summary>Topic Name invalid</summary>
        TopicNameInvalid = 0x90,

        /// <summary>Packet Identifier in use</summary>
        PacketIdentifierInUse = 0x91,

        /// <summary>Packet Identifier not found</summary>
        PacketIdentifierNotFound = 0x92,

        /// <summary>Receive Maximum exceeded</summary>
        ReceiveMaximumExceeded = 0x93,

        /// <summary>Topic Alias invalid</summary>
        TopicAliasInvalid = 0x94,

        /// <summary>Packet too large</summary>
        PacketTooLarge = 0x95,

        /// <summary>Message rate too high</summary>
        MessageRateTooHigh = 0x96,

        /// <summary>Quota exceeded</summary>
        QuotaExceeded = 0x97,

        /// <summary>Administrative action</summary>
        AdministrativeAction = 0x98,

        /// <summary>Payload format invalid</summary>
        PayloadFormatInvalid = 0x99,

        /// <summary>Retain not supported</summary>
        RetainNotSupported = 0x9A,

        /// <summary>QoS not supported</summary>
        QoSNotSupported = 0x9B,

        /// <summary>Use another server</summary>
        UseAnotherServer = 0x9C,

        /// <summary>Server moved</summary>
        ServerMoved = 0x9D,

        /// <summary>Shared Subscriptions not supported</summary>
        SharedSubscriptionsNotSupported = 0x9E,

        /// <summary>Connection rate exceeded</summary>
        ConnectionRateExceeded = 0x9F,

        /// <summary>Maximum connect time</summary>
        MaximumConnectTime = 0xA0,

        /// <summary>Subscription Identifiers not supported</summary>
        SubscriptionIdentifiersNotSupported = 0xA1,

        /// <summary>Wildcard Subscriptions not supported</summary>
        WildcardSubscriptionsNotSupported = 0xA2,

        // v3.1.1.1
        /// <summary>The Server does not support the level of the MQTT protocol requested by the Client, </summary>
        ConnectionRefusedUnacceptableProtocolVersion = 0x01,

        /// <summary>The Client identifier is correct UTF-8 but not allowed by the Server, </summary>
        ConnectionRefusedIdentifierRejected = 0x02,

        /// <summary>The Network Connection has been made but the MQTT service is unavailable, </summary>
        ConnectionRefusedServerUnavailable = 0x03,

        /// <summary>The data in the user name or password is malformed, </summary>
        ConnectionRefusedBadUserNameOrPassword = 0x04,

        /// <summary>The Client is not authorized to connect, </summary>
        ConnectionRefusedNotAuthorized = 0x05,

    }
}
