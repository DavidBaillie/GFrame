using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[GlobalClass]
public partial class LevelManager : Node
{
    public static LevelManager Instance { get; private set; }

    [Export]
    public PackedScene DefaultLoadingScreen { get; set; }

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

    /// <summary>
    /// Loads the provided game mode and scenes for the mode
    /// </summary>
    /// <param name="request">Level collection to load</param>
    /// <param name="freeUntrackedScenes">If unknown nodes should be removed</param>
    public static void LoadSceneCollection(SceneChangeRequest request, bool freeUntrackedScenes = true)
        => Instance.DeferLoadingStart(request, freeUntrackedScenes);

    private void DeferLoadingStart(SceneChangeRequest request, bool freeUntrackedScenes = true)
    {
        if (request == null)
        {
            GD.PrintErr($"Cannot load a null request.");
            return;
        }

        LevelCollectionTag collection = null;

        try
        {
            collection = GD.Load<LevelCollectionTag>(request.LevelCollectionPath)
                ?? throw new ArgumentException($"No such resource at path: {request.LevelCollectionPath}");
        }
        catch (Exception e)
        {
            GD.PrintErr($"No such collection exists with name {request.LevelCollectionPath}.\nException: {e}");
            return;
        }

        if (CurrentCollection == collection)
        {
            GD.PrintErr($"Cannot load the provided collection {request.ResourceName} because it is already live!");
            return;
        }

        StartLoadingNewLevel(collection, freeUntrackedScenes);
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

        if (loadingScreen == null)
        {
            LoadNewNodes(collection, new Node(), freeUntrackedScenes);
        }
        else
        {
            AsyncResourceLoadingManager.AddResourceLoadEvent($"{nameof(LevelManager)}-{nameof(LoadNewNodes)}", loadingScreen.ResourcePath,
                (assets, failures) => LoadNewNodes(collection, (assets[0] as PackedScene).Instantiate(), freeUntrackedScenes),
                (percentLoaded) => { /* Loading percentage */ },
                (error) => GD.PrintErr($"Failed to load the loading screen [{loadingScreen?.ResourceName}]"));
        }
    }

    /// <summary>
    /// Now that we have a loading screen, free the current nodes and load the next ones
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="freeUntrackedScenes"></param>
    private void LoadNewNodes(LevelCollectionTag collection, Node loadingScreen, bool freeUntrackedScenes = true)
    {
        loadingScreenNode = loadingScreen;
        GetTree().Root.CallDeferred(Node.MethodName.AddChild, loadingScreenNode);
        
        //GetTree().Root.AddChild(loadingScreenNode);

        //Create new node to hold everything
        var newNode = new Node();
        newNode.Name = string.IsNullOrWhiteSpace(collection.EditorDisplayName) ? collection.ResourceName : collection.EditorDisplayName;
        GetTree().Root.CallDeferred(Node.MethodName.AddChild, newNode);

        // Same gamemode, different collection
        if (CurrentCollection?.GameMode is not null && CurrentCollection.GameMode == collection.GameMode)
            CurrentCollection.GameMode.ChangeCollection(CurrentBaseNode, newNode);
        // Different gamemode, different collection
        else
            CurrentCollection?.GameMode?.Cleanup(CurrentBaseNode);
        
        // Determine what changes are needed
        var nodeActions = GenerateSceneActions(collection);
        
        // Move over the nodes we're keeping
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

        AsyncResourceLoadingManager.AddResourcesLoadEvent($"{nameof(LevelManager)}-{nameof(InstanceNewNodes)}", nodeActions.ScenesToLoad.Select(x => x.ResourcePath).ToList(),
            (assets, failures) => { InstanceNewNodes(collection, assets, failures); },
            (progress) => { /* Progress Bar */ },
            (error) => { GD.PrintErr($"Failed to load any new scenes during transition!"); });
    }

    private void InstanceNewNodes(LevelCollectionTag collection, List<Resource> scenes, List<AsyncResourceLoadingManager.LoadFailure> failedLoads)
    {
        if (failedLoads.Count > 0)
            GD.PrintErr($"Failed to load {string.Join(", ", failedLoads.Select(x => x.Path))}\nAble to load: {string.Join(", ", scenes.Select(x => x.ResourcePath))}");

        try
        {
            foreach (var resource in scenes)
            {
                if (resource is PackedScene scene)
                {
                    var node = scene.Instantiate();
                    CurrentBaseNode.AddChild(node);
                }
            }

            CurrentCollection = collection;
            collection.GameMode?.Setup(CurrentBaseNode);
            loadingScreenNode?.QueueFree();
            loadingScreenNode = null;
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to instance new nodes: {e}");
        }
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
            return collection.Scenes
                .Where(x => x != null)
                .ToList();

        var currentNodes = CurrentBaseNode.GetChildren()
            .Where(x => NodeUtilities.IsSceneNode(x) || NodeUtilities.IsUiNode(x) && !string.IsNullOrEmpty(x.SceneFilePath));

        return collection.Scenes
            .Where(inboundScene => inboundScene != null && !currentNodes.Any(curr => curr.SceneFilePath == inboundScene.ResourcePath))
            .ToList();
    }

    private struct SceneActionCollection
    {
        public List<PackedScene> ScenesToLoad { get; set; }
        public List<Node> NodesToTransfer { get; set; }
        public List<Node> UntrackedNodes { get; set; }
    }
}
