using Carbon.Client;
using UnityEngine;

public class CustomEnty : MonoBehaviour
{
	public string test;
	public Transform childpls;

	public void Start()
	{
		Debug.Log($"aww {childpls?.name} | {test}");
	}
}
