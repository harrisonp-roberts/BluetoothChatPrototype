using System;
using System.Collections.Generic;
using Windows.Storage.Streams;
using Windows.Devices.Bluetooth;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace BluetoothChatPrototype.Network
{
    public class ConnectedDevice
    {
        public string name { get; set; }
        public BluetoothDevice device { get; }
        DataWriter writer;
        DataReader reader;
        public LinkedList<Message> messages { get; }
        public NetworkController netctl;

        public ConnectedDevice(string name, BluetoothDevice device, DataWriter writer, DataReader reader, NetworkController netctl)
        {
            messages = new LinkedList<Message>();
            this.name = name;
            this.device = device;
            this.writer = writer;
            this.reader = reader;
            this.netctl = netctl;
            ReceiveLoop();
        }
        public async void SendMessage(Message message)
        {
            var serializedMessage = Serialize(message);
            writer.WriteUInt32((uint)serializedMessage.Length);
            writer.WriteBytes(serializedMessage);
            await writer.StoreAsync();
            messages.AddLast(message);
        }

        public async void ReceiveLoop()
        {

            try
            {
                uint size = await reader.LoadAsync(sizeof(uint));

                if(size == 0)
                {
                    Logging.Log.Error("Host Disconnection. Removing " + name);
                    netctl.RemoveDevice(this);
                }

                if (size < sizeof(uint))
                {
                    Logging.Log.Error("Size Not SIZEOF");
                    return;
                }

                uint length = reader.ReadUInt32();
                uint actualStringLength = await reader.LoadAsync(length);

                if (actualStringLength != length)
                {
                    Logging.Log.Error("LENGTH NOT ACTUAL");
                    return;
                }

                var bytes = new byte[length];
                reader.ReadBytes(bytes);

                var message = Deserialize(bytes);
                message.timestamp = DateTime.Now;
                messages.AddLast(message);

                netctl.ReceiveMessage(message);

                ReceiveLoop();
            }
            catch (Exception ex)
            {
                Logging.Log.Error("An Error Occured");
            }
        }
        

        private byte[] Serialize(Object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
        private Message Deserialize(byte[] arrBytes)
        {
            using (var memStream = new MemoryStream())
            {
                var binForm = new BinaryFormatter();
                memStream.Write(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                var obj = binForm.Deserialize(memStream);
                return (Message)obj;
            }
        }

    }
}
