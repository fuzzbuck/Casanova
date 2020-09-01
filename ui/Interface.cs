using System;
using Casanova.core;
using Casanova.core.net;
using Casanova.ui.fragments;
using Godot;
using Godot.Collections;
using World = Casanova.core.main.world.World;

namespace Casanova.ui
{
	public class Interface : Node
	{
		public static Array<Godot.Button> ButtonGroup = new Array<Godot.Button>();
		public static Array<Label> LabelGroup = new Array<Label>();
		public static Array<AnimationPlayer> cardAnimationGroup = new Array<AnimationPlayer>();
		public static int CurrentSelected = -1; // current button/category selected   -1 = none,  0 = play, 1 = settings, 2 = about, 3 = exit (dont select)
		public static SceneTree tree;

		public override void _Ready()
		{
			tree = GetTree();
		}

		public class Utils
		{
			public static Node createFragment(string fragment)
			{ 
				return ResourceLoader.Load<PackedScene>(Vars.path_frags + $"/{fragment}.tscn").Instance();
			}

			public static MobileTextInput spawnMte(string text)
			{
				MobileTextInput mte = (MobileTextInput) createFragment("MobileTextInput");
				tree.CurrentScene.AddChild(mte);
				
				ThreadManager.ExecuteOnMainThread(() =>
				{
					mte.texteditor.Text = text;
					mte.preview.BbcodeText = text;
					mte.texteditor.GrabFocus();
				});

				return mte;
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
						tree.ChangeSceneTo(ResourceLoader.Load<PackedScene>(Vars.path_world + "/World.tscn"));
						
						ThreadManager.ExecuteOnMainThread(() =>
						{
							World world = (World) tree.CurrentScene;
							
							world.StartServer();
							world.StartClient();
						});
					}
				},
				{
					1, () =>
					{
						tree.CurrentScene.AddChild(Utils.createFragment("ServerJoin"));
					}
				}
			};
			public static void Open()
			{
				IsShown = true;
				for (var i = 0; i < cardAnimationGroup.Count; i++)
				{
					cardAnimationGroup[i].Play("enter");
				}
			}

			public static void Close()
			{
				IsShown = false;
				for (var i = 0; i < cardAnimationGroup.Count; i++)
				{
					if(cardAnimationGroup[i].AssignedAnimation != "exit") 
						cardAnimationGroup[i].Play("exit");
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
