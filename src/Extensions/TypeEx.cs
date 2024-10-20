using Carbon.Extensions;
using Il2CppInterop.Runtime.Injection;
using System;

public static class TypeEx
{
	internal static readonly Type il2cppObject = typeof(Il2CppSystem.Object);

	public static void RecursivelyRegisterType(this Type type)
	{
		if (type == null)
		{
			return;
		}

		try
		{
			if (il2cppObject.IsAssignableFrom(type))
			{
				if (!ClassInjector.IsTypeRegisteredInIl2Cpp(type))
				{
					ClassInjector.RegisterTypeInIl2Cpp(type);
				}
				else
				{
					Console.WriteLine($" '{type.FullName}' is already registered [{il2cppObject.FullName}]");
				}
			}
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.LogError($"Failed type registration for '{type.FullName}' {ex.GetFullStackTrace()}");
		}

		var nested = type.GetNestedTypes();

		if (nested != null)
		{
			foreach (var nestedType in type.GetNestedTypes())
			{
				RecursivelyRegisterType(nestedType);
			}
		}
	}
}