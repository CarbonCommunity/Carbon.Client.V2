using System;
using System.Collections.Generic;
using EasyRoads3Dv3;
using UnityEngine;

namespace Carbon;

public class Rust
{
	public static Dictionary<string, GameObject> prefabs = new();

    public static GameObjectCache MenuUI = new("MenuUI(Clone)");
    public static GameObjectCache EngineUI = new("EngineUI(Clone)");

    public static Action OnMenuShow;
    public static Action OnMenuHide;

	public static GameObject FindPrefab(string name)
	{
		if(prefabs.TryGetValue(name, out var prefab))
		{
			return prefab;
		}

		var bundles = AssetBundle.GetAllLoadedAssetBundles_Native();

		foreach (var bundle in bundles)
		{
			var asset = bundle.LoadAsset<GameObject>(name);

			if (asset != null)
			{
				prefabs[name] = asset;
				return asset;
			}
		}

		return null;
	}
	public static GameObject SpawnPrefab(string name, Vector3 pos = default, Quaternion rot = default, Vector3 scale = default)
	{
		var source = FindPrefab(name);

		if(source == null)
		{
			return null;
		}

		var instance = GameObject.Instantiate(source);
		instance.transform.SetPositionAndRotation(pos, rot);

		if(scale != default)
		{
			instance.transform.localScale = scale;
		}

		return instance;
	}
}
