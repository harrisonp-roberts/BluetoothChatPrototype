using BluetoothChatPrototype.Constants;
using BluetoothChatPrototype.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluetoothChatPrototype
{
    class Program
    {
        static void Main(string[] args)
        {

            Task task = Task.Run(() =>
            {
                Form1 form = new Form1();
                form.Text = "Ad Hoc Chat Application";
                form.ShowDialog();
            });

            Console.WriteLine("Would you like to broadcast (b) or receive (r)");
            char input = Console.ReadKey().KeyChar;
            Console.WriteLine();
            if (input == 'b')
            {
                var broadcast = new Broadcast();
                broadcast.startBroadcast();
            } else if (input == 'r')
            {
                var rec = new Receive();
                rec.Initialize();
            }

            Console.ReadKey();


        }
    }
}
