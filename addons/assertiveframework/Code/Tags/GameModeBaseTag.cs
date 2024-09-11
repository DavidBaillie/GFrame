using Godot;

/// <summary>
/// Game mode base tag intended to be implemented by game modes in the project
/// </summary>
[GlobalClass, Tool]
public partial class GameModeTag : Tag
{
    /// <summary>
    /// Called when the gamemode is first loaded. 
    /// </summary>
    /// <param name="rootNode">Root node that other resources can be loaded under that will be disposed of later automatically</param>
    public virtual void Setup(Node rootNode) { }

    /// <summary>
    /// Called when the gamemode is being ended and should perform any needed cleanup
    /// </summary>
    /// <param name="rootNode">Root node that is being cleaned up as part of the transition</param>
    public virtual void Cleanup(Node rootNode) { }
}
