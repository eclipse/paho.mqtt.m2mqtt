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

using System.Net.Security;
using System.Net.Sockets;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System;

namespace nanoFramework.M2Mqtt
{
    /// <summary>
    /// Channel to communicate over the network
    /// </summary>
    public class MqttNetworkChannel : IMqttNetworkChannel
    {
        // remote host information
        private readonly string _remoteHostName;
        private readonly IPAddress _remoteIpAddress;
        private readonly int _remotePort;

        // socket for communication
        private Socket _socket;
        // using SSL
        private readonly bool _secure;

        // CA certificate (on client)
        private readonly X509Certificate _caCert;

        // client certificate (on client)
        private readonly X509Certificate _clientCert;

        // SSL/TLS protocol version
        private readonly MqttSslProtocols _sslProtocol;

        // SSL stream
        private SslStream _sslStream;

        /// <summary>
        /// Remote host name
        /// </summary>
        public string RemoteHostName => _remoteHostName;

        /// <summary>
        /// Remote IP address
        /// </summary>
        public IPAddress RemoteIpAddress => _remoteIpAddress;

        /// <summary>
        /// Remote port
        /// </summary>
        public int RemotePort => _remotePort;

        /// <summary>
        /// Data available on the channel
        /// </summary>
        public bool DataAvailable => _secure ? _sslStream.DataAvailable : _socket.Available > 0;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="socket">Socket opened with the client</param>
        public MqttNetworkChannel(Socket socket)
            : this(socket, false, null, MqttSslProtocols.None)
        { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="socket">Socket opened with the client</param>
        /// <param name="secure">Secure connection (SSL/TLS)</param>
        /// <param name="serverCert">Server X509 certificate for secure connection</param>
        /// <param name="sslProtocol">SSL/TLS protocol version</param>
        public MqttNetworkChannel(Socket socket, bool secure, X509Certificate serverCert, MqttSslProtocols sslProtocol)
        {
            _socket = socket;
            _secure = secure;
            _sslProtocol = sslProtocol;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="remoteHostName">Remote Host name</param>
        /// <param name="remotePort">Remote port</param>
        public MqttNetworkChannel(string remoteHostName, int remotePort)
            : this(remoteHostName, remotePort, false, null, null, MqttSslProtocols.None)
        { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="remoteHostName">Remote Host name</param>
        /// <param name="remotePort">Remote port</param>
        /// <param name="secure">Using SSL</param>
        /// <param name="caCert">CA certificate</param>
        /// <param name="clientCert">Client certificate</param>
        /// <param name="sslProtocol">SSL/TLS protocol version</param>
        public MqttNetworkChannel(string remoteHostName, int remotePort, bool secure, X509Certificate caCert, X509Certificate clientCert, MqttSslProtocols sslProtocol)
        {
            IPAddress hostIpAddress = null;
            IPHostEntry hostEntry = Dns.GetHostEntry(remoteHostName);
            if ((hostEntry != null) && (hostEntry.AddressList.Length > 0))
            {
                // check for the first address not null
                // it seems that with .Net Micro Framework, the IPV6 addresses aren't supported and return "null"
                int i = 0;
                while (hostEntry.AddressList[i] == null) i++;
                hostIpAddress = hostEntry.AddressList[i];
            }
            else
            {
                throw new Exception("No address found for the remote host name");
            }

            _remoteHostName = remoteHostName;
            _remoteIpAddress = hostIpAddress;
            _remotePort = remotePort;
            _secure = secure;
            _caCert = caCert;
            _clientCert = clientCert;
            _sslProtocol = sslProtocol;
        }

        /// <summary>
        /// Connect to remote server
        /// </summary>
        public void Connect()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // try connection to the broker
            _socket.Connect(new IPEndPoint(_remoteIpAddress, _remotePort));

            // secure channel requested
            if (_secure)
            {
                // create SSL stream
                _sslStream = new SslStream(_socket);

                // server authentication (SSL/TLS handshake)
                _sslStream.AuthenticateAsClient(_remoteHostName,
                    _clientCert,
                    _caCert,
                    MqttSslUtility.ToSslPlatformEnum(_sslProtocol));
            }
        }

        /// <summary>
        /// Send data on the network channel
        /// </summary>
        /// <param name="buffer">Data buffer to send</param>
        /// <returns>Number of byte sent</returns>
        public int Send(byte[] buffer)
        {
            if (_secure)
            {
                _sslStream.Write(buffer, 0, buffer.Length);
                _sslStream.Flush();
                return buffer.Length;
            }
            else
            {
                return _socket.Send(buffer, 0, buffer.Length, SocketFlags.None);
            }
        }

        /// <summary>
        /// Receive data from the network
        /// </summary>
        /// <param name="buffer">Data buffer for receiving data</param>
        /// <returns>Number of bytes received</returns>
        public int Receive(byte[] buffer)
        {
            // read all data needed (until fill buffer)
            int idx = 0;
            int read;
            if (_secure)
            {
                while (idx < buffer.Length)
                {
                    // fixed scenario with socket closed gracefully by peer/broker and
                    // Read return 0. Avoid infinite loop.
                    read = _sslStream.Read(buffer, idx, buffer.Length - idx);
                    if (read == 0)
                    {
                        return 0;
                    }

                    idx += read;
                }
                return buffer.Length;
            }
            else
            {
                while (idx < buffer.Length)
                {
                    // fixed scenario with socket closed gracefully by peer/broker and
                    // Read return 0. Avoid infinite loop.
                    read = _socket.Receive(buffer, idx, buffer.Length - idx, SocketFlags.None);
                    if (read == 0)
                    {
                        return 0;
                    }

                    idx += read;
                }
                return buffer.Length;
            }
        }

        /// <summary>
        /// Receive data from the network channel with a specified timeout
        /// </summary>
        /// <param name="buffer">Data buffer for receiving data</param>
        /// <param name="timeout">Timeout on receiving (in milliseconds)</param>
        /// <returns>Number of bytes received</returns>
        public int Receive(byte[] buffer, int timeout)
        {
            // check data availability (timeout is in microseconds)
            if (_socket.Poll(timeout * 1000, SelectMode.SelectRead))
            {
                return Receive(buffer);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Close the network channel
        /// </summary>
        public void Close()
        {
            if (_secure)
            {
                _sslStream.Close();
                _sslStream.Dispose();
                _sslStream = null;
            }

            _socket.Close();
        }

        /// <summary>
        /// Accept connection from a remote client
        /// </summary>
        public void Accept()
        {
            // Doesn't do anything as not a broker
        }
    }

    /// <summary>
    /// IPAddress Utility class
    /// </summary>
    public static class IPAddressUtility
    {
        /// <summary>
        /// Return AddressFamily for the IP address
        /// </summary>
        /// <param name="ipAddress">IP address to check</param>
        /// <returns>Address family</returns>
        public static AddressFamily GetAddressFamily(this IPAddress ipAddress)
        {
            return (ipAddress.ToString().IndexOf(':') != -1) ?
                AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork;
        }
    }

    /// <summary>
    /// MQTT SSL utility class
    /// </summary>
    public static class MqttSslUtility
    {
        /// <summary>
        /// Defines the possible versions of Secure Sockets Layer (SSL).
        /// </summary>
        /// <remarks>
        /// Note: Following the recommendation of the .NET documentation, nanoFramework implementation does not have SSL3 nor Default because those are deprecated and unsecure.
        /// </remarks>
        public static SslProtocols ToSslPlatformEnum(MqttSslProtocols mqttSslProtocol)
        {
            switch (mqttSslProtocol)
            {
                case MqttSslProtocols.None:
                    return SslProtocols.None;
                case MqttSslProtocols.TLSv1_0:
                    return SslProtocols.Tls;
                case MqttSslProtocols.TLSv1_1:
                    return SslProtocols.Tls11;
                case MqttSslProtocols.TLSv1_2:
                    return SslProtocols.Tls12;
                case MqttSslProtocols.TLSv1_3:
                    throw new ArgumentException("TLS 1.3 is currently disabled, awaiting support from OS/Device Firmware.");
                default:
                    throw new ArgumentException("SSL/TLS protocol version not supported");
            }
        }

    }
}

