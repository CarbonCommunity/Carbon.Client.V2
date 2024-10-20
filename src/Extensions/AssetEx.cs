using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Carbon.Client;
using Carbon.Client.Assets;
using Carbon.Client.Extensions;
using Carbon.Components;
using Carbon.Extensions;
using Il2CppInterop.Runtime;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using static Carbon.Client.GameManager;
using static Carbon.Client.RustComponent;

public static class AssetEx
{
	public static IEnumerable<Addon.Manifest> CurrentAddons;
	public static Shader RustTerrainStandardShader = null;
	public static readonly char[] LayerSplitter = ['|'];
	public static IEnumerable<AudioMixerGroup> AudioMixerGroups;

	internal static void RefreshAudioMixerGroups()
	{
		AudioMixerGroups = Resources.FindObjectsOfTypeAll<AudioMixerGroup>().Reverse();
	}

	public static IEnumerator Client_UnpackBundleAsync(this Asset asset, Action<float> onProgressChanged = null)
	{
		var request = AssetBundle.LoadFromMemoryAsync(asset.data);
		var currentProgress = request.progress;

		while (!request.isDone)
		{
			var progress = request.progress;

			if (progress != currentProgress)
			{
				onProgressChanged?.Invoke(progress);
				currentProgress = progress;
			}

			yield return null;
		}


		asset.cachedBundle = request.assetBundle;
		asset.cachedRustBundle = RustBundle.Deserialize(asset.additionalData);
		asset.PrewarmAsset();

		onProgressChanged?.Invoke(1f);
	}
	public static void Client_CreateRustPrefabs(Transform target, IEnumerable<RustPrefab> prefabs, Action callback = null)
	{
		if (prefabs == null)
		{
			callback?.Invoke();
			return;
		}

		GameManager.ins.StartCoroutine(Client_CreateBasedOnPrefabsAsyncImpl(target, prefabs, callback).WrapToIl2Cpp());
	}

	public static void Client_CreateFromCacheAsync(string path, Action<GameObject> callback = null)
	{
		if (string.IsNullOrEmpty(path))
		{
			Debug.LogWarning($"Couldn't find '{path}' as it's an empty string. (CreateFromCacheAsync)");
			callback?.Invoke(null);
			return;
		}

		if (AddonManager.Instance.Prefabs.TryGetValue(path, out var prefab))
		{
			callback += prefabInstance =>
			{
				Client_CreateRustPrefabs(prefabInstance.transform, prefab.RustPrefabs, null);
			};

			GameManager.ins.StartCoroutine(Client_CreateBasedOnAsyncImpl(prefab.Object, callback).WrapToIl2Cpp());

		}
		else
		{
			Debug.LogWarning($"Couldn't find '{path}' as it hasn't been cached yet. Use 'CreateFromAssetAsync'? (CreateFromCacheAsync)");
			callback?.Invoke(null);
		}
	}
	public static void Client_CreateFromAssetAsync(string path, Asset asset, Action<GameObject> callback = null)
	{
		if (asset == null)
		{
			Debug.LogWarning($"Couldn't find '{path}' as the asset provided is null. (CreateFromAssetAsync)");
			callback?.Invoke(null);
			return;
		}

		if (string.IsNullOrEmpty(path))
		{
			Debug.LogWarning($"Couldn't find '{path}' as it's an empty string. (CreateFromAssetAsync)");
			callback?.Invoke(null);
			return;
		}

		var prefab = asset.LoadPrefab<GameObject>(path);

		if (prefab != null)
		{
			GameManager.ins.StartCoroutine(Client_CreateBasedOnAsyncImpl(prefab, callback).WrapToIl2Cpp());
		}
		else
		{
			Debug.LogWarning($"Couldn't find '{path}' in any addons or assets. (CreateFromAssetAsync)");
			callback?.Invoke(null);
		}
	}
	public static IEnumerator Client_CreateBasedOnPrefabsAsyncImpl(Transform target, IEnumerable<RustPrefab> prefabs, Action callback = null)
	{
		foreach (var prefab in prefabs)
		{
			var lookup = Carbon.Rust.FindPrefab(prefab.rustPath);
			var entity = lookup?.GetComponent<BaseEntity>();
			Debug.Log($"Looking to create {prefab.rustPath} |{lookup}|{entity}|{prefab.entity.enforcePrefab}");

			if (lookup == null)
			{
				Debug.LogWarning($"Couldn't find '{prefab.rustPath}' as the asset provided is null.");
				continue;
			}

			if (entity && !prefab.entity.enforcePrefab)
			{
				continue;
			}

			var instance = (GameObject)null;

			yield return instance = Carbon.Rust.SpawnPrefab(prefab.rustPath, prefab.position, Quaternion.Euler(prefab.rotation), prefab.scale);

			AddonManager.Instance.CreatedRustPrefabs.Add(instance);

			if (prefab.parent)
			{
				var parent = LookupParent(target, prefab.parentPath);

				if (parent != null)
				{
					instance.transform.SetParent(parent, true);
				}
			}
		}

		callback?.Invoke();
	}
	public static IEnumerator Client_CreateBasedOnAsyncImpl(GameObject gameObject, Action<GameObject> callback = null)
	{
		var result = (GameObject)null;

		yield return result = UnityEngine.Object.Instantiate(gameObject);
		//SceneManager.MoveGameObjectToScene(result, AssetProcessor.CarbonScene);
		AddonManager.Instance.CreatedPrefabs.Add(result);

		callback?.Invoke(result);
	}

