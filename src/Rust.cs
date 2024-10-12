using System;

namespace Carbon;

public class Rust
{
    public static GameObjectCache MenuUI = new("MenuUI(Clone)");
    public static GameObjectCache EngineUI = new("EngineUI(Clone)");

    public static Action OnMenuShow;
    public static Action OnMenuHide;
}
