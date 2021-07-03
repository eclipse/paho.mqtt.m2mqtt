/*
Contributors:   
   .NET Foundation and Contributors - nanoFramework support
*/

namespace nanoFramework.M2Mqtt.Messages
{
    /// <summary>
    /// MQTT Quality of Service Level
    /// </summary>
    public enum MqttQoSLevel
    {
        /// <summary>
        /// QOS At Most Once
        /// </summary>
        AtMostOnce = 0x00,

        /// <summary>
        /// QOS At Least Once
        /// </summary>
        AtLeastOnce = 0x01,

        /// <summary>
        /// QOS Exactly Once
        /// </summary>
        ExactlyOnce = 0x02,

        /// <summary>
        /// Subscribe only QOS Granted Failure [v3.1.1]
        /// </summary>
        GrantedFailure = 0x80,
    }
}
