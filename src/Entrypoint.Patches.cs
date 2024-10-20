using Carbon.Extensions;
using HarmonyLib;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class Entrypoint
{
	public void MakePatch()
	{
		var patch = new Harmony("com.domain.carbon4client");
		patch.PatchAll(typeof(Entrypoint).Assembly);

		var methods = patch.GetPatchedMethods();
		var methodCount = methods.Count();

		foreach (var method in methods)
		{
			Debug.Log($"- {method}");
		}

		Debug.Log($"Carbon patched {methodCount:n0} {methodCount.Plural("method", "methods")}");
	}

	[HarmonyPatch(typeof(SceneManager), nameof(SceneManager.LoadScene), [typeof(string), typeof(LoadSceneMode)])]
	public class SM_SAS
	{
		public static void Postfix(string sceneName, LoadSceneMode mode)
		{
			Debug.Log($"CHANGED {sceneName} {mode}");
		}
	}

	[HarmonyPatch(typeof(GameObject), nameof(GameObject.SetActive), [typeof(bool)])]
	public class GO_SA
	{
		public static void Postfix(bool value, GameObject __instance)
		{
			if (__instance == Carbon.Rust.MenuUI.Get())
			{
				if (value)
				{
					Carbon.Rust.OnMenuShow?.Invoke();
				}
				else
				{
					Carbon.Rust.OnMenuHide?.Invoke();
				}
			}
		}
	}
}