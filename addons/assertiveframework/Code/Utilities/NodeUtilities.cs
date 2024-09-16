using Godot;
using System;

public static class NodeUtilities
{
    public static bool IsSceneNode(this Node node)
    {
        return node is Node3D || node is Node2D;
    }

    public static bool IsUiNode(this Node node)
    {
        return node is Control;
    }

    public static bool TryGetFirstChild<T>(this Node node, out T target) where T : Node
    {
        foreach (var child in node.GetChildren())
        {
            if (child is T)
            {
                target = (T)child;
                return true;
            }
        }

        target = default;
        return false;
    }

    public static T GetFirstChildOrThrow<T>(this Node node, Exception exception = null) where T : Node
    {
        foreach (var child in node.GetChildren())
        {
            if (child is T)
            {
                return (T)child;
            }
        }

        throw exception ?? new NullReferenceException($"Failed to find any nodes under {node} matching the desired type {typeof(T).GetType().Name}");
    }
}