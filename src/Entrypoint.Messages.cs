using Carbon.Client;
using UnityEngine;

public partial class Entrypoint
{
	internal static void Message_Approval(CarbonServer conn)
	{
		var protocol = conn.Read.Int32();

		if (protocol != Protocol.VERSION)
		{
			Debug.LogWarning($"Could not connect! Wrong protocol. (Expected {Protocol.VERSION}, got {protocol})");

			conn.Write.Start(Messages.Approval);
			conn.Write.Bool(false);
			conn.Write.Send();
			return;
		}

		var userId = conn.Read.UInt64();
		var username = conn.Read.String();

		Debug.Log($"Logged in Carbon 4 Client: {username}[{userId}]");

		conn.Write.Start(Messages.Approval);
		conn.Write.Bool(true);
		conn.Write.Send();
	}

	internal static void Message_AddonLoad(CarbonServer conn)
	{
		var uninstallAll = conn.Read.Bool();
		var addonCount = conn.Read.Int32();


	}
}
