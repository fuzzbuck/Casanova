using Godot;

namespace Casanova.core.net.client
{
    public class ClientHandler : Node
    {
        public override void _Process(float delta)
        {
            ThreadManager.UpdateMain();
        }

        public void ConnectToServer()
        {
            // awake client
            new Client().Awake();

            GD.Print("Connecting to server");
            Client.instance.ConnectToServer();
        }
        
    }
    

}
