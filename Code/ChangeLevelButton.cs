using Godot;
using System;

public partial class ChangeLevelButton : Button
{
    [Export]
    public SceneChangeRequest Level { get; set; }

    public override void _Pressed()
    {
        base._Pressed();
        LevelManager.LoadSceneCollection(Level);
    }
}
