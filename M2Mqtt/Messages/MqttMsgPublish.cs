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
using nanoFramework.M2Mqtt.Utility;

namespace nanoFramework.M2Mqtt.Messages
{
    /// <summary>
    /// Class for PUBLISH message from client to broker
    /// </summary>
    public class MqttMsgPublish : MqttMsgBase
    {
        /// <summary>
        /// Message topic
        /// </summary>
        public string Topic { get; set; }

        /// <summary>
        /// Message data
        /// </summary>
        public byte[] Message { get; set; }

        /// <summary>
        /// True if the payload is UTF8 encoded, v5.0 only
        /// </summary>
        public bool IsPayloadUTF8 { get; set; }

        /// <summary>
        /// Message Expiry Interval. If the server did not managed to process it on time, the message must be deleted.
        /// Value is the lifetime of the Will Message in seconds and is sent as the Publication Expiry Interval when the Server publishes the Will Message.
        /// v5.0 only
        /// </summary>
        /// <remarks>The value 0 is the default one, it means, it is not present</remarks>
        public uint MessageExpiryInterval { get; set; }

        /// <summary>
        /// Used instead of the Topic to reduce size of the Publish packet, v5.0 only
        /// </summary>
        /// <remarks>The 0 value is not permitted.
        /// The client must not send value higher than the Topic Alias Maximum received in the Connack/Connect message.</remarks>
        public ushort TopicAlias { get; set; }

        /// <summary>
        /// Response Topic is used as the Topic Name for a response message, v5.0 only
        /// </summary>
        public string ResponseTopic { get; set; }

        /// <summary>
        /// The Correlation Data is used by the sender of the Request Message to identify which request the Response Message is for when it is received, v5.0 only
        /// </summary>
        public byte[] CorrelationData { get; set; }

        /// <summary>
        /// The Subscription Identifier can have the value of 1 to 268,435,455, v5.0 only
        /// </summary>
        public int SubscriptionIdentifier { get; set; }

