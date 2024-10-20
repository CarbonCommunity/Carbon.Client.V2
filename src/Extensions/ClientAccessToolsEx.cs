using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace Carbon.Client.Extensions;

public static class ClientAccessToolsEx
{
	public static Type TypeByName(string name)
	{
		Type type = Type.GetType(name, throwOnError: false);
		if ((object)type == null)
		{
			type = AllTypes().FirstOrDefault((Type t) => t.FullName == name);
		}

		if ((object)type == null)
		{
			type = AllTypes().FirstOrDefault((Type t) => t.Name == name);
		}

		if ((object)type == null)
		{
			UnityEngine.Debug.LogError("AccessTools.TypeByName: Could not find type named " + name);
		}

		return type;
	}

	public static IEnumerable<Type> AllTypes()
	{
		foreach (var assembly in AllAssemblies())
		{
			foreach (var type in GetTypesFromAssembly(assembly))
			{
				yield return type;
			}
		}
	}

	public static Type[] GetTypesFromAssembly(Assembly assembly)
	{
		try
		{
			return assembly.GetTypes();
		}
		catch (ReflectionTypeLoadException ex)
		{
			// UnityEngine.Debug.LogError($"AccessTools.GetTypesFromAssembly: assembly {assembly} => {ex}");
			return ex.Types.Where((Type type) => (object)type != null).ToArray();
		}
	}

	public static IEnumerable<Assembly> AllAssemblies()
	{
		return from a in AppDomain.CurrentDomain.GetAssemblies()
			   where !a.FullName.StartsWith("Microsoft.VisualStudio")
			   select a;
	}

	public static IEnumerable<Type> GetConstraints(Type type)
	{
		// generics with only one type will be supported
		Type[] generics = type.GetGenericArguments();

		if (generics.Count() > 1)
			throw new Exception($"GetConstraints only supports generics with one type");

		Type generic = generics.First();
		return generic.GetGenericParameterConstraints();
	}

	public static IEnumerable<Type> MatchConstrains(IEnumerable<Type> constrains)
	{
		IEnumerable<Type> interfaces = constrains.Where(x => x.IsInterface);
		Type @base = constrains.Single(x => !x.IsInterface);

		return AllTypes().Where(type =>
			type.IsSubclassOf(@base) && type.GetInterfaces().Intersect(interfaces).Any());
	}

	public static CodeInstruction WithLabel(this CodeInstruction instruction, Label label)
	{
		instruction.labels.Add(label);
		return instruction;
	}
}
