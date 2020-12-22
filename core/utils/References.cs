using System;
using System.Collections.Generic;
using Godot;
using Path = System.IO.Path;

namespace Casanova.core.utils
{
    // Provides references for godot instances & preloads them
    public class References : Node
    {
        public static PackedScene base_unit;
        public static PackedScene main_camera;
        public static PackedScene main_world;
        
        # region core
        public static Dictionary<string, PackedScene> bodies = new Dictionary<string, PackedScene>();
        public static Dictionary<string, PackedScene> effects = new Dictionary<string, PackedScene>();
        # endregion
        
        # region ui
        public static Dictionary<string, PackedScene> fragments = new Dictionary<string, PackedScene>();
        public static Dictionary<string, PackedScene> elements = new Dictionary<string, PackedScene>();
        # endregion

        public static void LoadDirectory(Dictionary<string, PackedScene> dict, string path)
        {
            var dir = new Directory();
            dir.Open(path);
            dir.ListDirBegin();
            
            var file = dir.GetNext();
            while (file != string.Empty)
            {
                if (file.EndsWith(".tscn"))
                {
                    dict[Path.GetFileNameWithoutExtension(file)] = ResourceLoader.Load<PackedScene>(path + "/" + file);
                }

                file = dir.GetNext();
            }
            dir.ListDirEnd();
        }
        public static void Load()
        {
            base_unit = ResourceLoader.Load<PackedScene>(Vars.path_main + $"/units/Unit.tscn");
            main_camera = ResourceLoader.Load<PackedScene>(Vars.path_main + "/units/Camera.tscn");
            main_world = ResourceLoader.Load<PackedScene>(Vars.path_world + "/World.tscn");
            
            LoadDirectory(bodies, Vars.path_type_bodies);
            LoadDirectory(effects, Vars.path_type_effects);
            
            LoadDirectory(fragments, Vars.path_frags);
            LoadDirectory(elements, Vars.path_elems);

        }
    }
}