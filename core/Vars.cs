using Godot;

namespace Casanova.core
{
	public class Vars : Node
	{
		public static BundleHandler bundleHandler = new BundleHandler("en");
		public static string ver = "build 1 ver. indev";
		
		public static string path_core = "res://core";
		public static string path_main = path_core + "/main";
		
		public static string path_ui = "res://ui";
		public static string path_elems = path_ui + "/elements";
		public static string path_frags = path_ui + "/fragments";

		public class PersistentData
		{
			public static bool isMobile = false;
			
			public static string username = "fazzubaku";
			public static string ip = "";
		}
		
		public class PlayerCamera
		{
			public static bool rotates_with_player = false;
			public static float min_zoom_distance = 0.6f;
			public static float max_zoom_distance = 1.5f;
			public static float mobile_zoom_offset_multiplier = 0.25f;
			
			public static float zoom_sensitivity = 3f;
			public static float zoom_speed = 0.02f;
			public static float drag_sensitivity = 1.5f;
			public static float smoothness = 0.014f;
		}

		public class Networking
		{
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

			if (OS.HasTouchscreenUiHint())
			{
				PersistentData.isMobile = true;
				OS.WindowSize = new Vector2(1080, 720);
				
				GD.Print("Changed resolution to 720p");
			}
		}
	}
}
