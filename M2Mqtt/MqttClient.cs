/*
Copyright (c) 2013, 2014 Paolo Patierno

All rights reserved. This program and the accompanying materials
are made available under the terms of the Eclipse Public License v1.0
and Eclipse Distribution License v1.0 which accompany this distribution. 

The Eclipse Public License is available at 
   http://www.eclipse.org/legal/epl-v10.html
and the Eclipse Distribution License is available at 
   http://www.eclipse.org/org/documents/edl-v10.php.

Contributors:
   Paolo Patierno - initial API and implementation and/or initial documentation
   .NET Foundation and Contributors - nanoFramework support
*/

using nanoFramework.M2Mqtt.Exceptions;
using nanoFramework.M2Mqtt.Internal;
using nanoFramework.M2Mqtt.Messages;
using nanoFramework.M2Mqtt.Session;
using nanoFramework.M2Mqtt.Utility;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using MqttUtility = nanoFramework.M2Mqtt.Utility;

namespace nanoFramework.M2Mqtt
{
    /// <summary>
    /// MQTT Client
    /// </summary>
    public class MqttClient
    {
        // broker hostname (or ip address) and port
        private string _brokerHostName;
        private int _brokerPort;

        // running status of threads
        private bool _isRunning;
        // event for raising received message event
        private AutoResetEvent _receiveEventWaitHandle;

        // event for starting process inflight queue asynchronously
        private AutoResetEvent _inflightWaitHandle;

        // event for signaling synchronous receive
        AutoResetEvent _syncEndReceiving;
        // message received
        MqttMsgBase _msgReceived;

        // exception thrown during receiving
        Exception _exReceiving;

        // keep alive period (in ms)
        private int _keepAlivePeriod;
        // events for signaling on keep alive thread
        private AutoResetEvent _keepAliveEvent;
        private AutoResetEvent _keepAliveEventEnd;
        // last communication time in ticks
        private long _lastCommTime;
        // channel to communicate over the network
        private IMqttNetworkChannel _channel;

        // inflight messages queue
        private Queue _inflightQueue;
        // internal queue for received messages about inflight messages
        private Queue _internalQueue;
        // internal queue for dispatching events
        private Queue _eventQueue;
        // internal table of truth for QoS1 and QoS2
        private ArrayList _waitingForAnswer;
        // session
        private MqttClientSession _session;

        // reference to avoid access to singleton via property
        private MqttSettings _settings;

        // current message identifier generated
        private ushort _messageIdCounter = 0;

        // connection is closing due to peer
        private bool _isConnectionClosing;

        /// <summary>
        /// Delegate that defines event handler for PUBLISH message received
        /// </summary>
        public delegate void MqttMsgPublishEventHandler(object sender, MqttMsgPublishEventArgs e);

        /// <summary>
        /// Delegate that defines event handler for published message
        /// </summary>
        public delegate void MqttMsgPublishedEventHandler(object sender, MqttMsgPublishedEventArgs e);

        /// <summary>
        /// Delegate that defines event handler for subscribed topic
        /// </summary>
        public delegate void MqttMsgSubscribedEventHandler(object sender, MqttMsgSubscribedEventArgs e);

        /// <summary>
        /// Delegate that defines event handler for unsubscribed topic
        /// </summary>
        public delegate void MqttMsgUnsubscribedEventHandler(object sender, MqttMsgUnsubscribedEventArgs e);

        /// <summary>
        /// Delegate that defines event handler for authentication topic, v5.0 only
        /// </summary>
        public delegate void MqttMsgAuthenticationEventHandler(object sender, MqttMsgAuthenticationEventArgs e);

        /// <summary>
        /// Delegate that defines event handler for unsubscribed topic
        /// </summary>
        public delegate void ConnectionOpenedEventHandler(object sender, ConnectionOpenedEventArgs e);

        /// <summary>
        /// Delegate that defines event handler for client/peer disconnection
        /// </summary>
        public delegate void ConnectionClosedEventHandler(object sender, EventArgs e);

        /// <summary>
        /// Delegate that defines event handler for server disconnection request
        /// </summary>
        public delegate void ConnectionClosedRequestEventHandler(object sender, ConnectionClosedRequestEventArgs e);

        /// <summary>
        /// The event for PUBLISH message received
        /// </summary>
        public event MqttMsgPublishEventHandler MqttMsgPublishReceived;
        /// <summary>
        /// The event for published message
        /// </summary>
        public event MqttMsgPublishedEventHandler MqttMsgPublished;
        /// <summary>
        /// The event for subscribed topic
        /// </summary>
        public event MqttMsgSubscribedEventHandler MqttMsgSubscribed;
        /// <summary>
        /// The event for unsubscribed topic
        /// </summary>
        public event MqttMsgUnsubscribedEventHandler MqttMsgUnsubscribed;

        /// <summary>
        /// The event for peer/client disconnection
        /// </summary>
        public event ConnectionOpenedEventHandler ConnectionOpened;

        /// <summary>
        /// The event for peer/client disconnection
        /// </summary>
        public event ConnectionClosedEventHandler ConnectionClosed;

        /// <summary>
        /// The event for peer/client disconnection
        /// </summary>
        public event ConnectionClosedRequestEventHandler ConnectionClosedRequest;

        /// <summary>
        /// The event for peer/client disconnection
        /// </summary>
        public event MqttMsgAuthenticationEventHandler Authentication;

        /// <summary>
        /// Connection status between client and broker
        /// </summary>
        public bool IsConnected { get; private set; }

        /// <summary>
        /// Client identifier
        /// </summary>
        public string ClientId { get; private set; }

        /// <summary>
        /// Clean session flag
        /// </summary>
        public bool CleanSession { get; private set; }

        /// <summary>
        /// Will flag
        /// </summary>
        public bool WillFlag { get; private set; }

        /// <summary>
        /// Will QOS level
        /// </summary>
        public MqttQoSLevel WillQosLevel { get; private set; }

        /// <summary>
        /// Will topic
        /// </summary>
        public string WillTopic { get; private set; }

        /// <summary>
        /// Will message
        /// </summary>
        public string WillMessage { get; private set; }

        /// <summary>
        /// MQTT protocol version
        /// </summary>
        public MqttProtocolVersion ProtocolVersion { get; set; }

        /// <summary>
        /// Authentication Method, v5.0 only
        /// </summary>
        public string AuthenticationMethod { get; set; }

        /// <summary>
        /// Authentication Data, v5.0 only
        /// </summary>
        public byte[] AuthenticationData { get; set; }

        /// <summary>
        /// Session Expiry Interval, v5.0 only
        /// </summary>
        public uint SessionExpiryInterval { get; set; }

        /// <summary>
        /// Request Problem Information, v5.0 only
        /// </summary>
        public bool RequestProblemInformation { get; set; }

        /// <summary>
        /// Request Response Information, v5.0 only
        /// </summary>
        public bool RequestResponseInformation { get; set; }

        /// <summary>
        /// ReceiveMaximum, v5.0 only
        /// </summary>
        public ushort ReceiveMaximum { get; set; } = ushort.MaxValue;

        /// <summary>
        /// TopicAliasMaximum, v5.0 only
        /// </summary>
        public ushort TopicAliasMaximum { get; set; }

        /// <summary>
        /// Will Delay Interval, v5.0 only
        /// </summary>
        public uint WillDelayInterval { get; set; }

        /// <summary>
        /// User Property, v5.0 only
        /// </summary>
        public ArrayList UserProperties { get; internal set; } = new ArrayList();

        /// <summary>
        /// Maximum Packet Size, v5.0 only
        /// </summary>
        public uint MaximumPacketSize { get; set; }

        /// <summary>
        /// Set to true if you have an authentication flow, v5.0 only
        /// </summary>
        public bool IsAuthenticationFlow { get; set; } = false;

        /// <summary>
        /// MQTT client settings
        /// </summary>
        public MqttSettings Settings => _settings;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="brokerHostName">Broker Host Name or IP Address</param>
        public MqttClient(string brokerHostName) :
            this(brokerHostName, MqttSettings.BrokerDefaultPort, false, null, null, MqttSslProtocols.None)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="brokerHostName">Broker Host Name or IP Address</param>
        /// <param name="brokerPort">Broker port</param>
        /// <param name="secure">Using secure connection</param>
        /// <param name="caCert">CA certificate for secure connection</param>
        /// <param name="clientCert">Client certificate</param>
        /// <param name="sslProtocol">SSL/TLS protocol</param>
        public MqttClient(string brokerHostName, int brokerPort, bool secure, X509Certificate caCert, X509Certificate clientCert, MqttSslProtocols sslProtocol)
        {
            Init(brokerHostName, brokerPort, secure, caCert, clientCert, sslProtocol);
        }

        /// <summary>
        /// MqttClient initialization
        /// </summary>
        /// <param name="brokerHostName">Broker Host Name or IP Address</param>
        /// <param name="brokerPort">Broker port</param>
        /// <param name="secure">>Using secure connection</param>
        /// <param name="caCert">CA certificate for secure connection</param>
        /// <param name="clientCert">Client certificate</param>
        /// <param name="sslProtocol">SSL/TLS protocol version</param>
        private void Init(string brokerHostName, int brokerPort, bool secure, X509Certificate caCert, X509Certificate clientCert, MqttSslProtocols sslProtocol)
        {
            // set default MQTT protocol version (default is 3.1.1)
            ProtocolVersion = MqttProtocolVersion.Version_3_1_1;
            _brokerHostName = brokerHostName;
            _brokerPort = brokerPort;

            // reference to MQTT settings
            _settings = MqttSettings.Instance;
            // set settings port based on secure connection or not
            if (!secure)
            {
                _settings.Port = _brokerPort;
            }
            else
            {
                _settings.SslPort = _brokerPort;
            }

            _syncEndReceiving = new AutoResetEvent(false);
            _keepAliveEvent = new AutoResetEvent(false);

            // queue for handling inflight messages (publishing and acknowledge)
            _inflightWaitHandle = new AutoResetEvent(false);
            _inflightQueue = new Queue();

            // queue for received message
            _receiveEventWaitHandle = new AutoResetEvent(false);
            _eventQueue = new Queue();
            _internalQueue = new Queue();
            _waitingForAnswer = new ArrayList();

            // session
            _session = null;

            // create network channel
            _channel = new MqttNetworkChannel(_brokerHostName, _brokerPort, secure, caCert, clientCert, sslProtocol);
        }

