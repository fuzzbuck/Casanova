using System;
using Casanova.core;
using Casanova.core.main;
using Casanova.core.net;
using Casanova.core.net.types;
using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;
using LineEdit = Casanova.ui.elements.LineEdit;

namespace Casanova.ui.fragments
{
    public class Chat : PanelContainer
    {
        public static Chat instance;
        
        private VBoxContainer content;
        private ScrollContainer messageScrollerBox;
        private VBoxContainer messageBox;
        public HBoxContainer senderBox;
        private Button button;
        private LineEdit _lineEdit;

        private RichTextLabel chatMessage;
        private RandomNumberGenerator rnd;

        private int MaxMessages = 50;
        private Array<RichTextLabel> msgInstances = new Array<RichTextLabel>();
        
        private string lastMessage = String.Empty;
    
        public override void _Ready()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                instance.Dispose();
                instance = this;
            }
            
            if(Vars.PersistentData.isMobile)
                RectPosition = new Vector2(9, 0);

            content = GetNode("MarginContainer").GetNode<VBoxContainer>("Content");
            messageScrollerBox = content.GetNode<ScrollContainer>("Messages");
            messageBox = messageScrollerBox.GetNode<VBoxContainer>("MessageContainer");
            senderBox = content.GetNode<HBoxContainer>("Sender");
            chatMessage = messageBox.GetNode<RichTextLabel>("ChatMessage");

            button = senderBox.GetNode<Button>("Button");
            _lineEdit = senderBox.GetNode<LineEdit>("LineEdit");

            _lineEdit.Connect("focus_entered", this, "_onSenderBoxFocusEnter");
            _lineEdit.Connect("focus_exited", this, "_onFocusLost");
            _lineEdit.Connect("text_changed", this, "_onTextChanged");
            _lineEdit.custom_behaviour = true;

            rnd = new RandomNumberGenerator();

            chatMessage.Visible = false;
            senderBox.Modulate = new Color(0, 0, 0, 0);

            button.Connect("pressed", this, "SendPressed");
            messageScrollerBox.GetVScrollbar().Modulate = new Color(0, 0, 0, 0);
        }
        
        
        // called from anywhere, sends a message & activates chat if necessary
        public void SendMessage(string text, Player sender = null)
        {
            string sendername = sender == null ? "Server" : sender.username;
            string message = $"[color=#fa9e48]<[/color]{sendername}[color=#fa9e48]>[/color]: {text}";
            AddMessage(message);
            lastMessage = text;
            
            ThreadManager.ExecuteOnMainThread(() =>
            {
                messageScrollerBox.GetVScrollbar().Value = messageScrollerBox.GetVScrollbar().MaxValue;
            });
        }

        private void _onSenderBoxFocusEnter()
        {
            PlayerController.focus = _lineEdit;
        }
        
        private void _onFocusLost()
        {
            PlayerController.focus = null;
            
            _lineEdit.SetProcess(false);
            senderBox.Modulate = new Color(0, 0, 0, 0);
        }

        private void _onTextChanged(string text)
        {
            if (text.Length > 0)
            {
                button.Disabled = false;
            }
            else
            {
                button.Disabled = true;
            }
        }

        private void CancelSend()
        {
            _lineEdit.ReleaseFocus();
            _lineEdit.SetProcess(false);
            senderBox.Modulate = new Color(0, 0, 0, 0);
            HideMessages();
        }

        private void SendPressed()
        {
            if (_lineEdit.Text == String.Empty)
            {
                CancelSend();
            }
            else
            {
                Packets.ClientHandle.Send.ChatMessage(_lineEdit.Text);
                            
                _lineEdit.Text = String.Empty;
                CancelSend();
                            
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    messageScrollerBox.GetVScrollbar().Value = messageScrollerBox.GetVScrollbar().MaxValue;
                });
            }
        }

        public override void _Input(InputEvent ev)
        {
            if (ev is InputEventKey key)
            {
                if (key.Scancode == (int) KeyList.Enter && key.Pressed)
                {
                    if (PlayerController.focus == _lineEdit)
                    {
                        if (_lineEdit.Text == String.Empty)
                        {
                            CancelSend();
                        }
                        else
                        {
                            Packets.ClientHandle.Send.ChatMessage(_lineEdit.Text);
                            
                            _lineEdit.Text = String.Empty;
                            CancelSend();
                            
                            ThreadManager.ExecuteOnMainThread(() =>
                            {
                                messageScrollerBox.GetVScrollbar().Value = messageScrollerBox.GetVScrollbar().MaxValue;
                            });
                        }
                    }
                    else
                    {
                        TriggerSendBox();
                    }
                }

                if ((key.Scancode == (int) KeyList.Escape) && senderBox.Modulate == new Color(1, 1, 1))
                {
                    CancelSend();
                }
                
                if (key.Scancode == (int) KeyList.Up && key.Pressed)
                {
                    if (PlayerController.focus == _lineEdit)
                    {
                        _lineEdit.Text = lastMessage;
                        _lineEdit.CaretPosition = lastMessage.Length;
                    }
                }
            }

        }

        public void TriggerSendBox()
        {
            button.Disabled = true;
            
            senderBox.Modulate = new Color(1, 1, 1);
            _lineEdit.SetProcess(true);
                        
            ShowAllMessages();
            _lineEdit.GrabFocus();
                        
            ThreadManager.ExecuteOnMainThread(() =>
            {
                messageScrollerBox.GetVScrollbar().Value = messageScrollerBox.GetVScrollbar().MaxValue;
            });
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
            msgInstance.Name = rnd.RandiRange(1000, 99999999).ToString();
            messageBox.AddChild(msgInstance);
            msgInstance.BbcodeText = message;
            msgInstance.GetNode<AnimationPlayer>("Animation").Play("Enter");
            msgInstance.Visible = true;

            Timer msgTimer = msgInstance.GetNode<Timer>("Timer");
            msgTimer.Connect("timeout", this, "MessageTimeout", new Array { msgInstance });
            msgTimer.Start();
            
            msgInstances.Add(msgInstance);
            if (msgInstances.Count > MaxMessages)
            {
                var ms = msgInstances[0];
                msgInstances.Remove(ms);
                
                ms.Free();
            }
        }

        private bool processing;

        public void MessageTimeout(RichTextLabel msgInstance)
        {
            var anim = msgInstance.GetNode<AnimationPlayer>("Animation");

            if (processing)
            {
                anim.Stop();
                anim.Play("Leave");
            }
        }

        public void ShowAllMessages()
        {
            foreach (RichTextLabel msgInstance in messageBox.GetChildren())
            {
                if (msgInstance.GetNode<Timer>("Timer").TimeLeft > 0)
                {
                    msgInstance.GetNode<Timer>("Timer").SetProcess(false);
                    processing = false;
                    continue;
                }
                
                var anim = msgInstance.GetNode<AnimationPlayer>("Animation");
                anim.Stop();
                anim.Play("Enter");
            }
        }

        public void HideMessages()
        {
            foreach (RichTextLabel msgInstance in messageBox.GetChildren())
            {
                var anim = msgInstance.GetNode<AnimationPlayer>("Animation");
                var timer = msgInstance.GetNode<Timer>("Timer");
    
                // if message finished its timeout
                if (timer.TimeLeft <= 0)
                {
                    anim.Stop();
                    anim.Play("Leave");
                    continue;
                }
                timer.SetProcess(true);
                processing = true;
            }
        }
    }
}
