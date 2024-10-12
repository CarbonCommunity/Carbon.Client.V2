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
