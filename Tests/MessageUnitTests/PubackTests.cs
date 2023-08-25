// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using nanoFramework.M2Mqtt;
using nanoFramework.M2Mqtt.Messages;
using nanoFramework.TestFramework;

namespace MessageUnitTests
{
    [TestClass]
    public class PubackTests
    {
        [TestMethod]
        public void PubackBasicEncodeTestv311()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 64, 2, 0, 42 };
            MqttMsgPuback puback = new();
            puback.MessageId = 42;
            // Act
            byte[] encoded = puback.GetBytes(MqttProtocolVersion.Version_3_1_1);
            // Assert
            CollectionAssert.AreEqual(encodedCorrect, encoded);
        }

        [TestMethod]
        public void PubackBasicEncodeTestv5()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 64, 4, 0, 42, 0, 0 };
            MqttMsgPuback puback = new();
            puback.MessageId = 42;
            // Act
            byte[] encoded = puback.GetBytes(MqttProtocolVersion.Version_5);
            // Assert
            CollectionAssert.AreEqual(encodedCorrect, encoded);
        }

        [TestMethod]
        public void PubackAdvancedEncodeTestv5()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 64,53,0,42,151,49,31,0,26,89,111,117,32,104,97,118,101,32,101,120,99,101,101,100,
                32,121,111,117,114,32,113,117,111,116,97,38,0,4,80,114,111,112,0,1,49,38,0,4,80,114,
                111,112,0,1,50 };
            MqttMsgPuback puback = new();
            puback.MessageId = 42;
            puback.ReasonCode = MqttReasonCode.QuotaExceeded;
            puback.Reason = "You have exceed your quota";
            puback.UserProperties.Add(new UserProperty("Prop", "1"));
            puback.UserProperties.Add(new UserProperty("Prop", "2"));
            // Act
            byte[] encoded = puback.GetBytes(MqttProtocolVersion.Version_5);
            // Assert
            CollectionAssert.AreEqual(encodedCorrect, encoded);
        }

        [TestMethod]
        public void PubackAdvancedEncodeMaximumPacketSizeTestv5()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 64, 4, 0, 42, 0x97, 0 };
            MqttMsgPuback puback = new();
            puback.MessageId = 42;
            puback.MaximumPacketSize = 5;
            // This should not be send at all as exceeding the maximum packet size
            puback.ReasonCode = MqttReasonCode.QuotaExceeded;
            puback.Reason = "You have exceed your quota";
            puback.UserProperties.Add(new UserProperty("Prop", "1"));
            puback.UserProperties.Add(new UserProperty("Prop", "2"));
            // Act
            byte[] encoded = puback.GetBytes(MqttProtocolVersion.Version_5);
            // Assert
            CollectionAssert.AreEqual(encodedCorrect, encoded);
        }

        [TestMethod]
        public void PubackBasicDecodeTestv311()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 2, 0, 42 };
            MokChannel mokChannel = new(encodedCorrect);
            // Act
            MqttMsgPuback puback = MqttMsgPuback.Parse(64, MqttProtocolVersion.Version_3_1_1, mokChannel);
            // Assert
            Assert.AreEqual((ushort)42, puback.MessageId);
        }

        [TestMethod]
        public void PubackBasicDecodeTestv5()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 4, 0, 42, 0, 0 };
            MokChannel mokChannel = new(encodedCorrect);
            // Act
            MqttMsgPuback puback = MqttMsgPuback.Parse(64, MqttProtocolVersion.Version_5, mokChannel);
            // Assert
            Assert.AreEqual((ushort)42, puback.MessageId);
        }

        [TestMethod]
        public void PubackAdvanceDecodeTestv5()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 53,0,42,151,49,31,0,26,89,111,117,32,104,97,118,101,32,101,120,99,101,101,100,
                32,121,111,117,114,32,113,117,111,116,97,38,0,4,80,114,111,112,0,1,49,38,0,4,80,114,
                111,112,0,1,50 };
            MokChannel mokChannel = new(encodedCorrect);
            // Act
            MqttMsgPuback puback = MqttMsgPuback.Parse(64, MqttProtocolVersion.Version_5, mokChannel);
            // Assert
            Assert.AreEqual((ushort)42, puback.MessageId);
            Assert.AreEqual((byte)puback.ReasonCode, (byte)MqttReasonCode.QuotaExceeded);
            Assert.AreEqual(puback.Reason, "You have exceed your quota");
            Assert.AreEqual(puback.UserProperties.Count, 2);
            var prop = new UserProperty("Prop", "1");
            Assert.AreEqual(((UserProperty)puback.UserProperties[0]).Name, prop.Name);
            Assert.AreEqual(((UserProperty)puback.UserProperties[0]).Value, prop.Value);
            prop = new UserProperty("Prop", "2");
            Assert.AreEqual(((UserProperty)puback.UserProperties[1]).Name, prop.Name);
            Assert.AreEqual(((UserProperty)puback.UserProperties[1]).Value, prop.Value);
        }
    }
}
