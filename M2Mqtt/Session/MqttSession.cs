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

using System.Collections;

namespace nanoFramework.M2Mqtt.Session
{
    /// <summary>
    /// MQTT Session base class
    /// </summary>
    public abstract class MqttSession
    {
        /// <summary>
        /// Client Id
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Messages inflight during session
        /// </summary>
        public Hashtable InflightMessages { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        protected MqttSession()
            : this(null)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="clientId">Client Id to create session</param>
        protected MqttSession(string clientId)
        {
            ClientId = clientId;
            InflightMessages = new Hashtable();
        }

        /// <summary>
        /// Clean session
        /// </summary>
        public virtual void Clear()
        {
            ClientId = null;
            InflightMessages.Clear();
        }
    }
}
