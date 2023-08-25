// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using nanoFramework.M2Mqtt;
using nanoFramework.M2Mqtt.Messages;
using nanoFramework.TestFramework;

namespace MessageUnitTests
{
    [TestClass]
    class ConnackTests
    {
        [TestMethod]
        public void ConnacktBasicEncodeTestv311()
        {
            // Arrange
            MqttMsgConnack connack = new();
            connack.SessionPresent = true;
            connack.ReturnCode = MqttReasonCode.Banned;
            byte[] encodedCorrect = new byte[] { 32, 2, 1, 138 };
            // Act
            byte[] encoded = connack.GetBytes(MqttProtocolVersion.Version_3_1_1);
            // Assert
            CollectionAssert.AreEqual(encodedCorrect, encoded);
        }

        [TestMethod]
        public void ConnacktBasicEncodeTestv5()
        {
            // Arrange
            MqttMsgConnack connack = new();
            connack.SessionPresent = true;
            connack.ReturnCode = MqttReasonCode.Banned;
            byte[] encodedCorrect = new byte[] { 32, 3, 1, 138, 0 };
            // Act
            byte[] encoded = connack.GetBytes(MqttProtocolVersion.Version_5);
            // Assert
            CollectionAssert.AreEqual(encodedCorrect, encoded);
        }

        [TestMethod]
        public void ConnacktSimpleEncodeTestv5()
        {
            // Arrange
            MqttMsgConnack connack = new();
            connack.SessionPresent = true;
            connack.ReturnCode = MqttReasonCode.Banned;
            connack.AssignedClientIdentifier = "Tagada";
            byte[] encodedCorrect = new byte[] { 32, 12, 1, 138, 9, 18, 0, 6, 84, 97, 103, 97, 100, 97 };
            // Act
            byte[] encoded = connack.GetBytes(MqttProtocolVersion.Version_5);
            // Assert
            CollectionAssert.AreEqual(encodedCorrect, encoded);
            Assert.AreEqual("Tagada", connack.AssignedClientIdentifier);
        }

        [TestMethod]
        public void ConnacktAdvanceEncodeTestv5()
        {
            // Arrange
            MqttMsgConnack connack = new();
            connack.SessionPresent = true;
            connack.ReturnCode = MqttReasonCode.Banned;
            connack.AssignedClientIdentifier = "Tagada";
            connack.AuthenticationData = new byte[] { 1, 2, 3, 4 };
            connack.AuthenticationMethod = "method";
            connack.MaximumPacketSize = 4567;
            connack.MaximumQoS = true;
            connack.Reason = "none";
            connack.ReceiveMaximum = 89;
            connack.ResponseInformation = "infromation";
            connack.RetainAvailable = true;
            connack.ServerKeepAlive = 1357;
            connack.ServerReference = "reference";
            connack.SessionExpiryInterval = 2468;
            connack.SharedSubscriptionAvailable = true;
            connack.SubscriptionIdentifiersAvailable = true;
            connack.TopicAliasMaximum = 148;
            connack.UserProperties.Add(new UserProperty("One", "Property"));
            connack.UserProperties.Add(new UserProperty("Two", "Properties"));
            connack.WildcardSubscriptionAvailable = true;
            byte[] encodedCorrect = new byte[] { 32,124,1,138,121,17,0,0,9,164,33,0,89,36,1,37,1,39,0,0,17,215,18,0,6,84,97,103,97,
                100,97,34,0,148,31,0,4,110,111,110,101,38,0,3,79,110,101,0,8,80,114,111,112,101,114,
                116,121,38,0,3,84,119,111,0,10,80,114,111,112,101,114,116,105,101,115,40,1,41,1,42,
                1,19,5,77,26,0,11,105,110,102,114,111,109,97,116,105,111,110,28,0,9,114,101,102,101,
                114,101,110,99,101,21,0,6,109,101,116,104,111,100,22,0,4,1,2,3,4 };
            // Act
            byte[] encoded = connack.GetBytes(MqttProtocolVersion.Version_5);
            Helpers.DumpBuffer(encoded);
            // Assert
            CollectionAssert.AreEqual(encodedCorrect, encoded);
        }

