using UnityEngine;

public class AssetProcessor
{
    public static readonly Shader RustStandardShader = Shader.Find("Rust/Standard");

    public static void Postprocess(Transform transform)
    {
        var renderer = transform.GetComponent<Renderer>();

        if (renderer != null)
        {
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

        for (int i = 0; i < transform.childCount; i++)
        {
            Postprocess(transform.GetChild(i));
        }
    }
}
