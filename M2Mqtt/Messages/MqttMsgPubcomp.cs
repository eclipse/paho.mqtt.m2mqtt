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
    /// Class for PUBCOMP message from broker to client
    /// </summary>
    public class MqttMsgPubcomp : MqttMsgBase
    {
        /// <summary>
        /// Return Code, v5.0 only
        /// </summary>
        public MqttReasonCode ReasonCode { get; set; }

        /// <summary>
        /// The Reason as a string, v5.0 only
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public MqttMsgPubcomp()
        {
            Type = MqttMessageType.PublishComplete;
        }

        /// <summary>
        /// Returns the bytes that represents the current object.
        /// </summary>
        /// <param name="protocolVersion">MQTT protocol version</param>
        /// <returns>An array of bytes that represents the current object.</returns>
        public override byte[] GetBytes(MqttProtocolVersion protocolVersion)
        {
            int fixedHeaderSize;
            int varHeaderSize = 0;
            int payloadSize = 0;
            int remainingLength = 0;
            byte[] buffer;
            int indexPubcomp = 0;

            int varHeaderPropSize = 0;
            byte[] reason = null;
            byte[] userProperties = null;

            // message identifier
            varHeaderSize += MESSAGE_ID_SIZE;

            if (protocolVersion == MqttProtocolVersion.Version_5)
            {
                // Puback code
                varHeaderSize += 1;

                if (!string.IsNullOrEmpty(Reason))
                {
                    reason = Encoding.UTF8.GetBytes(Reason);
                    // Check if we are over the Maximum size
                    if ((MaximumPacketSize > 0) && (reason.Length + varHeaderSize > MaximumPacketSize))
                    {
                        reason = null;
                    }
                    else
                    {
                        varHeaderPropSize += ENCODING_UTF8_SIZE + reason.Length;
                    }
                }

                if (UserProperties.Count > 0)
                {
                    userProperties = EncodeDecodeHelper.EncodeUserProperties(UserProperties);
                    // Check if we are over the Maximum size
                    if ((MaximumPacketSize > 0) && (userProperties.Length + varHeaderSize > MaximumPacketSize))
                    {
                        userProperties = null;
                    }
                    else
                    {
                        varHeaderPropSize += userProperties.Length;
                    }
                }

                varHeaderSize += varHeaderPropSize + EncodeDecodeHelper.EncodeLength(varHeaderPropSize);
            }

            remainingLength += varHeaderSize + payloadSize;

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
            buffer = new byte[fixedHeaderSize + varHeaderSize + payloadSize];

            // first fixed header byte
            buffer[indexPubcomp++] = (byte)MqttMessageType.PublishComplete << MSG_TYPE_OFFSET;

            // encode remaining length
            indexPubcomp = EncodeVariableByte(remainingLength, buffer, indexPubcomp);

            // get message identifier
            buffer[indexPubcomp++] = (byte)((MessageId >> 8) & 0x00FF); // MSB
            buffer[indexPubcomp++] = (byte)(MessageId & 0x00FF); // LSB 

            // v5 specific
            if (protocolVersion == MqttProtocolVersion.Version_5)
            {
                // ReasonCode
                buffer[indexPubcomp++] = (byte)ReasonCode;

                // Encode length and the properties
                indexPubcomp = EncodeVariableByte(varHeaderPropSize, buffer, indexPubcomp);

                if (reason != null)
                {
                    EncodeDecodeHelper.EncodeUTF8FromBuffer(MqttProperty.ReasonString, reason, buffer, ref indexPubcomp);
                }

                if (userProperties != null)
                {
                    Array.Copy(userProperties, 0, buffer, indexPubcomp, userProperties.Length);
                }
            }
            return buffer;
        }

        /// <summary>
        /// Parse bytes for a PUBCOMP message
        /// </summary>
        /// <param name="fixedHeaderFirstByte">First fixed header byte</param>
        /// <param name="protocolVersion">MQTT Protocol Version</param>
        /// <param name="channel">Channel connected to the broker</param>
        /// <returns>PUBCOMP message instance</returns>
        public static MqttMsgPubcomp Parse(byte fixedHeaderFirstByte, MqttProtocolVersion protocolVersion, IMqttNetworkChannel channel)
        {
            byte[] buffer;
            int index = 0;
            MqttMsgPubcomp msg = new MqttMsgPubcomp();

            if ((protocolVersion == MqttProtocolVersion.Version_3_1_1) || (protocolVersion == MqttProtocolVersion.Version_5))
            {
                // [v3.1.1] check flag bits
                if ((fixedHeaderFirstByte & MSG_FLAG_BITS_MASK) != MQTT_MSG_PUBCOMP_FLAG_BITS)
                {
                    throw new MqttClientException(MqttClientErrorCode.InvalidFlagBits);
                }
            }

            // get remaining length and allocate buffer
            int remainingLength = MqttMsgBase.DecodeVariableByte(channel);
            buffer = new byte[remainingLength];

            // read bytes from socket...
            channel.Receive(buffer);

            // message id
            msg.MessageId = (ushort)((buffer[index++] << 8) & 0xFF00);
            msg.MessageId |= buffer[index++];

            if ((protocolVersion == MqttProtocolVersion.Version_5) && (index < buffer.Length))
            {
                msg.ReasonCode = (MqttReasonCode)buffer[index++];
                // size of the properties
                int propSize = EncodeDecodeHelper.GetPropertySize(buffer, ref index);
                propSize += index;
                MqttProperty prop;

                while (propSize > index)
                {
                    prop = (MqttProperty)buffer[index++];
                    switch (prop)
                    {
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
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
#if DEBUG
            return this.GetTraceString(
                "PUBCOMP",
                new object[] { "messageId" },
                new object[] { MessageId });
#else
            return base.ToString();
#endif
        }
    }
}
