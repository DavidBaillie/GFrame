using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class MAIN : Node3D
{
    [Export]
    public SceneChangeRequest CollectionToLoad { get; set; }

    public override void _Ready()
    {
        base._Ready();

        LevelManager.LoadSceneCollection(CollectionToLoad);
    }
}
