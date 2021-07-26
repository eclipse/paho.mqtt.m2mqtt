// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using nanoFramework.M2Mqtt.Exceptions;
using nanoFramework.M2Mqtt.Utility;
using System;
using System.Text;

namespace nanoFramework.M2Mqtt.Messages
{
    /// <summary>
    /// Class for AUTH message from broker to client or client to broker
    /// as part of an extended authentication exchange, such as challenge / response authentication. 
    /// </summary>
    public class MqttMsgAuthentication : MqttMsgBase
    {
        /// <summary>
        /// Authentication Method, v5.0 only
        /// </summary>
        public string AuthenticationMethod { get; set; }

        /// <summary>
        /// Authentication Data, v5.0 only
        /// </summary>
        public byte[] AuthenticationData { get; set; }

        /// <summary>
        /// The Reason as a string, v5.0 only
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// Return Code, v5.0 only
        /// </summary>
        public MqttReasonCode ReasonCode { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public MqttMsgAuthentication()
        {
            Type = MqttMessageType.Authentication;
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
            int remainingLength = 0;
            byte[] buffer;
            int index = 0;
            int varHeaderPropSize = 0;
            byte[] reason = null;
            byte[] userProperties = null;
            byte[] authenticationMethod = null;
            byte[] authenticationData = null;

            if (protocolVersion != MqttProtocolVersion.Version_5)
            {
                throw new NotSupportedException();
            }

            // ResonCode only
            varHeaderSize += 1;

            if (!string.IsNullOrEmpty(Reason))
            {
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

            buffer[index++] = (byte)MqttMessageType.Authentication << MSG_TYPE_OFFSET;
            // encode remaining length
            index = EncodeVariableByte(remainingLength, buffer, index);
            buffer[index++] = (byte)ReasonCode;
            index = EncodeVariableByte(varHeaderPropSize, buffer, index);

            if (authenticationMethod != null)
            {
                EncodeDecodeHelper.EncodeUTF8FromBuffer(MqttProperty.AuthenticationMethod, authenticationMethod, buffer, ref index);
            }

            if (authenticationData != null)
            {
                Array.Copy(authenticationData, 0, buffer, index, authenticationData.Length);
                index += authenticationData.Length;
            }

            if (reason != null)
            {
                EncodeDecodeHelper.EncodeUTF8FromBuffer(MqttProperty.ReasonString, reason, buffer, ref index);
            }

            if (userProperties != null)
            {
                Array.Copy(userProperties, 0, buffer, index, userProperties.Length);
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
        public static MqttMsgAuthentication Parse(byte fixedHeaderFirstByte, MqttProtocolVersion protocolVersion, IMqttNetworkChannel channel)
        {
            if (protocolVersion != MqttProtocolVersion.Version_5)
            {
                throw new NotSupportedException();
            }

            if ((fixedHeaderFirstByte & MSG_FLAG_BITS_MASK) != MQTT_MSG_AUTH_FLAG_BITS)
            {
                throw new MqttClientException(MqttClientErrorCode.InvalidFlagBits);
            }

            MqttMsgAuthentication msg = new();
            byte[] buffer;
            int index = 0;
            int length = 0;

            // get remaining length and allocate buffer
            int remainingLength = DecodeVariableByte(channel);
            buffer = new byte[remainingLength];

            // read bytes from socket...
            channel.Receive(buffer);

            msg.ReasonCode = (MqttReasonCode)buffer[index++];

            int propSize = EncodeDecodeHelper.GetPropertySize(buffer, ref index);
            propSize += index;
            MqttProperty prop;

            while (propSize > index)
            {
                prop = (MqttProperty)buffer[index++];
                switch (prop)
                {
                    case MqttProperty.AuthenticationMethod:
                        // UTF8 encoded
                        msg.AuthenticationMethod = EncodeDecodeHelper.GetUTF8FromBuffer(buffer, ref index);
                        break;
                    case MqttProperty.AuthenticationData:
                        // byte[]
                        length = ((buffer[index++] << 8) & 0xFF00);
                        length |= buffer[index++];
                        msg.AuthenticationData = new byte[length];
                        Array.Copy(buffer, index, msg.AuthenticationData, 0, length);
                        index += length;
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
                    default:
                        // non supported property
                        index = propSize;
                        break;
                }
            }

            return msg;
        }
    }
}
