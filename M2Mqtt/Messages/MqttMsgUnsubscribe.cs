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
    /// Class for UNSUBSCRIBE message from client to broker
    /// </summary>
    public class MqttMsgUnsubscribe : MqttMsgBase
    {
        /// <summary>
        /// List of topics to unsubscribe
        /// </summary>
        public string[] Topics { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public MqttMsgUnsubscribe()
        {
            Type = MQTT_MSG_UNSUBSCRIBE_TYPE;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="topics">List of topics to unsubscribe</param>
        public MqttMsgUnsubscribe(string[] topics)
        {
            Type = MQTT_MSG_UNSUBSCRIBE_TYPE;

            Topics = topics;

            // UNSUBSCRIBE message uses QoS Level 1 (not "officially" in 3.1.1)
            QosLevel = MqttQoSLevel.AtLeastOnce;
        }

        /// <summary>
        /// Parse bytes for a UNSUBSCRIBE message
        /// </summary>
        /// <param name="fixedHeaderFirstByte">First fixed header byte</param>
        /// <param name="protocolVersion">MQTT Protocol Version</param>
        /// <param name="channel">Channel connected to the broker</param>
        /// <returns>UNSUBSCRIBE message instance</returns>
        public static MqttMsgUnsubscribe Parse(byte fixedHeaderFirstByte, byte protocolVersion, IMqttNetworkChannel channel)
        {
            byte[] buffer;
            int indexUnsub = 0;
            byte[] topicUtf8;
            int topicUtf8Length;
            MqttMsgUnsubscribe msg = new MqttMsgUnsubscribe();

            if (protocolVersion == MqttMsgConnect.PROTOCOL_VERSION_V3_1_1)
            {
                // [v3.1.1] check flag bits
                if ((fixedHeaderFirstByte & MSG_FLAG_BITS_MASK) != MQTT_MSG_UNSUBSCRIBE_FLAG_BITS)
                {
                    throw new MqttClientException(MqttClientErrorCode.InvalidFlagBits);
                }
            }

            // get remaining length and allocate buffer
            int remainingLength = DecodeRemainingLength(channel);
            buffer = new byte[remainingLength];

            // read bytes from socket...
            int received = channel.Receive(buffer);

            if (protocolVersion == MqttMsgConnect.PROTOCOL_VERSION_V3_1)
            {
                // only 3.1.0
                // read QoS level from fixed header
                msg.QosLevel = (MqttQoSLevel)((fixedHeaderFirstByte & QOS_LEVEL_MASK) >> QOS_LEVEL_OFFSET);
                // read DUP flag from fixed header
                msg.DupFlag = (((fixedHeaderFirstByte & DUP_FLAG_MASK) >> DUP_FLAG_OFFSET) == 0x01);
                // retain flag not used
                msg.Retain = false;
            }

            // message id
            msg.MessageId = (ushort)((buffer[indexUnsub++] << 8) & 0xFF00);
            msg.MessageId |= buffer[indexUnsub++];

            // payload contains topics
            // NOTE : before, I don't know how many topics will be in the payload (so use List)

            IList tmpTopics = new ArrayList();
            do
            {
                // topic name
                topicUtf8Length = ((buffer[indexUnsub++] << 8) & 0xFF00);
                topicUtf8Length |= buffer[indexUnsub++];
                topicUtf8 = new byte[topicUtf8Length];
                Array.Copy(buffer, indexUnsub, topicUtf8, 0, topicUtf8Length);
                indexUnsub += topicUtf8Length;
                tmpTopics.Add(new String(Encoding.UTF8.GetChars(topicUtf8)));
            } while (indexUnsub < remainingLength);

            // copy from list to array
            msg.Topics = new string[tmpTopics.Count];
            for (int i = 0; i < tmpTopics.Count; i++)
            {
                msg.Topics[i] = (string)tmpTopics[i];
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
            int fixedHeaderSize;
            int varHeaderSize = 0;
            int payloadSize = 0;
            int remainingLength = 0;
            byte[] buffer;
            int indexUnsubsc = 0;

            // topics list empty
            if ((Topics == null) || (Topics.Length == 0))
            {
                throw new MqttClientException(MqttClientErrorCode.TopicsEmpty);
            }

            // message identifier
            varHeaderSize += MESSAGE_ID_SIZE;

            int topicIdx;
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
                buffer[indexUnsubsc++] = (MQTT_MSG_UNSUBSCRIBE_TYPE << MSG_TYPE_OFFSET) | MQTT_MSG_UNSUBSCRIBE_FLAG_BITS; // [v.3.1.1]
            }
            else
            {
                buffer[indexUnsubsc] = (byte)((MQTT_MSG_UNSUBSCRIBE_TYPE << MSG_TYPE_OFFSET) |
                                   ((byte)QosLevel << QOS_LEVEL_OFFSET));
                buffer[indexUnsubsc] |= DupFlag ? (byte)(1 << DUP_FLAG_OFFSET) : (byte)0x00;
                indexUnsubsc++;
            }

            // encode remaining length
            indexUnsubsc = this.EncodeRemainingLength(remainingLength, buffer, indexUnsubsc);

            // check message identifier assigned
            if (MessageId == 0)
            {
                throw new MqttClientException(MqttClientErrorCode.WrongMessageId);
            }

            buffer[indexUnsubsc++] = (byte)((MessageId >> 8) & 0x00FF); // MSB
            buffer[indexUnsubsc++] = (byte)(MessageId & 0x00FF); // LSB 

            for (topicIdx = 0; topicIdx < Topics.Length; topicIdx++)
            {
                // topic name
                buffer[indexUnsubsc++] = (byte)((topicsUtf8[topicIdx].Length >> 8) & 0x00FF); // MSB
                buffer[indexUnsubsc++] = (byte)(topicsUtf8[topicIdx].Length & 0x00FF); // LSB
                Array.Copy(topicsUtf8[topicIdx], 0, buffer, indexUnsubsc, topicsUtf8[topicIdx].Length);
                indexUnsubsc += topicsUtf8[topicIdx].Length;
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
                "UNSUBSCRIBE",
                new object[] { "messageId", "topics" },
                new object[] { MessageId, Topics });
#else
            return base.ToString();
#endif
        }
    }
}