	public static void Client_PreApplyComponent(RustComponent component, GameObject go)
	{
		if (!component.Component.CreateOn.Client || component.Client == RustComponent.PostProcessMode.Destroyed || component._instance != null)
		{
			return;
		}

		var type = ClientAccessToolsEx.TypeByName(component.Component.Type);
		var il2cppType = Il2CppType.From(type);
		var componentInstance = go.AddComponent(il2cppType);
		component._instance = componentInstance;

		const Il2CppSystem.Reflection.BindingFlags _monoFlags = Il2CppSystem.Reflection.BindingFlags.Instance | Il2CppSystem.Reflection.BindingFlags.Public;

		if (component.Component.Members != null && component.Component.Members.Length > 0)
		{
			foreach (var member in component.Component.Members)
			{
				var field = il2cppType.GetField(member.Name, _monoFlags);

				if (field == null)
				{
					Debug.LogWarning($" Couldn't find member '{member.Name}' in {type}");
					continue;
				}

				var memberType = field.FieldType;
				var value = (object)null;

				try
				{
					if (memberType.Name == "LayerMask")
					{
						field.SetValue(componentInstance, new LayerMask { value = int.Parse(member.Value) }.BoxIl2CppObject());
					}
					else if (memberType.Name == "Vector2")
					{
						using var temp = TempArray<string>.New(member.Value.Split(','));
						field.SetValue(componentInstance, new Vector2(temp.Get(0, "0").ToFloat(), temp.Get(1, "0").ToFloat()).BoxIl2CppObject());
					}
					else if (memberType.Name == "Vector3")
					{
						using var temp = TempArray<string>.New(member.Value.Split(','));
						field.SetValue(componentInstance, new Vector3(temp.Get(0, "0").ToFloat(), temp.Get(1, "0").ToFloat(), temp.Get(2, "0").ToFloat()).BoxIl2CppObject());
					}
					else if (memberType.IsEnum)
					{
						value = Il2CppSystem.Enum.Parse(memberType, member.Value);
						field.SetValue(componentInstance, (Il2CppSystem.Object)value);
					}
					else
					{
						value = Il2CppSystem.Convert.ChangeType(member.Value, memberType);
						field.SetValue(componentInstance, (Il2CppSystem.Object)value);
					}
				}
				catch (Exception ex)
				{
					Debug.LogWarning($" Failed assigning member '{member.Name}' ({member.Value}) of type {memberType} ({ex.Message})\n{ex.StackTrace}");
				}
			}
		}
	}
	public static bool Client_PreApplyObject(RustComponent component, GameObject go)
	{
		if (!HandleDisabled(component, go))
		{
			return !HandleDestroy(component, go);
		}

		return true;
	}

