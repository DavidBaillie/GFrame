using Godot;

[GlobalClass]
public partial class FrameworkStartScene : Node3D
{
    [Export]
    public SceneChangeRequest StartSceneCollection { get; set; }

    public override void _Ready()
    {
        var loadedId = OS.GetEnvironment("sceneCollectionOverride");

        if (OS.IsDebugBuild())
        {
            if (TagManager.TryGetTag<LevelCollectionTag>(loadedId, out var tag))
            {
                LevelManager.LoadSceneCollection(tag.ResourcePath);
            }
            else
            {
                LevelManager.LoadSceneCollection(StartSceneCollection);
            }
        }
        else
        {
            LevelManager.LoadSceneCollection(StartSceneCollection);
        }
    }
}
