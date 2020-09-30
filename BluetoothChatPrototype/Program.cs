using BluetoothChatPrototype.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BluetoothChatPrototype.Network;

namespace BluetoothChatPrototype
{
    class Program
    {
        static void Main(string[] args)
        {
            var broadcast = new Broadcast();
            broadcast.StartRfcommServer();
;           //var rec = new Receive();
            //rec.Initialize();
            Console.ReadKey();
        }
    }
}
