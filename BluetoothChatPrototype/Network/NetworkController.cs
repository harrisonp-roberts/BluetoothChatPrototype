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

        public void Start()
        {
            Thread child = new Thread(new ParameterizedThreadStart(search.Initialize));
            child.Start(this);
            Thread.Sleep(30000);
            search.Stop();
            search = null;
            child.Abort();

            broadcast.StartBroadcast(this);
        }

        public void AddDevice(ConnectedDevice device)
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

        public void RemoveDevice(ConnectedDevice device)
        {
            Logging.Log.Trace("Removing " + device.name);
            Logging.Log.Info(device.name + " Has Disconnected.");
            devices.Remove(device);
        }

        public void SendMessage(string message, ConnectedDevice recipient)
        {
            string s = System.Environment.MachineName;
            string r = recipient.name;
            Message m = new Message(s, r, message);
            recipient.SendMessage(m);
        }

        public void ReceiveMessage(Message m)
        {
            Logging.Log.Info("Message Received From: " + m.sender);
        }

    }
}
