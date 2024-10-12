using System;
using System.Collections.Generic;
using UnityEngine;

public class BaseProcessor : MonoBehaviour
{
    public float Rate;

    internal Dictionary<string, BaseProcess> processes = new();
    internal float lastTick;

    public virtual string GetName()
    {
        return "Default";
    }

    public virtual void Setup()
    {
        lastTick = Time.realtimeSinceStartup + Rate;
    }

    public virtual void Awake()
    {
        DontDestroyOnLoad(gameObject);

        Setup();

        Debug.Log($"Initialized {GetName()}");
    }

    public void FixedUpdate()
    {
        if (Time.realtimeSinceStartup < lastTick)
        {
            return;
        }

        if (processes.Count > 0)
        {
            foreach (var process in processes)
            {
                if (!process.Value.IsDirty)
                {
                    continue;
                }

                try
                {
                    Debug.LogWarning($"Processing {process.Key}...");
                    process.Value.Do();
                    process.Value.IsDirty = false;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Process '{process.Key}' for {GetName()} failed ({ex.Message})\n{ex.StackTrace}");
                }
            }
        }

        lastTick = Time.realtimeSinceStartup + Rate;
    }

    public void AddProcess(string key, BaseProcess process)
    {
        processes.Add(key, process);
    }

    public void RemoveProcess(string key)
    {
        if (processes.ContainsKey(key))
        {
            processes.Remove(key);
        }
    }

    public void MarkDirty(string key)
    {
        if (processes.TryGetValue(key, out var process))
        {
            process.IsDirty = true;
        }
    }

    public abstract class BaseProcess
    {
        public bool IsDirty;

        public abstract void Do();
    }
}
