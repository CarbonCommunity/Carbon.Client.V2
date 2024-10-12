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
            Unload();
        }

        plugins[Path] = this;
        OnLoad();

        Console.WriteLine($"Loaded plugin '{Name}'");
    }

    internal void Unload()
    {
        plugins[Path] = null;
        OnUnload();

        Console.WriteLine($"Unloaded plugin '{Name}'");
    }

    public virtual void OnLoad()
    {

    }

    public virtual void OnUnload()
    {

    }
}
