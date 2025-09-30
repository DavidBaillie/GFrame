using Godot;

[GlobalClass, Tool]
public partial class GameModeTag : Tag
{
    public static GameModeTag CurrentGameMode = null;

    public Node RootNode { get; set; }

    /// <summary>
    /// Called when the gamemode is first loaded. 
    /// </summary>
    /// <param name="rootNode">Root node that other resources can be loaded under that will be disposed of later automatically</param>
    public virtual void Setup(Node rootNode)
    {
        RootNode = rootNode;

        if (CurrentGameMode != null)
            GD.PrintErr($"WARN: GameModeTag Assigning a new current mode when another mode is still referenced!");

        CurrentGameMode = this;
    }

    /// <summary>
    /// Called when a new collection is loaded but the same gamemode is being used
    /// </summary>
    /// <param name="rootNode">Root node to store assets under</param>
    public virtual void ChangeCollection(Node oldRootNode, Node newRootNode)
    {
        RootNode = newRootNode;
    }

    /// <summary>
    /// Called when the gamemode is being ended and should perform any needed cleanup
    /// </summary>
    /// <param name="rootNode">Root node that is being cleaned up as part of the transition</param>
    public virtual void Cleanup(Node rootNode)
    {
        RootNode = null;
        CurrentGameMode = null;
    }
}
