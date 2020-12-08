using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.Devices.Bluetooth;

namespace BluetoothChatPrototype.Network
{
    public class ConnectedDevice
    {
        string name { get; set; }
        BluetoothDevice device;
        DataWriter writer;
        DataReader reader;

        public ConnectedDevice(string name, BluetoothDevice device, DataWriter writer, DataReader reader)
        {
            this.name = name;
            this.device = device;
            this.writer = writer;
            this.reader = reader;
        }
        public void sendMessage(string message)
        {
        }
    }
}
