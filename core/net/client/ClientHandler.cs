using Godot;

namespace Casanova.core.net.client
{
    public class ClientHandler : Node
    {
        public void ConnectToServer()
        {
            // awake client
            new Client().Awake();

            GD.Print("Connecting to server");
            Client.instance.ConnectToServer();
        }
        
    }
    

}
