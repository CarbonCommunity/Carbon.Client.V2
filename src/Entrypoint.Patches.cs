using HarmonyLib;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Linq;
using Carbon.Extensions;

public partial class Entrypoint
{
	public void MakePatch()
	{

		var patch = new Harmony("com.domain.carbon4client");
		patch.PatchAll(typeof(Entrypoint).Assembly);

		var methods = patch.GetPatchedMethods();
		var methodCount = methods.Count();

		foreach(var method in methods)
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
}