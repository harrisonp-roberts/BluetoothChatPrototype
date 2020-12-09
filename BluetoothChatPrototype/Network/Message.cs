using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluetoothChatPrototype.Network
{
    [Serializable()]
    public class Message
    {
        public string sender { get; set; }
        string recipient { get; set; }
        public string message { get; set; }
        int hops { get; }
        DateTime timestamp;

        public Message(string s, string r, string m)
        {
            this.sender = s;
            this.recipient = r;
            this.message = m;
            hops = 0;
            timestamp = DateTime.Now;
        }
    }
}
