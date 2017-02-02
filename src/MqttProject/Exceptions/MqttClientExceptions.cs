using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace uPLibrary.Networking.M2Mqtt.Exceptions
{
    /// <summary>
    /// MQTT client exception
    /// </summary>
    public class MqttClientException : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="code">Error code</param>
        public MqttClientException(MqttClientErrorCode errorCode)
        {
            this.errorCode = errorCode;
        }

        // error code
        private MqttClientErrorCode errorCode;

        /// <summary>
        /// Error code
        /// </summary>
        public MqttClientErrorCode ErrorCode
        {
            get { return this.errorCode; }
            set { this.errorCode = value; }
        }
    }

    /// <summary>
    /// MQTT client erroro code
    /// </summary>
    public enum MqttClientErrorCode
    {
        /// <summary>
        /// Will error (topic, message or QoS level)
        /// </summary>
        WillWrong = 1,

        /// <summary>
        /// Keep alive period too large
        /// </summary>
        KeepAliveWrong,

        /// <summary>
        /// Topic contains wildcards
        /// </summary>
        TopicWildcard,

        /// <summary>
        /// Topic length wrong
        /// </summary>
        TopicLength,

        /// <summary>
        /// QoS level not allowed
        /// </summary>
        QosNotAllowed,

        /// <summary>
        /// Topics list empty for subscribe
        /// </summary>
        TopicsEmpty,

        /// <summary>
        /// Qos levels list empty for subscribe
        /// </summary>
        QosLevelsEmpty,

        /// <summary>
        /// Topics / Qos Levels not match in subscribe
        /// </summary>
        TopicsQosLevelsNotMatch,

        /// <summary>
        /// Wrong message from broker
        /// </summary>
        WrongBrokerMessage,

        /// <summary>
        /// Wrong Message Id
        /// </summary>
        WrongMessageId,

        /// <summary>
        /// Inflight queue is full
        /// </summary>
        InflightQueueFull,

        // [v3.1.1]
        /// <summary>
        /// Invalid flag bits received 
        /// </summary>
        InvalidFlagBits,

        // [v3.1.1]
        /// <summary>
        /// Invalid connect flags received
        /// </summary>
        InvalidConnectFlags,

        // [v3.1.1]
        /// <summary>
        /// Invalid client id
        /// </summary>
        InvalidClientId,

        // [v3.1.1]
        /// <summary>
        /// Invalid protocol name
        /// </summary>
        InvalidProtocolName
    }
}
