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
using System.Collections;
using System.Diagnostics;
using System.Text;
using nanoFramework.M2Mqtt.Exceptions;
using nanoFramework.M2Mqtt.Utility;

namespace nanoFramework.M2Mqtt.Messages
{
    /// <summary>
    /// Class for CONNACK message from broker to client
    /// </summary>
    public class MqttMsgConnack : MqttMsgBase
    {
        #region Constants...

        private const byte TOPIC_NAME_COMP_RESP_BYTE_SIZE = 1;
        private const byte CONN_ACK_FLAGS_BYTE_SIZE = 1;
        // [v3.1.1] session present flag
        private const byte SESSION_PRESENT_FLAG_MASK = 0x01;
        private const byte SESSION_PRESENT_FLAG_OFFSET = 0x00;
        private const byte CONN_RETURN_CODE_BYTE_SIZE = 1;

        #endregion

        #region Properties...

        // [v3.1.1] session present flag
        /// <summary>
        /// Session present flag
        /// </summary>
        public bool SessionPresent { get; set; }

        /// <summary>
        /// Return Code
        /// </summary>
        public MqttReasonCode ReturnCode { get; set; }

        /// <summary>
        /// Session Expiry Interval, v5.0 only
        /// </summary>
        public uint SessionExpiryInterval { get; set; }

        /// <summary>
        /// Receive Maximum, v5.0 only
        /// </summary>
        public ushort ReceiveMaximum { get; set; } = ushort.MaxValue;

        /// <summary>
        /// True if there is a maximum QoS, v5.0 only
        /// </summary>
        public bool MaximumQoS { get; set; }

        /// <summary>
        /// True if retain is available, v5.0 only
        /// </summary>
        public bool RetainAvailable { get; set; }

        /// <summary>
        /// The client ID to use to connect to the server. This must replace the initial Client ID used for the connection, v5.0 only
        /// </summary>
        public string AssignedClientIdentifier { get; set; }

        /// <summary>
        /// TopicAliasMaximum, v5.0 only
        /// </summary>
        public ushort TopicAliasMaximum { get; set; }

        /// <summary>
        /// The Reason as a string, v5.0 only
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// True if Wildcard Subscription are Available on the server, v5.0 only
        /// </summary>
        public bool WildcardSubscriptionAvailable { get; set; }

        /// <summary>
        /// True if Subscription Identifiers are Available on the server, v5.0 only
        /// </summary>
        public bool SubscriptionIdentifiersAvailable { get; set; }

        /// <summary>
        /// True if Shared Subscription are Available on the server, v5.0 only
        /// </summary>
        public bool SharedSubscriptionAvailable { get; set; }

        /// <summary>
        /// Use this value instead of the one present in the client sent on Connect, v5.0 only
        /// </summary>
        public ushort ServerKeepAlive { get; set; }

        /// <summary>
        /// Used as the basis for creating a Response Topic, v5.0 only
        /// </summary>
        public string ResponseInformation { get; set; }

        /// <summary>
        /// Used by the Client to identify another Server to use, v5.0 only
        /// </summary>
        public string ServerReference { get; set; }

        /// <summary>
        /// Authentication Method, v5.0 only
        /// </summary>
        public string AuthenticationMethod { get; set; }

        /// <summary>
        /// Authentication Data, v5.0 only
        /// </summary>
        public byte[] AuthenticationData { get; set; }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public MqttMsgConnack()
        {
            Type = MqttMessageType.ConnectAck;
        }

