using BepInEx;
using Carbon.Client;
using UnityEngine;
using UnityEngine.SceneManagement;

[BepInPlugin("c751f97e5a284fe299230f3a2f046931", "Carbon.Client", "2.0")]
public partial class Entrypoint : BepInEx.Unity.IL2CPP.BasePlugin
{
	public static GameObject Home => _home ??= UnityEx.SpawnGameObject("Carbon4Client");

	internal static GameObject _home;

	public override void Load()
	{
		Debug.Log($"Launching Carbon 4 Client...");

		TypeEx.RecursivelyRegisterType(typeof(System.IO.FileSystemWatcher));
		TypeEx.RecursivelyRegisterType(typeof(BaseProcessor.BaseProcess));
		TypeEx.RecursivelyRegisterType(typeof(BaseProcessor));
		TypeEx.RecursivelyRegisterType(typeof(CarbonClientNetwork));
		TypeEx.RecursivelyRegisterType(typeof(CustomEnty));
		TypeEx.RecursivelyRegisterType(typeof(BaseCarbonEntity));

		MakePatch();

		Carbon.Rust.OnMenuShow += OnCarbon;
	}

	public void OnCarbon()
	{
		MakeProcessors();

		AssetProcessor.CarbonScene = SceneManager.CreateScene("Carbon Scene", new(LocalPhysicsMode.Physics2D));
		ClientNetwork.ins = new CarbonClientNetwork();

		UnityEx.SpawnGameObject("CarbonGameManager").AddUnityComponent<GameManager>().Init(false);

		Carbon.Rust.OnMenuShow -= OnCarbon;
	}

	public class CarbonClientNetwork : ClientNetwork
	{
		public override void OnData(Messages msg, CarbonServerConnection conn)
		{
			switch (msg)
			{
				case Messages.Approval:
					Message_Approval(conn);
					break;

				case Messages.AddonsLoading:
					Message_AddonLoad(conn);
					break;

				case Messages.PlayerLoad:
					Message_PlayerLoad(conn);
					break;

				case Messages.EntityCreate:
					Message_EntityCreate(conn);
					break;

				case Messages.EntityUpdate:
					Message_EntityUpdate(conn);
					break;

				case Messages.EntityPosition:
					Message_EntityPosition(conn);
					break;

				case Messages.EntityDestroy:
					Message_EntityDestroy(conn);
					break;

				default:
					base.OnData(msg, conn);
					break;
			}
		}
	}
}
