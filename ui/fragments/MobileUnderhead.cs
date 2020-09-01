using Casanova.core;
using Godot;

namespace Casanova.ui.fragments
{
    public class MobileUnderhead : CenterContainer
    {
        public Button chatButton;
        
        public override void _Ready()
        {
            chatButton = GetNode<Button>("ChatButton");

            if (!Vars.PersistentData.isMobile)
            {
                Visible = false;
                SetProcess(false);
            }
            else
            {
                chatButton.Connect("pressed", this, "_onChatButtonPress");
            }
        }

        public void _onChatButtonPress()
        {
            if(Chat.instance.senderBox.Modulate != new Color(1, 1, 1))
                Chat.instance.TriggerSendBox();
        }
    }
}
