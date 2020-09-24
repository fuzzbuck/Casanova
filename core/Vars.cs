using System;
using System.Net;
using Casanova.core.content;
using Casanova.core.net;
using Casanova.core.net.server;
using Casanova.ui;
using Godot;
using Client = Casanova.core.net.client.Client;

namespace Casanova.core
{
	public class Vars : Node
	{
		public static BundleHandler bundleHandler = new BundleHandler("en");
		public static string ver = "build 2 ver. indev";
		
		public static string path_core = "res://core";
		public static string path_main = path_core + "/main";
		public static string path_world = path_main + "/world";
		public static string path_types = path_core + "/types";
		public static string path_units = path_main + "/units";
		
		public static string path_ui = "res://ui";
		public static string path_elems = path_ui + "/elements";
		public static string path_frags = path_ui + "/fragments";

		public static string path_net = path_main + "/net";
		public static string path_client = path_net + "/client";
		public static string path_server = path_net + "/server";

		public override void _Ready()
		{
			Load();
		}

		public override void _Process(float delta)
		{
			ThreadManager.UpdateMain();
		}

		public class PersistentData
		{
			public static bool isMobile = false;
			
			public static string username = "unnamed";
			public static string ip = "fuzzbuck.dev:375";
		}
		
		public class PlayerCamera
		{
			public static bool rotates_with_player = false;
			public static float min_zoom_distance = 0.44f;
			public static float max_zoom_distance = 2f;
			public static float mobile_zoom_offset_multiplier = 0.25f;

			public static float mobile_cam_distance_treshold = 35f;
			public static float zoom_sensitivity = 2f;
			public static float zoom_speed = 0.02f;
			public static float drag_sensitivity = 0.2f;
			public static float smoothness = 0.034f;
		}

		public enum State
		{
			Menu,
			World,
			Tutorial,
			MultiplayerWorld
		}

		public static State CurrentState = State.Menu;
		public class Networking
		{
			public static float unit_desync_treshold = 12f;
			public static float unit_desync_interpolation = 0.05f;

			public static bool isHeadless = false;
			public static int defaultPort = 375;
			public static int Port = defaultPort;

			public static string[] ParseIpString(string ip)
			{
				try
				{
					string[] addy = ip.Split(":");
					int port = defaultPort;

					if (addy.Length > 1 && int.TryParse(addy[1], out int newport))
						port = newport;

					if (!IPAddress.TryParse(addy[0], out IPAddress address))
					{
						var ips = Dns.GetHostAddresses(addy[0]);
						if (ips.Length > 0)
						{
							addy[0] = ips[0].ToString().Split(":")[0];
						}
					}

					return new[] {addy[0], port.ToString()};
				}
				catch (Exception)
				{
					throw new Exception($"Failed parsing IP: {ip}");
				}
			}
		}
		public class Pals
		{
			public static Color accent = new Color(248, 248, 126);
			public static Color highlight = new Color(255, 255, 255);
		}
		
		public static void Load()
		{
			// todo: display loading screen
			UnitTypes.Init();
			
			bundleHandler.updateBundle("en");

			if (OS.HasTouchscreenUiHint())
			{
				PersistentData.isMobile = true;
				OS.WindowSize = new Vector2(1080, 720);
			}
		}

		public static void Reload()
		{
			Interface.LabelGroup.Clear();
			Interface.CardsGroup.Clear();
			Interface.ButtonGroup.Clear();
			
			CurrentState = State.Menu;
			Interface.tree.ChangeScene(Vars.path_frags + "/Menu.tscn");
		}

		public static void Unload()
		{
			if(Server.IsHosting || Client.isConnected)
				Reload();
			
			// todo: save important data, do pre-exit things
			Interface.tree.Quit();
		}
	}
}
