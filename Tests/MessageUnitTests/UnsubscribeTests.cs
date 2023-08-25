// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using nanoFramework.M2Mqtt;
using nanoFramework.M2Mqtt.Messages;
using nanoFramework.TestFramework;

namespace MessageUnitTests
{
    [TestClass]
    public class UnsubscribeTests
    {
        [TestMethod]
        public void UnbscribeBasicEncodeTestv311()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 162, 18, 0, 42, 0, 6, 116, 112, 111, 105, 99, 49, 0, 6, 116, 111, 112, 105, 99, 50 };
            MqttMsgUnsubscribe unsubscribe = new MqttMsgUnsubscribe(new string[] { "tpoic1", "topic2" });
            unsubscribe.MessageId = 42;
            // Act
            byte[] encoded = unsubscribe.GetBytes(MqttProtocolVersion.Version_3_1_1);
            // Assert
            CollectionAssert.AreEqual(encodedCorrect, encoded);
        }

        [TestMethod]
        public void UnbscribeBasicEncodeTestv5()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 162, 19, 0, 42, 0, 0, 6, 116, 112, 111, 105, 99, 49, 0, 6, 116, 111, 112, 105, 99, 50 };
            MqttMsgUnsubscribe unsubscribe = new MqttMsgUnsubscribe(new string[] { "tpoic1", "topic2" });
            unsubscribe.MessageId = 42;
            // Act
            byte[] encoded = unsubscribe.GetBytes(MqttProtocolVersion.Version_5);
            // Assert
            CollectionAssert.AreEqual(encodedCorrect, encoded);
        }

        [TestMethod]
        public void UnbscribeAdvanceEncodeTestv5()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 162,54,0,42,35,38,0,4,80,114,111,112,0,26,111,110,108,121,32,111,110,101,32,116,104,
                105,115,32,116,105,109,101,32,102,111,114,32,102,117,110,0,6,116,112,111,105,99,49,
                0,6,116,111,112,105,99,50 };
            MqttMsgUnsubscribe unsubscribe = new MqttMsgUnsubscribe(new string[] { "tpoic1", "topic2" });
            unsubscribe.MessageId = 42;
            unsubscribe.UserProperties.Add(new UserProperty("Prop", "only one this time for fun"));
            // Act
            byte[] encoded = unsubscribe.GetBytes(MqttProtocolVersion.Version_5);
            Helpers.DumpBuffer(encoded);
            // Assert
            CollectionAssert.AreEqual(encodedCorrect, encoded);
        }

        [TestMethod]
        public void UnbscribeBasicDecodeTestv311()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 18, 0, 42, 0, 6, 116, 112, 111, 105, 99, 49, 0, 6, 116, 111, 112, 105, 99, 50 };
            MokChannel mokChannel = new(encodedCorrect);
            // Act
            MqttMsgUnsubscribe unsubscribe = MqttMsgUnsubscribe.Parse(162, MqttProtocolVersion.Version_3_1_1, mokChannel);
            // Assert
            Assert.AreEqual((ushort)42, unsubscribe.MessageId);
            Assert.AreEqual(unsubscribe.Topics.Length, 2);
            CollectionAssert.AreEqual(unsubscribe.Topics, new string[] { "tpoic1", "topic2" });
        }

        [TestMethod]
        public void UnbscribeBasicDecodeTestv5()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 19, 0, 42, 0, 0, 6, 116, 112, 111, 105, 99, 49, 0, 6, 116, 111, 112, 105, 99, 50 };
            MokChannel mokChannel = new(encodedCorrect);
            // Act
            MqttMsgUnsubscribe unsubscribe = MqttMsgUnsubscribe.Parse(162, MqttProtocolVersion.Version_5, mokChannel);
            // Assert
            Assert.AreEqual((ushort)42, unsubscribe.MessageId);
            Assert.AreEqual(unsubscribe.Topics.Length, 2);
            CollectionAssert.AreEqual(unsubscribe.Topics, new string[] { "tpoic1", "topic2" });
        }

        [TestMethod]
        public void UnbscribeAdvanceDecodeTestv5()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 54,0,42,35,38,0,4,80,114,111,112,0,26,111,110,108,121,32,111,110,101,32,116,104,
                105,115,32,116,105,109,101,32,102,111,114,32,102,117,110,0,6,116,112,111,105,99,49,
                0,6,116,111,112,105,99,50 };
            MokChannel mokChannel = new(encodedCorrect);
            // Act
            MqttMsgUnsubscribe unsubscribe = MqttMsgUnsubscribe.Parse(162, MqttProtocolVersion.Version_5, mokChannel);
            // Assert
            Assert.AreEqual((ushort)42, unsubscribe.MessageId);
            Assert.AreEqual(unsubscribe.Topics.Length, 2);
            CollectionAssert.AreEqual(unsubscribe.Topics, new string[] { "tpoic1", "topic2" });
            var prop = new UserProperty("Prop", "only one this time for fun");
            Assert.AreEqual(((UserProperty)unsubscribe.UserProperties[0]).Name, prop.Name);
            Assert.AreEqual(((UserProperty)unsubscribe.UserProperties[0]).Value, prop.Value);
        }
    }
}
