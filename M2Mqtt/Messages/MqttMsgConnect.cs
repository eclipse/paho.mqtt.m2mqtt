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
*/

using System;
using System.Text;
using uPLibrary.Networking.M2Mqtt.Exceptions;

namespace uPLibrary.Networking.M2Mqtt.Messages
{
    /// <summary>
    /// Class for CONNECT message from client to broker
    /// </summary>
    public class MqttMsgConnect : MqttMsgBase
    {
        #region Constants...

        // protocol name supported
        internal const string PROTOCOL_NAME = "MQIsdp";

        // max length for client id
        internal const int CLIENT_ID_MAX_LENGTH = 23;

        // variable header fields
        internal const byte PROTOCOL_NAME_LEN_SIZE = 2;
        internal const byte PROTOCOL_NAME_SIZE = 6;
        internal const byte PROTOCOL_VERSION_NUMBER_SIZE = 1;
        internal const byte CONNECT_FLAGS_SIZE = 1;
        internal const byte KEEP_ALIVE_TIME_SIZE = 2;

        internal const byte PROTOCOL_VERSION = 0x03;
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

        #endregion

        #region Properties...

        /// <summary>
        /// Protocol name
        /// </summary>
        public string ProtocolName
        {
            get { return this.protocolName; }
            set { this.protocolName = value; }
        }

        /// <summary>
        /// Protocol version
        /// </summary>
        public byte ProtocolVersion
        {
            get { return this.protocolVersion; }
            set { this.protocolVersion = value; }
        }

        /// <summary>
        /// Client identifier
        /// </summary>
        public string ClientId
        {
            get { return this.clientId; }
            set { this.clientId = value; }
        }

        /// <summary>
        /// Will retain flag
        /// </summary>
        public bool WillRetain
        {
            get { return this.willRetain; }
            set { this.willRetain = value; }
        }

        /// <summary>
        /// Will QOS level
        /// </summary>
        public byte WillQosLevel
        {
            get { return this.willQosLevel; }
            set { this.willQosLevel = value; }
        }

        /// <summary>
        /// Will flag
        /// </summary>
        public bool WillFlag
        {
            get { return this.willFlag; }
            set { this.willFlag = value; }
        }

        /// <summary>
        /// Will topic
        /// </summary>
        public string WillTopic
        {
            get { return this.willTopic; }
            set { this.willTopic = value; }
        }

        /// <summary>
        /// Will message
        /// </summary>
        public string WillMessage
        {
            get { return this.willMessage; }
            set { this.willMessage = value; }
        }

        /// <summary>
        /// Username
        /// </summary>
        public string Username
        {
            get { return this.username; }
            set { this.username = value; }
        }

        /// <summary>
        /// Password
        /// </summary>
        public string Password
        {
            get { return this.password; }
            set { this.password = value; }
        }

        /// <summary>
        /// Clean session flag
        /// </summary>
        public bool CleanSession
        {
            get { return this.cleanSession; }
            set { this.cleanSession = value; }
        }

        /// <summary>
        /// Keep alive period
        /// </summary>
        public ushort KeepAlivePeriod
        {
            get { return this.keepAlivePeriod; }
            set { this.keepAlivePeriod = value; }
        }

        #endregion

        // protocol name
        private string protocolName;
        // protocol version
        private byte protocolVersion;
        // client identifier
        private string clientId;
        // will retain flag
        protected bool willRetain;
        // will quality of service level
        protected byte willQosLevel;
        // will flag
        private bool willFlag;
        // will topic
        private string willTopic;
        // will message
        private string willMessage;
        // username
        private string username;
        // password
        private string password;
        // clean session flag
        private bool cleanSession;
        // keep alive period (in sec)
        private ushort keepAlivePeriod;
        
