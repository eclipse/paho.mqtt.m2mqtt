// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using nanoFramework.M2Mqtt;
using nanoFramework.M2Mqtt.Messages;
using nanoFramework.TestFramework;

namespace MessageUnitTests
{
    [TestClass]
    public class PubrecTests
    {
        [TestMethod]
        public void PubrecBasicEncodeTestv311()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 80, 2, 0, 42 };
            MqttMsgPubrec pubrec = new();
            pubrec.MessageId = 42;
            // Act
            byte[] encoded = pubrec.GetBytes(MqttProtocolVersion.Version_3_1_1);
            Helpers.DumpBuffer(encoded);
            // Assert
            CollectionAssert.AreEqual(encodedCorrect, encoded);
        }

        [TestMethod]
        public void PubrecBasicEncodeTestv5()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 80, 4, 0, 42, 0, 0 };
            MqttMsgPubrec pubrec = new();
            pubrec.MessageId = 42;
            // Act
            byte[] encoded = pubrec.GetBytes(MqttProtocolVersion.Version_5);
            // Assert
            CollectionAssert.AreEqual(encodedCorrect, encoded);
        }

        [TestMethod]
        public void PubrecAdvancedEncodeTestv5()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 80,53,0,42,151,49,31,0,26,89,111,117,32,104,97,118,101,32,101,120,99,101,101,100,
                32,121,111,117,114,32,113,117,111,116,97,38,0,4,80,114,111,112,0,1,49,38,0,4,80,114,
                111,112,0,1,50 };
            MqttMsgPubrec pubrec = new();
            pubrec.MessageId = 42;
            pubrec.ReasonCode = MqttReasonCode.QuotaExceeded;
            pubrec.Reason = "You have exceed your quota";
            pubrec.UserProperties.Add(new UserProperty("Prop", "1"));
            pubrec.UserProperties.Add(new UserProperty("Prop", "2"));
            // Act
            byte[] encoded = pubrec.GetBytes(MqttProtocolVersion.Version_5);
            Helpers.DumpBuffer(encoded);
            // Assert
            CollectionAssert.AreEqual(encodedCorrect, encoded);
        }

        [TestMethod]
        public void PubrecAdvancedEncodeMaximumPacketSizeTestv5()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 80, 4, 0, 42, 0x97, 0 };
            MqttMsgPubrec pubrec = new();
            pubrec.MessageId = 42;
            pubrec.MaximumPacketSize = 5;
            // This should not be send at all as exceeding the maximum packet size
            pubrec.ReasonCode = MqttReasonCode.QuotaExceeded;
            pubrec.Reason = "You have exceed your quota";
            pubrec.UserProperties.Add(new UserProperty("Prop", "1"));
            pubrec.UserProperties.Add(new UserProperty("Prop", "2"));
            // Act
            byte[] encoded = pubrec.GetBytes(MqttProtocolVersion.Version_5);
            // Assert
            CollectionAssert.AreEqual(encodedCorrect, encoded);
        }

        [TestMethod]
        public void PubrecBasicDecodeTestv311()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 2, 0, 42 };
            MokChannel mokChannel = new(encodedCorrect);
            // Act
            MqttMsgPubrec pubrec = MqttMsgPubrec.Parse(80, MqttProtocolVersion.Version_3_1_1, mokChannel);
            // Assert
            Assert.AreEqual((ushort)42, pubrec.MessageId);
        }

        [TestMethod]
        public void PubrecBasicDecodeTestv5()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 4, 0, 42, 0x97, 0 };
            MokChannel mokChannel = new(encodedCorrect);
            // Act
            MqttMsgPubrec pubrec = MqttMsgPubrec.Parse(80, MqttProtocolVersion.Version_5, mokChannel);
            // Assert
            Assert.AreEqual((byte)MqttMessageType.PublishReceived, (byte)pubrec.Type);
            Assert.AreEqual((ushort)42, pubrec.MessageId);
            Assert.AreEqual((byte)0x97, (byte)pubrec.ReasonCode);
        }

        [TestMethod]
        public void PubrecAdvanceDecodeTestv5()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 53,0,42,151,49,31,0,26,89,111,117,32,104,97,118,101,32,101,120,99,101,101,100,
                32,121,111,117,114,32,113,117,111,116,97,38,0,4,80,114,111,112,0,1,49,38,0,4,80,114,
                111,112,0,1,50 };
            MokChannel mokChannel = new(encodedCorrect);
            // Act
            MqttMsgPubrec pubrec = MqttMsgPubrec.Parse(80, MqttProtocolVersion.Version_5, mokChannel);
            // Assert
            Assert.AreEqual((ushort)42, pubrec.MessageId);
            Assert.AreEqual((byte)pubrec.ReasonCode, (byte)MqttReasonCode.QuotaExceeded);
            Assert.AreEqual(pubrec.Reason, "You have exceed your quota");
            Assert.AreEqual(pubrec.UserProperties.Count, 2);
            var prop = new UserProperty("Prop", "1");
            Assert.AreEqual(((UserProperty)pubrec.UserProperties[0]).Name, prop.Name);
            Assert.AreEqual(((UserProperty)pubrec.UserProperties[0]).Value, prop.Value);
            prop = new UserProperty("Prop", "2");
            Assert.AreEqual(((UserProperty)pubrec.UserProperties[1]).Name, prop.Name);
            Assert.AreEqual(((UserProperty)pubrec.UserProperties[1]).Value, prop.Value);
        }
    }
}
