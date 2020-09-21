using System;
using Casanova.core;
using Casanova.core.net;
using Casanova.core.net.server;
using Casanova.ui.fragments;
using Godot;
using Godot.Collections;
using Client = Casanova.core.net.client.Client;
using World = Casanova.core.main.world.World;

namespace Casanova.ui
{
	public class Interface : Node
	{
		public static Array<Godot.Button> ButtonGroup = new Array<Godot.Button>();
		public static Array<Label> LabelGroup = new Array<Label>();
		public static Array<Panel> CardsGroup = new Array<Panel>();
		public static int CurrentSelected = -1; // current button/category selected   -1 = none,  0 = play, 1 = settings, 2 = about, 3 = exit (dont select)
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

			public static MobileTextInput SpawnMte(string text)
			{
				MobileTextInput mte = (MobileTextInput) CreateFragment("MobileTextInput");
				tree.CurrentScene.AddChild(mte);
				
				ThreadManager.ExecuteOnMainThread(() =>
				{
					mte.texteditor.Text = text;
					mte.preview.BbcodeText = text;
					mte.texteditor.GrabFocus();
				});

				return mte;
			}

			public static Node SpawnOverlayFragment(string fragment)
			{
				var frag = CreateFragment(fragment);

				if (Vars.CurrentState == Vars.State.Menu)
				{
					tree.CurrentScene.AddChild(frag);
				}
				else
				{
					tree.CurrentScene.GetNode<CanvasLayer>("TopLayer").AddChild(frag);
				}

				return frag;
			}
		}

		public static class Cards
		{
			public static bool IsShown;
			
			public static readonly System.Collections.Generic.Dictionary<int, Action> IndexBindings = new System.Collections.Generic.Dictionary<int, Action>
			{
				{
					0, () =>
					{
						Vars.CurrentState = Vars.State.World;
						tree.ChangeScene(Vars.path_world + "/World.tscn");
						
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
					1, () =>
					{
						Utils.SpawnOverlayFragment("ServerJoin");
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
				{
					CardsGroup[i].GetNode<AnimationPlayer>("AnimationPlayer").Play("exit");
					
				}
			}
		}

		public static class MainMenu
		{
			public static readonly System.Collections.Generic.Dictionary<int, Action> IndexBindings = new System.Collections.Generic.Dictionary<int, Action>
			{
				{
					0, Cards.Open
				},
				{
					1, () =>
					{
					}
				},
				{
					2, () =>
					{
					}
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
