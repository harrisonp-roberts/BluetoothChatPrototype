using BluetoothChatPrototype.Constants;
using BluetoothChatPrototype.Network;
using BluetoothChatPrototype.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BluetoothChatPrototype
{
    class Program
    {
        static void Main(string[] args)
        {

            NetworkController netCtl = new NetworkController();
            Thread thr = new Thread(new ThreadStart(netCtl.start));
            thr.Start();

            UserInterface ui = new UserInterface(netCtl);


            //BEGIN GUI
            //Task task = Task.Run(() =>
            //{
            //    Form1 form = new Form1();
            //    form.Text = "Ad Hoc Chat Application";
            //    form.ShowDialog();
            //});

        }
    }
}
