using UnityEngine;

public struct GameObjectCache
{
    public string key;
    public GameObject value;

    public GameObjectCache(string key)
    {
        this.key = key;
        this.value = Lookup(key).value;
    }
    public GameObjectCache(string key, GameObject value)
    {
        this.key = key;
        this.value = value;
    }

    public GameObject Get() => value ??= Lookup(key).value;

    public static GameObjectCache Lookup(string key)
    {
        return new GameObjectCache(key, GameObject.Find(key));
    }
}

public struct LocalPlayerCache
{
	public BasePlayer player;
	public PlayerEyes eyes;
	public Camera camera;

	public BasePlayer Get()
	{
		if (player == null)
		{
			player ??= Carbon.Rust.LocalPlayer.Get()?.GetComponent<BasePlayer>();
			eyes = player.GetComponent<PlayerEyes>();
			camera = GameObject.Find("Main Camera").GetComponent<Camera>();
		}

		return player;
	}
}
