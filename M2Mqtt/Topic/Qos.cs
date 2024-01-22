using System;
using System.Collections.Generic;
using System.Text;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace uPLibrary.Networking.M2Mqtt.Topic
{
    public class Qos
    {
        private Options qos;

        public enum Options {
            AT_MOST_ONCE,
            AT_LEAST_ONCE,
            EXACTLY_ONCE
        };

        public byte GetByte() {
            byte t = 0x00;

            switch(this.qos){
                case Options.AT_LEAST_ONCE:
                    t = MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE;
                    break;
                case Options.AT_MOST_ONCE:
                    t = MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE;
                    break;
                case Options.EXACTLY_ONCE:
                    t = MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE;
                    break;
            }
            return t;
        }

        public Qos() {
            this.qos = Options.AT_MOST_ONCE;
        }

        public Qos(Options o) {
            this.qos = o;
        }

        public Qos(byte b) {
            Qos.Options t = Options.AT_LEAST_ONCE;

            switch (b)
            {
                case MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE:
                    t = Options.AT_MOST_ONCE;
                    break;
                case MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE:
                    t = Options.AT_LEAST_ONCE;
                    break;
                case MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE:
                    t = Options.EXACTLY_ONCE;
                    break;
            }
            qos = t;
        }
    }
}
