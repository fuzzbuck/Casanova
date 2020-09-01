using System;
using System.Runtime.CompilerServices;
using Casanova.core;
using Casanova.core.net;
using Godot;
using World = Casanova.core.main.world.World;

namespace Casanova.ui.fragments
{
    public class ServerJoin : VBoxContainer
    {
        private HBoxContainer username;
        private HBoxContainer ip;

        private LineEdit username_field;
        private LineEdit ip_field;

        private Button connect_button;
        
        public override void _Ready()
        {
            connect_button = GetNode<Button>("Button");
            
            username = GetNode<HBoxContainer>("Username");
            ip = GetNode<HBoxContainer>("Ip");

            username_field = username.GetNode<LineEdit>("LineEdit");
            ip_field = ip.GetNode<LineEdit>("LineEdit");

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

        private void _onConnectButtonPress()
        {
            var tree = GetTree();
            tree.ChangeSceneTo(ResourceLoader.Load<PackedScene>(Vars.path_world + "/World.tscn"));
						
            ThreadManager.ExecuteOnMainThread(() =>
            {
                World world = (World) tree.CurrentScene;
                
                GD.Print("Connecting to " + Vars.PersistentData.ip + " with username " + Vars.PersistentData.username);

                try
                {
                    world.StartClient();
                }
                catch (Exception e)
                {
                    GD.Print(e);
                    
                    // todo: display connection failed popup
                }
            });
        }
    }
}
