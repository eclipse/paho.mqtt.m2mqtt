using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;

namespace uPLibrary.Networking.M2Mqtt.Messages
{
    public class GenericEventArgs : EventArgs
    {
        public Dictionary<string, object> Dict;

        public GenericEventArgs(Events.Events.Options eventType, EventArgs EA) {

            Dict = new Dictionary<string, object>();
#if (!MONO)

            // I don't like this, but it's handy
            foreach (var p in EA.GetType().GetProperties()) {
                var t = p.GetValue(EA);
                dynamic q = t;
                if (t.GetType() == typeof(byte))
                {
                    int s = (int)q;
                    q = $"{s}";
                }
                else if (t.GetType() == typeof(byte[]))
                {
                    // Maybe represent as hex instead??..
                    q = Encoding.ASCII.GetString((byte[])q);
                }
                this.Dict.Add(p.Name, q);
            }
            Dict.Add("EventType", eventType.ToString());
#endif
        }
    }
}
