using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt;

namespace MqttClientConsole
{
    public class Program
    {
        public static void Main(string[] args)
        {
            MqttClient client = new MqttClient("localhost");
        }
    }
}
