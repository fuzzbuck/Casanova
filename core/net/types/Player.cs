using Casanova.core.main.units;

namespace Casanova.core.net.types
{
    public class Player
    {
        public readonly short netId;
        public readonly bool isLocal;
        public readonly string Username;
        public Unit Unit;

        public Player(short _netId, string username, Unit _unit, bool isLocal = false)
        {
            netId = _netId;
            Username = username;
            Unit = _unit;
            this.isLocal = isLocal;
        }
    }
}