using Godot;
using Godot.Collections;

/// <summary>
/// Represents a collection of nodes along with a gamemode that will be loaded as a 'scene/level'
/// </summary>
[GlobalClass, Tool]
public partial class LevelCollectionTag : Tag
{
    /// <summary>
    /// Display name used for debugging
    /// </summary>
    [Export]
    public string EditorDisplayName { get; set; } = string.Empty;

    /// <summary>
    /// When a required node in this collection already exists in the previous collection, should the transition reload that node or keep it?
    /// </summary>
    [Export]
    public bool ReloadAlreadyExistingNodes { get; set; } = false;

    /// <summary>
    /// Gamemode that will be loaded with the level
    /// </summary>
    [Export(PropertyHint.ResourceType)]
    public GameModeTag GameMode { get; set; } = null;

    /// <summary>
    /// Optional custom loading screen node/scene that will be shown during transition.
    /// Will use the global default otherwise.
    /// </summary>
    [Export]
    public PackedScene LoadingScreenOverride { get; set; } = null;

    /// <summary>
    /// Nodes/Scenes that will be part of loading this level
    /// </summary>
    [Export(PropertyHint.GlobalFile)]
    public Array<PackedScene> Scenes { get; set; } = new();
}