        /// <summary>
        /// Constructor
        /// </summary>
        public MqttMsgConnect()
        {
            this.type = MQTT_MSG_CONNECT_TYPE;

            this.protocolName = PROTOCOL_NAME;
            this.protocolVersion = PROTOCOL_VERSION;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="clientId">Client identifier</param>
        public MqttMsgConnect(string clientId) :
            this(clientId, null, null, false, QOS_LEVEL_AT_LEAST_ONCE, false, null, null, true, KEEP_ALIVE_PERIOD_DEFAULT)
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
        public MqttMsgConnect(string clientId, 
            string username, 
            string password,
            bool willRetain,
            byte willQosLevel,
            bool willFlag,
            string willTopic,
            string willMessage,
            bool cleanSession,
            ushort keepAlivePeriod
            )
        {
            this.type = MQTT_MSG_CONNECT_TYPE;

            this.protocolName = PROTOCOL_NAME;
            this.protocolVersion = PROTOCOL_VERSION;

            this.clientId = clientId;
            this.username = username;
            this.password = password;
            this.willRetain = willRetain;
            this.willQosLevel = willQosLevel;
            this.willFlag = willFlag;
            this.willTopic = willTopic;
            this.willMessage = willMessage;
            this.cleanSession = cleanSession;
            this.keepAlivePeriod = keepAlivePeriod;
        }

        /// <summary>
        /// Parse bytes for a CONNECT message
        /// </summary>
        /// <param name="fixedHeaderFirstByte">First fixed header byte</param>
        /// <param name="channel">Channel connected to the broker</param>
        /// <returns>CONNECT message instance</returns>
        public static MqttMsgConnect Parse(byte fixedHeaderFirstByte, IMqttNetworkChannel channel)
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
            int remainingLength = MqttMsgBase.decodeRemainingLength(channel);
            buffer = new byte[remainingLength];

            // read bytes from socket...
            channel.Receive(buffer);

            // protocol name
            protNameUtf8Length = ((buffer[index++] << 8) & 0xFF00);
            protNameUtf8Length |= buffer[index++];
            protNameUtf8 = new byte[protNameUtf8Length];
            Array.Copy(buffer, index, protNameUtf8, 0, protNameUtf8Length);
            index += protNameUtf8Length;
            msg.protocolName = new String(Encoding.UTF8.GetChars(protNameUtf8));

            // protocol version
            msg.protocolVersion = buffer[index];
            index += PROTOCOL_VERSION_NUMBER_SIZE;

            // connect flags
            isUsernameFlag = (buffer[index] & USERNAME_FLAG_MASK) != 0x00;
            isPasswordFlag = (buffer[index] & PASSWORD_FLAG_MASK) != 0x00;
            msg.willRetain = (buffer[index] & WILL_RETAIN_FLAG_MASK) != 0x00;
            msg.willQosLevel = (byte)((buffer[index] & WILL_QOS_FLAG_MASK) >> WILL_QOS_FLAG_OFFSET);
            msg.willFlag = (buffer[index] & WILL_FLAG_MASK) != 0x00;
            msg.cleanSession = (buffer[index] & CLEAN_SESSION_FLAG_MASK) != 0x00;
            index += CONNECT_FLAGS_SIZE;

            // keep alive timer
            msg.keepAlivePeriod = (ushort)((buffer[index++] << 8) & 0xFF00);
            msg.keepAlivePeriod |= buffer[index++];

            // client identifier
            clientIdUtf8Length = ((buffer[index++] << 8) & 0xFF00);
            clientIdUtf8Length |= buffer[index++];
            clientIdUtf8 = new byte[clientIdUtf8Length];
            Array.Copy(buffer, index, clientIdUtf8, 0, clientIdUtf8Length);
            index += clientIdUtf8Length;
            msg.clientId = new String(Encoding.UTF8.GetChars(clientIdUtf8));

            // will topic and will message
            if (msg.willFlag)
            {
                willTopicUtf8Length = ((buffer[index++] << 8) & 0xFF00);
                willTopicUtf8Length |= buffer[index++];
                willTopicUtf8 = new byte[willTopicUtf8Length];
                Array.Copy(buffer, index, willTopicUtf8, 0, willTopicUtf8Length);
                index += willTopicUtf8Length;
                msg.willTopic = new String(Encoding.UTF8.GetChars(willTopicUtf8));

                willMessageUtf8Length = ((buffer[index++] << 8) & 0xFF00);
                willMessageUtf8Length |= buffer[index++];
                willMessageUtf8 = new byte[willMessageUtf8Length];
                Array.Copy(buffer, index, willMessageUtf8, 0, willMessageUtf8Length);
                index += willMessageUtf8Length;
                msg.willMessage = new String(Encoding.UTF8.GetChars(willMessageUtf8));
            }

            // username
            if (isUsernameFlag)
            {
                usernameUtf8Length = ((buffer[index++] << 8) & 0xFF00);
                usernameUtf8Length |= buffer[index++];
                usernameUtf8 = new byte[usernameUtf8Length];
                Array.Copy(buffer, index, usernameUtf8, 0, usernameUtf8Length);
                index += usernameUtf8Length;
                msg.username = new String(Encoding.UTF8.GetChars(usernameUtf8));
            }

            // password
            if (isPasswordFlag)
            {
                passwordUtf8Length = ((buffer[index++] << 8) & 0xFF00);
                passwordUtf8Length |= buffer[index++];
                passwordUtf8 = new byte[passwordUtf8Length];
                Array.Copy(buffer, index, passwordUtf8, 0, passwordUtf8Length);
                index += passwordUtf8Length;
                msg.password = new String(Encoding.UTF8.GetChars(passwordUtf8));
            }

            return msg;
        }

