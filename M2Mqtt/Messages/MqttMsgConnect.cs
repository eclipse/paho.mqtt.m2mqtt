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
    /// Class for CONNECT message from client to broker
    /// </summary>
    public class MqttMsgConnect : MqttMsgBase
    {
        #region Constants...

        // protocol name supported
        internal const string PROTOCOL_NAME_V3_1 = "MQIsdp";
        internal const string PROTOCOL_NAME_V3_1_1 = "MQTT"; // [v.3.1.1] and v5.0

        // max length for client id (removed in 3.1.1)
        internal const int CLIENT_ID_MAX_LENGTH = 23;

        // variable header fields
        internal const byte PROTOCOL_NAME_LEN_SIZE = 2;
        internal const byte PROTOCOL_NAME_V3_1_SIZE = 6;
        internal const byte PROTOCOL_NAME_V3_1_1_SIZE = 4; // [v.3.1.1]
        internal const byte PROTOCOL_VERSION_SIZE = 1;
        internal const byte CONNECT_FLAGS_SIZE = 1;
        internal const byte KEEP_ALIVE_TIME_SIZE = 2;

        internal const ushort KEEP_ALIVE_PERIOD_DEFAULT = 60; // seconds
        internal const ushort MAX_KEEP_ALIVE = 65535; // 16 bit

        // connect flags
        internal const byte USERNAME_FLAG_MASK = 0x80;
        internal const byte USERNAME_FLAG_OFFSET = 0x07;
        internal const byte USERNAME_FLAG_SIZE = 0x01;
        internal const byte PASSWORD_FLAG_MASK = 0x40;
        internal const byte PASSWORD_FLAG_OFFSET = 0x06;
        internal const byte PASSWORD_FLAG_SIZE = 0x01;
        internal const byte WILL_RETAIN_FLAG_MASK = 0x20;
        internal const byte WILL_RETAIN_FLAG_OFFSET = 0x05;
        internal const byte WILL_RETAIN_FLAG_SIZE = 0x01;
        internal const byte WILL_QOS_FLAG_MASK = 0x18;
        internal const byte WILL_QOS_FLAG_OFFSET = 0x03;
        internal const byte WILL_QOS_FLAG_SIZE = 0x02;
        internal const byte WILL_FLAG_MASK = 0x04;
        internal const byte WILL_FLAG_OFFSET = 0x02;
        internal const byte WILL_FLAG_SIZE = 0x01;
        internal const byte CLEAN_SESSION_FLAG_MASK = 0x02;
        internal const byte CLEAN_SESSION_FLAG_OFFSET = 0x01;
        internal const byte CLEAN_SESSION_FLAG_SIZE = 0x01;
        // [v.3.1.1] lsb (reserved) must be now 0
        internal const byte RESERVED_FLAG_MASK = 0x01;
        internal const byte RESERVED_FLAG_OFFSET = 0x00;
        internal const byte RESERVED_FLAG_SIZE = 0x01;

        #endregion

        #region Properties...

        /// <summary>
        /// Protocol name
        /// </summary>
        public string ProtocolName { get; set; }

        /// <summary>
        /// Protocol version
        /// </summary>
        public MqttProtocolVersion ProtocolVersion { get; set; }

        /// <summary>
        /// Client identifier
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Will retain flag
        /// </summary>
        public bool WillRetain { get; set; }

        /// <summary>
        /// Will QOS level
        /// </summary>
        public byte WillQosLevel { get; set; }

        /// <summary>
        /// Will flag
        /// </summary>
        public bool WillFlag { get; set; }

        /// <summary>
        /// Will topic
        /// </summary>
        public string WillTopic { get; set; }

        /// <summary>
        /// Will message
        /// </summary>
        public string WillMessage { get; set; }

        /// <summary>
        /// Username
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Clean session flag
        /// </summary>
        public bool CleanSession { get; set; }

        /// <summary>
        /// Keep alive period
        /// </summary>
        public ushort KeepAlivePeriod { get; set; }

        /// <summary>
        /// Authentication Method, v5.0 only
        /// </summary>
        public string AuthenticationMethod { get; set; }

        /// <summary>
        /// Authentication Data, v5.0 only
        /// </summary>
        public byte[] AuthenticationData { get; set; }

        /// <summary>
        /// Session Expiry Interval, v5.0 only
        /// </summary>
        public uint SessionExpiryInterval { get; set; }

        /// <summary>
        /// Request Problem Information, v5.0 only
        /// </summary>
        public bool RequestProblemInformation { get; set; }

        /// <summary>
        /// Request Response Information, v5.0 only
        /// </summary>
        public bool RequestResponseInformation { get; set; }

        /// <summary>
        /// ReceiveMaximum, v5.0 only
        /// </summary>
        public ushort ReceiveMaximum { get; set; } = ushort.MaxValue;

        /// <summary>
        /// TopicAliasMaximum, v5.0 only
        /// </summary>
        public ushort TopicAliasMaximum { get; set; }

        /// <summary>
        /// Will Delay Interval, v5.0 only
        /// </summary>
        public uint WillDelayInterval { get; set; }
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public MqttMsgConnect()
        {
            Type = MqttMessageType.Connect;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="clientId">Client identifier</param>
        public MqttMsgConnect(string clientId) :
            this(clientId, null, null, false, MqttQoSLevel.AtLeastOnce, false, null, null, true, KEEP_ALIVE_PERIOD_DEFAULT, MqttProtocolVersion.Version_3_1_1)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="clientId">Client identifier</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="willRetain">Will retain flag</param>
        /// <param name="willQosLevel">Will QOS level</param>
        /// <param name="willFlag">Will flag</param>
        /// <param name="willTopic">Will topic</param>
        /// <param name="willMessage">Will message</param>
        /// <param name="cleanSession">Clean sessione flag</param>
        /// <param name="keepAlivePeriod">Keep alive period</param>
        /// <param name="protocolVersion">MQTT Protocol version</param>
        public MqttMsgConnect(string clientId,
            string username,
            string password,
            bool willRetain,
            MqttQoSLevel willQosLevel,
            bool willFlag,
            string willTopic,
            string willMessage,
            bool cleanSession,
            ushort keepAlivePeriod,
            MqttProtocolVersion protocolVersion
            )
        {
            Type = MqttMessageType.Connect;

            ClientId = clientId;
            Username = username;
            Password = password;
            WillRetain = willRetain;
            WillQosLevel = (byte)willQosLevel;
            WillFlag = willFlag;
            WillTopic = willTopic;
            WillMessage = willMessage;
            CleanSession = cleanSession;
            KeepAlivePeriod = keepAlivePeriod;
            // [v.3.1.1] added new protocol name and version valid for 5.0
            ProtocolVersion = protocolVersion;
            ProtocolName = (ProtocolVersion == MqttProtocolVersion.Version_3_1_1 || ProtocolVersion == MqttProtocolVersion.Version_5) ? PROTOCOL_NAME_V3_1_1 : PROTOCOL_NAME_V3_1;
        }

        /// <summary>
        /// Parse bytes for a CONNECT message
        /// </summary>
        /// <param name="fixedHeaderFirstByte">First fixed header byte</param>
        /// <param name="protocolVersion">MQTT Protocol Version</param>
        /// <param name="channel">Channel connected to the broker</param>
        /// <returns>CONNECT message instance</returns>
        public static MqttMsgConnect Parse(byte fixedHeaderFirstByte, MqttProtocolVersion protocolVersion, IMqttNetworkChannel channel)
        {
            byte[] buffer;
            int index = 0;
            bool isUsernameFlag;
            bool isPasswordFlag;
            int length;
            MqttMsgConnect msg = new MqttMsgConnect();

            // get remaining length and allocate buffer
            int remainingLength = DecodeVariableByte(channel);
            buffer = new byte[remainingLength];

            // read bytes from socket...
            channel.Receive(buffer);

            // protocol name
            msg.ProtocolName = EncodeDecodeHelper.GetUTF8FromBuffer(buffer, ref index);

            // [v3.1.1] wrong protocol name valid for 5.0 as well
            if (!msg.ProtocolName.Equals(PROTOCOL_NAME_V3_1) && !msg.ProtocolName.Equals(PROTOCOL_NAME_V3_1_1))
            {
                throw new MqttClientException(MqttClientErrorCode.InvalidProtocolName);
            }

            // protocol version
            msg.ProtocolVersion = (MqttProtocolVersion)buffer[index];
            index += PROTOCOL_VERSION_SIZE;

            // connect flags
            // [v3.1.1] check lsb (reserved) must be 0
            if (((msg.ProtocolVersion == MqttProtocolVersion.Version_3_1_1) || (msg.ProtocolVersion == MqttProtocolVersion.Version_5)) &&
                ((buffer[index] & RESERVED_FLAG_MASK) != 0x00))
            {
                throw new MqttClientException(MqttClientErrorCode.InvalidConnectFlags);
            }

            isUsernameFlag = (buffer[index] & USERNAME_FLAG_MASK) != 0x00;
            isPasswordFlag = (buffer[index] & PASSWORD_FLAG_MASK) != 0x00;
            msg.WillRetain = (buffer[index] & WILL_RETAIN_FLAG_MASK) != 0x00;
            msg.WillQosLevel = (byte)((buffer[index] & WILL_QOS_FLAG_MASK) >> WILL_QOS_FLAG_OFFSET);
            msg.WillFlag = (buffer[index] & WILL_FLAG_MASK) != 0x00;
            msg.CleanSession = (buffer[index] & CLEAN_SESSION_FLAG_MASK) != 0x00;
            index += CONNECT_FLAGS_SIZE;

            // keep alive timer
            msg.KeepAlivePeriod = (ushort)((buffer[index++] << 8) & 0xFF00);
            msg.KeepAlivePeriod |= buffer[index++];

            // In case of v5, we have properties encoded with length and type.
            // This is different than in the v3.x protocols
            if (msg.ProtocolVersion == MqttProtocolVersion.Version_5)
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
                        case MqttProperty.RequestProblemInformation:
                            // 1 byte
                            msg.RequestProblemInformation = buffer[index++] != 0;
                            break;
                        case MqttProperty.RequestResponseInformation:
                            // 1 byte
                            msg.RequestResponseInformation = buffer[index++] != 0;
                            break;
                        case MqttProperty.ReceiveMaximum:
                            // 2 bytes
                            msg.ReceiveMaximum = (ushort)(buffer[index++] << 8);
                            msg.ReceiveMaximum |= buffer[index++];
                            break;
                        case MqttProperty.TopicAliasMaximum:
                            // 2 bytes
                            msg.TopicAliasMaximum = (ushort)(buffer[index++] << 8);
                            msg.TopicAliasMaximum |= buffer[index++];
                            break;
                        case MqttProperty.UserProperty:
                            // UTF8 key value encoding, so 2 strings in a raw
                            string key = EncodeDecodeHelper.GetUTF8FromBuffer(buffer, ref index);
                            string value = EncodeDecodeHelper.GetUTF8FromBuffer(buffer, ref index);
                            msg.UserProperties.Add(new UserProperty(key, value));
                            break;
                        case MqttProperty.MaximumPacketSize:
                            // 4 bytes
                            msg.MaximumPacketSize = EncodeDecodeHelper.DecodeUint(buffer, ref index);
                            break;
                        case MqttProperty.SessionExpiryInterval:
                            // 4 bytes
                            msg.SessionExpiryInterval = EncodeDecodeHelper.DecodeUint(buffer, ref index);
                            break;
                        case MqttProperty.WillDelayInterval:
                            // 4 bytes                            
                            msg.WillDelayInterval = EncodeDecodeHelper.DecodeUint(buffer, ref index);
                            break;
                        default:
                            // non supported property
                            index = propSize;
                            break;
                    }
                }
            }

            // client identifier [v3.1.1] it may be zero bytes long (empty string)
            msg.ClientId = EncodeDecodeHelper.GetUTF8FromBuffer(buffer, ref index);
            // [v3.1.1] if client identifier is zero bytes long, clean session must be true
            if ((msg.ProtocolVersion == MqttProtocolVersion.Version_3_1_1) && (msg.ClientId.Length == 0) && (!msg.CleanSession))
            {
                throw new MqttClientException(MqttClientErrorCode.InvalidClientId);
            }

            // will topic and will message
            if (msg.WillFlag)
            {
                msg.WillTopic = EncodeDecodeHelper.GetUTF8FromBuffer(buffer, ref index);
                msg.WillMessage = EncodeDecodeHelper.GetUTF8FromBuffer(buffer, ref index);
            }

            // username
            if (isUsernameFlag)
            {
                msg.Username = EncodeDecodeHelper.GetUTF8FromBuffer(buffer, ref index);
            }

            // password
            if (isPasswordFlag)
            {
                msg.Password = EncodeDecodeHelper.GetUTF8FromBuffer(buffer, ref index);
            }

            return msg;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="protocolVersion">MQTT protocol version</param>
        /// <returns></returns>
        public override byte[] GetBytes(MqttProtocolVersion protocolVersion)
        {
            int fixedHeaderSize = 0;
            int varHeaderSize = 0;
            int payloadSize = 0;
            int remainingLength = 0;
            byte[] buffer;
            int index = 0;
            int varHeaderPropSize = 0;

            byte[] clientIdUtf8 = Encoding.UTF8.GetBytes(ClientId);
            byte[] willTopicUtf8 = (WillFlag && (WillTopic != null)) ? Encoding.UTF8.GetBytes(WillTopic) : null;
            byte[] willMessageUtf8 = (WillFlag && (WillMessage != null)) ? Encoding.UTF8.GetBytes(WillMessage) : null;
            byte[] usernameUtf8 = ((Username != null) && (Username.Length > 0)) ? Encoding.UTF8.GetBytes(Username) : null;
            byte[] passwordUtf8 = ((Password != null) && (Password.Length > 0)) ? Encoding.UTF8.GetBytes(Password) : null;
            byte[] authenticationMethod = null;
            byte[] authenticationData = null;
            byte[] userProperties = null;

            // [v3.1.1]
            if ((ProtocolVersion == MqttProtocolVersion.Version_3_1_1) || (ProtocolVersion == MqttProtocolVersion.Version_5))
            {
                // will flag set, will topic and will message MUST be present
                if (WillFlag && ((WillQosLevel >= 0x03) ||
                                       (willTopicUtf8 == null) || (willMessageUtf8 == null) ||
                                       ((willTopicUtf8 != null) && (willTopicUtf8.Length == 0)) ||
                                       ((willMessageUtf8 != null) && (willMessageUtf8.Length == 0))))
                {
                    throw new MqttClientException(MqttClientErrorCode.WillWrong);
                }
                // willflag not set, retain must be 0 and will topic and message MUST NOT be present
                else if (!WillFlag && ((WillRetain) ||
                                            (willTopicUtf8 != null) || (willMessageUtf8 != null) ||
                                            ((willTopicUtf8 != null) && (willTopicUtf8.Length != 0)) ||
                                            ((willMessageUtf8 != null) && (willMessageUtf8.Length != 0))))
                {
                    throw new MqttClientException(MqttClientErrorCode.WillWrong);
                }
            }

            if (KeepAlivePeriod > MAX_KEEP_ALIVE)
            {
                throw new MqttClientException(MqttClientErrorCode.KeepAliveWrong);
            }

            // check on will QoS Level
            if ((WillQosLevel < (byte)MqttQoSLevel.AtMostOnce) ||
                (WillQosLevel > (byte)MqttQoSLevel.ExactlyOnce))
            {
                throw new MqttClientException(MqttClientErrorCode.WillWrong);
            }

            // protocol name field size
            // MQTT version 3.1
            if (ProtocolVersion == MqttProtocolVersion.Version_3_1)
            {
                varHeaderSize += (PROTOCOL_NAME_LEN_SIZE + PROTOCOL_NAME_V3_1_SIZE);
            }
            // MQTT version 3.1.1
            else
            {
                varHeaderSize += (PROTOCOL_NAME_LEN_SIZE + PROTOCOL_NAME_V3_1_1_SIZE);
            }
            // protocol level field size
            varHeaderSize += PROTOCOL_VERSION_SIZE;
            // connect flags field size
            varHeaderSize += CONNECT_FLAGS_SIZE;
            // keep alive timer field size
            varHeaderSize += KEEP_ALIVE_TIME_SIZE;

            if (ProtocolVersion == MqttProtocolVersion.Version_5)
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

                if (SessionExpiryInterval > 0)
                {
                    varHeaderPropSize += ENCODING_FOUR_BYTE_SIZE;
                }

                if (ReceiveMaximum != ushort.MaxValue)
                {
                    varHeaderPropSize += ENCODING_TWO_BYTE_SIZE;
                }

                if (MaximumPacketSize > 0)
                {
                    varHeaderPropSize += ENCODING_FOUR_BYTE_SIZE;
                }

                if (TopicAliasMaximum > 0)
                {
                    varHeaderPropSize += ENCODING_TWO_BYTE_SIZE;
                }

                if (UserProperties.Count > 0)
                {
                    userProperties = EncodeDecodeHelper.EncodeUserProperties(UserProperties);
                    varHeaderPropSize += userProperties.Length;
                }

                if (RequestProblemInformation)
                {
                    varHeaderPropSize += ENCODING_BYTE_SIZE;
                }

                if (RequestResponseInformation)
                {
                    varHeaderPropSize += ENCODING_BYTE_SIZE;
                }

                if (WillDelayInterval > 0)
                {
                    varHeaderPropSize += ENCODING_FOUR_BYTE_SIZE;
                }

                varHeaderSize += varHeaderPropSize + EncodeDecodeHelper.EncodeLength(varHeaderPropSize);
            }

            // client identifier field size
            payloadSize += clientIdUtf8.Length + 2;
            // will topic field size
            payloadSize += (willTopicUtf8 != null) ? (willTopicUtf8.Length + 2) : 0;
            // will message field size
            payloadSize += (willMessageUtf8 != null) ? (willMessageUtf8.Length + 2) : 0;
            // username field size
            payloadSize += (usernameUtf8 != null) ? (usernameUtf8.Length + 2) : 0;
            // password field size
            payloadSize += (passwordUtf8 != null) ? (passwordUtf8.Length + 2) : 0;

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
            buffer[index++] = (byte)MqttMessageType.Connect << MSG_TYPE_OFFSET;

            // encode remaining length
            index = EncodeVariableByte(remainingLength, buffer, index);

            // protocol name
            buffer[index++] = 0; // MSB protocol name size
            // MQTT version 3.1
            if (ProtocolVersion == MqttProtocolVersion.Version_3_1)
            {
                buffer[index++] = PROTOCOL_NAME_V3_1_SIZE; // LSB protocol name size
                Array.Copy(Encoding.UTF8.GetBytes(PROTOCOL_NAME_V3_1), 0, buffer, index, PROTOCOL_NAME_V3_1_SIZE);
                index += PROTOCOL_NAME_V3_1_SIZE;
            }
            // MQTT version 3.1.1 and v5
            else
            {
                buffer[index++] = PROTOCOL_NAME_V3_1_1_SIZE; // LSB protocol name size
                Array.Copy(Encoding.UTF8.GetBytes(PROTOCOL_NAME_V3_1_1), 0, buffer, index, PROTOCOL_NAME_V3_1_1_SIZE);
                index += PROTOCOL_NAME_V3_1_1_SIZE;
            }

            // protocol version
            buffer[index++] = (byte)ProtocolVersion;

            // connect flags
            byte connectFlags = 0x00;
            connectFlags |= (usernameUtf8 != null) ? (byte)(1 << USERNAME_FLAG_OFFSET) : (byte)0x00;
            connectFlags |= (passwordUtf8 != null) ? (byte)(1 << PASSWORD_FLAG_OFFSET) : (byte)0x00;
            connectFlags |= (WillRetain) ? (byte)(1 << WILL_RETAIN_FLAG_OFFSET) : (byte)0x00;
            // only if will flag is set, we have to use will QoS level (otherwise is MUST be 0)
            if (WillFlag)
            {
                connectFlags |= (byte)(WillQosLevel << WILL_QOS_FLAG_OFFSET);
            }

            connectFlags |= (WillFlag) ? (byte)(1 << WILL_FLAG_OFFSET) : (byte)0x00;
            connectFlags |= (CleanSession) ? (byte)(1 << CLEAN_SESSION_FLAG_OFFSET) : (byte)0x00;
            buffer[index++] = connectFlags;

            // keep alive period
            buffer[index++] = (byte)((KeepAlivePeriod >> 8) & 0x00FF); // MSB
            buffer[index++] = (byte)(KeepAlivePeriod & 0x00FF); // LSB

            // If protocole is 5.0, we have the properties to add
            if (protocolVersion == MqttProtocolVersion.Version_5)
            {
                // The header size

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

                if (SessionExpiryInterval > 0)
                {
                    index = EncodeDecodeHelper.EncodeUint(MqttProperty.SessionExpiryInterval, SessionExpiryInterval, buffer, index);
                }

                if (ReceiveMaximum != ushort.MaxValue)
                {
                    index = EncodeDecodeHelper.EncodeUshort(MqttProperty.ReceiveMaximum, ReceiveMaximum, buffer, index);
                }

                if (MaximumPacketSize > 0)
                {
                    index = EncodeDecodeHelper.EncodeUint(MqttProperty.MaximumPacketSize, MaximumPacketSize, buffer, index);
                }

                if (TopicAliasMaximum > 0)
                {
                    index = EncodeDecodeHelper.EncodeUshort(MqttProperty.TopicAliasMaximum, TopicAliasMaximum, buffer, index);
                }

                if (WillDelayInterval > 0)
                {
                    index = EncodeDecodeHelper.EncodeUint(MqttProperty.WillDelayInterval, WillDelayInterval, buffer, index);
                }

                if (UserProperties.Count > 0)
                {
                    Array.Copy(userProperties, 0, buffer, index, userProperties.Length);
                    index += userProperties.Length;
                }

                if (RequestProblemInformation)
                {
                    buffer[index++] = (byte)MqttProperty.RequestProblemInformation;
                    buffer[index++] = (byte)(RequestResponseInformation ? 1 : 0);
                }

                if (RequestResponseInformation)
                {
                    buffer[index++] = (byte)MqttProperty.RequestResponseInformation;
                    buffer[index++] = (byte)(RequestResponseInformation ? 1 : 0);
                }
            }

            // client identifier
            buffer[index++] = (byte)((clientIdUtf8.Length >> 8) & 0x00FF); // MSB
            buffer[index++] = (byte)(clientIdUtf8.Length & 0x00FF); // LSB
            Array.Copy(clientIdUtf8, 0, buffer, index, clientIdUtf8.Length);
            index += clientIdUtf8.Length;

            // will topic
            if (WillFlag && (willTopicUtf8 != null))
            {
                buffer[index++] = (byte)((willTopicUtf8.Length >> 8) & 0x00FF); // MSB
                buffer[index++] = (byte)(willTopicUtf8.Length & 0x00FF); // LSB
                Array.Copy(willTopicUtf8, 0, buffer, index, willTopicUtf8.Length);
                index += willTopicUtf8.Length;
            }

            // will message
            if (WillFlag && (willMessageUtf8 != null))
            {
                buffer[index++] = (byte)((willMessageUtf8.Length >> 8) & 0x00FF); // MSB
                buffer[index++] = (byte)(willMessageUtf8.Length & 0x00FF); // LSB
                Array.Copy(willMessageUtf8, 0, buffer, index, willMessageUtf8.Length);
                index += willMessageUtf8.Length;
            }

            // username
            if (usernameUtf8 != null)
            {
                buffer[index++] = (byte)((usernameUtf8.Length >> 8) & 0x00FF); // MSB
                buffer[index++] = (byte)(usernameUtf8.Length & 0x00FF); // LSB
                Array.Copy(usernameUtf8, 0, buffer, index, usernameUtf8.Length);
                index += usernameUtf8.Length;
            }

            // password
            if (passwordUtf8 != null)
            {
                buffer[index++] = (byte)((passwordUtf8.Length >> 8) & 0x00FF); // MSB
                buffer[index++] = (byte)(passwordUtf8.Length & 0x00FF); // LSB
                Array.Copy(passwordUtf8, 0, buffer, index, passwordUtf8.Length);
                index += passwordUtf8.Length;
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
                "CONNECT",
                new object[] { "protocolName", "protocolVersion", "clientId", "willFlag", "willRetain", "willQosLevel", "willTopic", "willMessage", "username", "password", "cleanSession", "keepAlivePeriod" },
                new object[] { ProtocolName, ProtocolVersion, ClientId, WillFlag, WillRetain, WillQosLevel, WillTopic, WillMessage, Username, Password, CleanSession, KeepAlivePeriod });
#else
            return base.ToString();
#endif
        }
    }
}
