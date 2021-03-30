using System;
using Casanova.core;
using Casanova.core.net;
using Casanova.core.net.server;
using Casanova.core.types;
using Casanova.core.utils;
using Casanova.ui.elements;
using Casanova.ui.fragments;
using Godot;
using Godot.Collections;
using Client = Casanova.core.net.client.Client;
using Label = Casanova.ui.elements.Label;
using World = Casanova.core.main.world.World;

namespace Casanova.ui
{
    public class Interface : Node
    {
        public static Array<Godot.Button> ButtonGroup = new Array<Godot.Button>();
        public static Array<Label> LabelGroup = new Array<Label>();
        public static Array<Panel> CardsGroup = new Array<Panel>();
        public static InformalMessage LatestInformalMessage;

        public static int
            CurrentSelected =
                -1; // current button/category selected   -1 = none,  0 = play, 1 = settings, 2 = about, 3 = exit (dont select)

        public static SceneTree tree;
        public static LoadingOverlay CurrentLoading;

        public override void _Ready()
        {
            tree = GetTree();
        }

        public class Utils
        {
            public static Node CreateFragment(string fragment)
            {
                var frag = References.fragments[fragment].Instance();
                if (frag is LoadingOverlay lo)
                    CurrentLoading = lo;

                return frag;
            }

            public static Node CreateElement(string element)
            {
                return References.elements[element].Instance();
            }

            public static MobileTextInput SpawnMte(string text)
            {
                var mte = (MobileTextInput) CreateFragment("MobileTextInput");
                tree.CurrentScene.AddChild(mte);

                ThreadManager.ExecuteOnMainThread(() =>
                {
                    mte.texteditor.Text = text;
                    mte.preview.BbcodeText = text;
                    mte.texteditor.GrabFocus();
                });

                return mte;
            }

            public static void AddHudElement(Node elem)
            {
                if (Vars.CurrentState == Vars.State.Menu)
                    tree.CurrentScene.AddChild(elem);
                else
                    tree.CurrentScene.GetNode<CanvasLayer>("TopLayer").AddChild(elem);
            }

            public static Node SpawnOverlayFragment(string fragment)
            {
                var frag = CreateFragment(fragment);
                AddHudElement(frag);

                return frag;
            }

            public static InformalMessage CreateInformalMessage(string text, float time)
            {
                LatestInformalMessage?.Skip();

                var msg = (InformalMessage) CreateElement("InformalMessage"); // what the hell

                AddHudElement(msg);
                LatestInformalMessage = msg;

                ThreadManager.ExecuteOnMainThread(() =>
                {
                    try
                    {
                        msg.SetMessage(text);
                        msg.SetTime(time);
                    }
                    catch (Exception e)
                    {
                        if(Vars.log_log)
                            GD.PrintErr("<log>: can't access disposed " + typeof(InformalMessage) + " -> " + e.Message);
                    }
                });

                return msg;
            }
        }

        public static class Cards
        {
            public static bool IsShown;

            public static readonly System.Collections.Generic.Dictionary<int, Action> IndexBindings =
                new System.Collections.Generic.Dictionary<int, Action>
                {
                    {
                        /* PLAY TEST */
                        0, () =>
                        {
                            Server.Start(8, Vars.Networking.defaultPort);
                            Client.ConnectToServer("127.0.0.1", Vars.Networking.Port, _ => {});
                        }
                    },
                    {
                        /* SERVER JOIN */
                        1, () => { Utils.SpawnOverlayFragment("ServerJoin"); }
                    },
                    /* WORLD EDITOR */
                    {
                        2, () =>
                        {
                            Vars.CurrentState = Vars.State.Editor;
                            Server.Start(8, Vars.Networking.defaultPort);
                            Client.ConnectToServer("127.0.0.1", Vars.Networking.Port, _ => {});
                            World.rules = new Rules {Mode = Vars.Gamemode.Editor};
                        }
                    }
                };

            public static void Open()
            {
                IsShown = true;
                for (var i = 0; i < CardsGroup.Count; i++)
                {
                    CardsGroup[i].Visible = true;
                    CardsGroup[i].GetNode<AnimationPlayer>("AnimationPlayer").Play("enter");
                }
            }

            public static void Close()
            {
                IsShown = false;
                for (var i = 0; i < CardsGroup.Count; i++)
                    CardsGroup[i].GetNode<AnimationPlayer>("AnimationPlayer").Play("exit");
            }
        }

        public static class MainMenu
        {
            public static readonly System.Collections.Generic.Dictionary<int, Action> IndexBindings =
                new System.Collections.Generic.Dictionary<int, Action>
                {
                    {
                        0, Cards.Open
                    },
                    {
                        1, () => { }
                    },
                    {
                        2, () => { }
                    }
                    // todo: settings.open(), about.open()
                };

            public static void CloseAll()
            {
                Cards.Close();
            }
        }
    }
}