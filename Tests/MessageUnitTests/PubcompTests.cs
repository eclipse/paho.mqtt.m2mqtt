// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using nanoFramework.M2Mqtt;
using nanoFramework.M2Mqtt.Messages;
using nanoFramework.TestFramework;

namespace MessageUnitTests
{
    [TestClass]
    public class PubcompTests
    {
        [TestMethod]
        public void PubcompBasicEncodeTestv311()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 112, 2, 0, 42 };
            MqttMsgPubcomp pubcomp = new();
            pubcomp.MessageId = 42;
            // Act
            byte[] encoded = pubcomp.GetBytes(MqttProtocolVersion.Version_3_1_1);
            Helpers.DumpBuffer(encoded);
            // Assert
            CollectionAssert.AreEqual(encodedCorrect, encoded);
        }

        [TestMethod]
        public void PubcompBasicEncodeTestv5()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 112, 4, 0, 42, 0, 0 };
            MqttMsgPubcomp pubcomp = new();
            pubcomp.MessageId = 42;
            // Act
            byte[] encoded = pubcomp.GetBytes(MqttProtocolVersion.Version_5);
            // Assert
            CollectionAssert.AreEqual(encodedCorrect, encoded);
        }

        [TestMethod]
        public void PubcompAdvancedEncodeTestv5()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 112,53,0,42,151,49,31,0,26,89,111,117,32,104,97,118,101,32,101,120,99,101,101,100,
                32,121,111,117,114,32,113,117,111,116,97,38,0,4,80,114,111,112,0,1,49,38,0,4,80,114,
                111,112,0,1,50 };
            MqttMsgPubcomp pubcomp = new();
            pubcomp.MessageId = 42;
            pubcomp.ReasonCode = MqttReasonCode.QuotaExceeded;
            pubcomp.Reason = "You have exceed your quota";
            pubcomp.UserProperties.Add(new UserProperty("Prop", "1"));
            pubcomp.UserProperties.Add(new UserProperty("Prop", "2"));
            // Act
            byte[] encoded = pubcomp.GetBytes(MqttProtocolVersion.Version_5);
            // Assert
            CollectionAssert.AreEqual(encodedCorrect, encoded);
        }

        [TestMethod]
        public void PubcompAdvancedEncodeMaximumPacketSizeTestv5()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 112, 4, 0, 42, 0x97, 0 };
            MqttMsgPubcomp pubcomp = new();
            pubcomp.MessageId = 42;
            pubcomp.MaximumPacketSize = 5;
            // This should not be send at all as exceeding the maximum packet size
            pubcomp.ReasonCode = MqttReasonCode.QuotaExceeded;
            pubcomp.Reason = "You have exceed your quota";
            pubcomp.UserProperties.Add(new UserProperty("Prop", "1"));
            pubcomp.UserProperties.Add(new UserProperty("Prop", "2"));
            // Act
            byte[] encoded = pubcomp.GetBytes(MqttProtocolVersion.Version_5);
            // Assert
            CollectionAssert.AreEqual(encodedCorrect, encoded);
        }

        [TestMethod]
        public void PubcompBasicDecodeTestv311()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 2, 0, 42 };
            MokChannel mokChannel = new(encodedCorrect);
            // Act
            MqttMsgPubcomp pubcomp = MqttMsgPubcomp.Parse(112, MqttProtocolVersion.Version_3_1_1, mokChannel);
            // Assert
            Assert.AreEqual((ushort)42, pubcomp.MessageId);
        }

        [TestMethod]
        public void PubcompBasicDecodeTestv5()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 4, 0, 42, 0x97, 0 };
            MokChannel mokChannel = new(encodedCorrect);
            // Act
            MqttMsgPubcomp pubcomp = MqttMsgPubcomp.Parse(112, MqttProtocolVersion.Version_5, mokChannel);
            // Assert
            Assert.AreEqual((byte)MqttMessageType.PublishComplete, (byte)pubcomp.Type);
            Assert.AreEqual((ushort)42, pubcomp.MessageId);
            Assert.AreEqual((byte)0x97, (byte)pubcomp.ReasonCode);
        }

        [TestMethod]
        public void PubcompAdvanceDecodeTestv5()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 53,0,42,151,49,31,0,26,89,111,117,32,104,97,118,101,32,101,120,99,101,101,100,
                32,121,111,117,114,32,113,117,111,116,97,38,0,4,80,114,111,112,0,1,49,38,0,4,80,114,
                111,112,0,1,50 };
            MokChannel mokChannel = new(encodedCorrect);
            // Act
            MqttMsgPubcomp pubcomp = MqttMsgPubcomp.Parse(112, MqttProtocolVersion.Version_5, mokChannel);
            // Assert
            Assert.AreEqual((ushort)42, pubcomp.MessageId);
            Assert.AreEqual((byte)pubcomp.ReasonCode, (byte)MqttReasonCode.QuotaExceeded);
            Assert.AreEqual(pubcomp.Reason, "You have exceed your quota");
            Assert.AreEqual(pubcomp.UserProperties.Count, 2);
            var prop = new UserProperty("Prop", "1");
            Assert.AreEqual(((UserProperty)pubcomp.UserProperties[0]).Name, prop.Name);
            Assert.AreEqual(((UserProperty)pubcomp.UserProperties[0]).Value, prop.Value);
            prop = new UserProperty("Prop", "2");
            Assert.AreEqual(((UserProperty)pubcomp.UserProperties[1]).Name, prop.Name);
            Assert.AreEqual(((UserProperty)pubcomp.UserProperties[1]).Value, prop.Value);
        }
    }
}
