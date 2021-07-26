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

using nanoFramework.M2Mqtt.Exceptions;
using nanoFramework.M2Mqtt.Utility;
using System;
using System.Text;

namespace nanoFramework.M2Mqtt.Messages
{
    /// <summary>
    /// Class for DISCONNECT message from client to broker
    /// </summary>
    public class MqttMsgDisconnect : MqttMsgBase
    {
        /// <summary>
        /// Session Expiry Interval, v5.0 only
        /// </summary>
        public uint SessionExpiryInterval { get; set; }

        /// <summary>
        /// The Reason as a string, v5.0 only
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// Used by the Client to identify another Server to use, v5.0 only
        /// </summary>
        public string ServerReference { get; set; }

        /// <summary>
        /// Reason Code, v5.0 only
        /// </summary>
        public MqttReasonCode ResonCode { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public MqttMsgDisconnect()
        {
            Type = MqttMessageType.Disconnect;
        }

        /// <summary>
        /// Parse bytes for a DISCONNECT message
        /// </summary>
        /// <param name="fixedHeaderFirstByte">First fixed header byte</param>
        /// <param name="protocolVersion">MQTT Protocol Version</param>
        /// <param name="channel">Channel connected to the broker</param>
        /// <returns>DISCONNECT message instance</returns>
        public static MqttMsgDisconnect Parse(byte fixedHeaderFirstByte, MqttProtocolVersion protocolVersion, IMqttNetworkChannel channel)
        {
            byte[] buffer;
            int index = 0;
            MqttMsgDisconnect msg = new MqttMsgDisconnect();

            if (((protocolVersion == MqttProtocolVersion.Version_3_1_1) || (protocolVersion == MqttProtocolVersion.Version_5))
                && ((fixedHeaderFirstByte & MSG_FLAG_BITS_MASK) != MQTT_MSG_DISCONNECT_FLAG_BITS))
            {
                throw new MqttClientException(MqttClientErrorCode.InvalidFlagBits);
            }

            // get remaining length and allocate buffer
            int remainingLength = DecodeVariableByte(channel);
            // NOTE : remainingLength must be 0
            if ((remainingLength != 0) && (protocolVersion == MqttProtocolVersion.Version_5))
            {
                // V5.0 specific
                buffer = new byte[remainingLength];

                // read bytes from socket...
                channel.Receive(buffer);
                msg.ResonCode = (MqttReasonCode)buffer[index++];
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
                        case MqttProperty.ServerReference:
                            // UTF8 encoded
                            msg.ServerReference = EncodeDecodeHelper.GetUTF8FromBuffer(buffer, ref index);
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
            int remainingLength = 0;
            byte[] buffer;
            int index = 0;
            int varHeaderPropSize = 0;
            byte[] reason = null;
            byte[] userProperties = null;
            byte[] serverReference = null;

            // first fixed header byte
            if ((protocolVersion == MqttProtocolVersion.Version_3_1_1) || (protocolVersion == MqttProtocolVersion.Version_3_1) || (ResonCode == MqttReasonCode.Success))
            {
                buffer = new byte[2];
                buffer[index++] = ((byte)(MqttMessageType.Disconnect) << MSG_TYPE_OFFSET);
                // Success
                buffer[index] = 0x00;
            }
            else
            {
                // Disconnect Reason Code
                varHeaderSize += 1;

                if (SessionExpiryInterval > 0)
                {
                    varHeaderPropSize += ENCODING_FOUR_BYTE_SIZE;
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

                if (!string.IsNullOrEmpty(ServerReference))
                {
                    serverReference = Encoding.UTF8.GetBytes(ServerReference);
                    varHeaderPropSize += ENCODING_UTF8_SIZE + serverReference.Length;
                }

                varHeaderSize += varHeaderPropSize + EncodeDecodeHelper.EncodeLength(varHeaderPropSize);

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
                buffer[index++] = (byte)MqttMessageType.Disconnect << MSG_TYPE_OFFSET;
                index = EncodeVariableByte(remainingLength, buffer, index);
                buffer[index++] = (byte)ResonCode;
                index = EncodeVariableByte(varHeaderPropSize, buffer, index);
                if (SessionExpiryInterval > 0)
                {
                    index = EncodeDecodeHelper.EncodeUint(MqttProperty.SessionExpiryInterval, SessionExpiryInterval, buffer, index);
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

                if (serverReference != null)
                {
                    EncodeDecodeHelper.EncodeUTF8FromBuffer(MqttProperty.ServerReference, serverReference, buffer, ref index);
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
                "DISCONNECT",
                null,
                null);
#else
            return base.ToString();
#endif
        }
    }
}
