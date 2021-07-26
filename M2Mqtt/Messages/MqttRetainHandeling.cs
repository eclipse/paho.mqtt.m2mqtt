// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace nanoFramework.M2Mqtt.Messages
{
    /// <summary>
    /// Retain Handling option. This option specifies whether retained messages are sent when the subscription is established.
    /// This does not affect the sending of retained messages at any point after the subscribe.
    /// If there are no retained messages matching the Topic Filter, all of these values act the same.
    /// </summary>
    public enum  MqttRetainHandeling
    {
        /// <summary>
        /// Send retained messages at the time of the subscribe
        /// </summary>
        AllTime= 0,

        /// <summary>
        /// Send retained messages at subscribe only if the subscription does not currently exist
        /// </summary>
        SubscribeTimeOnly = 1,

        /// <summary>
        /// Do not send retained messages at the time of the subscribe
        /// </summary>
        NoRetain = 2,
    }
}
