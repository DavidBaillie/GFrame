#if TOOLS
using Godot;

[Tool]
public partial class FrameworkEditorManager : EditorPlugin
{
	public const string AsyncResourceLoadedName = "resource_loader";
	public const string TagManagerName = "tag_manager";
	public const string LevelManagerName = "level_manager";

	public override void _EnterTree()
	{
		AddAutoloadSingleton(AsyncResourceLoadedName, "res://addons/assertiveframework/Nodes/AsyncResourceLoadingManager.tscn");
		AddAutoloadSingleton(TagManagerName, "res://addons/assertiveframework/Nodes/TagManager.tscn");
		AddAutoloadSingleton(LevelManagerName, "res://addons/assertiveframework/Nodes/LevelManager.tscn");
	}

	public override void _ExitTree()
	{
        RemoveAutoloadSingleton(AsyncResourceLoadedName);
        RemoveAutoloadSingleton(TagManagerName);
        RemoveAutoloadSingleton(LevelManagerName);
    }
}
#endif