	public static IEnumerator DownloadAddonsAsync(string[] urls, bool load = true)
	{
		var addons = new List<Addon>();

		yield return CoroutineEx.waitForSeconds(.5f);

		for (int i = 0; i < urls.Length; i++)
		{
			var url = urls[i];
			var buffer = (byte[])null;

			if (url.ToLower().Contains("http"))
			{
				Debug.LogWarning($"Downloading Carbon addon '{Path.GetFileName(url)}'...");

				var client = UnityWebRequest.Get(url);

				client.BeginWebRequest();

				while (!client.isDone)
				{
					yield return null;
				}

				buffer = client.downloadHandler.data;
			}
			else
			{
				Debug.LogWarning($"Loading local Carbon addon '{Path.GetFileName(url)}'...");

				yield return CoroutineEx.waitForSeconds(1f);

				buffer = File.ReadAllBytes(url);
			}

			var addon = Addon.Deserialize(buffer);
			LocalCache.StoreCache(addon);

			Debug.Log($"Loaded '{addon.name} v{addon.version}' by {addon.author} ({addon.assets.Count:n0} assets)");

			AddonManager.Instance.LoadedAddons.Add(addon, default);

			addons.Add(addon);
			yield return CoroutineEx.waitForSeconds(0.2f);
		}

		if (load)
		{
			yield return LoadAddonsAsync();
		}
	}
	public static IEnumerator LoadAddonsAsync()
	{
		var currentAssets = 1;

		yield return CoroutineEx.waitForEndOfFrame;

		for (int i = 0; i < AddonManager.Instance.LoadedAddons.Count; i++)
		{
			var addon = AddonManager.Instance.LoadedAddons.ElementAt(i).Key;

			foreach (var asset in addon.assets)
			{
				yield return asset.Value.Client_UnpackBundleAsync();
				currentAssets++;

				yield return CoroutineEx.waitForEndOfFrame;
				yield return CoroutineEx.waitForEndOfFrame;
			}

			AddonManager.Instance.LoadedAddons[addon] = GetAddonCache(addon);

			yield return CoroutineEx.waitForEndOfFrame;
			yield return CoroutineEx.waitForEndOfFrame;
		}

		ClientNetwork.ins.serverConnection.Write.Start(Messages.AddonsLoaded);
		ClientNetwork.ins.serverConnection.Write.Send();

		yield return CreateScene();

		Debug.Log($"Finished loading addons!");
	}
	public static IEnumerator LoadAddonsBasedOnManifestAsync(Addon.Manifest[] manifests)
	{
		CurrentAddons = manifests;

		var addons = new List<Addon>();

		yield return CoroutineEx.waitForSeconds(.5f);

		var nonCached = new List<string>();

		foreach (var manifest in manifests)
		{
			if (LocalCache.TryGetCache(manifest, out var item, true))
			{
				try
				{
					var addon = LocalCache.LoadFromCache(item);
					addons.Add(addon);

					Debug.Log($"Loaded '{addon.name} v{addon.version}' by {addon.author} ({addon.assets.Count:n0} assets)");

					AddonManager.Instance.LoadedAddons.Add(addon, default);
				}
				catch (Exception ex)
				{
					Debug.LogError($"Failed {manifest.info.CacheName} ({ex.Message})\n{ex.StackTrace}");
					nonCached.Add(manifest.url);
				}
			}
			else
			{
				nonCached.Add(manifest.url);
			}
		}

		using var temp = TempArray<string>.New(nonCached.ToArray());
		yield return DownloadAddonsAsync(temp.array, load: true);

		nonCached.Clear();
		nonCached = null;
	}

	public static IEnumerator CreateScene()
	{
		foreach (var addon in AddonManager.Instance.LoadedAddons)
		{
			if (addon.Value.ScenePrefabs == null)
			{
				continue;
			}

			foreach (var prefab in addon.Value.ScenePrefabs)
			{
				Client_CreateFromCacheAsync(prefab, prefab =>
				{
					if (prefab != null)
					{
						prefab.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
						prefab.transform.localScale = Vector3.one;
					}
				});

				yield return CoroutineEx.waitForEndOfFrame;
			}
		}
	}

	public static AddonManager.CacheAddon GetAddonCache(Addon addon)
	{
		AddonManager.CacheAddon cache = default;
		cache.Scene = addon.assets.FirstOrDefault(x => x.Key == "scene").Value;
		cache.Models = addon.assets.FirstOrDefault(x => x.Key == "models").Value;

		if (cache.Scene != null)
		{
			cache.ScenePrefabs = cache.Scene.cachedBundle.GetAllAssetNames();
		}

		return cache;
	}

	internal static Transform LookupParent(Transform origin, string parent)
	{
		return origin?.Find(parent);
	}
	internal static void PrewarmAsset(this Asset asset)
	{
		foreach (var name in asset.cachedBundle.GetAllAssetNames())
		{
			var modifiedName = name.ToLower();

			if (AddonManager.Instance.Prefabs.ContainsKey(modifiedName))
			{
				continue;
			}

			AddonManager.CachePrefab prefab = default;
			prefab.Path = modifiedName;
			prefab.Object = asset.cachedBundle.LoadAsset<GameObject>(name);

			RegisterSpawnablePrefab(modifiedName, prefab.Object);
			AssetProcessor.ProcessGameObject(prefab.Object, asset);

			if (asset.cachedRustBundle.rustPrefabs != null &&
				asset.cachedRustBundle.rustPrefabs.TryGetValue(modifiedName, out var prefabs))
			{
				prefab.RustPrefabs = prefabs;
			}

			AddonManager.Instance.Prefabs.Add(modifiedName, prefab);
		}
	}

	public static void RegisterSpawnablePrefab(string assetPath, GameObject gameObject)
	{
		try
		{
			if (gameObject == null)
			{
				return;
			}

			if (gameObject.GetComponent<BaseCarbonEntity>() == null)
			{
				return;
			}

			var info = new PrefabInfo(assetPath, gameObject);
			ins.spawnablePrefabs[info.assetId] = info;
		}
		catch (Exception ex)
		{
			Debug.LogError($"{assetPath} ({ex.Message})\n{ex.StackTrace}");
		}
	}

	public static bool HandleDisabled(RustComponent component, GameObject go)
	{
		if (component.Client != PostProcessMode.Disabled)
		{
			return false;
		}

		go.SetActive(false);
		return true;
	}
	public static bool HandleDestroy(RustComponent component, GameObject go)
	{
		if (component.Client != PostProcessMode.Destroyed)
		{
			return false;
		}

		UnityEngine.Object.Destroy(go);
		return true;
	}
}
