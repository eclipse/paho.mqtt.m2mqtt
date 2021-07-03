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
using System.Text;
using nanoFramework.M2Mqtt.Exceptions;

namespace nanoFramework.M2Mqtt.Messages
{
    /// <summary>
    /// Class for SUBSCRIBE message from client to broker
    /// </summary>
    public class MqttMsgSubscribe : MqttMsgBase
    {
        /// <summary>
        /// List of topics to subscribe
        /// </summary>
        public string[] Topics { get; set; }

        /// <summary>
        /// List of QOS Levels related to topics
        /// </summary>
        public MqttQoSLevel[] QoSLevels { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public MqttMsgSubscribe()
        {
            Type = MQTT_MSG_SUBSCRIBE_TYPE;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="topics">List of topics to subscribe</param>
        /// <param name="qosLevels">List of QOS Levels related to topics</param>
        public MqttMsgSubscribe(string[] topics, MqttQoSLevel[] qosLevels)
        {
            Type = MQTT_MSG_SUBSCRIBE_TYPE;

            Topics = topics;
            QoSLevels = qosLevels;

            // SUBSCRIBE message uses QoS Level 1 (not "officially" in 3.1.1)
            QosLevel = (byte)MqttQoSLevel.AtLeastOnce;
        }

        /// <summary>
        /// Parse bytes for a SUBSCRIBE message
        /// </summary>
        /// <param name="fixedHeaderFirstByte">First fixed header byte</param>
        /// <param name="protocolVersion">MQTT Protocol Version</param>
        /// <param name="channel">Channel connected to the broker</param>
        /// <returns>SUBSCRIBE message instance</returns>
        public static MqttMsgSubscribe Parse(byte fixedHeaderFirstByte, byte protocolVersion, IMqttNetworkChannel channel)
        {
            byte[] buffer;
            int index = 0;
            byte[] topicUtf8;
            int topicUtf8Length;
            MqttMsgSubscribe msg = new MqttMsgSubscribe();

            if (protocolVersion == MqttMsgConnect.PROTOCOL_VERSION_V3_1_1)
            {
                // [v3.1.1] check flag bits
                if ((fixedHeaderFirstByte & MSG_FLAG_BITS_MASK) != MQTT_MSG_SUBSCRIBE_FLAG_BITS)
                {
                    throw new MqttClientException(MqttClientErrorCode.InvalidFlagBits);
                }
            }

            // get remaining length and allocate buffer
            int remainingLength = MqttMsgBase.DecodeRemainingLength(channel);
            buffer = new byte[remainingLength];

            // read bytes from socket...
            int received = channel.Receive(buffer);

            if (protocolVersion == MqttMsgConnect.PROTOCOL_VERSION_V3_1)
            {
                // only 3.1.0

                // read QoS level from fixed header
                msg.QosLevel = (byte)((fixedHeaderFirstByte & QOS_LEVEL_MASK) >> QOS_LEVEL_OFFSET);
                // read DUP flag from fixed header
                msg.DupFlag = (((fixedHeaderFirstByte & DUP_FLAG_MASK) >> DUP_FLAG_OFFSET) == 0x01);
                // retain flag not used
                msg.Retain = false;
            }

            // message id
            msg.MessageId = (ushort)((buffer[index++] << 8) & 0xFF00);
            msg.MessageId |= (buffer[index++]);

            // payload contains topics and QoS levels
            // NOTE : before, I don't know how many topics will be in the payload (so use List)

            IList tmpTopics = new ArrayList();
            IList tmpQosLevels = new ArrayList();
            do
            {
                // topic name
                topicUtf8Length = ((buffer[index++] << 8) & 0xFF00);
                topicUtf8Length |= buffer[index++];
                topicUtf8 = new byte[topicUtf8Length];
                Array.Copy(buffer, index, topicUtf8, 0, topicUtf8Length);
                index += topicUtf8Length;
                tmpTopics.Add(new String(Encoding.UTF8.GetChars(topicUtf8)));

                // QoS level
                tmpQosLevels.Add(buffer[index++]);

            } while (index < remainingLength);

            // copy from list to array
            msg.Topics = new string[tmpTopics.Count];
            msg.QoSLevels = new MqttQoSLevel[tmpQosLevels.Count];
            for (int i = 0; i < tmpTopics.Count; i++)
            {
                msg.Topics[i] = (string)tmpTopics[i];
                msg.QoSLevels[i] = (MqttQoSLevel)tmpQosLevels[i];
            }

            return msg;
        }

        /// <summary>
        /// Returns the bytes that represents the current object.
        /// </summary>
        /// <param name="protocolVersion">MQTT protocol version</param>
        /// <returns>An array of bytes that represents the current object.</returns>
        public override byte[] GetBytes(byte protocolVersion)
        {
            int fixedHeaderSize = 0;
            int varHeaderSize = 0;
            int payloadSize = 0;
            int remainingLength = 0;
            byte[] buffer;
            int index = 0;

            // topics list empty
            if ((Topics == null) || (Topics.Length == 0))
            {
                throw new MqttClientException(MqttClientErrorCode.TopicsEmpty);
            }

            // qos levels list empty
            if ((QoSLevels == null) || (QoSLevels.Length == 0))
            {
                throw new MqttClientException(MqttClientErrorCode.QosLevelsEmpty);
            }

            // topics and qos levels lists length don't match
            if (Topics.Length != QoSLevels.Length)
            {
                throw new MqttClientException(MqttClientErrorCode.TopicsQosLevelsNotMatch);
            }

            // message identifier
            varHeaderSize += MESSAGE_ID_SIZE;

            int topicIdx = 0;
            byte[][] topicsUtf8 = new byte[Topics.Length][];

            for (topicIdx = 0; topicIdx < Topics.Length; topicIdx++)
            {
                // check topic length
                if ((Topics[topicIdx].Length < MIN_TOPIC_LENGTH) || (Topics[topicIdx].Length > MAX_TOPIC_LENGTH))
                {
                    throw new MqttClientException(MqttClientErrorCode.TopicLength);
                }

                topicsUtf8[topicIdx] = Encoding.UTF8.GetBytes(Topics[topicIdx]);
                payloadSize += 2; // topic size (MSB, LSB)
                payloadSize += topicsUtf8[topicIdx].Length;
                payloadSize++; // byte for QoS
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
            if (protocolVersion == MqttMsgConnect.PROTOCOL_VERSION_V3_1_1)
            {
                buffer[index++] = (MQTT_MSG_SUBSCRIBE_TYPE << MSG_TYPE_OFFSET) | MQTT_MSG_SUBSCRIBE_FLAG_BITS; // [v.3.1.1]
            }
            else
            {
                buffer[index] = (byte)((MQTT_MSG_SUBSCRIBE_TYPE << MSG_TYPE_OFFSET) |
                                   (QosLevel << QOS_LEVEL_OFFSET));
                buffer[index] |= DupFlag ? (byte)(1 << DUP_FLAG_OFFSET) : (byte)0x00;
                index++;
            }

            // encode remaining length
            index = EncodeRemainingLength(remainingLength, buffer, index);

            // check message identifier assigned (SUBSCRIBE uses QoS Level 1, so message id is mandatory)
            if (MessageId == 0)
            {
                throw new MqttClientException(MqttClientErrorCode.WrongMessageId);
            }

            buffer[index++] = (byte)((MessageId >> 8) & 0x00FF); // MSB
            buffer[index++] = (byte)(MessageId & 0x00FF); // LSB 

            for (topicIdx = 0; topicIdx < Topics.Length; topicIdx++)
            {
                // topic name
                buffer[index++] = (byte)((topicsUtf8[topicIdx].Length >> 8) & 0x00FF); // MSB
                buffer[index++] = (byte)(topicsUtf8[topicIdx].Length & 0x00FF); // LSB
                Array.Copy(topicsUtf8[topicIdx], 0, buffer, index, topicsUtf8[topicIdx].Length);
                index += topicsUtf8[topicIdx].Length;

                // requested QoS
                buffer[index++] = (byte)QoSLevels[topicIdx];
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
                "SUBSCRIBE",
                new object[] { "messageId", "topics", "qosLevels" },
                new object[] { MessageId, Topics, QoSLevels });
#else
            return base.ToString();
#endif
        }
    }
}
