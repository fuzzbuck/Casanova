using System;
using Godot;

namespace Casanova.core.net.client
{
    public class ClientHandler : Node
    {
        public void ConnectToServer(string ip)
        {
            // awake client
            new Client().Awake();

            GD.Print("Connecting to server");
            string[] addy = ip.Split(":");
            if (addy.Length > 1 && int.TryParse(addy[1], out int temp))
            {
                Client.instance.ConnectToServer(addy[0], int.Parse(addy[1]));
            }
            else
            {
                throw new Exception("Invalid IP address issued! Requires IP/Hostname & Port seperated by `:`");
            }
        }
        
    }
    

}
