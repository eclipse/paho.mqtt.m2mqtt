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

using System;

namespace nanoFramework.M2Mqtt.Messages
{
    /// <summary>
    /// Event Args class for unsubscribe request on topics
    /// </summary>
    public class MqttMsgUnsubscribeEventArgs : EventArgs
    {
        /// <summary>
        /// Message identifier
        /// </summary>
        public ushort MessageId { get; internal set; }

        /// <summary>
        /// Topics requested to subscribe
        /// </summary>
        public string[] Topics { get; internal set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="messageId">Message identifier for subscribed topics</param>
        /// <param name="topics">Topics requested to subscribe</param>
        public MqttMsgUnsubscribeEventArgs(ushort messageId, string[] topics)
        {
            MessageId = messageId;
            Topics = topics;
        }
    }
}
