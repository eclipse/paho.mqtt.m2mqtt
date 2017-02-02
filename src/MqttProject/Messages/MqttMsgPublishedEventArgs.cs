using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace uPLibrary.Networking.M2Mqtt.Messages
{
    /// <summary>
    /// Event Args class for published message
    /// </summary>
    public class MqttMsgPublishedEventArgs : EventArgs
    {
        #region Properties...

        /// <summary>
        /// Message identifier
        /// </summary>
        public ushort MessageId
        {
            get { return this.messageId; }
            internal set { this.messageId = value; }
        }

        /// <summary>
        /// Message published (or failed due to retries)
        /// </summary>
        public bool IsPublished
        {
            get { return this.isPublished; }
            internal set { this.isPublished = value; }
        }

        #endregion

        // message identifier
        ushort messageId;

        // published flag
        bool isPublished;

        /// <summary>
        /// Constructor (published message)
        /// </summary>
        /// <param name="messageId">Message identifier published</param>
        public MqttMsgPublishedEventArgs(ushort messageId)
            : this(messageId, true)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="messageId">Message identifier</param>
        /// <param name="isPublished">Publish flag</param>
        public MqttMsgPublishedEventArgs(ushort messageId, bool isPublished)
        {
            this.messageId = messageId;
            this.isPublished = isPublished;
        }
    }
}
