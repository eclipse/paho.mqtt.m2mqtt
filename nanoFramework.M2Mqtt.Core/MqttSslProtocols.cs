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
    /// Supported SSL/TLS protocol versions
    /// </summary>
    public enum MqttSslProtocols
    {
        /// <summary>
        /// None
        /// </summary>
        None,
        /// <summary>
        /// SSL version 3
        /// </summary>
        SSLv3,
        /// <summary>
        /// TLS version 1.0
        /// </summary>
        TLSv1_0,
        /// <summary>
        /// TLS version 1.1
        /// </summary>
        TLSv1_1,
        /// <summary>
        /// TLS version 1.2
        /// </summary>
        TLSv1_2,
        /// <summary>
        /// TLS version 1.3
        /// </summary>
        TLSv1_3
    }
}
