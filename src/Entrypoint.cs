using BepInEx;
using BepInEx.Unity.IL2CPP;
using UnityEngine;

[BepInPlugin("c751f97e5a284fe299230f3a2f046931", "Carbon.Client", "2.0")]
public partial class Entrypoint : BasePlugin
{
    public override void Load()
    {
        Debug.Log($"Launching Carbon 4 Client...");

        MakePatch();
    }
}
