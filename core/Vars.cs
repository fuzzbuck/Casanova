using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Runtime.Remoting.Channels;
using Casanova.core.content;
using Casanova.core.main;
using Casanova.core.main.world;
using Casanova.core.net;
using Casanova.core.net.server;
using Casanova.core.types;
using Casanova.core.utils;
using Casanova.ui;
using Godot;
using Client = Casanova.core.net.client.Client;

namespace Casanova.core
{
    public class Vars : Node
    {
        public enum State
        {
            Menu,
            World,
            Tutorial
        }

        public static BundleHandler bundleHandler = new BundleHandler("en");
        public static string ver = "build 3 ver. indev";

        public static string path_core = "res://core";
        public static string path_main = path_core + "/main";
        public static string path_world = path_main + "/world";
        public static string path_types = path_core + "/types";
        public static string path_type_effects = path_types + "/effects";
        public static string path_type_bodies = path_types + "/bodies";
        public static string path_units = path_main + "/units";

        public static string path_ui = "res://ui";
        public static string path_elems = path_ui + "/elements";
        public static string path_frags = path_ui + "/fragments";

        public static string path_net = path_main + "/net";
        public static string path_client = path_net + "/client";
        public static string path_server = path_net + "/server";
        
        public static string path_assets = "res://assets";
        public static string path_sprites = path_assets + "/sprites";

        public static short WeightMassMultiplier = 150;

        public static State CurrentState = State.Menu;

        public override void _Ready()
        {
            Load();
            PlayerController.Init();
        }

        public override void _Process(float delta)
        {
            ThreadManager.UpdateMain();
        }

        
        public class Enums
        {
            public enum MovementType
            {
                Ground,
                Air
            }

            public static Dictionary<int, UnitType> UnitTypes = new Dictionary<int, UnitType>();
        }
        public static void Load()
        {
            UnitTypes.Init();
            
            // todo: display loading screen
            
            References.Load();
            UnitTypes.Load();

            bundleHandler.updateBundle("en");

            if (OS.HasTouchscreenUiHint())
                PersistentData.isMobile = true;
            // OS.WindowSize = new Vector2(1080, 720);
        }

        public static void Reload()
        {
            Interface.LabelGroup.Clear();
            Interface.CardsGroup.Clear();
            Interface.ButtonGroup.Clear();

            PlayerController.LocalPlayer = null;
            PlayerController.LocalUnit = null;

            Client.IsConnected = false;
            NetworkManager.HostPlayer = null;
            NetworkManager.PlayersGroup.Clear();
            NetworkManager.UnitsGroup.Clear();

            CurrentState = State.Menu;
            Interface.tree.ChangeScene(path_frags + "/Menu.tscn");
        }

        public static void Unload()
        {
            if (Server.IsHosting || Client.IsConnected)
                Reload();

            // todo: save important data, do pre-exit things
            Interface.tree.Quit();
        }

        public class PersistentData
        {
            public static bool isMobile;

            public static string username = "unnamed";
            public static string ip = "127.0.0.1:375";
        }

        public class PlayerCamera
        {
            public static bool rotates_with_player = false;
            public static float min_zoom_distance = 0.44f;
            public static float max_zoom_distance = 2f;
            public static float mobile_zoom_offset_multiplier = 0.2f;

            public static float mobile_cam_distance_treshold = 16f;
            public static float zoom_sensitivity = 2f;
            public static float zoom_speed = 0.02f;
            public static float drag_sensitivity = 0.2f;
            public static float smoothness = 0.034f;
        }

        public class Networking
        {
            /* Maximum distance for smoothing unit's position */
            public static float unit_desync_treshold = 6f;

            /* How smooth unit desync interpolation should be */
            public static float unit_desync_smoothing = 5f;
            
            /* Maximum distance for unit desync treshold, if this is exceeded interpolation won't be smooth anymore */
            public static float unit_desync_max_dist = 46f;

            /* In degrees */
            public static short unit_desync_rotation_treshold = 20;

            public static bool IsHeadless = false;
            public static int defaultPort = 375;
            public static int Port = defaultPort;
        }

        public class Pals
        {
            public static System.Drawing.Color command = ColorTranslator.FromHtml("#ffbe76");
            public static System.Drawing.Color unimportant = ColorTranslator.FromHtml("#bdbdbd");
        }
    }
}