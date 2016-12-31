using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace uPLibrary.Networking.M2Mqtt.Internal
{
    /// <summary>
    /// Internal event for a published message
    /// </summary>
    public class MsgPublishedInternalEvent : MsgInternalEvent
    {
        #region Properties...

        /// <summary>
        /// Message published (or failed due to retries)
        /// </summary>
        public bool IsPublished
        {
            get { return this.isPublished; }
            internal set { this.isPublished = value; }
        }

        #endregion

        // published flag
        bool isPublished;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="msg">Message published</param>
        /// <param name="isPublished">Publish flag</param>
        public MsgPublishedInternalEvent(MqttMsgBase msg, bool isPublished)
            : base(msg)
        {
            this.isPublished = isPublished;
        }
    }

}
