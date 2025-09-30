using Godot;
using System;
using System.Collections.Generic;

public static class NodeUtilities
{
    /// <summary>
    /// Determines if the provided node is a non-gui node
    /// </summary>
    /// <param name="node">Node to check</param>
    public static bool IsSceneNode(this Node node)
    {
        return node is Node3D || node is Node2D;
    }

    /// <summary>
    /// Determines if the provided node is a gui node
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public static bool IsUiNode(this Node node)
    {
        return node is Control;
    }

    /// <summary>
    /// Determines if the collision mask of the node intersects with the given mask. 
    /// Returns false when the node is not a physics node.
    /// </summary>
    /// <param name="node">Node to check</param>
    /// <param name="mask">bit mask</param>
    /// <returns></returns>
    public static bool NodeIntersectsMask(this Node node, uint mask)
    {
        if (node is CollisionObject3D colObject)
            return (colObject.CollisionLayer & mask) != 0;

        if (node is CsgCombiner3D combiner)
            return (combiner.CollisionLayer & mask) != 0;

        if (node is CsgShape3D shape)
            return (shape.CollisionLayer & mask) != 0;

        return false;
    }

    /// <summary>
    /// Searches a nodes children for the first instance of T
    /// </summary>
    /// <typeparam name="T">Type of node to search for</typeparam>
    /// <param name="node">Node to check</param>
    /// <param name="target">output for found node</param>
    /// <returns>If a matching T was found</returns>
    public static bool TryGetFirstChild<T>(this Node node, out T target) where T : class
    {
        foreach (var child in node.GetChildren())
        {
            if (child is T value)
            {
                target = value;
                return true;
            }
        }

        target = default;
        return false;
    }

    /// <summary>
    /// Searches a nodes children for the first instance of T. Throws an exception when none is found.
    /// </summary>
    /// <typeparam name="T">Type of node to search for</typeparam>
    /// <param name="node">Node to check the childrne of</param>
    /// <param name="exception">Exception to throw when failure</param>
    /// <returns>The found node of type T</returns>
    public static T GetFirstChildOrThrow<T>(this Node node, Exception exception = null) where T : class
    {
        foreach (var child in node.GetChildren())
        {
            if (child is T value)
            {
                return value;
            }
        }

        throw exception ?? new NullReferenceException($"Failed to find any nodes under {node} matching the desired type {typeof(T).GetType().Name}");
    }

    /// <summary>
    /// Returns all nodes of type T under a parent node
    /// </summary>
    /// <typeparam name="T">Type to search for</typeparam>
    /// <param name="source">Root node</param>
    /// <param name="includeSubChildren">If sub-children in the tree should be searched</param>
    /// <returns>All found nodes of T under the root</returns>
    public static IEnumerable<T> GetChildrenOfType<T>(this Node source, bool includeSubChildren = false) where T : class
    {
        List<T> nodes = new();
        foreach (var child in source.GetChildren())
        {
            if (child is T target)
                nodes.Add(target);

            if (includeSubChildren)
                nodes.AddRange(child.GetChildrenOfType<T>(includeSubChildren));
        }

        return nodes;
    }

    /// <summary>
    /// Tries to find the first occurence of the given T under the root node
    /// </summary>
    /// <typeparam name="T">Type to search for</typeparam>
    /// <param name="source">Root node</param>
    /// <param name="node">output for found node</param>
    /// <returns>If a node was found</returns>
    public static bool TryGetFirstChildOfType<T>(this Node source, out T node) where T : class
    {
        node = null;

        // Check if each current children are T
        foreach (var child in source.GetChildren())
        {
            if (child is T target)
            {
                node = target;
                return true;
            }
        }

        // Check all the children for a child that contains what we're looking fors
        foreach (var child in source.GetChildren())
        {
            if (child.TryGetFirstChildOfType(out node))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Searches all parent of the root node for T
    /// </summary>
    /// <typeparam name="T">Type to search for</typeparam>
    /// <param name="node">Root node</param>
    /// <param name="target">output for found node</param>
    /// <returns>If a Node of T was found</returns>
    public static bool TryGetFirstParent<T>(this Node node, out T target)
    {
        Node current = node.GetParent();

        while (current != null)
        {
            if (current is T value)
            {
                target = value;
                return true;
            }

            current = current.GetParent();
        }

        target = default;
        return false;
    }
}