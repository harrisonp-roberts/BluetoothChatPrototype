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
        private LinkedList<ConnectedDevice> devices;

        public NetworkController()
        {
            search = new Receive();
            broadcast = new Broadcast();
            devices = new LinkedList<ConnectedDevice>();
        }

        // This class should be Broadcaster/Receiver agnostic
        // Basically that means that, when start is called, this class
        // will search for existing connections for 10 seconds, then 
        // it will fall back to receiving new connections.
        // No matter what stage a new connection is discovered at, it should be
        // added to a list of connections 
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
        }

        public void addDevice(ConnectedDevice device)
        {
            Console.WriteLine("Adding Device.");
            devices.AddLast(device);
        }

    }
}
