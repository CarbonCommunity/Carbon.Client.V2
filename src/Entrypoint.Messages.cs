using BepInEx.Unity.IL2CPP.Utils.Collections;
using Carbon.Client;
using Carbon.Client.Assets;
using UnityEngine;

public partial class Entrypoint
{
	internal static void Message_Approval(CarbonServerConnection conn)
	{
		var protocol = conn.Read.Int32();

		if (protocol != Protocol.VERSION)
		{
			Debug.LogWarning($"Could not connect! Wrong protocol. (Expected {Protocol.VERSION}, got {protocol})");

			conn.Write.Start(Messages.Approval);
			conn.Write.Bool(false);
			conn.Write.Send();
			return;
		}

		var userId = conn.Read.UInt64();
		var username = conn.Read.String();

		var server = ClientNetwork.ins.serverConnection;
		server.Username = username;
		server.UserId = userId;

		Debug.Log($"Logged in Carbon 4 Client: {username}[{userId}]");

		conn.Write.Start(Messages.Approval);
		conn.Write.Bool(true);
		conn.Write.Send();
	}

	internal static void Message_AddonLoad(CarbonServerConnection conn)
	{
		var uninstallAll = conn.Read.Bool();
		var addonCount = conn.Read.Int32();
		var manifests = new Addon.Manifest[addonCount];

		for (int i = 0; i < addonCount; i++)
		{
			var manifest = new Addon.Manifest();
			manifest.Load(conn.Read);
			manifests[i] = manifest;
		}

		if (uninstallAll)
		{
			AddonManager.Instance.Uninstall(entities: false);
		}

		GameManager.ins.StartCoroutine(AssetEx.LoadAddonsBasedOnManifestAsync(manifests).WrapToIl2Cpp());
	}

	internal static void Message_PlayerLoad(CarbonServerConnection conn)
	{
	}

	internal static void Message_EntityCreate(CarbonServerConnection conn)
	{
		var assetId = conn.Read.UInt32();
		var spawnable = GameManager.ins.CreateSpawnable(assetId);
		spawnable.netId = new(conn.Read.UInt64());
		spawnable.Spawn();
	}

	internal static void Message_EntityUpdate(CarbonServerConnection conn)
	{
		var netId = conn.Read.NetworkId();

		if (GameManager.ins.spawnedEntities.TryGetValue(netId.Value, out var entity))
		{
			entity.Load(conn.Read);
		}
		else
		{
			Debug.LogWarning($"Tried entity update [{netId}] for a non-spawned entity");
		}
	}

	internal static void Message_EntityPosition(CarbonServerConnection conn)
	{
		var netId = conn.Read.NetworkId();

		if (GameManager.ins.spawnedEntities.TryGetValue(netId.Value, out var entity))
		{
			entity.transform.SetPositionAndRotation(conn.Read.Vector3(), conn.Read.Quaternion());
			entity.transform.localScale = conn.Read.Vector3();
		}
		else
		{
			Debug.LogWarning($"Tried entity update [{netId}] for a non-spawned entity");
		}
	}

	internal static void Message_EntityDestroy(CarbonServerConnection conn)
	{
		var netId = conn.Read.NetworkId();

		if (GameManager.ins.spawnedEntities.TryGetValue(netId.Value, out var entity))
		{
			Object.Destroy(entity.gameObject);
			GameManager.ins.spawnedEntities = null;
		}
	}
}
