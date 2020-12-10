using Casanova.core.main.units;

namespace Casanova.core.net.types
{
    public class Player
    {
        public readonly short netId;
        public readonly bool isLocal;
        public readonly string Username;
        public Unit Unit;

        public Player(short _netId, string _username, bool _isLocal = false)
        {
            netId = _netId;
            Username = _username;
            isLocal = _isLocal;
        }
    }
}