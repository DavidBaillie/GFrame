#if TOOLS
using Godot;
using System;
using System.Linq;

[Tool]
public partial class FrameworkEditorManager : EditorPlugin
{
	public const string AsyncResourceLoadedName = "resource_loader";
	public const string TagManagerName = "tag_manager";
	public const string LevelManagerName = "level_manager";

    private const string controlPath = "res://addons/assertiveframework/Nodes/Dropdown.tscn";

    private OptionButton menuControl;
    private LevelCollectionTag[] availableSceneCollections = new LevelCollectionTag[0];

    public override void _EnterTree()
	{
		AddAutoloadSingleton(AsyncResourceLoadedName, "res://addons/assertiveframework/Nodes/AsyncResourceLoadingManager.tscn");
		AddAutoloadSingleton(TagManagerName, "res://addons/assertiveframework/Nodes/TagManager.tscn");
		AddAutoloadSingleton(LevelManagerName, "res://addons/assertiveframework/Nodes/LevelManager.tscn");

        var scene = GD.Load<PackedScene>(controlPath);
        if (scene is null)
        {
            GD.PrintErr($"Failed to load asset at path: {controlPath}");
            return;
        }

        menuControl = scene.Instantiate() as OptionButton;
        menuControl.ItemSelected += OnDropdownItemSelected;

        LoadSceneCollectionsTagsFromSystem();
        LoadSceneCollectionsIntoDropdown();

        AddControlToContainer(CustomControlContainer.Toolbar, menuControl);
    }

	public override void _ExitTree()
	{
        RemoveAutoloadSingleton(AsyncResourceLoadedName);
        RemoveAutoloadSingleton(TagManagerName);
        RemoveAutoloadSingleton(LevelManagerName);

        if (menuControl is not null)
        {
            //menuControl.ItemSelected -= OnDropdownItemSelected;
            RemoveControlFromContainer(CustomControlContainer.Toolbar, menuControl);
            menuControl.Free();
        }
    }

    /// <summary>
    /// Called when the dev selects an item from the dropdown
    /// </summary>
    /// <param name="index">Index of item selected</param>
    private void OnDropdownItemSelected(long index)
    {
        OS.SetEnvironment("sceneCollectionOverride", index == 0 ? "" : availableSceneCollections[index - 1].Id.ToString());

        LoadSceneCollectionsTagsFromSystem();
        LoadSceneCollectionsIntoDropdown();
    }

    /// <summary>
    /// Loads all available scene collection tags from the local system
    /// </summary>
    private void LoadSceneCollectionsTagsFromSystem()
    {
        var tags = TagManager.GetAllTags() ?? new();
        availableSceneCollections = tags.OfType<LevelCollectionTag>().ToArray();
    }

    /// <summary>
    /// Loads the available scene collections into the editor dropdown
    /// </summary>
    private void LoadSceneCollectionsIntoDropdown()
    {
        // Clear list
        while (menuControl.ItemCount > 0)
            menuControl.RemoveItem(0);

        // Add items
        menuControl.AddItem("Default", 0);
        for (int i = 0; i < availableSceneCollections.Length; i++)
            menuControl.AddItem(availableSceneCollections[i].EditorDisplayName, i + 1);

        menuControl.Selected = GetIndexOrDefault(availableSceneCollections,
            x => x.Id == OS.GetEnvironment("sceneCollectionOverride")) + 1;
    }

    private static int GetIndexOrDefault<T>(T[] values, Func<T, bool> evaluator)
    {
        for (int i = 0; i < values.Length; i++)
        {
            if (evaluator(values[i]))
                return i;
        }

        return -1;
    }
}
#endif
