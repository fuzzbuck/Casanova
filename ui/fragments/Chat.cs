using Godot;
using System;

public class Chat : PanelContainer
{
    private VBoxContainer content;
    private ScrollContainer messageScrollerBox;
    private VBoxContainer messageBox;
    private HBoxContainer senderBox;

    private RichTextLabel chatMessage;
    private Random rnd;
    
    public override void _Ready()
    {
        content = GetNode("MarginContainer").GetNode<VBoxContainer>("Content");
        messageScrollerBox = content.GetNode<ScrollContainer>("Messages");
        messageBox = messageScrollerBox.GetNode<VBoxContainer>("MessageContainer");
        senderBox = content.GetNode<HBoxContainer>("Sender");
        chatMessage = messageBox.GetNode<RichTextLabel>("ChatMessage");
        
        rnd = new Random();

        chatMessage.Visible = false;
        senderBox.Visible = false;
        messageScrollerBox.GetVScrollbar().Modulate = new Color(0, 0, 0, 0);
    }

    public void Clear()
    {
        foreach (RichTextLabel msgInstance in messageBox.GetChildren())
        {
            if(msgInstance != chatMessage)
                msgInstance.QueueFree();
        }
    }

    public void AddMessage(string message)
    {
        RichTextLabel msgInstance = chatMessage.Duplicate() as RichTextLabel;
        msgInstance.Name = rnd.Next(1000, 99999999).ToString();
        messageBox.AddChild(msgInstance);
        msgInstance.BbcodeText = message;
        msgInstance.GetNode<AnimationPlayer>("Animation").Play("Enter");
        msgInstance.Visible = true;
    }
}
