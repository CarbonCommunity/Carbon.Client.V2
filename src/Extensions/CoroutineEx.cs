using System.Collections.Generic;
using System.Net.Cache;
using UnityEngine;

public static class CoroutineEx
{
	internal static Dictionary<float, WaitForSeconds> waitForSecondsCache = new();

	public static WaitForEndOfFrame waitForEndOfFrame = new();
	public static WaitForSeconds waitForSeconds(float seconds)
	{
		if(waitForSecondsCache.TryGetValue(seconds, out var cache))
		{
			return cache;
		}

		return waitForSecondsCache[seconds] = new WaitForSeconds(seconds);
	}
}
