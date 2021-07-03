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
using System.Text;
using nanoFramework.M2Mqtt.Exceptions;

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
        internal const string PROTOCOL_NAME_V3_1_1 = "MQTT"; // [v.3.1.1]
        
        // max length for client id (removed in 3.1.1)
        internal const int CLIENT_ID_MAX_LENGTH = 23;

        // variable header fields
        internal const byte PROTOCOL_NAME_LEN_SIZE = 2;
        internal const byte PROTOCOL_NAME_V3_1_SIZE = 6;
        internal const byte PROTOCOL_NAME_V3_1_1_SIZE = 4; // [v.3.1.1]
        internal const byte PROTOCOL_VERSION_SIZE = 1;
        internal const byte CONNECT_FLAGS_SIZE = 1;
        internal const byte KEEP_ALIVE_TIME_SIZE = 2;

        internal const byte PROTOCOL_VERSION_V3_1 = 0x03;
        internal const byte PROTOCOL_VERSION_V3_1_1 = 0x04; // [v.3.1.1]
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
        public byte ProtocolVersion { get; set; }

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

        #endregion
        
        /// <summary>
        /// Constructor
        /// </summary>
        public MqttMsgConnect()
        {
            Type = MQTT_MSG_CONNECT_TYPE;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="clientId">Client identifier</param>
        public MqttMsgConnect(string clientId) :
            this(clientId, null, null, false, MqttQoSLevel.AtLeastOnce, false, null, null, true, KEEP_ALIVE_PERIOD_DEFAULT, PROTOCOL_VERSION_V3_1_1)
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
            byte protocolVersion
            )
        {
            Type = MQTT_MSG_CONNECT_TYPE;

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
            // [v.3.1.1] added new protocol name and version
            ProtocolVersion = protocolVersion;
            ProtocolName = (ProtocolVersion == PROTOCOL_VERSION_V3_1_1) ? PROTOCOL_NAME_V3_1_1 : PROTOCOL_NAME_V3_1;
        }

        /// <summary>
        /// Parse bytes for a CONNECT message
        /// </summary>
        /// <param name="fixedHeaderFirstByte">First fixed header byte</param>
        /// <param name="protocolVersion">MQTT Protocol Version</param>
        /// <param name="channel">Channel connected to the broker</param>
        /// <returns>CONNECT message instance</returns>
        public static MqttMsgConnect Parse(byte fixedHeaderFirstByte, byte protocolVersion, IMqttNetworkChannel channel)
        {
            byte[] buffer;
            int index = 0;
            int protNameUtf8Length;
            byte[] protNameUtf8;
            bool isUsernameFlag;
            bool isPasswordFlag;
            int clientIdUtf8Length;
            byte[] clientIdUtf8;
            int willTopicUtf8Length;
            byte[] willTopicUtf8;
            int willMessageUtf8Length;
            byte[] willMessageUtf8;
            int usernameUtf8Length;
            byte[] usernameUtf8;
            int passwordUtf8Length;
            byte[] passwordUtf8;
            MqttMsgConnect msg = new MqttMsgConnect();
            
            // get remaining length and allocate buffer
            int remainingLength = MqttMsgBase.DecodeRemainingLength(channel);
            buffer = new byte[remainingLength];

            // read bytes from socket...
            channel.Receive(buffer);

            // protocol name
            protNameUtf8Length = ((buffer[index++] << 8) & 0xFF00);
            protNameUtf8Length |= buffer[index++];
            protNameUtf8 = new byte[protNameUtf8Length];
            Array.Copy(buffer, index, protNameUtf8, 0, protNameUtf8Length);
            index += protNameUtf8Length;
            msg.ProtocolName = new String(Encoding.UTF8.GetChars(protNameUtf8));

            // [v3.1.1] wrong protocol name
            if (!msg.ProtocolName.Equals(PROTOCOL_NAME_V3_1) && !msg.ProtocolName.Equals(PROTOCOL_NAME_V3_1_1))
                throw new MqttClientException(MqttClientErrorCode.InvalidProtocolName);

            // protocol version
            msg.ProtocolVersion = buffer[index];
            index += PROTOCOL_VERSION_SIZE;

            // connect flags
            // [v3.1.1] check lsb (reserved) must be 0
            if ((msg.ProtocolVersion == PROTOCOL_VERSION_V3_1_1) &&
                ((buffer[index] & RESERVED_FLAG_MASK) != 0x00))
                throw new MqttClientException(MqttClientErrorCode.InvalidConnectFlags);

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

            // client identifier [v3.1.1] it may be zero bytes long (empty string)
            clientIdUtf8Length = ((buffer[index++] << 8) & 0xFF00);
            clientIdUtf8Length |= buffer[index++];
            clientIdUtf8 = new byte[clientIdUtf8Length];
            Array.Copy(buffer, index, clientIdUtf8, 0, clientIdUtf8Length);
            index += clientIdUtf8Length;
            msg.ClientId = new String(Encoding.UTF8.GetChars(clientIdUtf8));
            // [v3.1.1] if client identifier is zero bytes long, clean session must be true
            if ((msg.ProtocolVersion == PROTOCOL_VERSION_V3_1_1) && (clientIdUtf8Length == 0) && (!msg.CleanSession))
                throw new MqttClientException(MqttClientErrorCode.InvalidClientId);

            // will topic and will message
            if (msg.WillFlag)
            {
                willTopicUtf8Length = ((buffer[index++] << 8) & 0xFF00);
                willTopicUtf8Length |= buffer[index++];
                willTopicUtf8 = new byte[willTopicUtf8Length];
                Array.Copy(buffer, index, willTopicUtf8, 0, willTopicUtf8Length);
                index += willTopicUtf8Length;
                msg.WillTopic = new String(Encoding.UTF8.GetChars(willTopicUtf8));

                willMessageUtf8Length = ((buffer[index++] << 8) & 0xFF00);
                willMessageUtf8Length |= buffer[index++];
                willMessageUtf8 = new byte[willMessageUtf8Length];
                Array.Copy(buffer, index, willMessageUtf8, 0, willMessageUtf8Length);
                index += willMessageUtf8Length;
                msg.WillMessage = new String(Encoding.UTF8.GetChars(willMessageUtf8));
            }

            // username
            if (isUsernameFlag)
            {
                usernameUtf8Length = ((buffer[index++] << 8) & 0xFF00);
                usernameUtf8Length |= buffer[index++];
                usernameUtf8 = new byte[usernameUtf8Length];
                Array.Copy(buffer, index, usernameUtf8, 0, usernameUtf8Length);
                index += usernameUtf8Length;
                msg.Username = new String(Encoding.UTF8.GetChars(usernameUtf8));
            }

            // password
            if (isPasswordFlag)
            {
                passwordUtf8Length = ((buffer[index++] << 8) & 0xFF00);
                passwordUtf8Length |= buffer[index++];
                passwordUtf8 = new byte[passwordUtf8Length];
                Array.Copy(buffer, index, passwordUtf8, 0, passwordUtf8Length);
                index += passwordUtf8Length;
                msg.Password = new String(Encoding.UTF8.GetChars(passwordUtf8));
            }

            return msg;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="protocolVersion">MQTT protocol version</param>
        /// <returns></returns>
        public override byte[] GetBytes(byte protocolVersion)
        {
            int fixedHeaderSize = 0;
            int varHeaderSize = 0;
            int payloadSize = 0;
            int remainingLength = 0;
            byte[] buffer;
            int index = 0;

            byte[] clientIdUtf8 = Encoding.UTF8.GetBytes(ClientId);
            byte[] willTopicUtf8 = (WillFlag && (WillTopic != null)) ? Encoding.UTF8.GetBytes(WillTopic) : null;
            byte[] willMessageUtf8 = (WillFlag && (WillMessage != null)) ? Encoding.UTF8.GetBytes(WillMessage) : null;
            byte[] usernameUtf8 = ((Username != null) && (Username.Length > 0)) ? Encoding.UTF8.GetBytes(Username) : null;
            byte[] passwordUtf8 = ((Password != null) && (Password.Length > 0)) ? Encoding.UTF8.GetBytes(Password) : null;

            // [v3.1.1]
            if (ProtocolVersion == PROTOCOL_VERSION_V3_1_1)
            {
                // will flag set, will topic and will message MUST be present
                if (WillFlag &&  ((WillQosLevel >= 0x03) ||
                                       (willTopicUtf8 == null) || (willMessageUtf8 == null) ||
                                       ((willTopicUtf8 != null) && (willTopicUtf8.Length == 0)) || 
                                       ((willMessageUtf8 != null) && (willMessageUtf8.Length == 0))))
                    throw new MqttClientException(MqttClientErrorCode.WillWrong);
                // willflag not set, retain must be 0 and will topic and message MUST NOT be present
                else if (!WillFlag && ((WillRetain) ||
                                            (willTopicUtf8 != null) || (willMessageUtf8 != null) ||
                                            ((willTopicUtf8 != null) && (willTopicUtf8.Length != 0)) || 
                                            ((willMessageUtf8 != null) && (willMessageUtf8.Length != 0))))
                    throw new MqttClientException(MqttClientErrorCode.WillWrong);
            }

            if (KeepAlivePeriod > MAX_KEEP_ALIVE)
                throw new MqttClientException(MqttClientErrorCode.KeepAliveWrong);

            // check on will QoS Level
            if ((WillQosLevel < (byte)MqttQoSLevel.AtMostOnce) ||
                (WillQosLevel > (byte)MqttQoSLevel.ExactlyOnce))
                throw new MqttClientException(MqttClientErrorCode.WillWrong);

            // protocol name field size
            // MQTT version 3.1
            if (ProtocolVersion == PROTOCOL_VERSION_V3_1)
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
            buffer[index++] = (MQTT_MSG_CONNECT_TYPE << MSG_TYPE_OFFSET) | MQTT_MSG_CONNECT_FLAG_BITS; // [v.3.1.1]

            // encode remaining length
            index = this.EncodeRemainingLength(remainingLength, buffer, index);

            // protocol name
            buffer[index++] = 0; // MSB protocol name size
            // MQTT version 3.1
            if (ProtocolVersion == PROTOCOL_VERSION_V3_1)
            {
                buffer[index++] = PROTOCOL_NAME_V3_1_SIZE; // LSB protocol name size
                Array.Copy(Encoding.UTF8.GetBytes(PROTOCOL_NAME_V3_1), 0, buffer, index, PROTOCOL_NAME_V3_1_SIZE);
                index += PROTOCOL_NAME_V3_1_SIZE;
                // protocol version
                buffer[index++] = PROTOCOL_VERSION_V3_1;
            }
            // MQTT version 3.1.1
            else
            {
                buffer[index++] = PROTOCOL_NAME_V3_1_1_SIZE; // LSB protocol name size
                Array.Copy(Encoding.UTF8.GetBytes(PROTOCOL_NAME_V3_1_1), 0, buffer, index, PROTOCOL_NAME_V3_1_1_SIZE);
                index += PROTOCOL_NAME_V3_1_1_SIZE;
                // protocol version
                buffer[index++] = PROTOCOL_VERSION_V3_1_1;
            }
            
            // connect flags
            byte connectFlags = 0x00;
            connectFlags |= (usernameUtf8 != null) ? (byte)(1 << USERNAME_FLAG_OFFSET) : (byte)0x00;
            connectFlags |= (passwordUtf8 != null) ? (byte)(1 << PASSWORD_FLAG_OFFSET) : (byte)0x00;
            connectFlags |= (WillRetain) ? (byte)(1 << WILL_RETAIN_FLAG_OFFSET) : (byte)0x00;
            // only if will flag is set, we have to use will QoS level (otherwise is MUST be 0)
            if (WillFlag)
                connectFlags |= (byte)(WillQosLevel << WILL_QOS_FLAG_OFFSET);
            connectFlags |= (WillFlag) ? (byte)(1 << WILL_FLAG_OFFSET) : (byte)0x00;
            connectFlags |= (CleanSession) ? (byte)(1 << CLEAN_SESSION_FLAG_OFFSET) : (byte)0x00;
            buffer[index++] = connectFlags;

            // keep alive period
            buffer[index++] = (byte)((KeepAlivePeriod >> 8) & 0x00FF); // MSB
            buffer[index++] = (byte)(KeepAlivePeriod & 0x00FF); // LSB

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
#if TRACE
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
