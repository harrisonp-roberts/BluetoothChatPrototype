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


        private void InitWatch()
        {
            Logging.Log.Trace("Initializing DeviceWatcher");

            deviceWatcher = DeviceInformation.CreateWatcher("(System.Devices.Aep.ProtocolId:=\"{e0cbf06c-cd8b-4647-bb8a-263b43f0f974}\")",
                                                            Constants.Constants.deviceProperties,
                                                            DeviceInformationKind.AssociationEndpoint);

            deviceWatcher.Added += new TypedEventHandler<DeviceWatcher, DeviceInformation>((watcher, foundDevice) =>
            {
                Logging.Log.Trace("Found device: " + foundDevice.Name);
                Connect(foundDevice);
            });

            deviceWatcher.EnumerationCompleted += new TypedEventHandler<DeviceWatcher, object> ((deviceWatcher, obj) =>
            {
                deviceWatcher.Stop();
                Logging.Log.Trace("DeviceWatcher Status: " + deviceWatcher.Status);
            });

            deviceWatcher.Stopped += new TypedEventHandler<DeviceWatcher, object>((deviceWatcher, obj) =>
            {
                deviceWatcher.Start();
                Logging.Log.Trace("DeviceWatcher Status: " + deviceWatcher.Status);
            });

            deviceWatcher.Start();
            Logging.Log.Trace("DeviceWatcher Status: " + deviceWatcher.Status);
        }

        private async void Connect(DeviceInformation devInfo)
        {
            try
            {
                Logging.Log.Trace("Attempting to connect to Bluetooth Device: " + devInfo.Name);
                targetDevice = await BluetoothDevice.FromIdAsync(devInfo.Id);

                targetDevice.ConnectionStatusChanged += new TypedEventHandler<BluetoothDevice, object>(async (btd, obj) =>
                {
                    Logging.Log.Trace("Changed Connection Status for " + btd.Name + " to " + btd.ConnectionStatus);
                });

                if (targetDevice != null)
                {
                    const BluetoothCacheMode bluetoothCacheMode = BluetoothCacheMode.Uncached;
                    var targetBluetoothServices = await targetDevice.GetRfcommServicesForIdAsync(RfcommServiceId.FromUuid(Constants.Constants.broadcastGuid), bluetoothCacheMode);

                    Logging.Log.Trace("Searching Target Device " + devInfo.Name + " for Bluetooth Chat Service.");

                    var retrievedServices = targetBluetoothServices.Services;

                    if (retrievedServices.Count > 0)
                    {
                        Logging.Log.Trace("Bluetooth Chat Service Found on " + devInfo.Name);
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

                        Logging.Log.Trace("Connection to " + devInfo.Name + " Chat Service Established. Awaiting data...");

                        var connectedDevice = new ConnectedDevice(devInfo.Name, targetDevice, bluetoothWriter, chatReader, netctl);
                        netctl.AddDevice(connectedDevice);
                    }
                    else
                    {
                        Logging.Log.Trace("No valid services could be found on " + devInfo.Name + ". Ignoring.");
                    }
                }
                else
                {
                    Logging.Log.Trace("Target Device is Null.");
                }
            }
            catch (Exception ex)
            {
                Logging.Log.Error("An Exception Occured. The target device may not have an active service, or something went wrong.\n" + ex.Message);
                return;
            }
        }

        public void Initialize(object netctl)
        {
            this.netctl = (NetworkController)netctl;
            InitWatch();
        }

        public void Stop()
        {
            deviceWatcher.Stop();
            deviceWatcher = null;
        }

    }
}
