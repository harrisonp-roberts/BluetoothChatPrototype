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

namespace BluetoothChatPrototype.Network

{

    public class Receive
    {
        Windows.Devices.Bluetooth.Rfcomm.RfcommServiceProvider _provider;
        string deviceId;
        DeviceWatcher deviceWatcher;

        public ObservableCollection<DeviceInformation> ResultCollection
        {
            get;
            private set;
        }

        private BluetoothDevice bluetoothDevice;

        private void StartUnpairedDeviceWatcher()
        {
            Console.WriteLine("StartUnpairedDeviceWatcher()");
            // Request additional properties
            string[] requestedProperties = new string[] { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected" };

            deviceWatcher = DeviceInformation.CreateWatcher("(System.Devices.Aep.ProtocolId:=\"{e0cbf06c-cd8b-4647-bb8a-263b43f0f974}\")",
                                                            requestedProperties,
                                                            DeviceInformationKind.AssociationEndpoint);

            // Hook up handlers for the watcher events before starting the watcher
            deviceWatcher.Added += new TypedEventHandler<DeviceWatcher, DeviceInformation>(async (watcher, deviceInfo) =>
            {
                ResultCollection.Add(deviceInfo);
                // Since we have the collection databound to a UI element, we need to update the collection on the UI thread.
                Console.WriteLine("Added " + deviceInfo.Name);
            });

            deviceWatcher.Updated += new TypedEventHandler<DeviceWatcher, DeviceInformationUpdate>(async (watcher, deviceInfoUpdate) =>
            {
                Console.WriteLine("Updated");
            });

            deviceWatcher.EnumerationCompleted += new TypedEventHandler<DeviceWatcher, Object>(async (watcher, obj) =>
            {
                foreach (var x in ResultCollection)
                {
                    Console.WriteLine("Connecting to " + x.Name);
                    connect(x);
                }   

            });

            deviceWatcher.Removed += new TypedEventHandler<DeviceWatcher, DeviceInformationUpdate>(async (watcher, deviceInfoUpdate) =>
            {
                Console.WriteLine("Removed");

                // Since we have the collection databound to a UI element, we need to update the collection on the UI thread.

            });

            deviceWatcher.Stopped += new TypedEventHandler<DeviceWatcher, Object>(async (watcher, obj) =>
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

            if(bluetoothDevice == null)
            {
                Console.WriteLine("Device is null");
            }

        }

        public void Initialize()
        {
            ResultCollection = new ObservableCollection<DeviceInformation>();
            Console.WriteLine("Initializing DeviceWatcher");
            StartUnpairedDeviceWatcher();
            Console.WriteLine("DeviceWatcher Started");





        }
        public async void save() { 
            Console.WriteLine("Initializing Receiver");

            var rfcomm = RfcommDeviceService.FromIdAsync(RfcommServiceId.SerialPort.AsString());
            var selector = BluetoothDevice.GetDeviceSelectorFromPairingState(false);

            deviceWatcher = DeviceInformation.CreateWatcher(selector, null, DeviceInformationKind.AssociationEndpoint);

            deviceWatcher.Added += async (w, i) =>
            { 
            };

            Console.WriteLine("Status: " + deviceWatcher.Status);

            selector.GetEnumerator();

            Console.WriteLine(selector);
        }
    }
}
