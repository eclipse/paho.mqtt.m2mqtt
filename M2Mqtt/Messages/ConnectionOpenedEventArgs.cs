// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace nanoFramework.M2Mqtt.Messages
{
    /// <summary>
    /// Event Args class for CONNECT message received from client
    /// </summary>
    public class ConnectionOpenedEventArgs : EventArgs
    {
        /// <summary>
        /// Message received from client
        /// </summary>
        public MqttMsgConnack Message { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connact">CONNECT message received from client</param>
        public ConnectionOpenedEventArgs(MqttMsgConnack connact)
        {
            Message = connact;
        }
    }
}