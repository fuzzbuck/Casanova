using Casanova.core.main.units;

namespace Casanova.core.net.types
{
    public class Player
    {
        public readonly int Id;
        public readonly bool IsLocal;
        public readonly string Username;
        public Unit Unit;

        public Player(int id, string username, Unit _unit, bool isLocal = false)
        {
            Id = id;
            Username = username;
            Unit = _unit;
            IsLocal = isLocal;
        }
    }
}