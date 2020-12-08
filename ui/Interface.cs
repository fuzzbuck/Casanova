using System;
using Casanova.core;
using Casanova.core.net;
using Casanova.core.net.server;
using Casanova.ui.elements;
using Casanova.ui.fragments;
using Godot;
using Godot.Collections;
using Client = Casanova.core.net.client.Client;
using Label = Casanova.ui.elements.Label;

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
                var frag = ResourceLoader.Load<PackedScene>(Vars.path_frags + $"/{fragment}.tscn").Instance();
                if (frag is LoadingOverlay lo)
                    CurrentLoading = lo;

                return frag;
            }

            public static Node CreateElement(string element)
            {
                return ResourceLoader.Load<PackedScene>(Vars.path_elems + $"/{element}.tscn").Instance();
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
                    msg.SetMessage(text);
                    msg.SetTime(time);
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
                        0, () =>
                        {
                            Server.Start(8, Vars.Networking.defaultPort);
                            Client.ConnectToServer("127.0.0.1", Vars.Networking.Port);

                            /*
                            ThreadManager.ExecuteOnMainThread(() =>
                            {
                                World world = (World) tree.CurrentScene;
                            });
                            */
                        }
                    },
                    {
                        1, () => { Utils.SpawnOverlayFragment("ServerJoin"); }
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