        /// <summary>
        /// Connect to broker
        /// </summary>
        /// <param name="clientId">Client identifier</param>
        /// <returns>Return code of CONNACK message from broker</returns>
        public MqttReasonCode Connect(string clientId)
        {
            return Connect(clientId, null, null, false, MqttQoSLevel.AtMostOnce, false, null, null, true, MqttMsgConnect.KEEP_ALIVE_PERIOD_DEFAULT);
        }

        /// <summary>
        /// Connect to broker
        /// </summary>
        /// <param name="clientId">Client identifier</param>
        /// <param name="cleanSession">Clean sessione flag</param>
        /// <returns>Return code of CONNACK message from broker</returns>
        public MqttReasonCode Connect(string clientId,
            bool cleanSession)
        {
            return Connect(clientId, null, null, false, MqttQoSLevel.AtMostOnce, false, null, null, cleanSession, MqttMsgConnect.KEEP_ALIVE_PERIOD_DEFAULT);
        }

        /// <summary>
        /// Connect to broker
        /// </summary>
        /// <param name="clientId">Client identifier</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <returns>Return code of CONNACK message from broker</returns>
        public MqttReasonCode Connect(string clientId,
            string username,
            string password)
        {
            return Connect(clientId, username, password, false, MqttQoSLevel.AtMostOnce, false, null, null, true, MqttMsgConnect.KEEP_ALIVE_PERIOD_DEFAULT);
        }

        /// <summary>
        /// Connect to broker
        /// </summary>
        /// <param name="clientId">Client identifier</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="cleanSession">Clean sessione flag</param>
        /// <param name="keepAlivePeriod">Keep alive period</param>
        /// <returns>Return code of CONNACK message from broker</returns>
        public MqttReasonCode Connect(string clientId,
            string username,
            string password,
            bool cleanSession,
            ushort keepAlivePeriod)
        {
            return Connect(clientId, username, password, false, MqttQoSLevel.AtMostOnce, false, null, null, cleanSession, keepAlivePeriod);
        }

        /// <summary>
        /// Connect to broker
        /// </summary>
        /// <param name="clientId">Client identifier</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="willRetain">Will retain flag</param>
        /// <param name="willQosLevel">Will QOS level</param>
        /// <param name="willFlag">Will flag</param>
        /// <param name="willTopic">Will topic</param>
        /// <param name="willMessage">Will message</param>
        /// <param name="cleanSession">Clean sessione flag</param>
        /// <param name="keepAlivePeriod">Keep alive period</param>
        /// <returns>Return code of CONNACK message from broker</returns>
        public MqttReasonCode Connect(string clientId,
            string username,
            string password,
            bool willRetain,
            MqttQoSLevel willQosLevel,
            bool willFlag,
            string willTopic,
            string willMessage,
            bool cleanSession,
            ushort keepAlivePeriod)
        {
            // create CONNECT message
            MqttMsgConnect connect = new MqttMsgConnect(clientId,
                username,
                password,
                willRetain,
                willQosLevel,
                willFlag,
                willTopic,
                willMessage,
                cleanSession,
                keepAlivePeriod,
                ProtocolVersion);

            try
            {
                // Set the certificate check
                _channel.ValidateServerCertificate = _settings.ValidateServerCertificate;
                // connect to the broker
                _channel.Connect();
            }
            catch (SocketException ex)
            {
                throw new MqttConnectionException($"Socket exception connecting to the broker: {ex.ErrorCode}", ex);
            }
            catch (Exception ex)
            {
                throw new MqttConnectionException("Exception connecting to the broker", ex);
            }

            _lastCommTime = 0;
            _isRunning = true;
            _isConnectionClosing = false;
            // start thread for receiving messages from broker
            Fx.StartThread(ReceiveThread);

            // Set the properties for v5
            if (ProtocolVersion == MqttProtocolVersion.Version_5)
            {
                connect.AuthenticationData = AuthenticationData;
                connect.AuthenticationMethod = AuthenticationMethod;
                connect.MaximumPacketSize = MaximumPacketSize;
                connect.ReceiveMaximum = ReceiveMaximum;
                connect.RequestProblemInformation = RequestProblemInformation;
                connect.RequestResponseInformation = RequestResponseInformation;
                connect.SessionExpiryInterval = SessionExpiryInterval;
                connect.TopicAliasMaximum = TopicAliasMaximum;
                connect.UserProperties = UserProperties;
            }

            MqttMsgConnack connack;
            // In v5 case the authentication method is not empty, we will wait for that AUTH message
            // The client **must** subscribe to the Authentication event and mange this.
            if (IsAuthenticationFlow)
            {
                Send(connect);
                connack = new MqttMsgConnack();
                connack.ReturnCode = MqttReasonCode.Success;
            }
            else
            {
                connack = (MqttMsgConnack)SendReceive(connect);
            }
            // if connection accepted, start keep alive timer and 
            if (connack.ReturnCode == MqttReasonCode.Success)
            {
                // set all client properties
                ClientId = clientId;
                CleanSession = cleanSession;
                WillFlag = willFlag;
                WillTopic = willTopic;
                WillMessage = willMessage;
                WillQosLevel = willQosLevel;

                _keepAlivePeriod = keepAlivePeriod * 1000; // convert in ms

                // restore previous session
                RestoreSession();

                // keep alive period equals zero means turning off keep alive mechanism
                if (_keepAlivePeriod != 0)
                {
                    // start thread for sending keep alive message to the broker
                    Fx.StartThread(KeepAliveThread);
                }

                // start thread for raising received message event from broker
                Fx.StartThread(DispatchEventThread);

                // start thread for handling inflight messages queue to broker asynchronously (publish and acknowledge)
                Fx.StartThread(ProcessInflightThread);

                if (!IsAuthenticationFlow)
                {
                    IsConnected = true;
                }
            }

            return connack.ReturnCode;

        }

        /// <summary>
        /// Disconnect from broker
        /// </summary>
        public void Disconnect()
        {
            MqttMsgDisconnect disconnect = new MqttMsgDisconnect();
            Send(disconnect);

            // close client
            OnConnectionClosing();
        }

        /// <summary>
        /// Close client
        /// </summary>
        private void Close()
        {
            // stop receiving thread
            _isRunning = false;

            // wait end receive event thread
            if (_receiveEventWaitHandle != null)
            {
                _receiveEventWaitHandle.Set();
            }

            // wait end process inflight thread
            if (_inflightWaitHandle != null)
            {
                _inflightWaitHandle.Set();
            }

            // unlock keep alive thread and wait
            _keepAliveEvent.Set();

            if (_keepAliveEventEnd != null)
            {
                _keepAliveEventEnd.WaitOne();
            }

            // clear all queues
            _inflightQueue.Clear();
            _internalQueue.Clear();
            _waitingForAnswer.Clear();
            _eventQueue.Clear();

            // close network channel
            _channel.Close();

            IsConnected = false;
        }

