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

        public async void StartRfcommServer()
        {
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
                Console.WriteLine("Connected to Client: " + device.Name);
            }
            catch(Exception e)
            {
                disconnect();
                Console.WriteLine("Error while creating socket: " + e.Message);
            }


        }

        private void disconnect()
        {
            if(commServiceProvider != null)
            {
                commServiceProvider.StopAdvertising();
                commServiceProvider = null;
            }

            if(listener != null)
            {
                listener.Dispose();
                listener = null;
            }

            if(socket != null)
            {
                socket.Dispose();
                socket = null;
            }

            if(writer != null)
            {
                writer.DetachStream();
                writer = null;
            }
            Console.WriteLine("Disconnected from device.");
         }

    }
}
