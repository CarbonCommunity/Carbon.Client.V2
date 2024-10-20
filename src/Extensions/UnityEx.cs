using Il2CppInterop.Runtime;
using UnityEngine;

public static class UnityEx
{
	public static GameObject SpawnGameObject(string name, Vector3 pos = default, Quaternion rot = default)
	{
		var go = new GameObject(name);
		go.transform.SetPositionAndRotation(pos, rot);
		UnityEngine.Object.DontDestroyOnLoad(go);
		return go;
	}

    public static T AddUnityComponent<T>(this GameObject go) where T : Component
    {
        if (go == null)
        {
            return null;
        }

        var type = typeof(T);
        type.RecursivelyRegisterType();

        return go.AddComponent(Il2CppType.From(type)).Cast<T>();
    }

    public static string GetRecursiveName(this Transform transform, string strEndName = "")
    {
        string text = transform.name;
        if (!string.IsNullOrEmpty(strEndName))
        {
            text = text + "/" + strEndName;
        }

        if (transform.parent != null)
        {
            text = transform.parent.GetRecursiveName(text);
        }

        return text;
    }
}
