using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using System;
using UnityEngine;

public static class UnityEx
{
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
}
