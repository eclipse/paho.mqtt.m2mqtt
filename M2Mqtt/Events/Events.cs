using System;
using System.Collections.Generic;
using System.Text;

namespace uPLibrary.Networking.M2Mqtt.Events
{
    public static class Events
    {
        public enum Options {
            publish_received,
            message_published,
            connection_closed,
            message_subscribed,
            message_unsubscribed,
            reconnected,
            connected,
            disconnected
        };

    }
}