        public override byte[] GetBytes()
        {
            int fixedHeaderSize = 0;
            int varHeaderSize = 0;
            int payloadSize = 0;
            int remainingLength = 0;
            byte[] buffer;
            int index = 0;

            byte[] clientIdUtf8 = Encoding.UTF8.GetBytes(this.clientId);
            byte[] willTopicUtf8 = (this.willTopic != null) ? Encoding.UTF8.GetBytes(this.willTopic) : null;
            byte[] willMessageUtf8 = (this.willMessage != null) ? Encoding.UTF8.GetBytes(this.willMessage) : null;
            byte[] usernameUtf8 = (this.username != null) ? Encoding.UTF8.GetBytes(this.username) : null;
            byte[] passwordUtf8 = (this.password != null) ? Encoding.UTF8.GetBytes(this.password) : null;

            // will flag set but will topic wrong
            if (this.willFlag && (willTopicUtf8.Length == 0))
                throw new MqttClientException(MqttClientErrorCode.WillTopicWrong);
            if (this.keepAlivePeriod > MAX_KEEP_ALIVE)
                throw new MqttClientException(MqttClientErrorCode.KeepAliveWrong);

            // protocol name field size
            varHeaderSize += (PROTOCOL_NAME_LEN_SIZE + PROTOCOL_NAME_SIZE);
            // protocol version number field size
            varHeaderSize += PROTOCOL_VERSION_NUMBER_SIZE;
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
                temp = temp / 128;
            } while (temp > 0);

            // allocate buffer for message
            buffer = new byte[fixedHeaderSize + varHeaderSize + payloadSize];

            // first fixed header byte
            buffer[index++] = (MQTT_MSG_CONNECT_TYPE << MSG_TYPE_OFFSET);

            // encode remaining length
            index = this.encodeRemainingLength(remainingLength, buffer, index);

            // protocol name
            buffer[index++] = 0; // MSB protocol name size
            buffer[index++] = PROTOCOL_NAME_SIZE; // LSB protocol name size
            buffer[index++] = (byte)'M';
            buffer[index++] = (byte)'Q';
            buffer[index++] = (byte)'I';
            buffer[index++] = (byte)'s';
            buffer[index++] = (byte)'d';
            buffer[index++] = (byte)'p';

