using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
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
            receiveLoop();
        }
        public async void sendMessage(Message message)
        {
            Console.WriteLine("SendMessage Called!");
            var serializedMessage = Serialize(message);
            Console.WriteLine("Message Serialized");
            writer.WriteUInt32((uint)serializedMessage.Length);
            writer.WriteBytes(serializedMessage);
            Console.WriteLine("Message Written to stream");
            var x = await writer.StoreAsync();
            Console.WriteLine(x);
            messages.AddLast(message);
            Console.WriteLine("Message Added to History");
        }

        public async void receiveLoop()
        {

            try
            {
                uint size = await reader.LoadAsync(sizeof(uint));
                Console.WriteLine("Size Received. Size: " + size);
                if (size < sizeof(uint))
                {
                    Console.WriteLine("Size Not SIZEOF");
                    return;
                }

                uint length = reader.ReadUInt32();
                Console.WriteLine("Length Received. Length: " + length);
                uint actualStringLength = await reader.LoadAsync(length);
                if (actualStringLength != length)
                {
                    Console.WriteLine("LENGTH NOT ACTUAL");
                    // netctl.disconnect(this)
                    // The underlying socket was closed before we were able to read the whole data
                    return;
                }

                var bytes = new byte[length];
                Console.WriteLine("Received Bytes.");
                reader.ReadBytes(bytes);

                var message = Deserialize(bytes);
                messages.AddLast(message);

                netctl.receiveMessage(message);

                receiveLoop();
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