        /// <summary>
        /// The content of the Application Message, v5.0 only
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public MqttMsgPublish()
        {
            Type = MqttMessageType.Publish;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="topic">Message topic</param>
        /// <param name="message">Message data</param>
        public MqttMsgPublish(string topic, byte[] message) :
            this(topic, message, false, MqttQoSLevel.AtMostOnce, false)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="topic">Message topic</param>
        /// <param name="message">Message data</param>
        /// <param name="dupFlag">Duplicate flag</param>
        /// <param name="qosLevel">Quality of Service level</param>
        /// <param name="retain">Retain flag</param>
        public MqttMsgPublish(string topic,
            byte[] message,
            bool dupFlag,
            MqttQoSLevel qosLevel,
            bool retain) : base()
        {
            Type = MqttMessageType.Publish;

            Topic = topic;
            Message = message;
            DupFlag = dupFlag;
            QosLevel = qosLevel;
            Retain = retain;
            MessageId = 0;
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
            int varHeaderPropSize = 0;
            int payloadSize = 0;
            int remainingLength = 0;
            byte[] buffer;
            int index = 0;
            byte[] responseTopic = null;
            byte[] correlationData = null;
            byte[] userProperties = null;
            byte[] contentType = null;

            // topic can't contain wildcards
            if ((Topic.IndexOf('#') != -1) || (Topic.IndexOf('+') != -1))
            {
                throw new MqttClientException(MqttClientErrorCode.TopicWildcard);
            }

            // check topic length
            if ((Topic.Length < MIN_TOPIC_LENGTH) || (Topic.Length > MAX_TOPIC_LENGTH))
            {
                throw new MqttClientException(MqttClientErrorCode.TopicLength);
            }

            // check wrong QoS level (both bits can't be set 1)
            if (QosLevel > MqttQoSLevel.ExactlyOnce)
            {
                throw new MqttClientException(MqttClientErrorCode.QosNotAllowed);
            }

            if (protocolVersion == MqttProtocolVersion.Version_5)
            {
                if (IsPayloadUTF8)
                {
                    varHeaderPropSize += ENCODING_BYTE_SIZE;
                }

                if (MessageExpiryInterval > 0)
                {
                    varHeaderPropSize += ENCODING_FOUR_BYTE_SIZE;
                }

                if (TopicAlias > 0)
                {
                    varHeaderPropSize += ENCODING_TWO_BYTE_SIZE;
                }

                if (!string.IsNullOrEmpty(ResponseTopic))
                {
                    responseTopic = Encoding.UTF8.GetBytes(ResponseTopic);
                    varHeaderPropSize += ENCODING_UTF8_SIZE + responseTopic.Length;
                }

                if ((CorrelationData != null) && (CorrelationData.Length > 0))
                {
                    correlationData = EncodeDecodeHelper.EncodeArray(MqttProperty.CorrelationData, CorrelationData);
                    varHeaderPropSize += correlationData.Length;
                }

                if (UserProperties.Count > 0)
                {
                    userProperties = EncodeDecodeHelper.EncodeUserProperties(UserProperties);
                    varHeaderPropSize += userProperties.Length;
                }

                if (SubscriptionIdentifier > 0)
                {
                    // Variable byte identifier, so 1 for the type and the size of the id.
                    varHeaderPropSize += 1 + EncodeDecodeHelper.EncodeLength(SubscriptionIdentifier);
                }

                if (!string.IsNullOrEmpty(ContentType))
                {
                    contentType = Encoding.UTF8.GetBytes(ContentType);
                    varHeaderPropSize += ENCODING_UTF8_SIZE + contentType.Length;
                }

                varHeaderSize += varHeaderPropSize + EncodeDecodeHelper.EncodeLength(varHeaderPropSize);
            }

            byte[] topicUtf8 = Encoding.UTF8.GetBytes(Topic);

            // topic name
            varHeaderSize += topicUtf8.Length + 2;

            // message id is valid only with QOS level 1 or QOS level 2
            if ((QosLevel == MqttQoSLevel.AtLeastOnce) ||
                (QosLevel == MqttQoSLevel.ExactlyOnce))
            {
                varHeaderSize += MESSAGE_ID_SIZE;
            }

            // check on message with zero length
            if (Message != null)
            {
                // message data
                payloadSize += Message.Length;
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
            buffer[index] = (byte)(((byte)MqttMessageType.Publish << MSG_TYPE_OFFSET) |
                                   ((byte)QosLevel << QOS_LEVEL_OFFSET));
            buffer[index] |= DupFlag ? (byte)(1 << DUP_FLAG_OFFSET) : (byte)0x00;
            buffer[index] |= Retain ? (byte)(1 << RETAIN_FLAG_OFFSET) : (byte)0x00;
            index++;

            // encode remaining length
            index = EncodeVariableByte(remainingLength, buffer, index);

            // topic name
            buffer[index++] = (byte)((topicUtf8.Length >> 8) & 0x00FF); // MSB
            buffer[index++] = (byte)(topicUtf8.Length & 0x00FF); // LSB
            Array.Copy(topicUtf8, 0, buffer, index, topicUtf8.Length);
            index += topicUtf8.Length;

            // message id is valid only with QOS level 1 or QOS level 2
            if ((QosLevel == MqttQoSLevel.AtLeastOnce) ||
                (QosLevel == MqttQoSLevel.ExactlyOnce))
            {
                // check message identifier assigned
                if (MessageId == 0)
                {
                    throw new MqttClientException(MqttClientErrorCode.WrongMessageId);
                }

                buffer[index++] = (byte)((MessageId >> 8) & 0x00FF); // MSB
                buffer[index++] = (byte)(MessageId & 0x00FF); // LSB
            }

            // Properties for v5.0
            if (protocolVersion == MqttProtocolVersion.Version_5)
            {
                index = EncodeVariableByte(varHeaderPropSize, buffer, index);

                if (IsPayloadUTF8)
                {
                    buffer[index++] = (byte)MqttProperty.PayloadFormatIndicator;
                    buffer[index++] = (byte)(IsPayloadUTF8 ? 1 : 0);
                }

                if (MessageExpiryInterval > 0)
                {
                    index = EncodeDecodeHelper.EncodeUint(MqttProperty.MessageExpiryInterval, MessageExpiryInterval, buffer, index);
                }

                if (TopicAlias > 0)
                {
                    index = EncodeDecodeHelper.EncodeUshort(MqttProperty.TopicAlias, TopicAlias, buffer, index);
                }

                if (responseTopic != null)
                {
                    EncodeDecodeHelper.EncodeUTF8FromBuffer(MqttProperty.ResponseTopic, responseTopic, buffer, ref index);
                }

                if (correlationData != null)
                {
                    Array.Copy(correlationData, 0, buffer, index, correlationData.Length);
                    index += correlationData.Length;
                }

                if (UserProperties.Count > 0)
                {
                    Array.Copy(userProperties, 0, buffer, index, userProperties.Length);
                    index += userProperties.Length;
                }

                if (SubscriptionIdentifier > 0)
                {
                    // Variable byte
                    buffer[index++] = (byte)MqttProperty.SubscriptionIdentifier;
                    index = EncodeVariableByte(SubscriptionIdentifier, buffer, index);
                }

                if (contentType != null)
                {
                    EncodeDecodeHelper.EncodeUTF8FromBuffer(MqttProperty.ContentType, contentType, buffer, ref index);
                }
            }

            // check on message with zero length
            if (Message != null)
            {
                // message data
                Array.Copy(Message, 0, buffer, index, Message.Length);
            }

            return buffer;
        }

        /// <summary>
        /// Parse bytes for a PUBLISH message
        /// </summary>
        /// <param name="fixedHeaderFirstByte">First fixed header byte</param>
        /// <param name="protocolVersion">MQTT Protocol Version</param>
        /// <param name="channel">Channel connected to the broker</param>
        /// <returns>PUBLISH message instance</returns>
        public static MqttMsgPublish Parse(byte fixedHeaderFirstByte, MqttProtocolVersion protocolVersion, IMqttNetworkChannel channel)
        {
            byte[] buffer;
            int index = 0;
            byte[] topicUtf8;
            int topicUtf8Length;
            MqttMsgPublish msg = new MqttMsgPublish();

            // get remaining length and allocate buffer
            int remainingLength = DecodeVariableByte(channel);
            buffer = new byte[remainingLength];

            // read bytes from socket...
            int received = channel.Receive(buffer);

            // topic name
            topicUtf8Length = ((buffer[index++] << 8) & 0xFF00);
            topicUtf8Length |= buffer[index++];
            topicUtf8 = new byte[topicUtf8Length];
            Array.Copy(buffer, index, topicUtf8, 0, topicUtf8Length);
            index += topicUtf8Length;
            msg.Topic = new string(Encoding.UTF8.GetChars(topicUtf8));

            // read QoS level from fixed header
            msg.QosLevel = (MqttQoSLevel)((fixedHeaderFirstByte & QOS_LEVEL_MASK) >> QOS_LEVEL_OFFSET);
            // check wrong QoS level (both bits can't be set 1)
            if (msg.QosLevel > MqttQoSLevel.ExactlyOnce)
            {
                throw new MqttClientException(MqttClientErrorCode.QosNotAllowed);
            }

            // read DUP flag from fixed header
            msg.DupFlag = (((fixedHeaderFirstByte & DUP_FLAG_MASK) >> DUP_FLAG_OFFSET) == 0x01);
            // read retain flag from fixed header
            msg.Retain = (((fixedHeaderFirstByte & RETAIN_FLAG_MASK) >> RETAIN_FLAG_OFFSET) == 0x01);

            // message id is valid only with QOS level 1 or QOS level 2
            if ((msg.QosLevel == MqttQoSLevel.AtLeastOnce) ||
                (msg.QosLevel == MqttQoSLevel.ExactlyOnce))
            {
                // message id
                msg.MessageId = (ushort)((buffer[index++] << 8) & 0xFF00);
                msg.MessageId |= buffer[index++];
            }

            // We do have the properties here to decode
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
                        case MqttProperty.PayloadFormatIndicator:
                            msg.IsPayloadUTF8 = buffer[index++] != 0;
                            break;
                        case MqttProperty.MessageExpiryInterval:
                            msg.MessageExpiryInterval = EncodeDecodeHelper.DecodeUint(buffer, ref index);
                            break;
                        case MqttProperty.TopicAlias:
                            msg.TopicAlias = (ushort)(buffer[index++] << 8);
                            msg.TopicAlias |= buffer[index++];
                            break;
                        case MqttProperty.ResponseTopic:
                            msg.ResponseTopic = EncodeDecodeHelper.GetUTF8FromBuffer(buffer, ref index);
                            break;
                        case MqttProperty.CorrelationData:
                            // byte[]
                            int length = ((buffer[index++] << 8) & 0xFF00);
                            length |= buffer[index++];
                            msg.CorrelationData = new byte[length];
                            Array.Copy(buffer, index, msg.CorrelationData, 0, length);
                            index += length;
                            break;
                        case MqttProperty.UserProperty:
                            // UTF8 key value encoding, so 2 strings in a raw
                            string key = EncodeDecodeHelper.GetUTF8FromBuffer(buffer, ref index);
                            string value = EncodeDecodeHelper.GetUTF8FromBuffer(buffer, ref index);
                            msg.UserProperties.Add(new UserProperty(key, value));
                            break;
                        case MqttProperty.SubscriptionIdentifier:
                            msg.SubscriptionIdentifier = EncodeDecodeHelper.DecodeVariableByte(buffer, ref index);
                            break;
                        case MqttProperty.ContentType:
                            msg.ContentType = EncodeDecodeHelper.GetUTF8FromBuffer(buffer, ref index);
                            break;
                        default:
                            // non supported property
                            index = propSize;
                            break;
                    }
                }
            }

            // get payload with message data
            int messageSize = remainingLength - index;
            int remaining = messageSize;
            int messageOffset = 0;
            msg.Message = new byte[messageSize];

            // BUG FIX 26/07/2013 : receiving large payload

            // copy first part of payload data received
            Array.Copy(buffer, index, msg.Message, messageOffset, received - index);
            remaining -= received - index;
            messageOffset += received - index;

            // if payload isn't finished
            while (remaining > 0)
            {
                // receive other payload data
                received = channel.Receive(buffer);
                Array.Copy(buffer, 0, msg.Message, messageOffset, received);
                remaining -= received;
                messageOffset += received;
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
                "PUBLISH",
                new object[] { "messageId", "topic", "message" },
                new object[] { MessageId, Topic, Message });
#else
            return base.ToString();
#endif
        }
    }
}
