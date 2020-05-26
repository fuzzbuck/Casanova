using Godot;
using System;
using Casanova.core;
using Casanova.ui;

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
		for (var i = 0; i < Interface.buttonGroup.Count; i++)
		{
			Button button = Interface.buttonGroup[i];
			GD.Print(button.Name);
            button.Text = Tr(button.Name.ToUpper());
		}
		
		// update labels
		for (var i = 0; i < Interface.labelGroup.Count; i++)
		{
			Label label = Interface.labelGroup[i];
			GD.Print(label.Name);
			label.Text = Tr(label.Name.ToUpper());
		}
	}
}
