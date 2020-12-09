using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
using static BluetoothChatPrototype.Logging.Log;

namespace BluetoothChatPrototype.Network

{

    public class Receive
    {
        DeviceWatcher deviceWatcher;
        private BluetoothDevice targetDevice;
        private NetworkController netctl;
        public ObservableCollection<DeviceInformation> BroadcastingDevices
        {
            get;
            set;
        }

        private void InitWatch()
        {
            Logging.Log.Trace("Initializing Device Watcher");

            deviceWatcher = DeviceInformation.CreateWatcher("(System.Devices.Aep.ProtocolId:=\"{e0cbf06c-cd8b-4647-bb8a-263b43f0f974}\")",
                                                            Constants.Constants.deviceProperties,
                                                            DeviceInformationKind.AssociationEndpoint);

            deviceWatcher.Added += new TypedEventHandler<DeviceWatcher, DeviceInformation>((watcher, foundDevice) =>
            {
                Logging.Log.Trace("Attempting to connect to " + foundDevice.Name);
                connect(foundDevice);
            });

            deviceWatcher.EnumerationCompleted += new TypedEventHandler<DeviceWatcher, object> ((deviceWatcher, obj) =>
            {
                deviceWatcher.Stop();
            });

            deviceWatcher.Stopped += new TypedEventHandler<DeviceWatcher, object>((deviceWatcher, obj) =>
            {
                deviceWatcher.Start();
            });

            Logging.Log.Trace("Device Watcher Initialized. Searching for connections...");
            deviceWatcher.Start();
            Logging.Log.Trace("DeviceWatcher Status: " + deviceWatcher.Status);
        }

        private async void connect(DeviceInformation devInfo)
        {
            try
            {
                Logging.Log.Trace("Connecting to Bluetooth Device: " + devInfo.Name);
                targetDevice = await BluetoothDevice.FromIdAsync(devInfo.Id);

                targetDevice.ConnectionStatusChanged += new TypedEventHandler<BluetoothDevice, object>(async (btd, obj) =>
                {
                    Logging.Log.Trace("Changed Connection Status for " + btd.Name + " to " + btd.ConnectionStatus);
                });

                if (targetDevice != null)
                {
                    const BluetoothCacheMode bluetoothCacheMode = BluetoothCacheMode.Uncached;
                    var targetBluetoothServices = await targetDevice.GetRfcommServicesForIdAsync(RfcommServiceId.FromUuid(Constants.Constants.broadcastGuid), bluetoothCacheMode);

                    var retrievedServices = targetBluetoothServices.Services;

                    if (retrievedServices.Count > 0)
                    {
                        var retrievedService = retrievedServices[0];
                        var attributes = await retrievedService.GetSdpRawAttributesAsync();
                        var attributeReader = DataReader.FromBuffer(attributes[Constants.Constants.serviceNameID]);
                        var attributeType = attributeReader.ReadByte();
                        var serviceNameLength = attributeReader.ReadByte();
                        StreamSocket bluetoothSocket = null;
                        DataWriter bluetoothWriter = null;

                        //lock (this)
                      //  {
                            bluetoothSocket = new StreamSocket();
                        //}

                        await bluetoothSocket.ConnectAsync(retrievedService.ConnectionHostName, retrievedService.ConnectionServiceName);
                        bluetoothWriter = new DataWriter(bluetoothSocket.OutputStream);
                        DataReader chatReader = new DataReader(bluetoothSocket.InputStream);

                        Logging.Log.Trace("Connection to " + devInfo.Name + " established. Awaiting data...");

                        var connectedDevice = new ConnectedDevice(devInfo.Name, targetDevice, bluetoothWriter, chatReader, netctl);
                        netctl.addDevice(connectedDevice);

                        readSocket(chatReader, bluetoothSocket);
                    }
                    else
                    {
                        Logging.Log.Error("No valid services could be found on " + devInfo.Name);
                    }
                }
                else
                {
                    Logging.Log.Error("Connection failed, target device not found.");
                }
            }
            catch (Exception ex)
            {
                Logging.Log.Error("An Exception Occured. The target device may not have an active service, or something went wrong.\n" + ex.Message);
                return;
            }
        }

        private async void readSocket(DataReader chatReader, StreamSocket chatSocket)
        {
            try
            {
                var size = await chatReader.LoadAsync(sizeof(uint));
                var stringLength = chatReader.ReadUInt32();
                var actualStringLength = await chatReader.LoadAsync(stringLength);

                if (actualStringLength == stringLength)
                {
                    readSocket(chatReader, chatSocket);
                } 
            }
            catch (Exception ex)
            {
                Logging.Log.Error("Host Disconnection.");
            }
        }

        public async void Initialize(NetworkController netctl)
        {
            this.netctl = netctl;
            BroadcastingDevices = new ObservableCollection<DeviceInformation>();
            InitWatch();
        }

        public void Stop()
        {
            if (deviceWatcher != null && deviceWatcher.Status.Equals(DeviceWatcherStatus.Started))
            {
                deviceWatcher.Stop();
            }

            deviceWatcher = null;
        }

    }
}
