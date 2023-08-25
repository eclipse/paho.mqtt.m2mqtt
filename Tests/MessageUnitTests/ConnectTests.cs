// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using nanoFramework.M2Mqtt;
using nanoFramework.M2Mqtt.Messages;
using nanoFramework.TestFramework;

namespace MessageUnitTests
{
    [TestClass]
    public class ConnectTests
    {
        private const string ClientID = "clientID";
        private const string UserName = "User Name";
        private const string Password = "A text password $$'/%";
        private const string WillTopic = "will topic";
        private const string WillMessage = "will message";
        private const ushort KeepAlivePeriod = 1234;

        [TestMethod]
        public void ConnectBasicEncodeTestv5()
        {
            // Arrange
            MqttMsgConnect connect = new(ClientID, UserName, Password, true, MqttQoSLevel.ExactlyOnce, true, WillTopic, WillMessage, true, KeepAlivePeriod, MqttProtocolVersion.Version_5);
            byte[] correctEncoded = new byte[] {16,81,0,4,77,81,84,84,5,246,4,210,0,0,8,99,108,105,101,110,116,73,68,0,10,119,105,108,108,
                32,116,111,112,105,99,0,12,119,105,108,108,32,109,101,115,115,97,103,101,0,9,
                85,115,101,114,32,78,97,109,101,0,21,65,32,116,101,120,116,32,112,97,115,115,
                119,111,114,100,32,36,36,39,47,37};
            // Act
            byte[] encoded = connect.GetBytes(connect.ProtocolVersion);
            // Assert
            Assert.AreEqual(correctEncoded.Length, encoded.Length);
            CollectionAssert.AreEqual(correctEncoded, encoded);
        }

        [TestMethod]
        public void ConnectBasicEncodeTestv311()
        {
            // Arrange
            MqttMsgConnect connect = new(ClientID, UserName, Password, true, MqttQoSLevel.ExactlyOnce, true, WillTopic, WillMessage, true, KeepAlivePeriod, MqttProtocolVersion.Version_3_1_1);
            byte[] correctEncoded = new byte[] {16,80,0,4,77,81,84,84,4,246,4,210,0,8,99,108,105,101,110,116,73,68,0,10,119,105,108,
                108,32,116,111,112,105,99,0,12,119,105,108,108,32,109,101,115,115,97,103,101,0,9,
                85,115,101,114,32,78,97,109,101,0,21,65,32,116,101,120,116,32,112,97,115,115,119,
                111,114,100,32,36,36,39,47,37};
            // Act
            byte[] encoded = connect.GetBytes(connect.ProtocolVersion);
            // Assert
            Assert.AreEqual(correctEncoded.Length, encoded.Length);
            CollectionAssert.AreEqual(correctEncoded, encoded);
        }

        [TestMethod]
        public void ConnectAdvancedEncodeTestv5()
        {
            // Arrange
            MqttMsgConnect connect = new(ClientID, UserName, Password, true, MqttQoSLevel.ExactlyOnce, true, WillTopic, WillMessage, true, KeepAlivePeriod, MqttProtocolVersion.Version_5);
            connect.AuthenticationData = new byte[5] { 0, 1, 2, 3, 4 };
            connect.AuthenticationMethod = "Wowo, cool";
            connect.MaximumPacketSize = uint.MaxValue - 100;
            connect.TopicAliasMaximum = ushort.MaxValue - 10;
            connect.ReceiveMaximum = ushort.MaxValue - 150;
            connect.RequestProblemInformation = true;
            connect.RequestResponseInformation = true;
            connect.SessionExpiryInterval = 54321;
            byte[] correctEncoded = new byte[] {16,122,0,4,77,81,84,84,5,246,4,210,41,21,0,10,87,111,119,111,44,32,99,111,111,108,
                22,0,5,0,1,2,3,4,17,0,0,212,49,33,255,105,39,255,255,255,155,34,255,245,23,1,25,1,
                0,8,99,108,105,101,110,116,73,68,0,10,119,105,108,108,32,116,111,112,105,99,0,12,
                119,105,108,108,32,109,101,115,115,97,103,101,0,9,85,115,101,114,32,78,97,109,101,
                0,21,65,32,116,101,120,116,32,112,97,115,115,119,111,114,100,32,36,36,39,47,37};
            // Act
            byte[] encoded = connect.GetBytes(connect.ProtocolVersion);
            // Assert
            Assert.AreEqual(correctEncoded.Length, encoded.Length);
            CollectionAssert.AreEqual(correctEncoded, encoded);
        }

