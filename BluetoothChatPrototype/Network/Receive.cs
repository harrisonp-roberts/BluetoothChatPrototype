using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Core;
using BluetoothChatPrototype.Constants;

namespace BluetoothChatPrototype.Network

{

    public class Receive
    {
        DeviceWatcher deviceWatcher;
        private BluetoothDevice bluetoothDevice;
        public ObservableCollection<DeviceInformation> BroadcastingDevices
        {
            get;
            set;
        }


        private void InitWatch()
        {

            deviceWatcher = DeviceInformation.CreateWatcher("(System.Devices.Aep.ProtocolId:=\"{e0cbf06c-cd8b-4647-bb8a-263b43f0f974}\")",
                                                            Constants.Constants.deviceProperties,
                                                            DeviceInformationKind.AssociationEndpoint);

            // Hook up handlers for the watcher events before starting the watcher
            deviceWatcher.Added += new TypedEventHandler<DeviceWatcher, DeviceInformation>((watcher, deviceInfo) =>
            {
                BroadcastingDevices.Add(deviceInfo);
                Console.WriteLine("Added " + deviceInfo.Name);
            });

            deviceWatcher.EnumerationCompleted += new TypedEventHandler<DeviceWatcher, Object>((watcher, obj) =>
            {
                foreach (var x in BroadcastingDevices)
                {
                    Console.WriteLine("Connecting to " + x.Name);
                    connect(x);
                }

            });

            deviceWatcher.Removed += new TypedEventHandler<DeviceWatcher, DeviceInformationUpdate>((watcher, deviceInfoUpdate) =>
            {
                Console.WriteLine("Removed device");
            });

            deviceWatcher.Stopped += new TypedEventHandler<DeviceWatcher, Object>((watcher, obj) =>
            {
                Console.WriteLine("Stopped");
            });

            deviceWatcher.Start();
        }

        private async void connect(DeviceInformation dev)
        {
            DeviceAccessStatus accessStatus = DeviceAccessInformation.CreateFromId(dev.Id).CurrentStatus;
            Console.WriteLine("Device Access Status: " + accessStatus.ToString());

            try
            {
                Console.WriteLine("Connecting to Bluetooth Device: " + dev.Name);
                bluetoothDevice = await BluetoothDevice.FromIdAsync(dev.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An Exception Occured: " + ex.Message);
                return;
            }

            if (bluetoothDevice == null)
            {
                Console.WriteLine("Device is null");
                return;
            }
            else
            {
                Console.WriteLine("Device " + bluetoothDevice.Name + " is not null.");
            }

            var cacheMode = BluetoothCacheMode.Uncached;
            var rfcommServices = await bluetoothDevice.GetRfcommServicesForIdAsync(RfcommServiceId.FromUuid(Constants.Constants.broadcastGuid), cacheMode);

            var retrievedServices = rfcommServices.Services;

            if (retrievedServices.Count > 0)
            {
                var retrievedService = retrievedServices[0];
                var attributes = await retrievedService.GetSdpRawAttributesAsync();

                Console.WriteLine("Enumerating all SDP Attribute Values being broadcast by the service...");
                foreach (var val in attributes.Values)
                {
                    Console.WriteLine(val.ToString());
                }

                var attributeReader = DataReader.FromBuffer(attributes[Constants.Constants.SdpServiceNameAttributeId]);
                var attributeType = attributeReader.ReadByte();

                if (attributeType != Constants.Constants.SdpServiceNameAttributeType)
                {
                    Console.WriteLine("Unexpected format. Exiting");
                    return;
                }
                var serviceNameLength = attributeReader.ReadByte();

                //attributeReader.UnicodeEncoding = UnicodeEncoding.Utf8;
                StreamSocket chatSocket = null;
                DataWriter chatWriter = null;
                lock (this)
                {
                    chatSocket = new StreamSocket();
                }
                try
                {
                    await chatSocket.ConnectAsync(retrievedService.ConnectionHostName, retrievedService.ConnectionServiceName);

                    chatWriter = new DataWriter(chatSocket.OutputStream);

                    DataReader chatReader = new DataReader(chatSocket.InputStream);
                    ReceiveStringLoop(chatReader, chatSocket);
                }
                catch (Exception ex) when ((uint)ex.HResult == 0x80070490) // ERROR_ELEMENT_NOT_FOUND
                {
                    Console.WriteLine("Are you sure you're running the correct thing?");
                }
                catch (Exception ex) when ((uint)ex.HResult == 0x80072740) // WSAEADDRINUSE
                {
                    Console.WriteLine("There may be another active connection on the server.");
                }
            }
            else
            {
                Console.WriteLine("There are no valid services on " + dev.Name);
            }

            bluetoothDevice.ConnectionStatusChanged += new TypedEventHandler<BluetoothDevice, object>(async (btd, obj) =>
            {
                Console.WriteLine("Changed Connection Status for " + btd.Name + " to " + btd.ConnectionStatus);
            });

        }

        private async void ReceiveStringLoop(DataReader chatReader, StreamSocket chatSocket)
        {
            try
            {
                uint size = await chatReader.LoadAsync(sizeof(uint));
                if (size < sizeof(uint))
                {
                    //TODO cleanup and exit. Host disconnected
                    return;
                }

                uint stringLength = chatReader.ReadUInt32();
                uint actualStringLength = await chatReader.LoadAsync(stringLength);
                if (actualStringLength != stringLength)
                {
                    // The underlying socket was closed before we were able to read the whole data
                    return;
                }

                //ConversationList.Items.Add("Received: " + chatReader.ReadString(stringLength));

                ReceiveStringLoop(chatReader, chatSocket);
            }
            catch (Exception ex)
            {
                lock (this)
                {
                    if (chatSocket == null)
                    {
                        // Do not print anything here -  the user closed the socket.
                        if ((uint)ex.HResult == 0x80072745)
                            Console.WriteLine("Disconnect triggered by remote device.");
                        else if ((uint)ex.HResult == 0x800703E3)
                            Console.WriteLine("The I / O operation has been aborted because of either a thread exit or an application request.");
                    }
                    else
                    {
                        //Disconnect("Read stream failed with error: " + ex.Message);
                    }
                }
            }
        }

        public async void Initialize()
        {
            BroadcastingDevices = new ObservableCollection<DeviceInformation>();
            StartUnpairedDeviceWatcher();
        }

    }
}
