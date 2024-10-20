using Carbon.Client.Assets;
using Carbon.Extensions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class LocalCache
{
	public static string Directory
	{
		get
		{
			var directory = Path.Combine(Application.dataPath, "..", "addons");

			if (!OsEx.Folder.Exists(directory))
			{
				OsEx.Folder.Create(directory);
			}

			return directory;
		}
	}

	public static List<CacheItem> Items = new();

	public class CacheItem
	{
		public string Path;
		public Addon.Manifest Manifest;
		public Addon Addon;
	}

	public static void LoadCache()
	{
		Items.Clear();

		OsEx.Folder.Create(Path.Combine(Directory, "cache"));

		foreach (var manifest in OsEx.Folder.GetFilesWithExtension(Directory, "addon"))
		{
			Debug.Log($"Loading cache from {manifest}");

			var manifestInstance = JsonConvert.DeserializeObject<Addon.Manifest>(OsEx.File.ReadText(manifest));

			Items.Add(new CacheItem
			{
				Manifest = manifestInstance,
				Path = Path.Combine(Directory, "cache", $"{manifestInstance.info.CacheName}.cca-cache")
			});
		}
	}

	public static bool TryGetCache(Addon.Manifest manifest, out CacheItem item, bool checksum)
	{
		item = Items.FirstOrDefault(x => x.Manifest.info.CacheName == manifest.info.CacheName);

		if (item != null && item.Manifest != null)
		{
			return item.Manifest.info.version == manifest.info.version &&
				   item.Manifest.info.CacheName == manifest.info.CacheName &&
				   (checksum ? item.Manifest.checksum == manifest.checksum : true);
		}

		return false;
	}

	public static Addon LoadFromCache(CacheItem item)
	{
		if (item.Addon != null)
		{
			Debug.Log($"Loaded addon cache for '{item.Manifest.info.name} v{item.Manifest.info.version}'");
			return item.Addon;
		}

		item.Addon = Addon.Deserialize(OsEx.File.ReadBytes(item.Path));

		Debug.Log($"Loaded initial addon cache for '{item.Manifest.info.name} v{item.Manifest.info.version}'");

		return item.Addon;
	}

	public static void StoreCache(Addon addon)
	{
		var manifest = addon.GetManifest();
		manifest.checksum = addon.checksum;
		manifest.url = null;

		OsEx.Folder.Create(Path.Combine(Directory, "cache"));
		OsEx.File.Create(Path.Combine(Directory, "cache", $"{manifest.info.CacheName}.cca-cache"), addon.buffer);
		OsEx.File.Create(Path.Combine(Directory, $"{manifest.info.CacheName}.addon"), JsonConvert.SerializeObject(manifest, Formatting.Indented));

		if (!TryGetCache(manifest, out var item, false))
		{
			item = new();
			Items.Add(item);
		}

		item.Manifest = manifest;
		item.Path = Path.Combine(Directory, "cache", $"{item.Manifest.info.CacheName}.cca-cache");
		item.Addon = addon;

		Debug.Log($"Stored addon cache for '{manifest.info.name}'");
	}
}
