// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using nanoFramework.M2Mqtt;
using nanoFramework.M2Mqtt.Messages;
using nanoFramework.TestFramework;

namespace MessageUnitTests
{
    [TestClass]
    public class PubretTests
    {
        [TestMethod]
        public void PubrelBasicEncodeTestv311()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 98, 2, 0, 42 };
            MqttMsgPubrel pubrel = new();
            pubrel.MessageId = 42;
            // Act
            byte[] encoded = pubrel.GetBytes(MqttProtocolVersion.Version_3_1_1);
            Helpers.DumpBuffer(encoded);
            // Assert
            CollectionAssert.AreEqual(encodedCorrect, encoded);
        }

        [TestMethod]
        public void PubrelBasicEncodeTestv5()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 98, 4, 0, 42, 0, 0 };
            MqttMsgPubrel pubrel = new();
            pubrel.MessageId = 42;
            // Act
            byte[] encoded = pubrel.GetBytes(MqttProtocolVersion.Version_5);
            // Assert
            CollectionAssert.AreEqual(encodedCorrect, encoded);
        }

        [TestMethod]
        public void PubrelAdvancedEncodeTestv5()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 98,53,0,42,151,49,31,0,26,89,111,117,32,104,97,118,101,32,101,120,99,101,101,100,
                32,121,111,117,114,32,113,117,111,116,97,38,0,4,80,114,111,112,0,1,49,38,0,4,80,114,
                111,112,0,1,50 };
            MqttMsgPubrel pubrel = new();
            pubrel.MessageId = 42;
            pubrel.ReasonCode = MqttReasonCode.QuotaExceeded;
            pubrel.Reason = "You have exceed your quota";
            pubrel.UserProperties.Add(new UserProperty("Prop", "1"));
            pubrel.UserProperties.Add(new UserProperty("Prop", "2"));
            // Act
            byte[] encoded = pubrel.GetBytes(MqttProtocolVersion.Version_5);
            Helpers.DumpBuffer(encoded);
            // Assert
            CollectionAssert.AreEqual(encodedCorrect, encoded);
        }

        [TestMethod]
        public void PubrelAdvancedEncodeMaximumPacketSizeTestv5()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 98, 4, 0, 42, 0x97, 0 };
            MqttMsgPubrel pubrel = new();
            pubrel.MessageId = 42;
            pubrel.MaximumPacketSize = 5;
            // This should not be send at all as exceeding the maximum packet size
            pubrel.ReasonCode = MqttReasonCode.QuotaExceeded;
            pubrel.Reason = "You have exceed your quota";
            pubrel.UserProperties.Add(new UserProperty("Prop", "1"));
            pubrel.UserProperties.Add(new UserProperty("Prop", "2"));
            // Act
            byte[] encoded = pubrel.GetBytes(MqttProtocolVersion.Version_5);
            // Assert
            CollectionAssert.AreEqual(encodedCorrect, encoded);
        }

        [TestMethod]
        public void PubrelBasicDecodeTestv311()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 2, 0, 42 };
            MokChannel mokChannel = new(encodedCorrect);
            // Act
            MqttMsgPubrel pubrel = MqttMsgPubrel.Parse(98, MqttProtocolVersion.Version_3_1_1, mokChannel);
            // Assert
            Assert.AreEqual((ushort)42, pubrel.MessageId);
        }

        [TestMethod]
        public void PubrelBasicDecodeTestv5()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 4, 0, 42, 0x97, 0 };
            MokChannel mokChannel = new(encodedCorrect);
            // Act
            MqttMsgPubrel pubrel = MqttMsgPubrel.Parse(98, MqttProtocolVersion.Version_5, mokChannel);
            // Assert
            Assert.AreEqual((ushort)42, pubrel.MessageId);
            Assert.AreEqual((byte)0x97, (byte)pubrel.ReasonCode);
        }

        [TestMethod]
        public void PubrelAdvanceDecodeTestv5()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 53,0,42,151,49,31,0,26,89,111,117,32,104,97,118,101,32,101,120,99,101,101,100,
                32,121,111,117,114,32,113,117,111,116,97,38,0,4,80,114,111,112,0,1,49,38,0,4,80,114,
                111,112,0,1,50 };
            MokChannel mokChannel = new(encodedCorrect);
            // Act
            MqttMsgPubrel pubrel = MqttMsgPubrel.Parse(98, MqttProtocolVersion.Version_5, mokChannel);
            // Assert
            Assert.AreEqual((ushort)42, pubrel.MessageId);
            Assert.AreEqual((byte)pubrel.ReasonCode, (byte)MqttReasonCode.QuotaExceeded);
            Assert.AreEqual(pubrel.Reason, "You have exceed your quota");
            Assert.AreEqual(pubrel.UserProperties.Count, 2);
            var prop = new UserProperty("Prop", "1");
            Assert.AreEqual(((UserProperty)pubrel.UserProperties[0]).Name, prop.Name);
            Assert.AreEqual(((UserProperty)pubrel.UserProperties[0]).Value, prop.Value);
            prop = new UserProperty("Prop", "2");
            Assert.AreEqual(((UserProperty)pubrel.UserProperties[1]).Name, prop.Name);
            Assert.AreEqual(((UserProperty)pubrel.UserProperties[1]).Value, prop.Value);
        }
    }
}
