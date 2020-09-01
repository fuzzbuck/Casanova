using System;
using System.Net;
using Godot;

namespace Casanova.core.net.client
{
    public class ClientHandler : Node
    {
        public void ConnectToServer(string ip)
        {
            // awake client
            new Client().Awake();

            
            string[] addy = ip.Split(":");
            int port = Vars.Networking.defaultPort;

            if (addy.Length > 1 && int.TryParse(addy[1], out int newport))
                port = newport;

            if (!IPAddress.TryParse(addy[0], out IPAddress address))
            {
                var ips = Dns.GetHostAddresses(addy[0]);
                if (ips.Length > 0)
                {
                    addy[0] = ips[0].ToString();
                }
            }
            
            GD.Print($"Parsed {addy}");
            
            Client.instance.ConnectToServer(addy[0], port);
        }
        
    }
    

}
