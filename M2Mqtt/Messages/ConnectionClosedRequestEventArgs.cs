// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace nanoFramework.M2Mqtt.Messages
{
    /// <summary>
    /// Event Args class for DISCONNECT message received from server
    /// </summary>
    public class ConnectionClosedRequestEventArgs : EventArgs
    {
        /// <summary>
        /// Message received from client
        /// </summary>
        public MqttMsgDisconnect Message { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="disconnect">DISCONNECT message received from client</param>
        public ConnectionClosedRequestEventArgs(MqttMsgDisconnect disconnect)
        {
            Message = disconnect;
        }
    }
}
