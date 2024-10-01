using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class TagManager : Node
{
    public static TagManager Instance { get; private set; }

    [Export]
    public string SingletonPath { get; set; } = "/root/Tag_Database";

    [Export]
    public string TagBasePath { get; set; } = "res://Tags";


    private List<Tag> allTags;
    private Dictionary<Type, IEnumerable<Tag>> cachedTags = new();

    /// <summary>
    /// Loaded into scene, setup
    /// </summary>
    public override void _Ready()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            GD.PrintErr("Tag Manager already has a singleton, this should never happen!");
            return;
        }

        allTags = GetAllTags(TagBasePath);
    }

    /// <summary>
    /// Get every tag in the project
    /// </summary>
    public static List<Tag> GetAllTags(string basePath = null)
    {
        if (string.IsNullOrWhiteSpace(basePath))
            basePath = Instance?.TagBasePath ?? "res://Tags";

        var tags = new List<Tag>();
        foreach (var path in FileUtilities.GetAllFilesAtPath(basePath, true, (x) => x.EndsWith(".tres")))
        {
            if (FileUtilities.TryLoadResource(path, out Tag tag))
            {
                if (tag.IsEnabled)
                    tags.Add(tag);
            }
            else
            {
                GD.PrintErr($"Failed to load tag: {path}");
            }
        }

        return tags;
    }

    /// <summary>
    /// Tries to load all tags of a given type
    /// </summary>
    /// <typeparam name="T">Type of the tag</typeparam>
    /// <param name="tags">Found collection of the type</param>
    /// <returns>If any tags were found</returns>
    public static bool TryGetTags<T>(out List<T> tags) where T : Tag
    {
        // Check the cache
        if (Instance.cachedTags.ContainsKey(typeof(T)))
        {
            tags = Instance.cachedTags[typeof(T)].Select(x => x as T).ToList();
            return true;
        }

        // No cache, check types of all possible objects
        var baseTags = Instance.allTags.Where(x => x is T).ToList();
        Instance.cachedTags.Add(typeof(T), baseTags);

        tags = baseTags.Select(x => x as T).ToList();
        return tags.Count > 0;
    }

    /// <summary>
    /// Tries to load a single tag with a matching Id to the provided value
    /// </summary>
    /// <typeparam name="T">Type of tag</typeparam>
    /// <param name="id">Tag Id</param>
    /// <param name="tag">Tag found</param>
    /// <returns>If a tag was found</returns>
    public static bool TryGetTag<T>(string id, out T tag) where T : Tag
    {
        var t = Instance.allTags.FirstOrDefault(x => x.Id == id);
        tag = t as T;

        return tag is not null;
    }
}
