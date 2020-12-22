using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using Godot;

namespace Casanova.core.utils
{
    // Provides references for godot instances & preloads them
    public class References : Node
    {
        public static PackedScene base_unit;
        public static Dictionary<string, PackedScene> bodies = new Dictionary<string, PackedScene>();
        public static PackedScene air_body;
        public static PackedScene main_camera;
        
        public override void _Ready()
        {
            base_unit = ResourceLoader.Load<PackedScene>(Vars.path_main + $"/units/Unit.tscn");
            
            var bodies_dir = new Directory();
            
            bodies_dir.Open(Vars.path_type_bodies);
            
            bodies_dir.ListDirBegin();

            var file = bodies_dir.GetNext();
            GD.Print("file: " + file);
            while (file != string.Empty)
            {
                if (file.EndsWith(".tscn"))
                {
                    GD.Print(file.Substring(file.LastIndexOf("/", StringComparison.Ordinal)).Reverse().Skip(5).Reverse().ToString());
                    bodies[
                        file.Substring(file.LastIndexOf("/", StringComparison.Ordinal)).Reverse().Skip(5).Reverse()
                            .ToString()] = ResourceLoader.Load<PackedScene>(file);
                }
            }
            bodies_dir.ListDirEnd();
        }
    }
}