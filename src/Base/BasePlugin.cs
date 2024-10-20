using System;
using System.Collections.Generic;
using UnityEngine;

public class BasePlugin
{
	public string Name;
	public string Path;

	public static Dictionary<string, BasePlugin> plugins = new();

	internal void Load()
	{
		if (plugins.TryGetValue(Path, out var existentPlugin))
		{
			existentPlugin.Unload();
		}

		Console.WriteLine($"Loaded plugin '{Name}'");

		plugins[Path] = this;
		OnLoad();
	}

	internal void Unload()
	{
		plugins[Path] = null;

		try
		{
			OnUnload();
		}
		catch (Exception ex)
		{
			Debug.LogError($"Failed unloading plugin '{Name}' ({ex.Message})\n{ex.StackTrace}");
		}

		Console.WriteLine($"Unloaded plugin '{Name}'");
	}

	public virtual void OnLoad()
	{

	}

	public virtual void OnUnload()
	{

	}
}