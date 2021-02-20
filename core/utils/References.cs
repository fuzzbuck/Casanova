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
        
        # region custom
        // (*will*) used with game mods
        public static Dictionary<string, PackedScene> resources = new Dictionary<string, PackedScene>();
        # endregion

        
        public static PackedScene Load(string path)
        {
            return ResourceLoader.Load<PackedScene>(path);
        }
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
                    dict[Path.GetFileNameWithoutExtension(file)] = Load(path + "/" + file);
                }

                file = dir.GetNext();
            }
            dir.ListDirEnd();
        }
        public static void Load()
        {
            // load & reference all statically accessed scenes/prefabs
            
            base_unit = Load(Vars.path_main + "/units/Unit.tscn");
            main_camera = Load(Vars.path_main + "/units/Camera.tscn");
            main_world = Load(Vars.path_world + "/World.tscn");

            LoadDirectory(bodies, Vars.path_type_bodies);
            LoadDirectory(effects, Vars.path_type_effects);
            
            LoadDirectory(fragments, Vars.path_frags);
            LoadDirectory(elements, Vars.path_elems);
            
        }

        public static void LoadCustom()
        {
            // todo: load & reference all custom resources via obtained paths
        }
    }
}