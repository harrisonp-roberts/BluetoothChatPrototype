using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluetoothChatPrototype.Logging
{
    public static class Log
    {
        private static bool trace = true;
        private static bool error = true;
        private static bool info = true;

        public static void Trace(string msg)
        {
            if (trace)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("TRACE: " + msg);
                Console.ResetColor();
            }
        }

        public static void Error(string err)
        {
            if(error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR: " + err);
                Console.ResetColor();
            }
        }

        public static void Info(string msg)
        {
            if(info)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("INFO MESSAGE: " + msg);
                Console.ResetColor();
            }
        }
    }
}
