/*
Copyright (c) 2013, 2014 Paolo Patierno

All rights reserved. This program and the accompanying materials
are made available under the terms of the Eclipse Public License v1.0
and Eclipse Distribution License v1.0 which accompany this distribution. 

The Eclipse Public License is available at 
   http://www.eclipse.org/legal/epl-v10.html
and the Eclipse Distribution License is available at 
   http://www.eclipse.org/org/documents/edl-v10.php.

Contributors:
   Paolo Patierno - initial API and implementation and/or initial documentation
   .NET Foundation and Contributors - nanoFramework support
*/

namespace nanoFramework.M2Mqtt
{
    /// <summary>
    /// Settings class for the MQTT broker
    /// </summary>
    public class MqttSettings
    {
        /// <summary>
        /// Default port for the MQTT protocol
        /// </summary>
        public const int BrokerDefaultPort = 1883;

        /// <summary>
        /// Default SSL port for the MQTT protocol
        /// </summary>
        public const int BrokerDefaultSslPort = 8883;

        /// <summary>
        /// Default timeout on receiving from client
        /// </summary>
        public const int DefaultTimeout = 30000;

        /// <summary>
        /// Max publish, subscribe and unsubscribe retry for QoS Level 1 or 2
        /// </summary>
        public const int MaximumAttemptsRetry = 3;

        /// <summary>
        /// Delay for retry publish, subscribe and unsubscribe for QoS Level 1 or 2
        /// </summary>
        public const int DefaultDelayRetry = 10000;

        /// <summary>
        /// Connection Timeout
        /// </summary>
        /// <remarks>
        /// The broker needs to receive the first message (CONNECT)
        /// within a reasonable amount of time after the initial TCP/IP connection 
        /// </remarks>
        public const int ConnectTimeout = 30000;

        /// <summary>
        /// The default inflight queue size
        /// </summary>
        public const int DefaultInflightQueueSize = int.MaxValue;

        /// <summary>
        /// Listening connection port
        /// </summary>
        public int Port { get; internal set; }

        /// <summary>
        /// Listening connection SSL port
        /// </summary>
        public int SslPort { get; internal set; }

        /// <summary>
        /// Timeout on client connection (before receiving CONNECT message)
        /// </summary>
        public int TimeoutOnConnection { get; internal set; }

        /// <summary>
        /// Timeout on receiving
        /// </summary>
        public int TimeoutOnReceiving { get; internal set; }

        /// <summary>
        /// Attempts on retry
        /// </summary>
        public int AttemptsOnRetry { get; internal set; }

        /// <summary>
        /// Delay on retry
        /// </summary>
        public int DelayOnRetry { get; internal set; }

        /// <summary>
        /// Inflight queue size
        /// </summary>
        public int InflightQueueSize { get; set; }
        
        /// <summary>
        /// Singleton instance of settings
        /// </summary>
        public static MqttSettings Instance
        {
            get
            {
                if (instance == null)
                    instance = new MqttSettings();
                return instance;
            }
        }

        // singleton instance
        private static MqttSettings instance;

        /// <summary>
        /// Constructor
        /// </summary>
        private MqttSettings()
        {
            Port = BrokerDefaultPort;
            SslPort = BrokerDefaultSslPort;
            TimeoutOnReceiving = DefaultTimeout;
            AttemptsOnRetry = MaximumAttemptsRetry;
            DelayOnRetry = DefaultDelayRetry;
            TimeoutOnConnection = ConnectTimeout;
            InflightQueueSize = DefaultInflightQueueSize;
        }
    }
}
