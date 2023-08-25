// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using nanoFramework.M2Mqtt;
using nanoFramework.M2Mqtt.Messages;
using nanoFramework.TestFramework;

namespace MessageUnitTests
{
    [TestClass]
    public class AuthenticationTests
    {
        [TestMethod]
        public void AuthenticationBasicEncodeTestsv5()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 240,47,24,45,21,0,9,83,111,109,101,116,104,105,110,103,22,0,5,1,2,3,4,5,31,0,22,66,
                101,99,97,117,115,101,32,105,116,39,115,32,108,105,107,101,32,116,104,97,116 };
            MqttMsgAuthentication authentication = new();
            authentication.AuthenticationMethod = "Something";
            authentication.AuthenticationData = new byte[] { 1, 2, 3, 4, 5 };
            authentication.ReasonCode = MqttReasonCode.ContinueAuthentication;
            authentication.Reason = "Because it's like that";
            // Act
            byte[] encoded = authentication.GetBytes(MqttProtocolVersion.Version_5);
            // Assert
            CollectionAssert.AreEqual(encodedCorrect, encoded);
        }

        [TestMethod]
        public void AuthenticationExceptionTestsv311()
        {
            Assert.ThrowsException(typeof(NotSupportedException), () =>
            {
                MqttMsgAuthentication authentication = new();
                authentication.GetBytes(MqttProtocolVersion.Version_3_1_1);
            });
            Assert.ThrowsException(typeof(NotSupportedException), () =>
            {
                MqttMsgAuthentication authentication = new();
                authentication.GetBytes(MqttProtocolVersion.Version_3_1);
            });
            Assert.ThrowsException(typeof(NotSupportedException), () =>
            {
                MokChannel mokChannel = new MokChannel(new byte[1] { 42 });
                MqttMsgAuthentication authentication = MqttMsgAuthentication.Parse(42, MqttProtocolVersion.Version_3_1_1, mokChannel);
            });
            Assert.ThrowsException(typeof(NotSupportedException), () =>
            {
                MokChannel mokChannel = new MokChannel(new byte[1] { 42 });
                MqttMsgAuthentication authentication = MqttMsgAuthentication.Parse(42, MqttProtocolVersion.Version_3_1, mokChannel);
            });
        }

        [TestMethod]
        public void AuthenticationBasicDecodeTestsv5()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 47,24,45,21,0,9,83,111,109,101,116,104,105,110,103,22,0,5,1,2,3,4,5,31,0,22,66,
                101,99,97,117,115,101,32,105,116,39,115,32,108,105,107,101,32,116,104,97,116 };
            MokChannel mokChannel = new(encodedCorrect);
            // Act
            MqttMsgAuthentication authentication = MqttMsgAuthentication.Parse(240, MqttProtocolVersion.Version_5, mokChannel);
            // Assert
            Assert.AreEqual(authentication.AuthenticationMethod, "Something");
            CollectionAssert.AreEqual(authentication.AuthenticationData, new byte[] { 1, 2, 3, 4, 5 });
            Assert.AreEqual((byte)authentication.ReasonCode, (byte)MqttReasonCode.ContinueAuthentication);
            Assert.AreEqual(authentication.Reason, "Because it's like that");
        }
    }
}