            // protocol version
            buffer[index++] = PROTOCOL_VERSION;

            // connect flags
            byte connectFlags = 0x00;
            connectFlags |= (this.username != null) ? (byte)(1 << USERNAME_FLAG_OFFSET) : (byte)0x00;
            connectFlags |= (this.password != null) ? (byte)(1 << PASSWORD_FLAG_OFFSET) : (byte)0x00;
            connectFlags |= (this.willRetain) ? (byte)(1 << WILL_RETAIN_FLAG_OFFSET) : (byte)0x00;
            connectFlags |= (byte)(this.willQosLevel << WILL_QOS_FLAG_OFFSET);
            connectFlags |= (this.willFlag) ? (byte)(1 << WILL_FLAG_OFFSET) : (byte)0x00;
            connectFlags |= (this.cleanSession) ? (byte)(1 << CLEAN_SESSION_FLAG_OFFSET) : (byte)0x00;
            buffer[index++] = connectFlags;

            // keep alive period
            buffer[index++] = (byte)((this.keepAlivePeriod >> 8) & 0x00FF); // MSB
            buffer[index++] = (byte)(this.keepAlivePeriod & 0x00FF); // LSB

            // client identifier
            buffer[index++] = (byte)((clientIdUtf8.Length >> 8) & 0x00FF); // MSB
            buffer[index++] = (byte)(clientIdUtf8.Length & 0x00FF); // LSB
            Array.Copy(clientIdUtf8, 0, buffer, index, clientIdUtf8.Length);
            index += clientIdUtf8.Length;

            // will topic
            if (this.willFlag && (this.willTopic != null))
            {
                buffer[index++] = (byte)((willTopicUtf8.Length >> 8) & 0x00FF); // MSB
                buffer[index++] = (byte)(willTopicUtf8.Length & 0x00FF); // LSB
                Array.Copy(willTopicUtf8, 0, buffer, index, willTopicUtf8.Length);
                index += willTopicUtf8.Length;
            }

            // will message
            if (this.willFlag && (this.willMessage != null))
            {
                buffer[index++] = (byte)((willMessageUtf8.Length >> 8) & 0x00FF); // MSB
                buffer[index++] = (byte)(willMessageUtf8.Length & 0x00FF); // LSB
                Array.Copy(willMessageUtf8, 0, buffer, index, willMessageUtf8.Length);
                index += willMessageUtf8.Length;
            }

            // username
            if (this.username != null)
            {
                buffer[index++] = (byte)((usernameUtf8.Length >> 8) & 0x00FF); // MSB
                buffer[index++] = (byte)(usernameUtf8.Length & 0x00FF); // LSB
                Array.Copy(usernameUtf8, 0, buffer, index, usernameUtf8.Length);
                index += usernameUtf8.Length;
            }

            // password
            if (this.password != null)
            {
                buffer[index++] = (byte)((passwordUtf8.Length >> 8) & 0x00FF); // MSB
                buffer[index++] = (byte)(passwordUtf8.Length & 0x00FF); // LSB
                Array.Copy(passwordUtf8, 0, buffer, index, passwordUtf8.Length);
                index += passwordUtf8.Length;
            }

            return buffer;
        }

        public override string ToString()
        {
#if TRACE
            return this.GetTraceString(
                "CONNECT",
                new object[] { "protocolName", "protocolVersion", "clientId", "willFlag", "willRetain", "willQosLevel", "willTopic", "willMessage", "username", "password", "cleanSession", "keepAlivePeriod" },
                new object[] { this.protocolName, this.protocolVersion, this.clientId, this.willFlag, this.willRetain, this.willQosLevel, this.willTopic, this.willMessage, this.username, this.password, this.cleanSession, this.keepAlivePeriod });
#else
            return base.ToString();
#endif
        }
    }
}
