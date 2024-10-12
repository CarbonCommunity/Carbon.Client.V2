using BepInEx;
using BepInEx.Unity.IL2CPP;
using UnityEngine;

[BepInPlugin("c751f97e5a284fe299230f3a2f046931", "Carbon.Client", "2.0")]
public partial class Entrypoint : BepInEx.Unity.IL2CPP.BasePlugin
{
    public GameObject Home 
    {
        get
        {
            if(_home != null)
            {
                return _home;
            }

            _home = new GameObject("Carbon4Client");
            GameObject.DontDestroyOnLoad(_home);
            return _home;
        } 
    }

    internal GameObject _home;

    public override void Load()
    {
        Debug.Log($"Launching Carbon 4 Client...");

        TypeEx.RecursivelyRegisterType(typeof(System.IO.FileSystemWatcher));
        TypeEx.RecursivelyRegisterType(typeof(BaseProcessor.BaseProcess));
        TypeEx.RecursivelyRegisterType(typeof(BaseProcessor));

        MakePatch();

        Carbon.Rust.OnMenuShow += OnCarbon;
    }

    public void OnCarbon()
    {
        MakeProcessors();

        Carbon.Rust.OnMenuShow -= OnCarbon;
    }
}
