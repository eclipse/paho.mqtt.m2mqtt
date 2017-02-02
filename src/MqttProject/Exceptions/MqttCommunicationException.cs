using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace uPLibrary.Networking.M2Mqtt.Exceptions
{/// <summary>
 /// Exception due to error communication with broker on socket
 /// </summary>
    public class MqttCommunicationException : Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public MqttCommunicationException()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="e">Inner Exception</param>
        public MqttCommunicationException(Exception e)
            : base(String.Empty, e)
        {
        }
    }
}