        [TestMethod]
        public void ConnackAdvanceDecodeTestv5()
        {
            //Arrange
            byte[] encodedCorrect = new byte[] { 124,1,138,121,17,0,0,9,164,33,0,89,36,1,37,1,39,0,0,17,215,18,0,6,84,97,103,97,
                100,97,34,0,148,31,0,4,110,111,110,101,38,0,3,79,110,101,0,8,80,114,111,112,101,114,
                116,121,38,0,3,84,119,111,0,10,80,114,111,112,101,114,116,105,101,115,40,1,41,1,42,
                1,19,5,77,26,0,11,105,110,102,114,111,109,97,116,105,111,110,28,0,9,114,101,102,101,
                114,101,110,99,101,21,0,6,109,101,116,104,111,100,22,0,4,1,2,3,4 };
            MokChannel mokChannel = new MokChannel(encodedCorrect);
            // Act
            MqttMsgConnack connack = MqttMsgConnack.Parse((byte)(MqttMessageType.ConnectAck) << 4, MqttProtocolVersion.Version_5, mokChannel);
            // Assert
            Assert.AreEqual(true, connack.SessionPresent);
            Assert.AreEqual((byte)MqttReasonCode.Banned, (byte)connack.ReturnCode);
            Assert.AreEqual(connack.AssignedClientIdentifier, "Tagada");
            CollectionAssert.AreEqual(connack.AuthenticationData, new byte[] { 1, 2, 3, 4 });
            Assert.AreEqual(connack.AuthenticationMethod, "method");
            Assert.AreEqual(connack.MaximumPacketSize, 4567);
            Assert.AreEqual(connack.MaximumQoS, true);
            Assert.AreEqual(connack.Reason, "none");
            Assert.AreEqual(connack.ReceiveMaximum, (ushort)89);
            Assert.AreEqual(connack.ResponseInformation, "infromation");
            Assert.AreEqual(connack.RetainAvailable, true);
            Assert.AreEqual(connack.ServerKeepAlive, (ushort)1357);
            Assert.AreEqual(connack.ServerReference, "reference");
            Assert.AreEqual(connack.SessionExpiryInterval, 2468);
            Assert.AreEqual(connack.SharedSubscriptionAvailable, true);
            Assert.AreEqual(connack.SubscriptionIdentifiersAvailable, true);
            Assert.AreEqual(connack.TopicAliasMaximum, (ushort)148);
            Assert.AreEqual(connack.UserProperties.Count, 2);
            var prop = new UserProperty("One", "Property");
            Assert.AreEqual(((UserProperty)connack.UserProperties[0]).Name, prop.Name);
            Assert.AreEqual(((UserProperty)connack.UserProperties[0]).Value, prop.Value);
            prop = new UserProperty("Two", "Properties");
            Assert.AreEqual(((UserProperty)connack.UserProperties[1]).Name, prop.Name);
            Assert.AreEqual(((UserProperty)connack.UserProperties[1]).Value, prop.Value);
            Assert.AreEqual(connack.WildcardSubscriptionAvailable, true);
        }

        [TestMethod]
        public void ConnackBasicDecodeTest311()
        {
            // Arrange
            byte[] correctEncoded = new byte[] { 2, 1, 138 };
            MokChannel mokChannel = new MokChannel(correctEncoded);
            // Act
            MqttMsgConnack connack = MqttMsgConnack.Parse((byte)(MqttMessageType.ConnectAck) << 4, MqttProtocolVersion.Version_3_1_1, mokChannel);
            // Assert
            Assert.AreEqual(true, connack.SessionPresent);
            Assert.AreEqual((byte)MqttReasonCode.Banned, (byte)connack.ReturnCode);
        }

        [TestMethod]
        public void ConnackBasicDecodeTest5()
        {
            // Arrange
            byte[] correctEncoded = new byte[] { 3, 1, 138, 0 };
            MokChannel mokChannel = new MokChannel(correctEncoded);
            // Act
            MqttMsgConnack connack = MqttMsgConnack.Parse((byte)(MqttMessageType.ConnectAck) << 4, MqttProtocolVersion.Version_5, mokChannel);
            // Assert
            Assert.AreEqual(true, connack.SessionPresent);
            Assert.AreEqual((byte)MqttReasonCode.Banned, (byte)connack.ReturnCode);
        }

    }
}
