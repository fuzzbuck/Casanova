using System;
using System.Linq;
using Casanova.core;
using Casanova.core.net.client;
using Casanova.core.types;
using Godot;
using LineEdit = Casanova.ui.elements.LineEdit;

namespace Casanova.ui.fragments
{
    public class ServerJoin : VBoxContainer
    {
        private Button connect_button;
        private HBoxContainer ip;
        private LineEdit ip_field;
        private HBoxContainer unit;
        private OptionButton unit_field;
        private HBoxContainer username;

        private LineEdit username_field;

        public override void _Ready()
        {
            connect_button = GetNode<Button>("Button");

            username = GetNode<HBoxContainer>("Username");
            ip = GetNode<HBoxContainer>("Ip");
            unit = GetNode<HBoxContainer>("UnitSelector");

            username_field = username.GetNode<LineEdit>("LineEdit");
            username_field.Text = Vars.PersistentData.username;

            ip_field = ip.GetNode<LineEdit>("LineEdit");
            unit_field = unit.GetNode<OptionButton>("OptionButton");

            unit_field.AddItem("Explorer", 0);
            unit_field.AddItem("Crimson", 1);
            unit_field.Select(Vars.Enums.UnitTypes.FirstOrDefault(x => x.Value == Vars.PersistentData.UnitType).Key);

            username_field.Connect("text_changed", this, "_onUsernameFieldTextChange");
            ip_field.Connect("text_changed", this, "_onIpFieldTextChange");
            connect_button.Connect("pressed", this, "_onConnectButtonPress");
            unit_field.Connect("item_selected", this, "_onUnitOptionSelect");

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

        private void _onUnitOptionSelect(int id)
        {
            Vars.PersistentData.UnitType = Vars.Enums.UnitTypes[id];
        }

        public bool AttemptConnection(string ip)
        {
            try
            {
                var addy =  Funcs.ParseIpString(ip);
                Client.ConnectToServer(addy[0], int.Parse(addy[1]));

                return true;
            }
            catch (Exception e)
            {
                Client.Disconnect();
                Interface.Utils.CreateInformalMessage(e.Message, 10);

                return false;
            }
        }

        private void _onConnectButtonPress()
        {
            var success = AttemptConnection(Vars.PersistentData.ip);
            if (success)
                GD.Print("Connected to " + Vars.PersistentData.ip + " with username " + Vars.PersistentData.username);
            /*
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    World world = (World) tree.CurrentScene;
                });
                */
        }
    }
}