        [TestMethod]
        public void ConnectUserPropEncodeTestv5()
        {
            // Arrange
            MqttMsgConnect connect = new(ClientID, UserName, Password, true, MqttQoSLevel.ExactlyOnce, true, WillTopic, WillMessage, true, KeepAlivePeriod, MqttProtocolVersion.Version_5);
            byte[] correctEncoded = new byte[] {16,115,0,4,77,81,84,84,5,246,4,210,34,38,0,3,79,110,101,0,8,80,114,111,112,101,114,
                116,121,38,0,3,84,119,111,0,10,80,114,111,112,101,114,116,105,101,115,0,8,99,108,
                105,101,110,116,73,68,0,10,119,105,108,108,32,116,111,112,105,99,0,12,119,105,108,
                108,32,109,101,115,115,97,103,101,0,9,85,115,101,114,32,78,97,109,101,0,21,65,32,
                116,101,120,116,32,112,97,115,115,119,111,114,100,32,36,36,39,47,37};
            connect.UserProperties.Add(new UserProperty("One", "Property"));
            connect.UserProperties.Add(new UserProperty("Two", "Properties"));
            // Act
            byte[] encoded = connect.GetBytes(connect.ProtocolVersion);
            Helpers.DumpBuffer(encoded);
            // Assert
            Assert.AreEqual(correctEncoded.Length, encoded.Length);
            CollectionAssert.AreEqual(correctEncoded, encoded);
        }

        [TestMethod]
        public void ConnectUserPropDecodeTestv5()
        {
            // Arrange
            byte[] correctEncoded = new byte[] {115,0,4,77,81,84,84,5,246,4,210,34,38,0,3,79,110,101,0,8,80,114,111,112,101,114,
                116,121,38,0,3,84,119,111,0,10,80,114,111,112,101,114,116,105,101,115,0,8,99,108,
                105,101,110,116,73,68,0,10,119,105,108,108,32,116,111,112,105,99,0,12,119,105,108,
                108,32,109,101,115,115,97,103,101,0,9,85,115,101,114,32,78,97,109,101,0,21,65,32,
                116,101,120,116,32,112,97,115,115,119,111,114,100,32,36,36,39,47,37};
            MokChannel mokChannel = new MokChannel(correctEncoded);
            // Act
            MqttMsgConnect connect = MqttMsgConnect.Parse((byte)MqttMessageType.Connect << 4, MqttProtocolVersion.Version_5, mokChannel);
            // Assert
            Assert.AreEqual((byte)MqttProtocolVersion.Version_5, (byte)connect.ProtocolVersion);
            Assert.AreEqual(ClientID, connect.ClientId);
            Assert.AreEqual(UserName, connect.Username);
            Assert.AreEqual(Password, connect.Password);
            Assert.AreEqual(WillTopic, connect.WillTopic);
            Assert.AreEqual(WillMessage, connect.WillMessage);
            Assert.AreEqual(KeepAlivePeriod, connect.KeepAlivePeriod);
            Assert.AreEqual(true, connect.CleanSession);
            Assert.AreEqual(2, connect.UserProperties.Count);
            var prop = new UserProperty("One", "Property");
            Assert.AreEqual(((UserProperty)connect.UserProperties[0]).Name, prop.Name);
            Assert.AreEqual(((UserProperty)connect.UserProperties[0]).Value, prop.Value);
            prop = new UserProperty("Two", "Properties");
            Assert.AreEqual(((UserProperty)connect.UserProperties[1]).Name, prop.Name);
            Assert.AreEqual(((UserProperty)connect.UserProperties[1]).Value, prop.Value);
        }

