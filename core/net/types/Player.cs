using Casanova.core.main.units;
using Godot;

namespace Casanova.core.net.types
{
    public class Player
    {
        public int id;
        public string username;
        public Unit unit;
        public bool isLocal;

        public Player(int _id, string _username, Unit _unit, bool _isLocal = false)
        {
            id = _id;
            username = _username;
            unit = _unit;
            isLocal = _isLocal;
        }
    }
}