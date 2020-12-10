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
            Thread child = new Thread(new ParameterizedThreadStart(search.Initialize));
            child.Start(this);
            //Thread.Sleep(30000);
            //search.Stop();
            //search = null;
            //child.Abort();

            //broadcast.startBroadcast(this);
        }

        public void addDevice(ConnectedDevice device)
        {
            Logging.Log.Info("Added " + device.name);
            if (devices.Find(device) == null)
            {
                devices.AddLast(device);
            } else
            {
                Logging.Log.Error("Device already exists");
            }
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
