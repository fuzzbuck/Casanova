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
		for (var i = 0; i < Vars.uiHandler.buttonGroup.Count; i++)
		{
			Button button = Vars.uiHandler.buttonGroup[i];
			GD.Print(button.Name);
            button.Text = Tr(button.Name.ToUpper());
		}
		
		// update labels
		for (var i = 0; i < Vars.uiHandler.labelGroup.Count; i++)
		{
			Label label = Vars.uiHandler.labelGroup[i];
			GD.Print(label.Name);
			label.Text = Tr(label.Name.ToUpper());
		}
	}
}
