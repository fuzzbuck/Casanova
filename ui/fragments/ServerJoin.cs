using System;
using Casanova.core;
using Casanova.core.net.client;
using Casanova.core.utils;
using Godot;
using LineEdit = Casanova.ui.elements.LineEdit;

namespace Casanova.ui.fragments
{
    public class ServerJoin : Overlay
    {
        private Button connect_button;
        
        private HBoxContainer ipbox;
        private HBoxContainer userbox;
        
        private LineEdit ip_field;
        private LineEdit username_field;

        public override void _Ready()
        {
            base._Ready();

            connect_button = content.GetNode<Button>("ConnectButton");
            userbox = content.GetNode<HBoxContainer>("UsernameBox");
            ipbox = content.GetNode<HBoxContainer>("IpBox");

            username_field = userbox.GetNode<LineEdit>("LineEdit");
            username_field.Text = Vars.PersistentData.username;

            ip_field = ipbox.GetNode<LineEdit>("LineEdit");

            username_field.Connect("text_changed", this, "_onUsernameFieldTextChange");
            ip_field.Connect("text_changed", this, "_onIpFieldTextChange");
            connect_button.Connect("pressed", this, "_onConnectButtonPress");

            Vars.PersistentData.ip = ip_field.Text;
        }

        private void _onUsernameFieldTextChange(string text)
        {
            Vars.PersistentData.username = text;
        }

        private void _onIpFieldTextChange(string text)
        {
            Vars.PersistentData.ip = text;
        }

        public void AttemptConnection(string ip)
        {
            try
            {
                var addy =  Funcs.ParseIpString(ip);
                Client.ConnectToServer(addy[0], int.Parse(addy[1]), success =>
                {
                    if (success)
                    {
                        GD.Print($"Connected to {Vars.PersistentData.ip} with username {Vars.PersistentData.username}");
                    }
                    else
                    {
                        GD.Print($"Connection to {Vars.PersistentData.ip} failed.");
                    }
                });
            }
            catch (Exception e)
            {
                Client.Disconnect();
                Interface.Utils.CreateInformalMessage(e.Message, 10);
            }
        }

        private void _onConnectButtonPress()
        {
            AttemptConnection(Vars.PersistentData.ip);
            /*
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    World world = (World) tree.CurrentScene;
                });
                */
        }
    }
}