        /// <summary>
        /// Parse bytes for a CONNACK message
        /// </summary>
        /// <param name="fixedHeaderFirstByte">First fixed header byte</param>
        /// <param name="protocolVersion">Protocol Version</param>
        /// <param name="channel">Channel connected to the broker</param>
        /// <returns>CONNACK message instance</returns>
        public static MqttMsgConnack Parse(byte fixedHeaderFirstByte, MqttProtocolVersion protocolVersion, IMqttNetworkChannel channel)
        {
            byte[] buffer;
            int index = 0;
            MqttMsgConnack msg = new MqttMsgConnack();

            if ((protocolVersion == MqttProtocolVersion.Version_3_1_1) || (protocolVersion == MqttProtocolVersion.Version_5))
            {
                // [v3.1.1] check flag bits
                if ((fixedHeaderFirstByte & MSG_FLAG_BITS_MASK) != MQTT_MSG_CONNACK_FLAG_BITS)
                {
                    throw new MqttClientException(MqttClientErrorCode.InvalidFlagBits);
                }
            }

            // get remaining length and allocate buffer
            int remainingLength = DecodeVariableByte(channel);
            buffer = new byte[remainingLength];

            // read bytes from socket...
            channel.Receive(buffer);
            if ((protocolVersion == MqttProtocolVersion.Version_3_1_1) || (protocolVersion == MqttProtocolVersion.Version_5))
            {
                // [v3.1.1] ... set session present flag ...
                msg.SessionPresent = (buffer[index++] & SESSION_PRESENT_FLAG_MASK) != 0x00;
            }
            // ...and set return code from broker
            msg.ReturnCode = (MqttReasonCode)buffer[index++];

            // v5.0 properties
            if (protocolVersion == MqttProtocolVersion.Version_5)
            {
                // size of the properties
                int propSize = EncodeDecodeHelper.GetPropertySize(buffer, ref index);
                propSize += index;
                MqttProperty prop;

                while (propSize > index)
                {
                    prop = (MqttProperty)buffer[index++];
                    switch (prop)
                    {
                        case MqttProperty.SessionExpiryInterval:
                            // 4 bytes
                            msg.SessionExpiryInterval = EncodeDecodeHelper.DecodeUint(buffer, ref index);
                            break;
                        case MqttProperty.ReceiveMaximum:
                            // 2 bytes
                            msg.ReceiveMaximum = (ushort)(buffer[index++] << 8);
                            msg.ReceiveMaximum |= buffer[index++];
                            break;
                        case MqttProperty.MaximumQoS:
                            msg.MaximumQoS = buffer[index++] != 0;
                            break;
                        case MqttProperty.RetainAvailable:
                            msg.RetainAvailable = buffer[index++] != 0;
                            break;
                        case MqttProperty.MaximumPacketSize:
                            // 4 bytes
                            msg.MaximumPacketSize = EncodeDecodeHelper.DecodeUint(buffer, ref index);
                            break;
                        case MqttProperty.AssignedClientIdentifier:
                            // UTF8 encoded
                            msg.AssignedClientIdentifier = EncodeDecodeHelper.GetUTF8FromBuffer(buffer, ref index);
                            break;
                        case MqttProperty.TopicAliasMaximum:
                            // 2 bytes
                            msg.TopicAliasMaximum = (ushort)(buffer[index++] << 8);
                            msg.TopicAliasMaximum |= buffer[index++];
                            break;
                        case MqttProperty.ReasonString:
                            // UTF8 encoded
                            msg.Reason = EncodeDecodeHelper.GetUTF8FromBuffer(buffer, ref index);
                            break;
                        case MqttProperty.UserProperty:
                            // UTF8 key value encoding, so 2 strings in a raw
                            string key = EncodeDecodeHelper.GetUTF8FromBuffer(buffer, ref index);
                            string value = EncodeDecodeHelper.GetUTF8FromBuffer(buffer, ref index);
                            msg.UserProperties.Add(new UserProperty(key, value));
                            break;
                        case MqttProperty.WildcardSubscriptionAvailable:
                            msg.WildcardSubscriptionAvailable = buffer[index++] != 0;
                            break;
                        case MqttProperty.SubscriptionIdentifierAvailable:
                            msg.SubscriptionIdentifiersAvailable = buffer[index++] != 0;
                            break;
                        case MqttProperty.SharedSubscriptionAvailable:
                            msg.SharedSubscriptionAvailable = buffer[index++] != 0;
                            break;
                        case MqttProperty.ServerKeepAlive:
                            // 2 bytes
                            msg.ServerKeepAlive = (ushort)(buffer[index++] << 8);
                            msg.ServerKeepAlive |= buffer[index++];
                            break;
                        case MqttProperty.ResponseInformation:
                            // UTF8 encoded
                            msg.ResponseInformation = EncodeDecodeHelper.GetUTF8FromBuffer(buffer, ref index);
                            break;
                        case MqttProperty.ServerReference:
                            // UTF8 encoded
                            msg.ServerReference = EncodeDecodeHelper.GetUTF8FromBuffer(buffer, ref index);
                            break;
                        case MqttProperty.AuthenticationMethod:
                            // UTF8 encoded
                            msg.AuthenticationMethod = EncodeDecodeHelper.GetUTF8FromBuffer(buffer, ref index);
                            break;
                        case MqttProperty.AuthenticationData:
                            // byte[]
                            int length = ((buffer[index++] << 8) & 0xFF00);
                            length |= buffer[index++];
                            msg.AuthenticationData = new byte[length];
                            Array.Copy(buffer, index, msg.AuthenticationData, 0, length);
                            index += length;
                            break;
                        default:
                            // non supported property
                            index = propSize;
                            break;
                    }
                }
            }

            return msg;
        }

