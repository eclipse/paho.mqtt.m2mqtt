// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using nanoFramework.M2Mqtt;
using nanoFramework.M2Mqtt.Messages;
using nanoFramework.TestFramework;

namespace MessageUnitTests
{
    [TestClass]
    public class DisconnectTests
    {
        [TestMethod]
        public void DisconnectBasicEncodeTestv311()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 224, 0 };
            MqttMsgDisconnect disconnect = new();
            // Act
            byte[] encoded = disconnect.GetBytes(MqttProtocolVersion.Version_3_1_1);
            //Asserts
            Assert.Equal(encodedCorrect, encoded);
        }

        [TestMethod]
        public void DisconnectBasicEncodeTestv5()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 224, 0 };
            MqttMsgDisconnect disconnect = new();
            // Act
            byte[] encoded = disconnect.GetBytes(MqttProtocolVersion.Version_5);
            //Assert
            Assert.Equal(encodedCorrect, encoded);
        }

        [TestMethod]
        public void DisconnectBasicNotSuccessEncodeTestv5()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 224, 2, 143, 0 };
            MqttMsgDisconnect disconnect = new();
            disconnect.ResonCode = MqttReasonCode.TopicFilterInvalid;
            // Act
            byte[] encoded = disconnect.GetBytes(MqttProtocolVersion.Version_5);
            //Assert
            Assert.Equal(encodedCorrect, encoded);
        }

        [TestMethod]
        public void DisconnectAdvanceEncodeTestv5()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 224,62,143,60,17,0,0,48,57,31,0,13,73,110,118,97,108,105,100,32,116,111,112,105,99,
                38,0,6,115,116,97,116,117,115,0,3,49,48,49,28,0,22,110,101,119,115,101,114,118,101,
                114,46,115,111,109,116,104,105,110,103,46,110,101,116 };
            MqttMsgDisconnect disconnect = new();
            disconnect.ResonCode = MqttReasonCode.TopicFilterInvalid;
            disconnect.SessionExpiryInterval = 12345;
            disconnect.Reason = "Invalid topic";
            disconnect.ServerReference = "newserver.somthing.net";
            disconnect.UserProperties.Add(new UserProperty("status", "101"));
            // Act
            byte[] encoded = disconnect.GetBytes(MqttProtocolVersion.Version_5);
            Helpers.DumpBuffer(encoded);
            //Assert
            Assert.Equal(encodedCorrect, encoded);
        }

        [TestMethod]
        public void DisconnectBasicDecodeTestv311()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 0 };
            MokChannel mokChannel = new MokChannel(encodedCorrect);
            // Act
            MqttMsgDisconnect disconnect = MqttMsgDisconnect.Parse(224, MqttProtocolVersion.Version_3_1_1, mokChannel);
            // Assert
            // Nothing to assert!
        }

        [TestMethod]
        public void DisconnectBasicDecodeTestv5()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 0 };
            MokChannel mokChannel = new MokChannel(encodedCorrect);
            // Act
            MqttMsgDisconnect disconnect = MqttMsgDisconnect.Parse(224, MqttProtocolVersion.Version_5, mokChannel);
            // Assert
        }

        [TestMethod]
        public void DisconnectBasicDecodeErrorCodeTestv5()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 2, 143, 0 };
            MokChannel mokChannel = new MokChannel(encodedCorrect);
            // Act
            MqttMsgDisconnect disconnect = MqttMsgDisconnect.Parse(224, MqttProtocolVersion.Version_5, mokChannel);
            // Assert
            Assert.Equal((byte)disconnect.ResonCode, (byte)MqttReasonCode.TopicFilterInvalid);
        }

        [TestMethod]
        public void DisconnectAdvanceDecodeTestv5()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 62,143,60,17,0,0,48,57,31,0,13,73,110,118,97,108,105,100,32,116,111,112,105,99,
                38,0,6,115,116,97,116,117,115,0,3,49,48,49,28,0,22,110,101,119,115,101,114,118,101,
                114,46,115,111,109,116,104,105,110,103,46,110,101,116 };
            MokChannel mokChannel = new MokChannel(encodedCorrect);
            // Act
            MqttMsgDisconnect disconnect = MqttMsgDisconnect.Parse(224, MqttProtocolVersion.Version_5, mokChannel);
            // Assert
            Assert.Equal((byte)disconnect.ResonCode, (byte)MqttReasonCode.TopicFilterInvalid);
            Assert.Equal(disconnect.SessionExpiryInterval, 12345);
            Assert.Equal(disconnect.Reason, "Invalid topic");
            Assert.Equal(disconnect.ServerReference, "newserver.somthing.net");
            var prop = new UserProperty("status", "101");
            Assert.Equal(((UserProperty)disconnect.UserProperties[0]).Name, prop.Name);
            Assert.Equal(((UserProperty)disconnect.UserProperties[0]).Value, prop.Value);
        }
    }
}
