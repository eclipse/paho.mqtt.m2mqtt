// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using nanoFramework.M2Mqtt;
using nanoFramework.M2Mqtt.Messages;
using nanoFramework.TestFramework;

namespace MessageUnitTests
{
    [TestClass]
    public class UnsubackTests
    {
        [TestMethod]
        public void UnsubackBasicEncodeTestv311()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 176, 2, 0, 42 };
            MqttMsgUnsuback unsuback = new();
            unsuback.MessageId = 42;
            // Act
            byte[] encoded = unsuback.GetBytes(MqttProtocolVersion.Version_3_1_1);
            Helpers.DumpBuffer(encoded);
            // Assert
            CollectionAssert.AreEqual(encodedCorrect, encoded);
        }

        [TestMethod]
        public void UnsubackBasicEncodeTestv5()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 176, 4, 0, 42, 0, 0 };
            MqttMsgUnsuback unsuback = new();
            unsuback.MessageId = 42;
            // Act
            byte[] encoded = unsuback.GetBytes(MqttProtocolVersion.Version_5);
            // Assert
            CollectionAssert.AreEqual(encodedCorrect, encoded);
        }

        [TestMethod]
        public void UnsubackAdvancedEncodeTestv5()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 176,53,0,42,151,49,31,0,26,89,111,117,32,104,97,118,101,32,101,120,99,101,101,100,
                32,121,111,117,114,32,113,117,111,116,97,38,0,4,80,114,111,112,0,1,49,38,0,4,80,114,
                111,112,0,1,50 };
            MqttMsgUnsuback unsuback = new();
            unsuback.MessageId = 42;
            unsuback.ReasonCode = MqttReasonCode.QuotaExceeded;
            unsuback.Reason = "You have exceed your quota";
            unsuback.UserProperties.Add(new UserProperty("Prop", "1"));
            unsuback.UserProperties.Add(new UserProperty("Prop", "2"));
            // Act
            byte[] encoded = unsuback.GetBytes(MqttProtocolVersion.Version_5);
            Helpers.DumpBuffer(encoded);
            // Assert
            CollectionAssert.AreEqual(encodedCorrect, encoded);
        }

        [TestMethod]
        public void UnsubackAdvancedEncodeMaximumPacketSizeTestv5()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 176, 4, 0, 42, 0x97, 0 };
            MqttMsgUnsuback unsuback = new();
            unsuback.MessageId = 42;
            unsuback.MaximumPacketSize = 5;
            // This should not be send at all as exceeding the maximum packet size
            unsuback.ReasonCode = MqttReasonCode.QuotaExceeded;
            unsuback.Reason = "You have exceed your quota";
            unsuback.UserProperties.Add(new UserProperty("Prop", "1"));
            unsuback.UserProperties.Add(new UserProperty("Prop", "2"));
            // Act
            byte[] encoded = unsuback.GetBytes(MqttProtocolVersion.Version_5);
            // Assert
            CollectionAssert.AreEqual(encodedCorrect, encoded);
        }

        [TestMethod]
        public void UnsubackBasicDecodeTestv311()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 2, 0, 42 };
            MokChannel mokChannel = new(encodedCorrect);
            // Act
            MqttMsgUnsuback unsuback = MqttMsgUnsuback.Parse(176, MqttProtocolVersion.Version_3_1_1, mokChannel);
            // Assert
            Assert.AreEqual((ushort)42, unsuback.MessageId);
        }

        [TestMethod]
        public void UnsubackBasicDecodeTestv5()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 4, 0, 42, 0x97, 0 };
            MokChannel mokChannel = new(encodedCorrect);
            // Act
            MqttMsgUnsuback unsuback = MqttMsgUnsuback.Parse(176, MqttProtocolVersion.Version_5, mokChannel);
            // Assert
            Assert.AreEqual((byte)MqttMessageType.UnsubscribeAck, (byte)unsuback.Type);
            Assert.AreEqual((ushort)42, unsuback.MessageId);
            Assert.AreEqual((byte)0x97, (byte)unsuback.ReasonCode);
        }

        [TestMethod]
        public void UnsubackAdvanceDecodeTestv5()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 53,0,42,151,49,31,0,26,89,111,117,32,104,97,118,101,32,101,120,99,101,101,100,
                32,121,111,117,114,32,113,117,111,116,97,38,0,4,80,114,111,112,0,1,49,38,0,4,80,114,
                111,112,0,1,50 };
            MokChannel mokChannel = new(encodedCorrect);
            // Act
            MqttMsgUnsuback unsuback = MqttMsgUnsuback.Parse(176, MqttProtocolVersion.Version_5, mokChannel);
            // Assert
            Assert.AreEqual((ushort)42, unsuback.MessageId);
            Assert.AreEqual((byte)unsuback.ReasonCode, (byte)MqttReasonCode.QuotaExceeded);
            Assert.AreEqual(unsuback.Reason, "You have exceed your quota");
            Assert.AreEqual(unsuback.UserProperties.Count, 2);
            var prop = new UserProperty("Prop", "1");
            Assert.AreEqual(((UserProperty)unsuback.UserProperties[0]).Name, prop.Name);
            Assert.AreEqual(((UserProperty)unsuback.UserProperties[0]).Value, prop.Value);
            prop = new UserProperty("Prop", "2");
            Assert.AreEqual(((UserProperty)unsuback.UserProperties[1]).Name, prop.Name);
            Assert.AreEqual(((UserProperty)unsuback.UserProperties[1]).Value, prop.Value);
        }

    }
}
