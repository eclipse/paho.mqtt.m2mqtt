using System;
using System.Collections.Generic;
using System.Text;

namespace uPLibrary.Networking.M2Mqtt.Topic
{
    public class Topic
    {
        public string topic { get; set; }
        public Qos qos { get; set; }

        public Topic(string topic, Qos qos) {
            this.topic = topic;
            this.qos = qos;
        }

        public Topic(string topic, byte qos)
        {
            this.topic = topic;
            this.qos = new Qos(qos);
        }

        public Topic(string topic)
        {
            this.topic = topic;
            qos = new Qos();
        }
    }
}
