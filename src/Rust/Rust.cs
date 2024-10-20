using System;
using System.Collections.Generic;
using UnityEngine;

namespace Carbon;

public class Rust
{
	public static Dictionary<string, GameObject> prefabs = new();

	public static GameObjectCache LocalPlayer = new("LocalPlayer");
	public static GameObjectCache MenuUI = new("MenuUI(Clone)");
	public static GameObjectCache EngineUI = new("EngineUI(Clone)");

	public static Action OnMenuShow;
	public static Action OnMenuHide;

	public static GameObject FindPrefab(string name)
	{
		if (prefabs.TryGetValue(name, out var prefab))
		{
			return prefab;
		}

		var bundles = AssetBundle.GetAllLoadedAssetBundles_Native();

		foreach (var bundle in bundles)
		{
			if (bundle.isStreamedSceneAssetBundle)
			{
				continue;
			}

			var asset = bundle.LoadAsset<GameObject>(name);

			if (asset != null)
			{
				return prefabs[name] = asset;
			}
		}

		return null;
	}
	public static GameObject SpawnPrefab(string name, Vector3 pos = default, Quaternion rot = default, Vector3 scale = default)
	{
		var source = FindPrefab(name);

		if (source == null)
		{
			return null;
		}

		var instance = UnityEngine.Object.Instantiate(source);
		instance.transform.SetPositionAndRotation(pos, rot);
		// SceneManager.MoveGameObjectToScene(instance, AssetProcessor.CarbonScene);

		if (scale != default)
		{
			instance.transform.localScale = scale;
		}

		return instance;
	}
}
