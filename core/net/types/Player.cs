using Casanova.core.main.units;
using Godot;

namespace Casanova.core.net.types
{
    public class Player
    {
        public int id;
        public string username;
        public PlayerUnit PlayerUnit;
        public bool isLocal;

        public Player(int _id, string _username, PlayerUnit playerUnit, bool _isLocal = false)
        {
            id = _id;
            username = _username;
            PlayerUnit = playerUnit;
            isLocal = _isLocal;
        }
    }
}