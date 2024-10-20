using Carbon.Client.Assets;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AssetProcessor
{
	public static Scene CarbonScene;
	public static readonly Shader RustStandardShader = Shader.Find("Rust/Standard");

	public static void ProcessGameObject(GameObject go, Asset asset)
	{
		ProcessTransform(go.transform, asset);
	}
	public static void ProcessTransform(Transform transform, Asset asset)
	{
		HandleRenderer(transform.GetComponent<Renderer>());
		HandleRustComponents(transform, asset);

		for (int i = 0; i < transform.childCount; i++)
		{
			ProcessTransform(transform.GetChild(i), asset);
		}
	}

	internal static void HandleRenderer(Renderer renderer)
	{
		if (renderer == null)
		{
			return;
		}

		foreach (var material in renderer.materials)
		{
			if (!material.HasProperty("_Mode"))
			{
				continue;
			}

			var render = material.GetFloat("_Mode");
			material.shader = RustStandardShader;

			if (!(render > 1))
			{
				continue;
			}

			material.SetFloat("_Mode", 0);
			material.renderQueue = 3000;
		}
	}
	internal static void HandleRustComponents(Transform transform, Asset asset)
	{
		if (asset.cachedRustBundle.components.TryGetValue(transform.GetRecursiveName().ToLower(), out var comps))
		{
			foreach (var component in comps)
			{
				if (AssetEx.Client_PreApplyObject(component, transform.gameObject))
				{
					AssetEx.Client_PreApplyComponent(component, transform.gameObject);
				}
			}
		}

		for (int i = 0; i < transform.childCount; i++)
		{
			HandleRustComponents(transform.GetChild(i), asset);
		}
	}
}
