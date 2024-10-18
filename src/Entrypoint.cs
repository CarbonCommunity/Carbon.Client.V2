using BepInEx;
using Carbon.Client;
using UnityEngine;

[BepInPlugin("c751f97e5a284fe299230f3a2f046931", "Carbon.Client", "2.0")]
public partial class Entrypoint : BepInEx.Unity.IL2CPP.BasePlugin
{
    public GameObject Home => _home ??= UnityEx.SpawnGameObject("Carbon4Client");

	internal GameObject _home;

    public override void Load()
    {
        Debug.Log($"Launching Carbon 4 Client...");

        TypeEx.RecursivelyRegisterType(typeof(System.IO.FileSystemWatcher));
        TypeEx.RecursivelyRegisterType(typeof(BaseProcessor.BaseProcess));
		TypeEx.RecursivelyRegisterType(typeof(BaseProcessor));
		TypeEx.RecursivelyRegisterType(typeof(CarbonClientNetwork));

		MakePatch();

        Carbon.Rust.OnMenuShow += OnCarbon;
    }

    public void OnCarbon()
    {
        MakeProcessors();

		ClientNetwork.ins = new CarbonClientNetwork();
		ClientNetwork.ins.Start();

		var manager = UnityEx.SpawnGameObject("CarbonGameManager").AddUnityComponent<GameManager>();
		manager.Init(false);

		// Carbon.ClientNetworking.Init();

		Carbon.Rust.OnMenuShow -= OnCarbon;
    }

	public class CarbonClientNetwork : ClientNetwork
	{
		public override void OnData(MessageType msg, Connection conn)
		{
			switch (msg)
			{
				case MessageType.Approval:
					Debug.Log($"Worky");
					break;

				default:
					base.OnData(msg, conn);
					break;
			}
		}
	}
}
