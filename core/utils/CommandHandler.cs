using System;
using System.Collections.Generic;
using Casanova.core.net.types;

namespace Casanova.core.utils
{
    public class CommandHandler
    {
        public string prefix;
        public Dictionary<string, Command> commands = new Dictionary<string, Command>();

        public void register(Command command)
        {
            commands.Add(command.name, command);
        }

        public CommandHandler(string _prefix)
        {
            prefix = _prefix;
        }

        public bool handle(Player caller, string msg)
        {
            if (!msg.StartsWith(prefix))
                return false;
            
            Command command = null;
            foreach (var cmd in commands.Values)
            {
                if (msg.IndexOf(cmd.name, StringComparison.CurrentCultureIgnoreCase) == 1 && msg.Length >= prefix.Length + cmd.name.Length + 1 && msg[prefix.Length + cmd.name.Length] == ' ')
                {
                    command = cmd;
                }
            }

            if (command != null)
            {
                command.action.Invoke(caller, msg.Substring(prefix.Length + command.name.Length + 1).Split(' '));
                return true;
            }

            return false;
        }
    }

    public class Command
    {
        public string name;
        public string desc;

        // action passed with array of parameters
        public Action<Player, object[]> action;

        public Command(string _name, string _desc, Action<Player, object[]> _action)
        {
            name = _name;
            desc = _desc;
            action = _action;
        }
    }
}