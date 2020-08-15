using Godot;

namespace Casanova.core
{
	public class Vars : Node
	{
		public static BundleHandler bundleHandler = new BundleHandler("en");
		public static string ver = "build 1 ver. indev";
		
		public static string path_core = "res://core";
		public static string path_main = path_core + "/main";
		
		public class PlayerCamera
		{
			public static bool rotates_with_player = false;
			public static float min_zoom_distance = 0.65f;
			public static float max_zoom_distance = 1.75f;
			public static float smoothness = 0.014f;
		}

		public class Networking
		{
			public static bool isConnected = false;
			public static float unit_desync_treshold = 8f;
		}
		public class Pals
		{
			public static Color accent = new Color(248, 248, 126);
			public static Color highlight = new Color(255, 255, 255);
		}
		
		public static void load()
		{
			GD.Print("Applying translation");
			bundleHandler.updateBundle("en");
		}
	}
}
