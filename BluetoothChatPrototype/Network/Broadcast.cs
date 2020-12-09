using System;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace BluetoothChatPrototype.Network
{
    class Broadcast
    {
        private RfcommServiceProvider commServiceProvider;
        private StreamSocketListener listener;
        private StreamSocket socket;
        private DataWriter writer;
        private NetworkController netctl;

        public async void startBroadcast(NetworkController netctl)
        {
            this.netctl = netctl;

            try
            {
                commServiceProvider = await RfcommServiceProvider.CreateAsync(RfcommServiceId.FromUuid(Constants.Constants.broadcastGuid));
                listener = new StreamSocketListener();
                listener.ConnectionReceived += recieveConnection;
                var rfcomm = commServiceProvider.ServiceId.AsString();

                await listener.BindServiceNameAsync(commServiceProvider.ServiceId.AsString(), SocketProtectionLevel.BluetoothEncryptionAllowNullAuthentication);

                Console.WriteLine("Initializing Session Description Protocal (SDP) Attributes");
                setupBroadcastAttributes(commServiceProvider);
                Console.WriteLine("SDP Attributes Initialized");

                commServiceProvider.StartAdvertising(listener, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occured advertising the bluetooth connection");
                Console.WriteLine(ex.Message);
                return;
            }

            Console.WriteLine("Broadcasting Connections.");
        }

        private void setupBroadcastAttributes(RfcommServiceProvider rfcommProvider)
        {
            Console.WriteLine("Initializing SDP Attributes...");
            var writer = new DataWriter();
            writer.WriteByte(Constants.Constants.type);
            writer.WriteByte((byte)Constants.Constants.serviceName.Length);
            writer.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
            writer.WriteString(Constants.Constants.serviceName);

            Console.WriteLine("Service Name: " + Constants.Constants.serviceName);
            Console.WriteLine("Provider Service ID: " + commServiceProvider.ServiceId.AsString());

            rfcommProvider.SdpRawAttributes.Add(Constants.Constants.serviceNameID, writer.DetachBuffer());
        }

        private async void recieveConnection(StreamSocketListener listener, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            Console.WriteLine("Connection Received from: " + listener.Information);

            try
            {
                socket = args.Socket;
                var device = await BluetoothDevice.FromHostNameAsync(socket.Information.RemoteHostName);

                writer = new DataWriter(socket.OutputStream);
                var reader = new DataReader(socket.InputStream);
                var connectedDevice = new ConnectedDevice(device.Name, device, writer, reader, netctl);
                netctl.addDevice(connectedDevice);
                Console.WriteLine("Connected to Client: " + device.Name);
            }
            catch(Exception e)
            {
                Console.WriteLine("Error while creating socket: " + e.Message);
            }
        }
    }
}
