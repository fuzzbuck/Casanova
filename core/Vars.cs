using Casanova.ui;
using Godot;

namespace Casanova.core
{
    public class Vars : Node
    {
        public static Interface uiHandler = new Interface();
        public static BundleHandler bundleHandler = new BundleHandler("en");

        public class Pals
        {
            public static Color accent = new Color(248, 248, 126);
            public static Color highlight = new Color(255, 255, 255);
        }
        
        public static void load()
        {
            GD.Print("Applying translation");
            new BundleHandler("pl").updateBundle("hr");
        }
    }
}
