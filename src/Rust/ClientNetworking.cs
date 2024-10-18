using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Carbon;

public static unsafe class ClientNetworking
{
	public static void SendMessage(ReadOnlySpan<byte> data, int priority, int reliability, int channel)
	{
		fixed (byte* be = data)
		{
			send_message(be, data.Length, priority, reliability, channel);
		}
	}

	[DllImport("Carbon_Client_Native", CallingConvention = CallingConvention.Cdecl)]
	public static extern void send_message(byte* bytes, int len, int priority, int reliability, int channel);

	public static void OnMesage(byte* ptr, int len)
	{
		ReadOnlySpan<byte> data = new(ptr, len);
		// do stuff with data

		Debug.Log($"brr {data.Length}");
	}

	public static void Init()
	{
		Debug.Log($"A) {LoadLibrary(Path.Combine(Application.dataPath, "Plugins", "x86_64", "RakNet.dll"))}");
		Debug.Log($"B) {LoadLibrary(Path.Combine(Application.dataPath, "Plugins", "x86_64", "EOSSDK-Win64-Shipping.dll"))}");

		init(&OnMesage);
	}

	[DllImport("Carbon_Client_Native", CallingConvention = CallingConvention.Cdecl)]
	public static extern void init(delegate*<byte*, int, void> callback);

	[DllImport("kernel32.dll")]
	public static extern IntPtr LoadLibrary(string dllToLoad);
}
