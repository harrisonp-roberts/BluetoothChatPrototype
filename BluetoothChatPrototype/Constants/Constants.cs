﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluetoothChatPrototype.Constants
{
    class Constants
    {
        public static readonly string[] deviceProperties = new string[] { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected" };
        public static readonly Guid broadcastGuid = Guid.Parse("c1d79703-f9d6-4e1a-a825-aa4d09180620");
        public const string serviceName = "CPSC4210 - Bluetooth Chat Application.";

        public const UInt16 serviceNameID = 0x100;
        public const byte type = (4 << 3) | 5;
    }
}