        [TestMethod]
        public void ConnecAdvancedDecodeTestv5()
        {
            // Arrange
            byte[] correctEncoded = new byte[] {122,0,4,77,81,84,84,5,246,4,210,41,21,0,10,87,111,119,111,44,32,99,111,111,108,
                22,0,5,0,1,2,3,4,17,0,0,212,49,33,255,105,39,255,255,255,155,34,255,245,23,1,25,1,
                0,8,99,108,105,101,110,116,73,68,0,10,119,105,108,108,32,116,111,112,105,99,0,12,
                119,105,108,108,32,109,101,115,115,97,103,101,0,9,85,115,101,114,32,78,97,109,101,
                0,21,65,32,116,101,120,116,32,112,97,115,115,119,111,114,100,32,36,36,39,47,37};
            MokChannel mokChannel = new MokChannel(correctEncoded);
            // Act
            MqttMsgConnect connect = MqttMsgConnect.Parse((byte)MqttMessageType.Connect << 4, MqttProtocolVersion.Version_5, mokChannel);
            // Assert
            Assert.AreEqual((byte)MqttProtocolVersion.Version_5, (byte)connect.ProtocolVersion);
            Assert.AreEqual(ClientID, connect.ClientId);
            Assert.AreEqual(UserName, connect.Username);
            Assert.AreEqual(Password, connect.Password);
            Assert.AreEqual(WillTopic, connect.WillTopic);
            Assert.AreEqual(WillMessage, connect.WillMessage);
            Assert.AreEqual(KeepAlivePeriod, connect.KeepAlivePeriod);
            Assert.AreEqual(true, connect.CleanSession);
            CollectionAssert.AreEqual(new byte[5] { 0, 1, 2, 3, 4 }, connect.AuthenticationData);
            Assert.AreEqual("Wowo, cool", connect.AuthenticationMethod);
            Assert.AreEqual(uint.MaxValue - 100, connect.MaximumPacketSize);
            Assert.AreEqual((ushort)(ushort.MaxValue - 10), connect.TopicAliasMaximum);
            Assert.AreEqual((ushort)(ushort.MaxValue - 150), connect.ReceiveMaximum);
            Assert.AreEqual(true, connect.RequestProblemInformation);
            Assert.AreEqual(true, connect.RequestResponseInformation);
            Assert.AreEqual(54321, connect.SessionExpiryInterval);
        }

        [TestMethod]
        public void ConnectBasicDecodeTestv5()
        {
            // Arrange
            byte[] correctEncoded = new byte[] {81,0,4,77,81,84,84,5,246,4,210,0,0,8,99,108,105,101,110,116,73,68,0,10,119,105,108,108,32,116,111,112,105,99,0,12,119,105,108,108,32,109,101,115,115,97,103,101,0,9,85,115,101,114,32,78,97,
                109,101,0,21,65,32,116,101,120,116,32,112,97,115,115,119,111,114,100,32,36,36,39,47,37};
            MokChannel mokChannel = new MokChannel(correctEncoded);
            // Act
            MqttMsgConnect connect = MqttMsgConnect.Parse((byte)MqttMessageType.Connect << 4, MqttProtocolVersion.Version_5, mokChannel);
            // Assert
            Assert.AreEqual((byte)MqttProtocolVersion.Version_5, (byte)connect.ProtocolVersion);
            Assert.AreEqual(ClientID, connect.ClientId);
            Assert.AreEqual(UserName, connect.Username);
            Assert.AreEqual(Password, connect.Password);
            Assert.AreEqual(WillTopic, connect.WillTopic);
            Assert.AreEqual(WillMessage, connect.WillMessage);
            Assert.AreEqual(KeepAlivePeriod, connect.KeepAlivePeriod);
            Assert.AreEqual(true, connect.CleanSession);
        }

        [TestMethod]
        public void ConnectBasicDecodeTestv311()
        {
            // Arrange
            byte[] correctEncoded = new byte[] {80,0,4,77,81,84,84,4,246,4,210,0,8,99,108,105,101,110,116,73,68,0,10,119,105,108,
                108,32,116,111,112,105,99,0,12,119,105,108,108,32,109,101,115,115,97,103,101,0,9,
                85,115,101,114,32,78,97,109,101,0,21,65,32,116,101,120,116,32,112,97,115,115,119,
                111,114,100,32,36,36,39,47,37};
            MokChannel mokChannel = new MokChannel(correctEncoded);
            // Act
            MqttMsgConnect connect = MqttMsgConnect.Parse((byte)MqttMessageType.Connect << 4, MqttProtocolVersion.Version_3_1_1, mokChannel);
            // Assert
            Assert.AreEqual((byte)MqttProtocolVersion.Version_3_1_1, (byte)connect.ProtocolVersion);
            Assert.AreEqual(ClientID, connect.ClientId);
            Assert.AreEqual(UserName, connect.Username);
            Assert.AreEqual(Password, connect.Password);
            Assert.AreEqual(WillTopic, connect.WillTopic);
            Assert.AreEqual(WillMessage, connect.WillMessage);
            Assert.AreEqual(KeepAlivePeriod, connect.KeepAlivePeriod);
            Assert.AreEqual(true, connect.CleanSession);
        }
    }
}
