// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using nanoFramework.M2Mqtt;
using nanoFramework.M2Mqtt.Messages;
using nanoFramework.TestFramework;

namespace MessageUnitTests
{
    [TestClass]
    public class SubscribeTests
    {
        [TestMethod]
        public void SubscribeEncodeBasicTestv311()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 130, 19, 0, 42, 0, 5, 102, 105, 114, 115, 116, 1, 0, 6, 115, 101, 99, 111, 110, 100, 2 };
            MqttMsgSubscribe subscribe = new();
            subscribe.MessageId = 42;
            subscribe.QoSLevels = new MqttQoSLevel[] { MqttQoSLevel.AtLeastOnce, MqttQoSLevel.ExactlyOnce };
            subscribe.Topics = new string[] { "first", "second" };
            // Act
            byte[] encoded = subscribe.GetBytes(MqttProtocolVersion.Version_3_1_1);
            // Assert
            Assert.Equal(encodedCorrect, encoded);
        }

        [TestMethod]
        public void SubscribeEncodeBasicTestv5()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 130, 20, 0, 42, 0, 0, 5, 102, 105, 114, 115, 116, 1, 0, 6, 115, 101, 99, 111, 110, 100, 2 };
            MqttMsgSubscribe subscribe = new();
            subscribe.MessageId = 42;
            subscribe.QoSLevels = new MqttQoSLevel[] { MqttQoSLevel.AtLeastOnce, MqttQoSLevel.ExactlyOnce };
            subscribe.Topics = new string[] { "first", "second" };
            // Act
            byte[] encoded = subscribe.GetBytes(MqttProtocolVersion.Version_5);
            // Assert
            Assert.Equal(encodedCorrect, encoded);
        }

        [TestMethod]
        public void SubscribeEncodeAvancedTestv5()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 130,249,1,0,42,223,1,11,253,255,255,127,38,0,8,108,111,110,103,32,111,110,101,0,210,
                111,110,101,32,118,101,114,121,32,108,111,110,103,32,111,110,101,32,116,111,32,116,
                101,115,116,32,102,111,114,32,111,110,99,101,32,104,111,119,32,116,104,105,115,32,
                119,105,108,108,32,101,110,99,111,100,101,32,97,110,100,32,109,97,107,101,32,105,
                116,32,114,101,97,108,108,108,108,108,108,121,121,121,121,121,121,121,32,115,111,
                111,111,111,111,111,111,111,111,111,111,111,32,115,111,111,111,111,111,111,111,111,
                111,111,111,111,111,111,32,115,111,111,111,111,111,111,111,111,111,111,111,111,111,
                111,111,111,32,115,111,111,111,111,111,111,111,111,111,32,108,111,110,103,46,32,89,
                101,97,44,32,116,104,97,116,32,115,104,111,117,108,100,32,98,101,32,116,101,115,116,
                101,100,32,102,111,114,32,114,101,97,108,32,105,110,32,116,104,101,32,114,101,97,
                108,32,108,105,102,101,32,97,115,32,119,101,108,108,0,5,102,105,114,115,116,1,0,6,
                115,101,99,111,110,100,2 };
            MqttMsgSubscribe subscribe = new();
            subscribe.MessageId = 42;
            subscribe.QoSLevels = new MqttQoSLevel[] { MqttQoSLevel.AtLeastOnce, MqttQoSLevel.ExactlyOnce };
            subscribe.Topics = new string[] { "first", "second" };
            subscribe.SubscriptionIdentifier = 268435453;
            subscribe.UserProperties.Add(new UserProperty("long one", "one very long one to test for once how this will encode and make it reallllllyyyyyyy soooooooooooo soooooooooooooo soooooooooooooooo sooooooooo long. Yea, that should be tested for real in the real life as well"));
            // Act
            byte[] encoded = subscribe.GetBytes(MqttProtocolVersion.Version_5);
            // Assert
            Assert.Equal(encodedCorrect, encoded);
        }

        [TestMethod]
        public void SubscribeDecodeBasicTestsv311()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 19, 0, 42, 0, 5, 102, 105, 114, 115, 116, 1, 0, 6, 115, 101, 99, 111, 110, 100, 2 };
            MokChannel mokChannel = new MokChannel(encodedCorrect);
            // Act
            MqttMsgSubscribe subscribe = MqttMsgSubscribe.Parse(130, MqttProtocolVersion.Version_3_1_1, mokChannel);
            // Assert
            Assert.Equal(subscribe.MessageId, (ushort)42);
            Assert.Equal(subscribe.QoSLevels, new MqttQoSLevel[] { MqttQoSLevel.AtLeastOnce, MqttQoSLevel.ExactlyOnce });
            Assert.Equal(subscribe.Topics, new string[] { "first", "second" });
        }

        [TestMethod]
        public void SubscribeDecodeBasicTestsv5()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 20, 0, 42, 0, 0, 5, 102, 105, 114, 115, 116, 1, 0, 6, 115, 101, 99, 111, 110, 100, 2 };
            MokChannel mokChannel = new MokChannel(encodedCorrect);
            // Act
            MqttMsgSubscribe subscribe = MqttMsgSubscribe.Parse(130, MqttProtocolVersion.Version_5, mokChannel);
            // Assert
            Assert.Equal(subscribe.MessageId, (ushort)42);
            Assert.Equal(subscribe.QoSLevels, new MqttQoSLevel[] { MqttQoSLevel.AtLeastOnce, MqttQoSLevel.ExactlyOnce });
            Assert.Equal(subscribe.Topics, new string[] { "first", "second" });
        }

        [TestMethod]
        public void SubscribeDecodeAvanceTestsv5()
        {
            // Arrange
            byte[] encodedCorrect = new byte[] { 249,1,0,42,223,1,11,253,255,255,127,38,0,8,108,111,110,103,32,111,110,101,0,210,
                111,110,101,32,118,101,114,121,32,108,111,110,103,32,111,110,101,32,116,111,32,116,
                101,115,116,32,102,111,114,32,111,110,99,101,32,104,111,119,32,116,104,105,115,32,
                119,105,108,108,32,101,110,99,111,100,101,32,97,110,100,32,109,97,107,101,32,105,
                116,32,114,101,97,108,108,108,108,108,108,121,121,121,121,121,121,121,32,115,111,
                111,111,111,111,111,111,111,111,111,111,111,32,115,111,111,111,111,111,111,111,111,
                111,111,111,111,111,111,32,115,111,111,111,111,111,111,111,111,111,111,111,111,111,
                111,111,111,32,115,111,111,111,111,111,111,111,111,111,32,108,111,110,103,46,32,89,
                101,97,44,32,116,104,97,116,32,115,104,111,117,108,100,32,98,101,32,116,101,115,116,
                101,100,32,102,111,114,32,114,101,97,108,32,105,110,32,116,104,101,32,114,101,97,
                108,32,108,105,102,101,32,97,115,32,119,101,108,108,0,5,102,105,114,115,116,1,0,6,
                115,101,99,111,110,100,2 };
            MokChannel mokChannel = new MokChannel(encodedCorrect);
            // Act
            MqttMsgSubscribe subscribe = MqttMsgSubscribe.Parse(130, MqttProtocolVersion.Version_5, mokChannel);
            // Assert
            Assert.Equal(subscribe.MessageId, (ushort)42);
            Assert.Equal(subscribe.QoSLevels, new MqttQoSLevel[] { MqttQoSLevel.AtLeastOnce, MqttQoSLevel.ExactlyOnce });
            Assert.Equal(subscribe.Topics, new string[] { "first", "second" });
            Assert.Equal(subscribe.SubscriptionIdentifier, 268435453);
            var prop = new UserProperty("long one", "one very long one to test for once how this will encode and make it reallllllyyyyyyy soooooooooooo soooooooooooooo soooooooooooooooo sooooooooo long. Yea, that should be tested for real in the real life as well");
            Assert.Equal(((UserProperty)subscribe.UserProperties[0]).Name, prop.Name);
            Assert.Equal(((UserProperty)subscribe.UserProperties[0]).Value, prop.Value);
        }
    }
}
