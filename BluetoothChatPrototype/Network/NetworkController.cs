using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;

namespace BluetoothChatPrototype.Network
{
    public class NetworkController
    {
        private Receive search;
        private Broadcast broadcast;
        public LinkedList<ConnectedDevice> devices { get; }

        public NetworkController()
        {
            search = new Receive();
            broadcast = new Broadcast();
            devices = new LinkedList<ConnectedDevice>();
        }

        public void start()
        {
            Console.WriteLine("Initializing Search...");
            search.Initialize(this);
            Console.WriteLine("Search Initialized");
            Thread.Sleep(40000);
            Console.WriteLine("Search Completed. Stopping...");
            search.Stop();
            Console.WriteLine("Search Stopped. Broadcasting Attributes...");
            broadcast.startBroadcast(this);
            Console.WriteLine("WAITING TO PRINT CONNECTED DEVICES");
            Thread.Sleep(10000);
            Console.WriteLine("PRINTING CONNECTED DEVICES");
            foreach(var d in devices) {
                Console.WriteLine("Device Name: " + d.name + ", Info: " + d.device.DeviceId);
            }
        }

        public void addDevice(ConnectedDevice device)
        {
            Console.BackgroundColor = ConsoleColor.Green;
            Console.WriteLine("Added " + device.name);
            Console.ResetColor();
            devices.AddLast(device);
        }

        public void sendMessage(string message, ConnectedDevice recipient)
        {
            string s = System.Environment.MachineName;
            string r = recipient.name;
            Message m = new Message(s, r, message);
            recipient.sendMessage(m);
        }

        public void receiveMessage(Message m)
        {
            Console.WriteLine("Message Received From: " + m.sender);
        }

    }
}
