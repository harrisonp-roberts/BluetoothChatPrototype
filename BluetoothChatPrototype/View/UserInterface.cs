using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BluetoothChatPrototype.Network;

namespace BluetoothChatPrototype.View
{
    public class UserInterface
    {
        NetworkController netctl;

        public UserInterface(NetworkController netctl)
        {
            this.netctl = netctl;

            string userIn = "";

            while(!userIn.ToLower().Equals("q"))
            {
                Console.WriteLine("Please Select an Option (1 - 3, q to quit):" +
                    "\n1. View available recipients" +
                    "\n2. View Messages" +
                    "\n3. Send Message");

                userIn = Console.ReadLine();

                if(userIn.Equals("1"))
                {
                    viewRecipients();
                } else if (userIn.Equals("2"))
                {
                    viewMessages();
                } else if (userIn.Equals("3"))
                {
                    sendMessage();
                }

            }
        }

        private void viewRecipients()
        {
            var devices = netctl.devices;
            Console.WriteLine();
            if(devices.Count == 0)
            {
                Console.WriteLine("There Are no Connected Devices.");
            }

            for(int i = 0; i < devices.Count; i++)
            {
                Console.WriteLine(i + ". " + devices.ElementAt(i).name);
            }

        }

        private void viewMessages()
        {
            var devices = netctl.devices;

            if (devices.Count > 0)
            {
                Console.WriteLine("\nWhose messages would you like to check?");

                for (int i = 0; i < devices.Count; i++)
                {
                    Console.WriteLine(i + ". " + devices.ElementAt(i).name);
                }

                Console.WriteLine();

                int userInt = -1;

                while (userInt < 0 || userInt > devices.Count)
                {
                    string userIn = Console.ReadLine();
                    userInt = int.Parse(userIn);
                    if (userInt >= 0 && userInt <= devices.Count)
                    {
                        var from = devices.ElementAt(userInt);
                        var messages = from.messages;

                        Console.WriteLine("\nConversation With " + from.name + "\n");

                        foreach (Message m in messages)
                        {
                            var sender = m.sender;
                            var message = m.message;
                            var time = m.timestamp;

                            Console.WriteLine(time.ToString() + " from " +sender + "\n> " + message + "\n");
                        }

                        Console.WriteLine();
                    }
                    else
                    {
                        Console.WriteLine("Invalid Input. Please Try Again\n");
                    }
                }
            } else
            {
                Console.WriteLine("There are no currently connected devices.");
            }
        }

        private void sendMessage()
        {
            var devices = netctl.devices;

            if (devices.Count > 0)
            {
                Console.WriteLine("Who would you like to message (0 - " + netctl.devices.Count + ")?");
                viewRecipients();

                int userInt = -1;

                while (userInt < 0 || userInt > devices.Count)
                {
                    var userIn = Console.ReadLine();
                    userInt = int.Parse(userIn);
                    if (userInt >= 0 && userInt <= devices.Count)
                    {
                        Console.WriteLine("\nEnter your message (Enter to send):");
                        string userMessage = Console.ReadLine();
                        netctl.sendMessage(userMessage, devices.ElementAt(userInt));
                        Console.WriteLine();
                    }
                    else
                    {
                        Console.WriteLine("Invalid Input. Please Try Again\n");
                    }
                }
            } else
            {
                Console.WriteLine("There are no currently connected devices.");
            }
            

        }

    }
}
