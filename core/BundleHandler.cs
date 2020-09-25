using Casanova.ui;
using Godot;

namespace Casanova.core
{
    public class BundleHandler : Node
    {
        public void updateBundle(string locale)
        {
            TranslationServer.SetLocale(locale);

            // update buttons
            for (var i = 0; i < Interface.ButtonGroup.Count; i++)
            {
                var button = Interface.ButtonGroup[i];
                button.Text = Tr(button.Name.ToUpper());
            }

            // update labels
            for (var i = 0; i < Interface.LabelGroup.Count; i++)
            {
                var label = Interface.LabelGroup[i];
                label.Text = Tr(label.Name.ToUpper());
            }
        }
    }
}