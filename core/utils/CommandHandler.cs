using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Casanova.core.net.server;
using Casanova.core.net.types;
using Godot;

namespace Casanova.core.utils
{
    public enum HandleResponse
    {
        Executed, BadPrefix, BadArguments, NonExistent, NoPermission
    }
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

        public (HandleResponse, Command) handle(string msg, Player caller = null)
        {
            if (!msg.StartsWith(prefix) && prefix != String.Empty)
                return (HandleResponse.BadPrefix, null);
            
            Command command = null;
            foreach (var cmd in commands.Values)
            {
                if (msg.IndexOf(cmd.name, StringComparison.CurrentCultureIgnoreCase) == prefix.Length)
                {
                    command = cmd;
                }
            }

            if (command != null)
            {
                if (!command.VerifyExec(caller))
                    return (HandleResponse.NoPermission, command);
                
                var textparams = new string[]{};

                if (command.textparam.Length > 0)
                {
                    if (msg.Length > prefix.Length + command.name.Length + 1)
                        textparams = msg.Substring(prefix.Length + command.name.Length + 1).Split(' ');
                    else
                        return (HandleResponse.BadArguments, command);
                }

                if (command.VerifyParams(String.Join(" ", textparams)))
                {
                    command.action.Invoke(caller, textparams);
                    return (HandleResponse.Executed, command);
                }
                else
                {
                    return (HandleResponse.BadArguments, command);
                }
            }

            return (HandleResponse.NonExistent, command);
        }
    }

    public class Command
    {
        public string name;
        public string desc;
        public string textparam;

        // action passed with array of parameters
        public Action<Player, object[]> action;

        public Command(string _name, string _textparam, string _desc, Action<Player, object[]> _action)
        {
            name = _name;
            desc = _desc;
            textparam = _textparam;
            action = _action;
        }


        public bool VerifyParams(string textparams)
        {
            return (textparams.Length == 0 || textparams.Trim().Count(i => i == ' ') == textparam.Trim().Count(i => i == ' '));
        }

        public virtual bool VerifyExec(Player player = null)
        {
            return true;
        }
    }

    public class AdminCommand : Command
    {
        public AdminCommand(string _name, string _textparam, string _desc, Action<Player, object[]> _action) : base(_name, _textparam, _desc, _action)
        { }

        public override bool VerifyExec(Player player = null)
        {
            if (player != null && player.IsHost)
                return true;

            Server.SendNoPermission(player);
            return false;
        }
    }
}