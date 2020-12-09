using Casanova.core;
using Casanova.core.main;
using Casanova.core.net;
using Casanova.core.net.types;
using Godot;
using Godot.Collections;
using LineEdit = Casanova.ui.elements.LineEdit;

namespace Casanova.ui.fragments
{
    public class Chat : PanelContainer
    {
        public static Chat instance;
        private LineEdit _lineEdit;
        private Button button;

        private RichTextLabel chatMessage;

        private VBoxContainer content;

        private string lastMessage = string.Empty;

        private readonly int MaxMessages = 50;
        private VBoxContainer messageBox;
        private ScrollContainer messageScrollerBox;
        private readonly Array<RichTextLabel> msgInstances = new Array<RichTextLabel>();

        private bool processing;
        private RandomNumberGenerator rnd;
        public HBoxContainer senderBox;

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

            if (Vars.PersistentData.isMobile)
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
            var message = sender != null ? $"[color=#fa9e48]<[/color]{sender.Username}[color=#fa9e48]>[/color]: {text}" : text;
            AddMessage(message);
            lastMessage = text;

            ThreadManager.ExecuteOnMainThread(() =>
            {
                messageScrollerBox.GetVScrollbar().Value = messageScrollerBox.GetVScrollbar().MaxValue;
            });
        }

        private void _onSenderBoxFocusEnter()
        {
            PlayerController.Focus = _lineEdit;
        }

        private void _onFocusLost()
        {
            PlayerController.Focus = null;

            _lineEdit.SetProcess(false);
            senderBox.Modulate = new Color(0, 0, 0, 0);
        }

        private void _onTextChanged(string text)
        {
            if (text.Length > 0)
                button.Disabled = false;
            else
                button.Disabled = true;
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
            if (_lineEdit.Text == string.Empty)
            {
                CancelSend();
            }
            else
            {
                Packets.ClientHandle.Send.ChatMessage(_lineEdit.Text);

                _lineEdit.Text = string.Empty;
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
                    if (PlayerController.Focus == _lineEdit)
                    {
                        if (_lineEdit.Text == string.Empty)
                        {
                            CancelSend();
                        }
                        else
                        {
                            Packets.ClientHandle.Send.ChatMessage(_lineEdit.Text);

                            _lineEdit.Text = string.Empty;
                            CancelSend();

                            ThreadManager.ExecuteOnMainThread(() =>
                            {
                                messageScrollerBox.GetVScrollbar().Value =
                                    messageScrollerBox.GetVScrollbar().MaxValue;
                            });
                        }
                    }
                    else
                    {
                        TriggerSendBox();
                    }
                }

                if (key.Scancode == (int) KeyList.Escape && senderBox.Modulate == new Color(1, 1, 1)) CancelSend();

                if (key.Scancode == (int) KeyList.Up && key.Pressed)
                    if (PlayerController.Focus == _lineEdit)
                    {
                        _lineEdit.Text = lastMessage;
                        _lineEdit.CaretPosition = lastMessage.Length;
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
                if (msgInstance != chatMessage)
                    msgInstance.QueueFree();
        }

        public void AddMessage(string message)
        {
            var msgInstance = chatMessage.Duplicate() as RichTextLabel;
            msgInstance.Name = rnd.RandiRange(1000, 99999999).ToString();
            messageBox.AddChild(msgInstance);
            msgInstance.BbcodeText = message;
            msgInstance.GetNode<AnimationPlayer>("Animation").Play("Enter");
            msgInstance.Visible = true;

            var msgTimer = msgInstance.GetNode<Timer>("Timer");
            msgTimer.Connect("timeout", this, "MessageTimeout", new Array {msgInstance});
            msgTimer.Start();

            msgInstances.Add(msgInstance);
            if (msgInstances.Count > MaxMessages)
            {
                var ms = msgInstances[0];
                msgInstances.Remove(ms);

                ms.Free();
            }
        }

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