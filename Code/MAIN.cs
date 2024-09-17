using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class MAIN : Node3D
{
    [Export]
    public LevelCollectionTag CollectionToLoad { get; set; }

    public override void _Ready()
    {
        base._Ready();

        AsyncResourceLoadingManager.AddResourceLoadEvent("MAIN", "res://Node/ExampleScene1.tscn",
            (resources, failures) =>
            {
                if (failures.Count > 0)
                {
                    GD.PrintErr($"Got error: {failures[0]}");
                }

                if (resources.Count > 0)
                {
                    GD.Print($"Got resource: {resources[0].ResourcePath}");
                }
            },
            (progress) => { GD.Print($"Progress: {progress}"); },
            (error) => { GD.PrintErr($"Failed: {error}"); });

        if (TagManager.TryGetTag("c948a7f7-9a74-49e4-a247-dab6af238a3b", out GameModeTag tag))
            GD.Print($"Got tag: {tag.ResourcePath}");
        else
            GD.Print($"Failed to get tag");

        if (TagManager.TryGetTags(out List<GameModeTag> tags))
            GD.Print($"Got all tags: {string.Join(", ", tags.Select(x => x.ResourcePath))}");
        else
            GD.Print($"Got no tags for GameMode");

        LevelManager.LoadSceneCollection(CollectionToLoad);
    }
}
