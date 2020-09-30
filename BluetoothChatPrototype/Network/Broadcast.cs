using System;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace BluetoothChatPrototype.Network
{
    class Broadcast
    {
        private RfcommServiceProvider rfcommProvider;
        private StreamSocketListener socketListener;
        private StreamSocket socket;
        private DataWriter writer;

        public async void StartRfcommServer()
        {
            try
            {
                rfcommProvider = await RfcommServiceProvider.CreateAsync(RfcommServiceId.FromUuid(Constants.Constants.broadcastGuid));
            }
            // Catch exception if bluetooth is not enabled.
            catch (Exception ex) when ((uint)ex.HResult == 0x800710DF)
            {
                // Bluetooth is off
                Console.WriteLine("Bluetooth is not enabled: " + ex.Message);
                return;
            }

            // Create a listener for this service and start listening
            socketListener = new StreamSocketListener();
            socketListener.ConnectionReceived += OnConnectionReceived;
            var rfcomm = rfcommProvider.ServiceId.AsString();

            await socketListener.BindServiceNameAsync(rfcommProvider.ServiceId.AsString(), SocketProtectionLevel.BluetoothEncryptionAllowNullAuthentication);

            InitializeSDPAttributes(rfcommProvider);

            try
            {
                rfcommProvider.StartAdvertising(socketListener, true);
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not advertise: " + e.Message);
                return;
            }

            Console.WriteLine("Listening for connections.");
        }

        // Init the SDP attributes
        private void InitializeSDPAttributes(RfcommServiceProvider rfcommProvider)
        {
            Console.WriteLine("Initializing SDP Attributes...");
            var sdpWriter = new DataWriter();
            sdpWriter.WriteByte(Constants.Constants.SdpServiceNameAttributeType);
            sdpWriter.WriteByte((byte)Constants.Constants.SdpServiceName.Length);
            sdpWriter.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
            sdpWriter.WriteString(Constants.Constants.SdpServiceName);

            // Set the SDP Attribute on the RFCOMM Service Provider.
            rfcommProvider.SdpRawAttributes.Add(Constants.Constants.SdpServiceNameAttributeId, sdpWriter.DetachBuffer());
            Console.WriteLine("Done initializing SDP attributes.");
        }

        // Connect when there is a connection received.
        private async void OnConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            Console.WriteLine("Connection Received from: " + sender.Information);
            // Make sure the old listener is gone.
            socketListener.Dispose();
            socketListener = null;

            try
            {
                socket = args.Socket;
            }
            catch(Exception e)
            {
                Disconnect();
                Console.WriteLine("Error while creating socket: " + e.Message);
                return;
            }

            var device = await BluetoothDevice.FromHostNameAsync(socket.Information.RemoteHostName);

            writer = new DataWriter(socket.OutputStream);
            var reader = new DataReader(socket.InputStream);
            Console.WriteLine("Connected to Client: " + device.Name);
        }

        private async void Disconnect()
        {
            if(rfcommProvider != null)
            {
                rfcommProvider.StopAdvertising();
                rfcommProvider = null;
            }

            if(socketListener != null)
            {
                socketListener.Dispose();
                socketListener = null;
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
         }

    }
}
