using Godot;

/// <summary>
/// This resource exists purely so I can avoid a circular load reference between scenes and LevelCollections. 
/// Effectively an intermediary bewteen the references.
/// </summary>
[GlobalClass]
public partial class SceneChangeRequest : Resource
{
    [Export(PropertyHint.File)]
    public string LevelCollectionPath { get; set; }
}
