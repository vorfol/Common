using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Vorfol.Common {
    public class SelectNetFromConsole {
        public static async Task<UnicastIPAddressInformation> start() {

            string choice = "";

            Console.WriteLine("Select interface");
            List<NetworkInterface> netCards = new List<NetworkInterface>();
            foreach(var netCard in NetworkInterface.GetAllNetworkInterfaces()) {
                if (netCard.OperationalStatus == OperationalStatus.Up) {
                    netCards.Add(netCard);
                    string mac = netCard.GetPhysicalAddress().ToString();
                    string ipAddr = "";
                    foreach(var card_addr in netCard.GetIPProperties().UnicastAddresses) {
                        if (card_addr.Address.AddressFamily == AddressFamily.InterNetwork) {
                            ipAddr = card_addr.Address.ToString();
                            break;
                        }
                    }
                    Console.WriteLine(netCards.Count + ": " + 
                        netCard.Name + ", " + 
                        netCard.Description + 
                        ", MAC: " + (String.IsNullOrEmpty(mac)?"none":mac) + 
                        ", IP: " + (String.IsNullOrEmpty(ipAddr)?"none":ipAddr));
                    if (string.IsNullOrEmpty(choice) && !string.IsNullOrEmpty(mac)) {
                        choice = netCards.Count.ToString();
                    }
                }
            }
            if (netCards.Count == 0) {
                Console.WriteLine("No Ethernet found");
                return null;
            }
            Console.Write("[" + choice + "] (you have only 5 seconds to choose another interface) ");
            
            CancellationTokenSource cancelReadConsole = new CancellationTokenSource();
            var readConsoleTask = Task.Run(async () => {
                do {
                    while (!Console.KeyAvailable) {
                        await Task.Delay(100);
                    }
                    var inner_choice = Console.ReadKey(true).KeyChar.ToString();
                    int i = -1;
                    if (int.TryParse(inner_choice, out i)) {
                        --i;
                        if (i >= 0 && i < netCards.Count) {
                            choice = inner_choice;
                            return;
                        }
                    }
                    Console.Beep(400, 300);
                } while(true);
            }, cancelReadConsole.Token);

            var completedTask = await Task.WhenAny(Task.Delay(5000), readConsoleTask);
            cancelReadConsole.Cancel();
            Console.WriteLine(choice);

            int cardIdx = int.Parse(choice) - 1;

            foreach(var unicastAddrress in netCards[cardIdx].GetIPProperties().UnicastAddresses) {
                if (unicastAddrress.Address.AddressFamily == AddressFamily.InterNetwork) {
                    // RETURN NOW
                    return unicastAddrress;
                }
            }

            return null;
        }
    }
}