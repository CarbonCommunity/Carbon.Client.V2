using UnityEngine;

public class AssetProcessor
{
    public static readonly Shader RustStandardShader = Shader.Find("Rust/Standard");

	public static void ProcessGameObject(GameObject go) => ProcessTransform(go.transform);
    public static void ProcessTransform(Transform transform)
    {
        HandleRenderer(transform.GetComponent<Renderer>());

        for (int i = 0; i < transform.childCount; i++)
        {
            ProcessTransform(transform.GetChild(i));
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
}
