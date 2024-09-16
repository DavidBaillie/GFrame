using Godot;
using System.Collections.Generic;
using System.Linq;

[GlobalClass]
public partial class LevelManager : Node
{
    public static LevelManager Instance { get; private set; }

    [Export]
    public string SingletonPath { get; set; } = "/root/Scene_Manager";

    [Export]
    public PackedScene DefaultLoadingScreen { get; set; }

    [Export]
    public LevelCollectionTag OverrideStartLevel { get; set; } = null;

    public bool HasLoadedACollection => CurrentCollection != null;
    public LevelCollectionTag CurrentCollection { get; set; } = null;
    public Node CurrentBaseNode { get; set; } = null;

    private Node loadingScreenNode = null;


    public override void _Ready()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            GD.PrintErr("Scene Manager already has a singleton, this should never happen!");
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);


    }

    /// <summary>
    /// Loads the provided game mode and scenes for the mode
    /// </summary>
    /// <param name="collection">Level collection to load</param>
    /// <param name="freeUntrackedScenes">If unknown nodes should be removed</param>
    public void LoadSceneCollection(LevelCollectionTag collection, bool freeUntrackedScenes = true)
    {
        if (collection == null)
        {
            GD.PrintErr($"Cannot load a null collection.");
            return;
        }

        if (CurrentCollection == collection)
        {
            GD.PrintErr($"Cannot load the provided collection {collection.ResourceName} because it is already live!");
            return;
        }

        CallDeferred(MethodName.StartLoadingNewLevel, collection, freeUntrackedScenes);
    }

    /// <summary>
    /// Kick off the loading process by creating the needed loading screen to hide the transition
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="freeUntrackedScenes"></param>
    private void StartLoadingNewLevel(LevelCollectionTag collection, bool freeUntrackedScenes = true)
    {
        // Load loading screen
        var loadingScreen = collection.LoadingScreenOverride ?? DefaultLoadingScreen;
        AsyncResourceLoadingManager.AddResourceLoadEvent(nameof(LevelManager), loadingScreen.ResourcePath,
            (assets, failures) => LoadNewNodes(collection, assets[0], freeUntrackedScenes),
            (percentLoaded) => { /* Loading percentage */ },
            (error) => GD.PrintErr($"Failed to load the loading screen [{loadingScreen?.ResourceName}]"));

        var loadingScreenNode = ResourceLoader.Load<PackedScene>(loadingScreen.ResourcePath).Instantiate<Node>();
    }

    /// <summary>
    /// Now that we have a loading screen, free the current nodes and load the next ones
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="freeUntrackedScenes"></param>
    private void LoadNewNodes(LevelCollectionTag collection, Resource loadingScreen, bool freeUntrackedScenes = true)
    {
        // Free nodes
        // Load new nodes

        loadingScreenNode = (loadingScreen as PackedScene).Instantiate();
        GetTree().Root.AddChild(loadingScreenNode);

        //End previous game mode and decide what we're doing
        CurrentCollection?.GameMode?.Cleanup(CurrentBaseNode);
        var nodeActions = GenerateSceneActions(collection);

        //Create new node to hold everything
        var newNode = new Node();
        newNode.Name = string.IsNullOrWhiteSpace(collection.EditorDisplayName) ? collection.ResourceName : collection.EditorDisplayName;
        GetTree().Root.AddChild(newNode);

        //Tranfer nodes we're keeping
        foreach (var node in nodeActions.NodesToTransfer)
        {
            node.GetParent().RemoveChild(node);
            newNode.AddChild(node);
        }

        //Kill the old node holding children
        if (CurrentBaseNode is not null)
            CurrentBaseNode.QueueFree();

        //Kill untracked nodes
        if (freeUntrackedScenes)
        {
            foreach (var node in nodeActions.UntrackedNodes)
            {
                node.QueueFree();
            }
        }

        //Load the new nodes
        CurrentBaseNode = newNode;

        AsyncResourceLoadingManager.AddResourcesLoadEvent(nameof(LevelManager), nodeActions.ScenesToLoad.Select(x => x.ResourcePath).ToList(),
            (assets, failures) => { InstanceNewNodes(collection, assets); },
            (progress) => { /* Progress Bar */ },
            (error) => { GD.PrintErr($"Failed to load any new scenes during transition!"); });

        foreach (var scene in nodeActions.ScenesToLoad) // Need to be able to load many scenes
        {
            var path = scene.ResourcePath;
            var loaded = GD.Load<PackedScene>(path);
            var instance = loaded.Instantiate();

            CurrentBaseNode.AddChild(instance);
        }

        //Start new mode and remove loading screen
        CurrentCollection = collection;
        collection.GameMode?.Setup(CurrentBaseNode);
        loadingScreenNode.QueueFree();
    }

    private void InstanceNewNodes(LevelCollectionTag collection, List<Resource> scenes)
    {
        foreach(var resource in scenes)
        {
            if (resource is PackedScene scene)
            {
                CurrentBaseNode.AddChild(scene.Instantiate());
            }
        }

        CurrentCollection = collection;
        collection.GameMode?.Setup(CurrentBaseNode);
        loadingScreenNode.QueueFree();
    }

    /// <summary>
    /// Determines what load actions need to be taken to transition from one level to another
    /// </summary>
    /// <param name="collection">The new level being loaded</param>
    /// <returns>Collection of actions to take</returns>
    private SceneActionCollection GenerateSceneActions(LevelCollectionTag collection)
    {
        var existingActions = GenerateActionsForExistingScenes(collection);
        var newActions = GenerateActionsForNewCollectionScenes(collection);

        return new SceneActionCollection()
        {
            ScenesToLoad = newActions,
            NodesToTransfer = existingActions.toMove,
            UntrackedNodes = existingActions.unknown
        };
    }

    /// <summary>
    /// Given a collection, determines what nodes need to be unloaded and what nodes are 'unknown'
    /// </summary>
    /// <param name="collection">Collection being loaded</param>
    private (List<Node> toMove, List<Node> unknown) GenerateActionsForExistingScenes(LevelCollectionTag collection)
    {
        var pretty = GetTreeStringPretty();
        var rootNodes = GetTree().Root.GetChildren().ToList();
        var unknownNodes = rootNodes.Where(x => NodeUtilities.IsSceneNode(x) || NodeUtilities.IsUiNode(x)).ToList();
        var toMoveNodes = new List<Node>();

        if (CurrentBaseNode != null && !collection.ReloadAlreadyExistingNodes)
        {
            foreach (var node in CurrentBaseNode.GetChildren().Where(x => NodeUtilities.IsSceneNode(x) || NodeUtilities.IsUiNode(x)))
            {
                if (!string.IsNullOrEmpty(node.SceneFilePath) && collection.Scenes.Any(x => x.ResourcePath == node.SceneFilePath))
                    toMoveNodes.Add(node);
            }
        }

        return (toMoveNodes, unknownNodes);
    }

    /// <summary>
    /// Given a collection, determines whick of the nodes need to be loaded into the game
    /// </summary>
    /// <param name="collection">Collection being loaded</param>
    private List<PackedScene> GenerateActionsForNewCollectionScenes(LevelCollectionTag collection)
    {
        if (CurrentBaseNode is null || collection.ReloadAlreadyExistingNodes)
            return collection.Scenes.ToList();

        var currentNodes = CurrentBaseNode.GetChildren()
            .Where(x => NodeUtilities.IsSceneNode(x) || NodeUtilities.IsUiNode(x) && !string.IsNullOrEmpty(x.SceneFilePath));

        return collection.Scenes.Where(inboundScene => !currentNodes.Any(curr => curr.SceneFilePath == inboundScene.ResourcePath)).ToList();
    }

    private struct SceneActionCollection
    {
        public List<PackedScene> ScenesToLoad { get; set; }
        public List<Node> NodesToTransfer { get; set; }
        public List<Node> UntrackedNodes { get; set; }
    }
}
