using Godot;
using System;
using Casanova.core;

public class BundleHandler : Node
{
	public BundleHandler(String locale)
	{
		TranslationServer.SetLocale(locale);
	}

	public void updateBundle(String locale)
	{
		TranslationServer.SetLocale(locale);
		// update buttons
		for (var i = 0; i < Vars.menuButtonGroup.Count; i++)
		{
			Button button = Vars.menuButtonGroup[i];
            button.Text = Tr(button.Name.ToUpper());
		}
	}
}
