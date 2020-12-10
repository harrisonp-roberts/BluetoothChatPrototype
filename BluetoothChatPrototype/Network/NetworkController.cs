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
            Logging.Log.Trace("Initializing Search...");
            search.Initialize(this);
            Logging.Log.Trace("Search Initialized");
            Thread.Sleep(40000);
            Logging.Log.Trace("Search Completed. Stopping...");
            search.Stop();
            search = null;
            Logging.Log.Trace("Search Stopped. Broadcasting Attributes...");
            broadcast.startBroadcast(this);
            Logging.Log.Trace("WAITING TO PRINT CONNECTED DEVICES");
            Thread.Sleep(10000);
            Logging.Log.Trace("PRINTING CONNECTED DEVICES");
            foreach(var d in devices) {
                Logging.Log.Trace("Device Name: " + d.name + ", Info: " + d.device.DeviceId);
            }
        }

        public void addDevice(ConnectedDevice device)
        {
            Logging.Log.Info("Added " + device.name);
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
            Logging.Log.Info("Message Received From: " + m.sender);
        }

    }
}