        /// <summary>
        /// Execute ping to broker for keep alive
        /// </summary>
        /// <returns>PINGRESP message from broker</returns>
        private MqttMsgPingResp Ping()
        {
            MqttMsgPingReq pingreq = new MqttMsgPingReq();
            try
            {
                // broker must send PINGRESP within timeout equal to keep alive period
                return (MqttMsgPingResp)SendReceive(pingreq, _keepAlivePeriod);
            }
#if DEBUG
            catch (Exception e)
            {

                Debug.WriteLine($"Exception occurred: {e}");
#else
            catch (Exception)
            {
#endif
                // client must close connection
                OnConnectionClosing();
                return null;
            }
        }

        /// <summary>
        /// Subscribe for message topics
        /// </summary>
        /// <param name="topics">List of topics to subscribe</param>
        /// <param name="qosLevels">QOS levels related to topics</param>
        /// <returns>Message Id related to SUBSCRIBE message</returns>
        public ushort Subscribe(string[] topics, MqttQoSLevel[] qosLevels)
        {
            MqttMsgSubscribe subscribe =
                new MqttMsgSubscribe(topics, qosLevels)
                {
                    MessageId = GetMessageId()
                };

            // enqueue subscribe request into the inflight queue
            EnqueueInflight(subscribe, MqttMsgFlow.ToPublish);

            return subscribe.MessageId;
        }

        /// <summary>
        /// Unsubscribe for message topics
        /// </summary>
        /// <param name="topics">List of topics to unsubscribe</param>
        /// <returns>Message Id in UNSUBACK message from broker</returns>
        public ushort Unsubscribe(string[] topics)
        {
            MqttMsgUnsubscribe unsubscribe =
                new MqttMsgUnsubscribe(topics)
                {
                    MessageId = GetMessageId()
                };

            // enqueue unsubscribe request into the inflight queue
            EnqueueInflight(unsubscribe, MqttMsgFlow.ToPublish);

            return unsubscribe.MessageId;
        }

        /// <summary>
        /// Publish a message asynchronously (QoS Level 0 and not retained)
        /// </summary>
        /// <param name="topic">Message topic</param>
        /// <param name="message">Message data (payload)</param>
        /// <returns>Message Id related to PUBLISH message</returns>
        public ushort Publish(string topic, byte[] message)
        {
            return Publish(topic, message, MqttQoSLevel.AtMostOnce, false);
        }

        /// <summary>
        /// Publish a message asynchronously
        /// </summary>
        /// <param name="topic">Message topic</param>
        /// <param name="message">Message data (payload)</param>
        /// <param name="qosLevel">QoS Level</param>
        /// <param name="retain">Retain flag</param>
        /// <returns>Message Id related to PUBLISH message</returns>
        public ushort Publish(string topic, byte[] message, MqttQoSLevel qosLevel, bool retain)
        {
            MqttMsgPublish publish =
                    new MqttMsgPublish(topic, message, false, qosLevel, retain)
                    {
                        MessageId = GetMessageId()
                    };

            // enqueue message to publish into the inflight queue
            if (publish.QosLevel != MqttQoSLevel.AtMostOnce)
            {
                lock (_waitingForAnswer)
                {
                    _waitingForAnswer.Add(publish.MessageId);
                }
            }

            bool enqueue = EnqueueInflight(publish, MqttMsgFlow.ToPublish);

            // message enqueued
            if (enqueue)
            {
                return publish.MessageId;
            }
            // infligh queue full, message not enqueued
            else
            {
                throw new MqttClientException(MqttClientErrorCode.InflightQueueFull);
            }
        }

        /// <summary>
        /// Wrapper method for raising events
        /// </summary>
        /// <param name="internalEvent">Internal event</param>
        private void OnInternalEvent(InternalEvent internalEvent)
        {
            lock (_eventQueue)
            {
                _eventQueue.Enqueue(internalEvent);
            }

            _receiveEventWaitHandle.Set();
        }

        /// <summary>
        /// Wrapper method for raising closing connection event
        /// </summary>
        private void OnConnectionClosing()
        {
            if (!_isConnectionClosing)
            {
                _isConnectionClosing = true;
                _receiveEventWaitHandle.Set();
            }
        }

        /// <summary>
        /// Wrapper method for raising PUBLISH message received event
        /// </summary>
        /// <param name="publish">PUBLISH message received</param>
        private void OnMqttMsgPublishReceived(MqttMsgPublish publish)
        {
            MqttMsgPublishReceived?.Invoke(this,
                new MqttMsgPublishEventArgs(publish.Topic, publish.Message, publish.DupFlag, publish.QosLevel, publish.Retain));
        }

        /// <summary>
        /// Wrapper method for raising published message event
        /// </summary>
        /// <param name="messageId">Message identifier for published message</param>
        /// <param name="isPublished">Publish flag</param>
        private void OnMqttMsgPublished(ushort messageId, bool isPublished)
        {
            MqttMsgPublished?.Invoke(this,
                new MqttMsgPublishedEventArgs(messageId, isPublished));
        }

        /// <summary>
        /// Wrapper method for raising subscribed topic event
        /// </summary>
        /// <param name="suback">SUBACK message received</param>
        private void OnMqttMsgSubscribed(MqttMsgSuback suback)
        {
            MqttMsgSubscribed?.Invoke(this,
                new MqttMsgSubscribedEventArgs(suback.MessageId, suback.GrantedQoSLevels));

        }

        /// <summary>
        /// Wrapper method for raising unsubscribed topic event
        /// </summary>
        /// <param name="messageId">Message identifier for unsubscribed topic</param>
        private void OnMqttMsgUnsubscribed(ushort messageId)
        {
            MqttMsgUnsubscribed?.Invoke(this,
                new MqttMsgUnsubscribedEventArgs(messageId));
        }

        private void OnMqttMsgAuthentication(MqttMsgAuthentication authentication)
        {
            Authentication?.Invoke(this, new MqttMsgAuthenticationEventArgs(authentication));
        }

        private void OnMqttMsgConnack(MqttMsgConnack connack)
        {
            ConnectionOpened?.Invoke(this, new ConnectionOpenedEventArgs(connack));
        }

        /// <summary>
        /// Wrapper method for peer/client disconnection
        /// </summary>
        private void OnConnectionClosed()
        {
            ConnectionClosed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Send a message
        /// </summary>
        /// <param name="msgBytes">Message bytes</param>
        private void Send(byte[] msgBytes)
        {
            try
            {
                // send message
                _channel.Send(msgBytes);

                // update last message sent ticks
                _lastCommTime = Environment.TickCount64;
            }
            catch (Exception e)
            {
#if DEBUG
                Debug.WriteLine($"Exception occurred: {e}");
#endif

                throw new MqttCommunicationException(e);
            }
        }

        /// <summary>
        /// Send a message
        /// </summary>
        /// <param name="msg">Message</param>
        private void Send(MqttMsgBase msg)
        {
#if DEBUG
            Debug.WriteLine($"SEND {msg}");
#endif
            Send(msg.GetBytes(ProtocolVersion));
        }

        /// <summary>
        /// Send a message to the broker and wait answer
        /// </summary>
        /// <param name="msgBytes">Message bytes</param>
        /// <param name="timeout">Timeout for receiving answer</param>
        /// <returns>MQTT message response</returns>
        private MqttMsgBase SendReceive(byte[] msgBytes, int timeout)
        {
            // reset handle before sending
            _syncEndReceiving.Reset();
            try
            {
                // send message
                _channel.Send(msgBytes);

                // update last message sent ticks
                _lastCommTime = Environment.TickCount64;
            }
            catch (Exception e)
            {
#if DEBUG
                Debug.WriteLine($"Exception occurred: {e}");
#endif

                throw new MqttCommunicationException(e);
            }

            // wait for answer from broker
            if (_syncEndReceiving.WaitOne(timeout, false))
            {
                // message received without exception
                if (_exReceiving == null)
                {
                    return _msgReceived;
                }
                // receiving thread catched exception
                else
                {
                    throw _exReceiving;
                }
            }
            else
            {
                // throw timeout exception
                throw new MqttCommunicationException();
            }
        }

        /// <summary>
        /// Send a message to the broker and wait answer
        /// </summary>
        /// <param name="msg">Message</param>
        /// <returns>MQTT message response</returns>
        private MqttMsgBase SendReceive(MqttMsgBase msg)
        {
            return SendReceive(msg, MqttSettings.DefaultTimeout);
        }

        /// <summary>
        /// Send a message to the broker and wait answer
        /// </summary>
        /// <param name="msg">Message</param>
        /// <param name="timeout">Timeout for receiving answer</param>
        /// <returns>MQTT message response</returns>
        private MqttMsgBase SendReceive(MqttMsgBase msg, int timeout)
        {
#if DEBUG
            Debug.WriteLine($"SENDRCV {msg}");
#endif
            return SendReceive(msg.GetBytes(ProtocolVersion), timeout);
        }

        /// <summary>
        /// Enqueue a message into the inflight queue
        /// </summary>
        /// <param name="msg">Message to enqueue</param>
        /// <param name="flow">Message flow (publish, acknowledge)</param>
        /// <returns>Message enqueued or not</returns>
        private bool EnqueueInflight(MqttMsgBase msg, MqttMsgFlow flow)
        {
            // enqueue is needed (or not)
            bool enqueue = true;

            // Authentication is special flow and can reproduce
            if ((flow == MqttMsgFlow.ToPublish) && (msg.Type == MqttMessageType.Authentication))
            {
                MqttMsgContext msgContext = new MqttMsgContext()
                {
                    Message = msg,
                    State = MqttMsgState.WaitForAuthentication,
                    Flow = flow,
                    Attempt = 0
                };
                lock (_inflightQueue)
                {
                    // enqueue message and unlock send thread
                    _inflightQueue.Enqueue(msgContext);
                }
#if DEBUG
                Debug.WriteLine($"enqueued authentication message {msg}");
#endif

                _inflightWaitHandle.Set();
                return enqueue;
            }

            // if it is a PUBLISH message with QoS Level 2
            if ((msg.Type == MqttMessageType.Publish) &&
                (msg.QosLevel == MqttQoSLevel.ExactlyOnce))
            {
                // if it is a PUBLISH message already received (it is in the inflight queue), the publisher
                // re-sent it because it didn't received the PUBREC. In this case, we have to re-send PUBREC

                // NOTE : I need to find on message id and flow because the broker could be publish/received
                //        to/from client and message id could be the same (one tracked by broker and the other by client)
                MqttMsgContextFinder msgCtxFinder = new MqttMsgContextFinder(msg.MessageId, MqttMsgFlow.ToAcknowledge);
                MqttMsgContext msgCtx = null;
                lock (_inflightQueue)
                {
                    msgCtx = (MqttMsgContext)_inflightQueue.Get(msgCtxFinder.Find);
                }

                // the PUBLISH message is alredy in the inflight queue, we don't need to re-enqueue but we need
                // to change state to re-send PUBREC
                if (msgCtx != null)
                {
                    msgCtx.State = MqttMsgState.QueuedQos2;
                    msgCtx.Flow = MqttMsgFlow.ToAcknowledge;
                    enqueue = false;
                }
            }

            if (enqueue)
            {
                // set a default state
                MqttMsgState state = MqttMsgState.QueuedQos0;

                // based on QoS level, the messages flow between broker and client changes
                switch (msg.QosLevel)
                {
                    // QoS Level 0
                    case MqttQoSLevel.AtMostOnce:

                        state = MqttMsgState.QueuedQos0;
                        break;

                    // QoS Level 1
                    case MqttQoSLevel.AtLeastOnce:

                        state = MqttMsgState.QueuedQos1;
                        break;

                    // QoS Level 2
                    case MqttQoSLevel.ExactlyOnce:

                        state = MqttMsgState.QueuedQos2;
                        break;
                }

                // [v3.1.1] SUBSCRIBE and UNSUBSCRIBE aren't "officially" QOS = 1
                //          so QueuedQos1 state isn't valid for them
                if (msg.Type == MqttMessageType.Subscribe)
                {
                    state = MqttMsgState.SendSubscribe;
                }
                else if (msg.Type == MqttMessageType.Unsubscribe)
                {
                    state = MqttMsgState.SendUnsubscribe;
                }

                // queue message context
                MqttMsgContext msgContext = new MqttMsgContext()
                {
                    Message = msg,
                    State = state,
                    Flow = flow,
                    Attempt = 0,
                    Timestamp = Environment.TickCount64
                };

                lock (_inflightQueue)
                {
                    // check number of messages inside inflight queue 
                    enqueue = (_inflightQueue.Count < _settings.InflightQueueSize);
#if DEBUG
                    Debug.WriteLine($"_inflightQueue.Count  {_inflightQueue.Count }");
#endif
                }

#if DEBUG
                lock (_internalQueue)
                {
                    Debug.WriteLine($"_internalQueue.Count  {_internalQueue.Count}");
                }
#endif

                if (enqueue)
                {
                    lock (_inflightQueue)
                    {
                        // enqueue message and unlock send thread
                        _inflightQueue.Enqueue(msgContext);
                    }

#if DEBUG
                    Debug.WriteLine($"enqueued {msg}");
#endif
                    // PUBLISH message
                    if (msg.Type == MqttMessageType.Publish)
                    {
                        // to publish and QoS level 1 or 2
                        if ((msgContext.Flow == MqttMsgFlow.ToPublish) &&
                            ((msg.QosLevel == MqttQoSLevel.AtLeastOnce) ||
                             (msg.QosLevel == MqttQoSLevel.ExactlyOnce)))
                        {
                            if (_session != null)
                            {
                                _session.InflightMessages.Add(msgContext.Key, msgContext);
                            }
                        }
                        // to acknowledge and QoS level 2
                        else if ((msgContext.Flow == MqttMsgFlow.ToAcknowledge) &&
                                 (msg.QosLevel == MqttQoSLevel.ExactlyOnce))
                        {
                            if (_session != null)
                            {
                                _session.InflightMessages.Add(msgContext.Key, msgContext);
                            }
                        }
                    }
                }

            }

            _inflightWaitHandle.Set();

            return enqueue;
        }

        /// <summary>
        /// Enqueue a message into the internal queue
        /// </summary>
        /// <param name="msg">Message to enqueue</param>
        private void EnqueueInternal(MqttMsgBase msg)
        {
            // enqueue is needed (or not)
            bool enqueue = true;

            lock (_waitingForAnswer)
            {
                if (_waitingForAnswer.Contains(msg.MessageId))
                {
                    enqueue = false;
                    _waitingForAnswer.Remove(msg.MessageId);
                    // Send the event as when here, it won't be enqueued.
                    if (msg.Type == MqttMessageType.PublishAck)
                    {
                        OnInternalEvent(new MsgInternalEvent(msg));
                        return;
                    }
                }
            }

            // if it is a PUBREL message (for QoS Level 2)
            if (msg.Type == MqttMessageType.PublishRelease)
            {
                // if it is a PUBREL but the corresponding PUBLISH isn't in the inflight queue,
                // it means that we processed PUBLISH message and received PUBREL and we sent PUBCOMP
                // but publisher didn't receive PUBCOMP so it re-sent PUBREL. We need only to re-send PUBCOMP.

                // NOTE : I need to find on message id and flow because the broker could be publish/received
                //        to/from client and message id could be the same (one tracked by broker and the other by client)
                MqttMsgContextFinder msgCtxFinder = new MqttMsgContextFinder(msg.MessageId, MqttMsgFlow.ToAcknowledge);
                MqttMsgContext msgCtx = null;
                lock (_inflightQueue)
                {
                    msgCtx = (MqttMsgContext)_inflightQueue.Get(msgCtxFinder.Find);
                }
                // the PUBLISH message isn't in the inflight queue, it was already processed so
                // we need to re-send PUBCOMP only
                if (msgCtx == null)
                {
                    MqttMsgPubcomp pubcomp = new MqttMsgPubcomp
                    {
                        MessageId = msg.MessageId
                    };

                    Send(pubcomp);

                    enqueue = false;
                }
            }
            // if it is a PUBCOMP message (for QoS Level 2)
            else if (msg.Type == MqttMessageType.PublishComplete)
            {
                // if it is a PUBCOMP but the corresponding PUBLISH isn't in the inflight queue,
                // it means that we sent PUBLISH message, sent PUBREL (after receiving PUBREC) and already received PUBCOMP
                // but publisher didn't receive PUBREL so it re-sent PUBCOMP. We need only to ignore this PUBCOMP.

                // NOTE : I need to find on message id and flow because the broker could be publish/received
                //        to/from client and message id could be the same (one tracked by broker and the other by client)
                MqttMsgContextFinder msgCtxFinder = new MqttMsgContextFinder(msg.MessageId, MqttMsgFlow.ToPublish);
                MqttMsgContext msgCtx = null;
                lock (_inflightQueue)
                {
                    msgCtx = (MqttMsgContext)_inflightQueue.Get(msgCtxFinder.Find);
                }
                // the PUBLISH message isn't in the inflight queue, it was already sent so we need to ignore this PUBCOMP
                if (msgCtx == null)
                {
                    enqueue = false;
                }
            }
            // if it is a PUBREC message (for QoS Level 2)
            else if (msg.Type == MqttMessageType.PublishReceived)
            {
                // if it is a PUBREC but the corresponding PUBLISH isn't in the inflight queue,
                // it means that we sent PUBLISH message more times (retries) but broker didn't send PUBREC in time
                // the publish is failed and we need only to ignore this PUBREC.

                // NOTE : I need to find on message id and flow because the broker could be publish/received
                //        to/from client and message id could be the same (one tracked by broker and the other by client)
                MqttMsgContextFinder msgCtxFinder = new MqttMsgContextFinder(msg.MessageId, MqttMsgFlow.ToPublish);
                MqttMsgContext msgCtx = null;
                lock (_inflightQueue)
                {
                    msgCtx = (MqttMsgContext)_inflightQueue.Get(msgCtxFinder.Find);
                }
                // the PUBLISH message isn't in the inflight queue, it was already sent so we need to ignore this PUBREC
                if (msgCtx == null)
                {
                    enqueue = false;
                }
            }

            if (enqueue)
            {
                lock (_internalQueue)
                {
                    _internalQueue.Enqueue(msg);
                }

#if DEBUG
                Debug.WriteLine($"enqueued {msg}");
#endif
                _inflightWaitHandle.Set();
            }
        }

        /// <summary>
        /// Thread for receiving messages
        /// </summary>
        private void ReceiveThread()
        {
            int readBytes = 0;
            byte[] fixedHeaderFirstByte = new byte[1];
            MqttMessageType msgType = MqttMessageType.Connect;

            while (_isRunning)
            {
                try
                {
                    // read first byte (fixed header)
                    readBytes = _channel.Receive(fixedHeaderFirstByte);

                    if (readBytes > 0)
                    {
                        // extract message type from received byte
                        msgType = (MqttMessageType)((fixedHeaderFirstByte[0] & MqttMsgBase.MSG_TYPE_MASK) >> MqttMsgBase.MSG_TYPE_OFFSET);
#if DEBUG
                        Debug.WriteLine($"RECV msgtype: {msgType}");
#endif   

                        switch (msgType)
                        {
                            // CONNECT message received
                            case MqttMessageType.Connect:
                                throw new MqttClientException(MqttClientErrorCode.WrongBrokerMessage);

                            // CONNACK message received
                            case MqttMessageType.ConnectAck:

                                _msgReceived = MqttMsgConnack.Parse(fixedHeaderFirstByte[0], ProtocolVersion, _channel);
                                var connack = (MqttMsgConnack)_msgReceived;
                                // In this case we're not following the normal behavior as there can be authentication in the mean time.
                                if ((connack.ReturnCode == MqttReasonCode.Success) && IsAuthenticationFlow)
                                {
                                    // restore previous session
                                    RestoreSession();

                                    IsConnected = true;
                                }
#if DEBUG
                                Debug.WriteLine($"RECV {_msgReceived}");
#endif                                
                                OnMqttMsgConnack(connack);
                                _syncEndReceiving.Set();
                                break;

                            // PINGREQ message received
                            case MqttMessageType.PingRequest:
                                throw new MqttClientException(MqttClientErrorCode.WrongBrokerMessage);

                            // PINGRESP message received
                            case MqttMessageType.PingResponse:

                                _msgReceived = MqttMsgPingResp.Parse(fixedHeaderFirstByte[0], ProtocolVersion, _channel);
                                _internalQueue.Clear();
                                _session?.Clear();
                                _waitingForAnswer.Clear();
#if DEBUG
                                Debug.WriteLine($"RECV {_msgReceived}");
#endif
                                _syncEndReceiving.Set();
                                break;

                            // SUBSCRIBE message received
                            case MqttMessageType.Subscribe:
                                throw new MqttClientException(MqttClientErrorCode.WrongBrokerMessage);

                            // SUBACK message received
                            case MqttMessageType.SubscribeAck:
                                // enqueue SUBACK message received (for QoS Level 1) into the internal queue
                                MqttMsgSuback suback = MqttMsgSuback.Parse(fixedHeaderFirstByte[0], ProtocolVersion, _channel);
#if DEBUG
                                Debug.WriteLine($"RECV {suback}");
#endif
                                // enqueue SUBACK message into the internal queue
                                EnqueueInternal(suback);

                                break;

                            // PUBLISH message received
                            case MqttMessageType.Publish:

                                MqttMsgPublish publish = MqttMsgPublish.Parse(fixedHeaderFirstByte[0], ProtocolVersion, _channel);
#if DEBUG
                                Debug.WriteLine($"RECV {publish}");
#endif
                                // enqueue PUBLISH message to acknowledge into the inflight queue
                                EnqueueInflight(publish, MqttMsgFlow.ToAcknowledge);

                                break;

                            // PUBACK message received
                            case MqttMessageType.PublishAck:

                                // enqueue PUBACK message received (for QoS Level 1) into the internal queue
                                MqttMsgPuback puback = MqttMsgPuback.Parse(fixedHeaderFirstByte[0], ProtocolVersion, _channel);
#if DEBUG
                                Debug.WriteLine($"RECV {puback}");
#endif
                                // enqueue PUBACK message into the internal queue
                                EnqueueInternal(puback);

                                break;

                            // PUBREC message received
                            case MqttMessageType.PublishReceived:

                                // enqueue PUBREC message received (for QoS Level 2) into the internal queue
                                MqttMsgPubrec pubrec = MqttMsgPubrec.Parse(fixedHeaderFirstByte[0], ProtocolVersion, _channel);
#if DEBUG
                                Debug.WriteLine($"RECV {pubrec}");
#endif
                                // enqueue PUBREC message into the internal queue
                                EnqueueInternal(pubrec);

                                break;

                            // PUBREL message received
                            case MqttMessageType.PublishRelease:

                                // enqueue PUBREL message received (for QoS Level 2) into the internal queue
                                MqttMsgPubrel pubrel = MqttMsgPubrel.Parse(fixedHeaderFirstByte[0], ProtocolVersion, _channel);
#if DEBUG
                                Debug.WriteLine($"RECV {pubrel}");
#endif

                                // enqueue PUBREL message into the internal queue
                                EnqueueInternal(pubrel);

                                break;

                            // PUBCOMP message received
                            case MqttMessageType.PublishComplete:

                                // enqueue PUBCOMP message received (for QoS Level 2) into the internal queue
                                MqttMsgPubcomp pubcomp = MqttMsgPubcomp.Parse(fixedHeaderFirstByte[0], ProtocolVersion, _channel);
#if DEBUG
                                Debug.WriteLine($"RECV {pubcomp}");
#endif
                                // enqueue PUBCOMP message into the internal queue
                                EnqueueInternal(pubcomp);

                                break;

                            // UNSUBSCRIBE message received
                            case MqttMessageType.Unsubscribe:
                                throw new MqttClientException(MqttClientErrorCode.WrongBrokerMessage);

                            // UNSUBACK message received
                            case MqttMessageType.UnsubscribeAck:
                                // enqueue UNSUBACK message received (for QoS Level 1) into the internal queue
                                MqttMsgUnsuback unsuback = MqttMsgUnsuback.Parse(fixedHeaderFirstByte[0], ProtocolVersion, _channel);
#if DEBUG
                                Debug.WriteLine($"RECV {unsuback}");
#endif

                                // enqueue UNSUBACK message into the internal queue
                                EnqueueInternal(unsuback);

                                break;

                            // DISCONNECT message received
                            case MqttMessageType.Disconnect:
                                // enqueue DISCONNECT message received (for QoS Level 1) into the internal queue
                                MqttMsgDisconnect disconnect = MqttMsgDisconnect.Parse(fixedHeaderFirstByte[0], ProtocolVersion, _channel);
#if DEBUG
                                Debug.WriteLine($"RECV {disconnect}");
#endif

                                // enqueue DISCONNECT message into the internal queue
                                ConnectionClosedRequest?.Invoke(this, new ConnectionClosedRequestEventArgs(disconnect));
                                EnqueueInternal(disconnect);
                                break;

                            case MqttMessageType.Authentication:
                                MqttMsgAuthentication authentication = MqttMsgAuthentication.Parse(fixedHeaderFirstByte[0], ProtocolVersion, _channel);
#if DEBUG
                                Debug.WriteLine($"AUTH {authentication}");
#endif
                                EnqueueInternal(authentication);
                                break;
                            default:
                                throw new MqttClientException(MqttClientErrorCode.WrongBrokerMessage);
                        }

                        _exReceiving = null;
                    }
                    // zero bytes read, peer gracefully closed socket
                    else
                    {
                        // wake up thread that will notify connection is closing
                        OnConnectionClosing();
                    }
                }
                catch (Exception e)
                {
#if DEBUG
                    Debug.WriteLine($"Exception occurred: {e}, msgtype: {msgType}");
#endif
                    _exReceiving = new MqttCommunicationException(e);

                    bool close = false;
                    if (e.GetType() == typeof(MqttClientException))
                    {
                        // [v3.1.1] scenarios the receiver MUST close the network connection
                        MqttClientException ex = e as MqttClientException;
                        close = ((ex.ErrorCode == MqttClientErrorCode.InvalidFlagBits) ||
                                (ex.ErrorCode == MqttClientErrorCode.InvalidProtocolName) ||
                                (ex.ErrorCode == MqttClientErrorCode.InvalidConnectFlags));
                    }
                    else if ((e.GetType() == typeof(IOException)) || (e.GetType() == typeof(SocketException)) ||
                             ((e.InnerException != null) && (e.InnerException.GetType() == typeof(SocketException)))) // added for SSL/TLS incoming connection that use SslStream that wraps SocketException
                    {
                        close = true;
                    }

                    if (close)
                    {
                        // wake up thread that will notify connection is closing
                        OnConnectionClosing();
                    }
                }
            }
        }

        /// <summary>
        /// Thread for handling keep alive message
        /// </summary>
        private void KeepAliveThread()
        {
            int delta = 0;
            int wait = _keepAlivePeriod;

            // create event to signal that current thread is end
            _keepAliveEventEnd = new AutoResetEvent(false);

            while (_isRunning)
            {
                // waiting...
                _keepAliveEvent.WaitOne(wait, false);

                if (_isRunning)
                {
                    delta = (int)(Environment.TickCount64 - _lastCommTime);

                    // if timeout exceeded ...
                    if (delta >= _keepAlivePeriod)
                    {
                        // ... send keep alive
                        Ping();
                        wait = _keepAlivePeriod;
                    }
                    else
                    {
                        // update waiting time
                        wait = _keepAlivePeriod - delta;
                    }
                }
            }

            // signal thread end
            _keepAliveEventEnd.Set();
        }

        /// <summary>
        /// Thread for raising event
        /// </summary>
        private void DispatchEventThread()
        {
            while (_isRunning)
            {
                if ((_eventQueue.Count == 0) && !_isConnectionClosing)
                    // wait on receiving message from client
                    _receiveEventWaitHandle.WaitOne();

                // check if it is running or we are closing client
                if (_isRunning)
                {
                    // get event from queue
                    InternalEvent internalEvent = null;
                    lock (_eventQueue)
                    {
                        if (_eventQueue.Count > 0)
                            internalEvent = (InternalEvent)_eventQueue.Dequeue();
                    }

                    // it's an event with a message inside
                    if (internalEvent != null)
                    {
                        MqttMsgBase msg = ((MsgInternalEvent)internalEvent).Message;

                        if (msg != null)
                        {
                            switch (msg.Type)
                            {
                                // CONNECT message received
                                case MqttMessageType.Connect:
                                    throw new MqttClientException(MqttClientErrorCode.WrongBrokerMessage);

                                // SUBSCRIBE message received
                                case MqttMessageType.Subscribe:
                                    throw new MqttClientException(MqttClientErrorCode.WrongBrokerMessage);

                                // SUBACK message received
                                case MqttMessageType.SubscribeAck:

                                    // raise subscribed topic event (SUBACK message received)
                                    OnMqttMsgSubscribed((MqttMsgSuback)msg);
                                    break;

                                // PUBLISH message received
                                case MqttMessageType.Publish:

                                    // PUBLISH message received in a published internal event, no publish succeeded
                                    if (internalEvent.GetType() == typeof(MsgPublishedInternalEvent))
                                        OnMqttMsgPublished(msg.MessageId, false);
                                    else
                                        // raise PUBLISH message received event 
                                        OnMqttMsgPublishReceived((MqttMsgPublish)msg);
                                    break;

                                // PUBACK message received
                                case MqttMessageType.PublishAck:

                                    // raise published message event
                                    // (PUBACK received for QoS Level 1)
                                    OnMqttMsgPublished(msg.MessageId, true);
                                    break;

                                // PUBREL message received
                                case MqttMessageType.PublishRelease:

                                    // raise message received event 
                                    // (PUBREL received for QoS Level 2)
                                    OnMqttMsgPublishReceived((MqttMsgPublish)msg);
                                    break;

                                // PUBCOMP message received
                                case MqttMessageType.PublishComplete:

                                    // raise published message event
                                    // (PUBCOMP received for QoS Level 2)
                                    OnMqttMsgPublished(msg.MessageId, true);
                                    break;

                                // UNSUBSCRIBE message received from client
                                case MqttMessageType.Unsubscribe:
                                    throw new MqttClientException(MqttClientErrorCode.WrongBrokerMessage);

                                // UNSUBACK message received
                                case MqttMessageType.UnsubscribeAck:

                                    // raise unsubscribed topic event
                                    OnMqttMsgUnsubscribed(msg.MessageId);
                                    break;

                                // DISCONNECT message received from client
                                case MqttMessageType.Disconnect:
                                    OnConnectionClosed();
                                    break; ;

                                // AUTH message received
                                case MqttMessageType.Authentication:
                                    OnMqttMsgAuthentication((MqttMsgAuthentication)msg);
                                    break;
                            }
                        }
                    }

                    // all events for received messages dispatched, check if there is closing connection
                    if ((_eventQueue.Count == 0) && _isConnectionClosing)
                    {
                        // client must close connection
                        Close();

                        // client raw disconnection
                        OnConnectionClosed();
                    }
                }
            }
        }

        /// <summary>
        /// Process inflight messages queue
        /// </summary>
        private void ProcessInflightThread()
        {
            MqttMsgContext msgContext = null;
            MqttMsgBase msgInflight = null;
            MqttMsgBase msgReceived = null;
            InternalEvent internalEvent = null;
            bool acknowledge = false;
            int timeout = Timeout.Infinite;
            int delta;
            bool msgReceivedProcessed = false;
            bool toEnqueue = true;

            try
            {
                while (_isRunning)
                {
                    // wait on message queueud to inflight
                    _inflightWaitHandle.WaitOne(timeout, false);

                    // it could be unblocked because Close() method is joining
                    if (_isRunning)
                    {

                        // message received and peeked from internal queue is processed
                        // NOTE : it has the corresponding message in inflight queue based on messageId
                        //        (ex. a PUBREC for a PUBLISH, a SUBACK for a SUBSCRIBE, ...)
                        //        if it's orphan we need to remove from internal queue
                        msgReceivedProcessed = false;
                        acknowledge = false;
                        msgReceived = null;

                        // set timeout tu MaxValue instead of Infinte (-1) to perform
                        // compare with calcultad current msgTimeout
                        timeout = Int32.MaxValue;

                        // a message inflight could be re-enqueued but we have to
                        // analyze it only just one time for cycle
                        int count = 0;
                        lock (_inflightQueue)
                        {
                            count = _inflightQueue.Count;
                        }

                        // process all inflight queued messages
                        while (count > 0)
                        {
                            count--;
                            acknowledge = false;
                            msgReceived = null;

                            // check to be sure that client isn't closing and all queues are now empty !
                            if (!_isRunning)
                            {
                                break;
                            }

                            lock (_inflightQueue)
                            {
                                // dequeue message context from queue
                                msgContext = (MqttMsgContext)_inflightQueue.Dequeue();
                            }

                            // get inflight message
                            msgInflight = msgContext.Message;

                            switch (msgContext.State)
                            {
                                case MqttMsgState.QueuedQos0:

                                    // QoS 0, PUBLISH message to send to broker, no state change, no acknowledge
                                    if (msgContext.Flow == MqttMsgFlow.ToPublish)
                                    {
                                        Send(msgInflight);
                                    }
                                    // QoS 0, no need acknowledge
                                    else if (msgContext.Flow == MqttMsgFlow.ToAcknowledge)
                                    {
                                        internalEvent = new MsgInternalEvent(msgInflight);
                                        // notify published message from broker (no need acknowledged)
                                        OnInternalEvent(internalEvent);
                                    }

#if DEBUG
                                    Debug.WriteLine($"processed {msgInflight}");
#endif
                                    break;

                                case MqttMsgState.QueuedQos1:
                                // [v3.1.1] SUBSCRIBE and UNSIBSCRIBE aren't "officially" QOS = 1
                                case MqttMsgState.SendSubscribe:
                                case MqttMsgState.SendUnsubscribe:

                                    // QoS 1, PUBLISH or SUBSCRIBE/UNSUBSCRIBE message to send to broker, state change to wait PUBACK or SUBACK/UNSUBACK                                    
                                    if (msgContext.Flow == MqttMsgFlow.ToPublish)
                                    {
                                        msgContext.Timestamp = Environment.TickCount64;
                                        msgContext.Attempt++;
                                        toEnqueue = true;

                                        if (msgInflight.Type == MqttMessageType.Publish)
                                        {
                                            // PUBLISH message to send, wait for PUBACK
                                            msgContext.State = MqttMsgState.WaitForPuback;
                                            // retry ? set dup flag [v3.1.1] only for PUBLISH message
                                            if (msgContext.Attempt > 1)
                                            {
                                                lock (_waitingForAnswer)
                                                {
                                                    if (!_waitingForAnswer.Contains(msgContext.Message.MessageId))
                                                    {
                                                        toEnqueue = false;
                                                    }
                                                }

                                                msgInflight.DupFlag = true;
                                            }
                                        }
                                        else if (msgInflight.Type == MqttMessageType.Subscribe)
                                        {
                                            // SUBSCRIBE message to send, wait for SUBACK
                                            msgContext.State = MqttMsgState.WaitForSuback;
                                        }
                                        else if (msgInflight.Type == MqttMessageType.Unsubscribe)
                                        {    // UNSUBSCRIBE message to send, wait for UNSUBACK
                                            msgContext.State = MqttMsgState.WaitForUnsuback;
                                        }

                                        if (toEnqueue)
                                        {
                                            Send(msgInflight);
                                            // re-enqueue message (I have to re-analyze for receiving PUBACK, SUBACK or UNSUBACK)
                                            lock (_inflightQueue)
                                            {
                                                _inflightQueue.Enqueue(msgContext);
                                            }
                                        }

                                        // update timeout : minimum between delay (based on current message sent) or current timeout
                                        timeout = (_settings.DelayOnRetry < timeout) ? _settings.DelayOnRetry : timeout;
                                    }
                                    // QoS 1, PUBLISH message received from broker to acknowledge, send PUBACK
                                    else if (msgContext.Flow == MqttMsgFlow.ToAcknowledge)
                                    {
                                        MqttMsgPuback puback = new MqttMsgPuback
                                        {
                                            MessageId = msgInflight.MessageId
                                        };

                                        Send(puback);

                                        internalEvent = new MsgInternalEvent(msgInflight);
                                        // notify published message from broker and acknowledged
                                        OnInternalEvent(internalEvent);

#if DEBUG
                                        Debug.WriteLine($"processed {msgInflight}");
#endif
                                    }

                                    break;

                                case MqttMsgState.QueuedQos2:

                                    // QoS 2, PUBLISH message to send to broker, state change to wait PUBREC
                                    if (msgContext.Flow == MqttMsgFlow.ToPublish)
                                    {
                                        msgContext.Timestamp = Environment.TickCount64;
                                        msgContext.Attempt++;
                                        toEnqueue = true;

                                        msgContext.State = MqttMsgState.WaitForPubrec;
                                        // retry ? set dup flag
                                        if (msgContext.Attempt > 1)
                                        {
                                            lock (_waitingForAnswer)
                                            {
                                                if (!_waitingForAnswer.Contains(msgContext.Message.MessageId))
                                                {
                                                    toEnqueue = false;
                                                }
                                            }

                                            msgInflight.DupFlag = true;
                                        }

                                        if (toEnqueue)
                                        {
                                            Send(msgInflight);
                                            lock (_inflightQueue)
                                            {
                                                // re-enqueue message (I have to re-analyze for receiving PUBREC)
                                                _inflightQueue.Enqueue(msgContext);
                                            }
                                        }

                                        // update timeout : minimum between delay (based on current message sent) or current timeout
                                        timeout = (_settings.DelayOnRetry < timeout) ? _settings.DelayOnRetry : timeout;
                                    }
                                    // QoS 2, PUBLISH message received from broker to acknowledge, send PUBREC, state change to wait PUBREL
                                    else if (msgContext.Flow == MqttMsgFlow.ToAcknowledge)
                                    {
                                        MqttMsgPubrec pubrec = new MqttMsgPubrec
                                        {
                                            MessageId = msgInflight.MessageId
                                        };

                                        msgContext.State = MqttMsgState.WaitForPubrel;

                                        Send(pubrec);

                                        // re-enqueue message (I have to re-analyze for receiving PUBREL)
                                        lock (_inflightQueue)
                                        {
                                            _inflightQueue.Enqueue(msgContext);
                                        }
                                    }

                                    break;

                                case MqttMsgState.WaitForPuback:
                                case MqttMsgState.WaitForSuback:
                                case MqttMsgState.WaitForUnsuback:

                                    // QoS 1, waiting for PUBACK of a PUBLISH message sent or
                                    //        waiting for SUBACK of a SUBSCRIBE message sent or
                                    //        waiting for UNSUBACK of a UNSUBSCRIBE message sent or
                                    if (msgContext.Flow == MqttMsgFlow.ToPublish)
                                    {
                                        acknowledge = false;
                                        lock (_internalQueue)
                                        {
                                            if (_internalQueue.Count > 0)
                                            {
                                                msgReceived = (MqttMsgBase)_internalQueue.Peek();
                                            }
                                        }

                                        // it is a PUBACK message or a SUBACK/UNSUBACK message
                                        if (msgReceived != null)
                                        {
                                            // PUBACK message or SUBACK/UNSUBACK message for the current message
                                            if (((msgReceived.Type == MqttMessageType.PublishAck) && (msgInflight.Type == MqttMessageType.Publish) && (msgReceived.MessageId == msgInflight.MessageId)) ||
                                                ((msgReceived.Type == MqttMessageType.SubscribeAck) && (msgInflight.Type == MqttMessageType.Subscribe) && (msgReceived.MessageId == msgInflight.MessageId)) ||
                                                ((msgReceived.Type == MqttMessageType.UnsubscribeAck) && (msgInflight.Type == MqttMessageType.Unsubscribe) && (msgReceived.MessageId == msgInflight.MessageId)))
                                            {
                                                acknowledge = true;
                                                msgReceivedProcessed = true;
#if DEBUG
                                                Debug.WriteLine($"dequeued {msgReceived}");
#endif
                                                // if PUBACK received, confirm published with flag
                                                if (msgReceived.Type == MqttMessageType.PublishAck)
                                                    internalEvent = new MsgPublishedInternalEvent(msgReceived, true);
                                                else
                                                    internalEvent = new MsgInternalEvent(msgReceived);

                                                // notify received acknowledge from broker of a published message or subscribe/unsubscribe message
                                                OnInternalEvent(internalEvent);

                                                // PUBACK received for PUBLISH message with QoS Level 1, remove from session state
                                                if ((msgInflight.Type == MqttMessageType.Publish) &&
                                                    (_session != null) &&
                                                    (_session.InflightMessages.Contains(msgContext.Key)))
                                                {
                                                    _session.InflightMessages.Remove(msgContext.Key);
                                                }

#if DEBUG
                                                Debug.WriteLine($"processed {msgInflight}");
#endif
                                            }
                                        }

                                        // current message not acknowledged, no PUBACK or SUBACK/UNSUBACK or not equal messageid 
                                        if (!acknowledge)
                                        {
                                            delta = (int)(Environment.TickCount64 - msgContext.Timestamp);
                                            // check timeout for receiving PUBACK since PUBLISH was sent or
                                            // for receiving SUBACK since SUBSCRIBE was sent or
                                            // for receiving UNSUBACK since UNSUBSCRIBE was sent
                                            if (delta >= _settings.DelayOnRetry)
                                            {
                                                // max retry not reached, resend
                                                if (msgContext.Attempt < _settings.AttemptsOnRetry)
                                                {
                                                    msgContext.State = MqttMsgState.QueuedQos1;

                                                    lock (_inflightQueue)
                                                    {
                                                        // re-enqueue message
                                                        _inflightQueue.Enqueue(msgContext);
                                                    }

                                                    // update timeout (0 -> reanalyze queue immediately)
                                                    timeout = 0;
                                                }
                                                else
                                                {
                                                    // if PUBACK for a PUBLISH message not received after retries, raise event for not published
                                                    if (msgInflight.Type == MqttMessageType.Publish)
                                                    {
                                                        // PUBACK not received in time, PUBLISH retries failed, need to remove from session inflight messages too
                                                        if ((_session != null) &&
                                                            (_session.InflightMessages.Contains(msgContext.Key)))
                                                        {
                                                            _session.InflightMessages.Remove(msgContext.Key);
                                                        }

                                                        internalEvent = new MsgPublishedInternalEvent(msgInflight, false);

                                                        // notify not received acknowledge from broker and message not published
                                                        OnInternalEvent(internalEvent);
                                                    }
                                                    // NOTE : not raise events for SUBACK or UNSUBACK not received
                                                    //        for the user no event raised means subscribe/unsubscribe failed
                                                }
                                            }
                                            else
                                            {
                                                lock (_inflightQueue)
                                                {
                                                    // re-enqueue message (I have to re-analyze for receiving PUBACK, SUBACK or UNSUBACK)
                                                    _inflightQueue.Enqueue(msgContext);
                                                }

                                                // update timeout
                                                int msgTimeout = (_settings.DelayOnRetry - delta);
                                                timeout = (msgTimeout < timeout) ? msgTimeout : timeout;
                                            }
                                        }
                                    }
                                    break;

                                case MqttMsgState.WaitForPubrec:

                                    // QoS 2, waiting for PUBREC of a PUBLISH message sent
                                    if (msgContext.Flow == MqttMsgFlow.ToPublish)
                                    {
                                        acknowledge = false;
                                        lock (_internalQueue)
                                        {
                                            if (_internalQueue.Count > 0)
                                            {
                                                msgReceived = (MqttMsgBase)_internalQueue.Peek();
                                            }
                                        }

                                        // it is a PUBREC message
                                        if ((msgReceived != null) && (msgReceived.Type == MqttMessageType.PublishReceived))
                                        {
                                            // PUBREC message for the current PUBLISH message, send PUBREL, wait for PUBCOMP
                                            if (msgReceived.MessageId == msgInflight.MessageId)
                                            {
                                                lock (_internalQueue)
                                                {
                                                    // received message processed
                                                    _internalQueue.Dequeue();
                                                }

                                                acknowledge = true;
                                                msgReceivedProcessed = true;
#if DEBUG
                                                Debug.WriteLine($"dequeued {msgReceived}");
#endif
                                                MqttMsgPubrel pubrel = new MqttMsgPubrel
                                                {
                                                    MessageId = msgInflight.MessageId
                                                };

                                                msgContext.State = MqttMsgState.WaitForPubcomp;
                                                msgContext.Timestamp = Environment.TickCount64;
                                                msgContext.Attempt = 1;

                                                Send(pubrel);

                                                // update timeout : minimum between delay (based on current message sent) or current timeout
                                                timeout = (_settings.DelayOnRetry < timeout) ? _settings.DelayOnRetry : timeout;

                                                // re-enqueue message
                                                lock (_inflightQueue)
                                                {
                                                    _inflightQueue.Enqueue(msgContext);
                                                }
                                            }
                                        }

                                        // current message not acknowledged
                                        if (!acknowledge)
                                        {
                                            delta = (int)(Environment.TickCount64 - msgContext.Timestamp);
                                            // check timeout for receiving PUBREC since PUBLISH was sent
                                            if (delta >= _settings.DelayOnRetry)
                                            {
                                                // max retry not reached, resend
                                                if (msgContext.Attempt < _settings.AttemptsOnRetry)
                                                {
                                                    msgContext.State = MqttMsgState.QueuedQos2;
                                                    msgContext.Timestamp = Environment.TickCount64;

                                                    // re-enqueue message
                                                    lock (_inflightQueue)
                                                    {
                                                        _inflightQueue.Enqueue(msgContext);
                                                    }

                                                    // update timeout (0 -> reanalyze queue immediately)
                                                    timeout = 0;
                                                }
                                                else
                                                {
                                                    // PUBREC not received in time, PUBLISH retries failed, need to remove from session inflight messages too
                                                    if ((_session != null) &&
                                                        (_session.InflightMessages.Contains(msgContext.Key)))
                                                    {
                                                        _session.InflightMessages.Remove(msgContext.Key);
                                                    }

                                                    // if PUBREC for a PUBLISH message not received after retries, raise event for not published
                                                    internalEvent = new MsgPublishedInternalEvent(msgInflight, false);
                                                    // notify not received acknowledge from broker and message not published
                                                    OnInternalEvent(internalEvent);
                                                }
                                            }
                                            else
                                            {
                                                // re-enqueue message
                                                lock (_inflightQueue)
                                                {
                                                    _inflightQueue.Enqueue(msgContext);
                                                }

                                                // update timeout
                                                int msgTimeout = (_settings.DelayOnRetry - delta);
                                                timeout = (msgTimeout < timeout) ? msgTimeout : timeout;
                                            }
                                        }
                                    }
                                    break;

                                case MqttMsgState.WaitForPubrel:

                                    // QoS 2, waiting for PUBREL of a PUBREC message sent
                                    if (msgContext.Flow == MqttMsgFlow.ToAcknowledge)
                                    {
                                        lock (_internalQueue)
                                        {
                                            if (_internalQueue.Count > 0)
                                            {
                                                msgReceived = (MqttMsgBase)_internalQueue.Peek();
                                            }
                                        }

                                        // it is a PUBREL message
                                        if ((msgReceived != null) && (msgReceived.Type == MqttMessageType.PublishRelease))
                                        {
                                            // PUBREL message for the current message, send PUBCOMP
                                            if (msgReceived.MessageId == msgInflight.MessageId)
                                            {
                                                lock (_internalQueue)
                                                {
                                                    // received message processed
                                                    _internalQueue.Dequeue();
                                                }

                                                msgReceivedProcessed = true;
#if DEBUG
                                                Debug.WriteLine($"dequeued {msgReceived}");
#endif
                                                MqttMsgPubcomp pubcomp = new MqttMsgPubcomp
                                                {
                                                    MessageId = msgInflight.MessageId
                                                };

                                                Send(pubcomp);

                                                internalEvent = new MsgInternalEvent(msgInflight);
                                                // notify published message from broker and acknowledged
                                                OnInternalEvent(internalEvent);

                                                // PUBREL received (and PUBCOMP sent) for PUBLISH message with QoS Level 2, remove from session state
                                                if ((msgInflight.Type == MqttMessageType.Publish) &&
                                                    (_session != null) &&
                                                    (_session.InflightMessages.Contains(msgContext.Key)))
                                                {
                                                    _session.InflightMessages.Remove(msgContext.Key);
                                                }

#if DEBUG
                                                Debug.WriteLine($"processed {msgInflight}");
#endif
                                            }
                                            else
                                            {
                                                lock (_inflightQueue)
                                                {
                                                    // re-enqueue message
                                                    _inflightQueue.Enqueue(msgContext);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            lock (_inflightQueue)
                                            {
                                                // re-enqueue message
                                                _inflightQueue.Enqueue(msgContext);
                                            }
                                        }
                                    }
                                    break;

                                case MqttMsgState.WaitForPubcomp:

                                    // QoS 2, waiting for PUBCOMP of a PUBREL message sent
                                    if (msgContext.Flow == MqttMsgFlow.ToPublish)
                                    {
                                        acknowledge = false;
                                        lock (_internalQueue)
                                        {
                                            if (_internalQueue.Count > 0)
                                            {
                                                msgReceived = (MqttMsgBase)_internalQueue.Peek();
                                            }
                                        }

                                        // it is a PUBCOMP message
                                        if ((msgReceived != null) && (msgReceived.Type == MqttMessageType.PublishComplete))
                                        {
                                            // PUBCOMP message for the current message
                                            if (msgReceived.MessageId == msgInflight.MessageId)
                                            {
                                                lock (_internalQueue)
                                                {
                                                    // received message processed
                                                    _internalQueue.Dequeue();
                                                }

                                                acknowledge = true;
                                                msgReceivedProcessed = true;
#if DEBUG
                                                Debug.WriteLine($"dequeued {msgReceived}");
#endif
                                                internalEvent = new MsgPublishedInternalEvent(msgReceived, true);
                                                // notify received acknowledge from broker of a published message
                                                OnInternalEvent(internalEvent);

                                                // PUBCOMP received for PUBLISH message with QoS Level 2, remove from session state
                                                if ((msgInflight.Type == MqttMessageType.Publish) &&
                                                    (_session != null) &&
                                                    (_session.InflightMessages.Contains(msgContext.Key)))
                                                {
                                                    _session.InflightMessages.Remove(msgContext.Key);
                                                }

#if DEBUG
                                                Debug.WriteLine($"processed {msgInflight}");
#endif
                                            }
                                        }
                                        // it is a PUBREC message
                                        else if ((msgReceived != null) && (msgReceived.Type == MqttMessageType.PublishReceived))
                                        {
                                            // another PUBREC message for the current message due to a retransmitted PUBLISH
                                            // I'm in waiting for PUBCOMP, so I can discard this PUBREC
                                            if (msgReceived.MessageId == msgInflight.MessageId)
                                            {
                                                lock (_internalQueue)
                                                {
                                                    // received message processed
                                                    _internalQueue.Dequeue();
                                                }

                                                acknowledge = true;
                                                msgReceivedProcessed = true;
#if DEBUG
                                                Debug.WriteLine($"dequeued {msgReceived}");
#endif
                                                lock (_inflightQueue)
                                                {
                                                    // re-enqueue message
                                                    _inflightQueue.Enqueue(msgContext);
                                                }
                                            }
                                        }

                                        // current message not acknowledged
                                        if (!acknowledge)
                                        {
                                            delta = (int)(Environment.TickCount64 - msgContext.Timestamp);
                                            // check timeout for receiving PUBCOMP since PUBREL was sent
                                            if (delta >= _settings.DelayOnRetry)
                                            {
                                                // max retry not reached, resend
                                                if (msgContext.Attempt < _settings.AttemptsOnRetry)
                                                {
                                                    msgContext.State = MqttMsgState.SendPubrel;
                                                    lock (_inflightQueue)
                                                    {
                                                        // re-enqueue message
                                                        _inflightQueue.Enqueue(msgContext);
                                                    }

                                                    // update timeout (0 -> reanalyze queue immediately)
                                                    timeout = 0;
                                                }
                                                else
                                                {
                                                    // PUBCOMP not received, PUBREL retries failed, need to remove from session inflight messages too
                                                    if ((_session != null) &&
                                                        (_session.InflightMessages.Contains(msgContext.Key)))
                                                    {
                                                        _session.InflightMessages.Remove(msgContext.Key);
#if DEBUG
                                                        Debug.WriteLine($"session remove {msgContext.Key}");
#endif
                                                    }

                                                    // if PUBCOMP for a PUBLISH message not received after retries, raise event for not published
                                                    internalEvent = new MsgPublishedInternalEvent(msgInflight, false);
                                                    // notify not received acknowledge from broker and message not published
                                                    OnInternalEvent(internalEvent);
                                                }
                                            }
                                            else
                                            {
                                                lock (_inflightQueue)
                                                {
                                                    // re-enqueue message
                                                    _inflightQueue.Enqueue(msgContext);
                                                }

                                                // update timeout
                                                int msgTimeout = (_settings.DelayOnRetry - delta);
                                                timeout = (msgTimeout < timeout) ? msgTimeout : timeout;
                                            }
                                        }
                                    }
                                    break;

                                case MqttMsgState.SendPubrec:

                                    // TODO : impossible ? --> QueuedQos2 ToAcknowledge
                                    break;

                                case MqttMsgState.SendPubrel:

                                    // QoS 2, PUBREL message to send to broker, state change to wait PUBCOMP
                                    if (msgContext.Flow == MqttMsgFlow.ToPublish)
                                    {
                                        MqttMsgPubrel pubrel = new MqttMsgPubrel
                                        {
                                            MessageId = msgInflight.MessageId
                                        };

                                        msgContext.State = MqttMsgState.WaitForPubcomp;
                                        msgContext.Timestamp = Environment.TickCount64;
                                        msgContext.Attempt++;
                                        // retry ? set dup flag [v3.1.1] no needed
                                        if (ProtocolVersion == MqttProtocolVersion.Version_3_1)
                                        {
                                            if (msgContext.Attempt > 1)
                                                pubrel.DupFlag = true;
                                        }

                                        Send(pubrel);

                                        // update timeout : minimum between delay (based on current message sent) or current timeout
                                        timeout = (_settings.DelayOnRetry < timeout) ? _settings.DelayOnRetry : timeout;

                                        lock (_inflightQueue)
                                        {
                                            // re-enqueue message
                                            _inflightQueue.Enqueue(msgContext);
                                        }
                                    }
                                    break;

                                case MqttMsgState.SendPubcomp:
                                    // TODO : impossible ?
                                    break;
                                case MqttMsgState.SendPuback:
                                    // TODO : impossible ? --> QueuedQos1 ToAcknowledge
                                    break;
                                default:
                                    break;
                            }
                        }

                        // if calculated timeout is MaxValue, it means that must be Infinite (-1)
                        if (timeout == int.MaxValue)
                        {
                            timeout = Timeout.Infinite;
                        }

                        // if message received is orphan, no corresponding message in inflight queue
                        // based on messageId, we need to remove from the queue
                        if ((msgReceived != null) && !msgReceivedProcessed)
                        {
                            lock (_internalQueue)
                            {
                                _internalQueue.Dequeue();
                            }
#if DEBUG
                            Debug.WriteLine($"dequeued {msgReceived} orphan");
#endif
                        }
                    }
                }
            }
#if DEBUG
            catch (MqttCommunicationException e)
            {
#else
            catch (MqttCommunicationException)
            {
#endif
                // possible exception on Send, I need to re-enqueue not sent message
                if (msgContext != null)
                {
                    lock (_inflightQueue)
                    {
                        // re-enqueue message
                        _inflightQueue.Enqueue(msgContext);
                    }
                }

#if DEBUG
                Debug.WriteLine($"Exception occurred: {e}");
#endif

                // raise disconnection client event
                OnConnectionClosing();
            }
        }

        /// <summary>
        /// Restore session
        /// </summary>
        private void RestoreSession()
        {
            // if not clean session
            if (!CleanSession)
            {
                // there is a previous session
                if (_session != null)
                {
                    lock (_inflightQueue)
                    {
                        foreach (MqttMsgContext msgContext in _session.InflightMessages.Values)
                        {
                            _inflightQueue.Enqueue(msgContext);

                            // if it is a PUBLISH message to publish
                            if ((msgContext.Message.Type == MqttMessageType.Publish) &&
                                (msgContext.Flow == MqttMsgFlow.ToPublish))
                            {
                                // it's QoS 1 and we haven't received PUBACK
                                if ((msgContext.Message.QosLevel == MqttQoSLevel.AtLeastOnce) &&
                                    (msgContext.State == MqttMsgState.WaitForPuback))
                                {
                                    // we haven't received PUBACK, we need to resend PUBLISH message
                                    msgContext.State = MqttMsgState.QueuedQos1;
                                }
                                // it's QoS 2
                                else if (msgContext.Message.QosLevel == MqttQoSLevel.ExactlyOnce)
                                {
                                    // we haven't received PUBREC, we need to resend PUBLISH message
                                    if (msgContext.State == MqttMsgState.WaitForPubrec)
                                    {
                                        msgContext.State = MqttMsgState.QueuedQos2;
                                    }
                                    // we haven't received PUBCOMP, we need to resend PUBREL for it
                                    else if (msgContext.State == MqttMsgState.WaitForPubcomp)
                                    {
                                        msgContext.State = MqttMsgState.SendPubrel;
                                    }
                                }
                            }
                        }
                    }

                    // unlock process inflight queue
                    _inflightWaitHandle.Set();
                }
                else
                {
                    // create new session
                    _session = new MqttClientSession(ClientId);
                }
            }
            // clean any previous session
            else
            {
                if (_session != null)
                    _session.Clear();
            }
        }

        /// <summary>
        /// Generate the next message identifier
        /// </summary>
        /// <returns>Message identifier</returns>
        private ushort GetMessageId()
        {
            // if 0 or max UInt16, it becomes 1 (first valid messageId)
            _messageIdCounter = ((_messageIdCounter % ushort.MaxValue) != 0) ? (ushort)(_messageIdCounter + 1) : (ushort)1;
            return _messageIdCounter;
        }

        /// <summary>
        /// Finder class for PUBLISH message inside a queue
        /// </summary>
        internal class MqttMsgContextFinder
        {
            // PUBLISH message id
            internal ushort MessageId { get; set; }
            // message flow into inflight queue
            internal MqttMsgFlow Flow { get; set; }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="messageId">Message Id</param>
            /// <param name="flow">Message flow inside inflight queue</param>
            internal MqttMsgContextFinder(ushort messageId, MqttMsgFlow flow)
            {
                MessageId = messageId;
                Flow = flow;
            }

            internal bool Find(object item)
            {
                MqttMsgContext msgCtx = (MqttMsgContext)item;
                return ((msgCtx.Message.Type == MqttMessageType.Publish) &&
                        (msgCtx.Message.MessageId == MessageId) &&
                        msgCtx.Flow == Flow);

            }
        }
    }

    /// <summary>
    /// MQTT protocol version
    /// </summary>
    public enum MqttProtocolVersion
    {
        /// <summary>
        /// MQTT protocol version 3.1
        /// </summary>
        Version_3_1 = 0x03,

        /// <summary>
        /// MQTT protocol version 3.1.1
        /// </summary>
        Version_3_1_1 = 0x04,

        /// <summary>
        /// MQTT protocol version 5.0
        /// </summary>
        Version_5 = 0x05,
    }
}
