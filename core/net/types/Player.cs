using Casanova.core.main.units.Player;
using Godot;

namespace Casanova.core.net.types
{
    public class Player
    {
        public int id;
        public string username;
        public PlayerUnit unit;

        public Player(int _id, string _username, PlayerUnit _unit)
        {
            id = _id;
            username = _username;
            unit = _unit;
        }
    }
}