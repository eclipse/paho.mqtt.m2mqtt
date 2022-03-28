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
    /// Class for PUBREC message from broker to client
    /// </summary>
    public class MqttMsgPubrec : MqttMsgBase
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
        /// 
        public MqttMsgPubrec()
        {
            Type = MqttMessageType.PublishReceived;
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
            int indexPubrec = 0;
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

            remainingLength += (varHeaderSize + payloadSize);

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
            buffer[indexPubrec++] = (byte)MqttMessageType.PublishReceived << MSG_TYPE_OFFSET;

            // encode remaining length
            indexPubrec = EncodeVariableByte(remainingLength, buffer, indexPubrec);

            // get message identifier
            buffer[indexPubrec++] = (byte)((MessageId >> 8) & 0x00FF); // MSB
            buffer[indexPubrec++] = (byte)(MessageId & 0x00FF); // LSB 

            // v5 specific
            if (protocolVersion == MqttProtocolVersion.Version_5)
            {
                // ReasonCode
                buffer[indexPubrec++] = (byte)ReasonCode;

                // Encode length and the properties
                indexPubrec = EncodeVariableByte(varHeaderPropSize, buffer, indexPubrec);

                if (reason != null)
                {
                    EncodeDecodeHelper.EncodeUTF8FromBuffer(MqttProperty.ReasonString, reason, buffer, ref indexPubrec);
                }

                if (userProperties != null)
                {
                    Array.Copy(userProperties, 0, buffer, indexPubrec, userProperties.Length);
                }
            }

            return buffer;
        }

        /// <summary>
        /// Parse bytes for a PUBREC message
        /// </summary>
        /// <param name="fixedHeaderFirstByte">First fixed header byte</param>
        /// <param name="protocolVersion">MQTT Protocol Version</param>
        /// <param name="channel">Channel connected to the broker</param>
        /// <returns>PUBREC message instance</returns>
        public static MqttMsgPubrec Parse(byte fixedHeaderFirstByte, MqttProtocolVersion protocolVersion, IMqttNetworkChannel channel)
        {
            byte[] buffer;
            int index = 0;
            MqttMsgPubrec msg = new MqttMsgPubrec();

            if (protocolVersion == MqttProtocolVersion.Version_3_1_1)
            {
                // [v3.1.1] check flag bits
                if ((fixedHeaderFirstByte & MSG_FLAG_BITS_MASK) != MQTT_MSG_PUBREC_FLAG_BITS)
                {
                    throw new MqttClientException(MqttClientErrorCode.InvalidFlagBits);
                }
            }

            // get remaining length and allocate buffer
            int remainingLength = DecodeVariableByte(channel);
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
                "PUBREC",
                new object[] { "messageId" },
                new object[] { MessageId });
#else
            return base.ToString();
#endif
        }
    }
}
