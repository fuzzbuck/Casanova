using Godot;
using System;
using Casanova.core;
using Casanova.@interface;

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
		for (var i = 0; i < Interface.uiElements.menuButtonGroup.Count; i++)
		{
			Button button = Interface.uiElements.menuButtonGroup[i];
            button.Text = Tr(button.Name.ToUpper());
		}
	}
}