        /// <summary>
        /// Returns the bytes that represents the current object.
        /// </summary>
        /// <param name="protocolVersion">MQTT protocol version</param>
        /// <returns>An array of bytes that represents the current object.</returns>
        public override byte[] GetBytes(MqttProtocolVersion protocolVersion)
        {
            int fixedHeaderSize = 0;
            int varHeaderSize = 0;
            int varHeaderPropSize = 0;
            int remainingLength = 0;
            byte[] buffer;
            int index = 0;
            byte[] authenticationMethod = null;
            byte[] assignedClientIdentifier = null;
            byte[] responseInformation = null;
            byte[] serverReference = null;
            byte[] reason = null;
            byte[] authenticationData = null;
            byte[] userProperties = null;

            if (protocolVersion == MqttProtocolVersion.Version_5)
            {
                if (SessionExpiryInterval > 0)
                {
                    varHeaderPropSize += ENCODING_FOUR_BYTE_SIZE;
                }

                if (ReceiveMaximum != ushort.MaxValue)
                {
                    varHeaderPropSize += ENCODING_TWO_BYTE_SIZE;
                }

                if (MaximumQoS)
                {
                    varHeaderPropSize += ENCODING_BYTE_SIZE;
                }

                if (RetainAvailable)
                {
                    varHeaderPropSize += ENCODING_BYTE_SIZE;
                }

                if (MaximumPacketSize > 0)
                {
                    varHeaderPropSize += ENCODING_FOUR_BYTE_SIZE;
                }

                if (!string.IsNullOrEmpty(AssignedClientIdentifier))
                {
                    assignedClientIdentifier = Encoding.UTF8.GetBytes(AssignedClientIdentifier);
                    varHeaderPropSize += ENCODING_UTF8_SIZE + assignedClientIdentifier.Length;
                }

                if (TopicAliasMaximum > 0)
                {
                    varHeaderPropSize += ENCODING_TWO_BYTE_SIZE;
                }

                if (!string.IsNullOrEmpty(Reason))
                {
                    reason = Encoding.UTF8.GetBytes(Reason);
                    varHeaderPropSize += ENCODING_UTF8_SIZE + reason.Length;
                }

                if (UserProperties.Count > 0)
                {
                    userProperties = EncodeDecodeHelper.EncodeUserProperties(UserProperties);
                    varHeaderPropSize += userProperties.Length;
                }

                if (WildcardSubscriptionAvailable)
                {
                    varHeaderPropSize += ENCODING_BYTE_SIZE;
                }

                if (SubscriptionIdentifiersAvailable)
                {
                    varHeaderPropSize += ENCODING_BYTE_SIZE;
                }

                if (SharedSubscriptionAvailable)
                {
                    varHeaderPropSize += ENCODING_BYTE_SIZE;
                }

                if (ServerKeepAlive > 0)
                {
                    varHeaderPropSize += ENCODING_TWO_BYTE_SIZE;
                }

                if (!string.IsNullOrEmpty(ResponseInformation))
                {
                    responseInformation = Encoding.UTF8.GetBytes(ResponseInformation);
                    varHeaderPropSize += ENCODING_UTF8_SIZE + responseInformation.Length;
                }

                if (!string.IsNullOrEmpty(ServerReference))
                {
                    serverReference = Encoding.UTF8.GetBytes(ServerReference);
                    varHeaderPropSize += ENCODING_UTF8_SIZE + serverReference.Length;
                }

                if (!string.IsNullOrEmpty(AuthenticationMethod))
                {
                    authenticationMethod = Encoding.UTF8.GetBytes(AuthenticationMethod);
                    varHeaderPropSize += ENCODING_UTF8_SIZE + authenticationMethod.Length;
                }

                if ((AuthenticationData != null) && (AuthenticationData.Length > 0))
                {
                    authenticationData = EncodeDecodeHelper.EncodeArray(MqttProperty.AuthenticationData, AuthenticationData);
                    varHeaderPropSize += authenticationData.Length;
                }

                varHeaderSize += varHeaderPropSize + EncodeDecodeHelper.EncodeLength(varHeaderPropSize);
            }

            if ((protocolVersion == MqttProtocolVersion.Version_3_1_1) || (protocolVersion == MqttProtocolVersion.Version_5))
            {
                // flags byte and connect return code
                varHeaderSize += (CONN_ACK_FLAGS_BYTE_SIZE + CONN_RETURN_CODE_BYTE_SIZE);
            }
            else
            {
                // topic name compression response and connect return code
                varHeaderSize += (TOPIC_NAME_COMP_RESP_BYTE_SIZE + CONN_RETURN_CODE_BYTE_SIZE);
            }

            remainingLength += varHeaderSize;

            // first byte of fixed header
            fixedHeaderSize = 1;

            int temp = remainingLength;
            // increase fixed header size based on remaining length
            // (each remaining length byte can encode until 128)
            do
            {
                fixedHeaderSize++;
                temp /= 128;
            } while (temp > 0);

            // allocate buffer for message
            buffer = new byte[fixedHeaderSize + varHeaderSize];

            // first fixed header byte            
            buffer[index++] = (byte)MqttMessageType.ConnectAck << MSG_TYPE_OFFSET;

            // encode remaining length
            index = EncodeVariableByte(remainingLength, buffer, index);

            if ((protocolVersion == MqttProtocolVersion.Version_3_1_1) || (protocolVersion == MqttProtocolVersion.Version_5))
            {
                // [v3.1.1] session present flag
                buffer[index++] = SessionPresent ? (byte)(1 << SESSION_PRESENT_FLAG_OFFSET) : (byte)0x00;
            }
            else
            {    // topic name compression response (reserved values. not used);
                buffer[index++] = 0x00;
            }

            // connect return code
            buffer[index++] = (byte)ReturnCode;

            // If protocole is 5.0, we have the properties to add
            if (protocolVersion == MqttProtocolVersion.Version_5)
            {
                // The header size

                index = EncodeVariableByte(varHeaderPropSize, buffer, index);

                if (SessionExpiryInterval > 0)
                {
                    index = EncodeDecodeHelper.EncodeUint(MqttProperty.SessionExpiryInterval, SessionExpiryInterval, buffer, index);
                }

                if (ReceiveMaximum != ushort.MaxValue)
                {
                    index = EncodeDecodeHelper.EncodeUshort(MqttProperty.ReceiveMaximum, ReceiveMaximum, buffer, index);
                }

                if (MaximumQoS)
                {
                    buffer[index++] = (byte)MqttProperty.MaximumQoS;
                    buffer[index++] = (byte)(MaximumQoS ? 1 : 0);
                }

                if (RetainAvailable)
                {
                    buffer[index++] = (byte)MqttProperty.RetainAvailable;
                    buffer[index++] = (byte)(RetainAvailable ? 1 : 0);
                }

                if (MaximumPacketSize > 0)
                {
                    index = EncodeDecodeHelper.EncodeUint(MqttProperty.MaximumPacketSize, MaximumPacketSize, buffer, index);
                }

                if (assignedClientIdentifier != null)
                {
                    EncodeDecodeHelper.EncodeUTF8FromBuffer(MqttProperty.AssignedClientIdentifier, assignedClientIdentifier, buffer, ref index);
                }

                if (TopicAliasMaximum > 0)
                {
                    index = EncodeDecodeHelper.EncodeUshort(MqttProperty.TopicAliasMaximum, TopicAliasMaximum, buffer, index);
                }

                if (reason != null)
                {
                    EncodeDecodeHelper.EncodeUTF8FromBuffer(MqttProperty.ReasonString, reason, buffer, ref index);
                }

                if (userProperties != null)
                {
                    Array.Copy(userProperties, 0, buffer, index, userProperties.Length);
                    index += userProperties.Length;
                }

                if (WildcardSubscriptionAvailable)
                {
                    buffer[index++] = (byte)MqttProperty.WildcardSubscriptionAvailable;
                    buffer[index++] = (byte)(WildcardSubscriptionAvailable ? 1 : 0);
                }

                if (SubscriptionIdentifiersAvailable)
                {
                    buffer[index++] = (byte)MqttProperty.SubscriptionIdentifierAvailable;
                    buffer[index++] = (byte)(SubscriptionIdentifiersAvailable ? 1 : 0);
                }

                if (SharedSubscriptionAvailable)
                {
                    buffer[index++] = (byte)MqttProperty.SharedSubscriptionAvailable;
                    buffer[index++] = (byte)(SharedSubscriptionAvailable ? 1 : 0);
                }

                if (ServerKeepAlive > 0)
                {
                    index = EncodeDecodeHelper.EncodeUshort(MqttProperty.ServerKeepAlive, ServerKeepAlive, buffer, index);
                }

                if (responseInformation != null)
                {
                    EncodeDecodeHelper.EncodeUTF8FromBuffer(MqttProperty.ResponseInformation, responseInformation, buffer, ref index);
                }

                if (serverReference != null)
                {
                    EncodeDecodeHelper.EncodeUTF8FromBuffer(MqttProperty.ServerReference, serverReference, buffer, ref index);
                }

                if (authenticationMethod != null)
                {
                    EncodeDecodeHelper.EncodeUTF8FromBuffer(MqttProperty.AuthenticationMethod, authenticationMethod, buffer, ref index);
                }

                if (authenticationData != null)
                {
                    Array.Copy(authenticationData, 0, buffer, index, authenticationData.Length);
                }
            }

            return buffer;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
#if DEBUG
            return this.GetTraceString(
                "CONNACK",
                new object[] { "returnCode" },
                new object[] { this.ReturnCode });
#else
            return base.ToString();
#endif
        }
    }
}
