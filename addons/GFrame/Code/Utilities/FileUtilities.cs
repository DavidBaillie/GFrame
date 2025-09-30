﻿using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Custom static class for interfacing with the file system
/// </summary>
public static class FileUtilities
{
    /// <summary>
    /// Loads all file paths from a start directory
    /// </summary>
    /// <param name="basePath">Start point to load from</param>
    /// <param name="loadSubdirectories">Should files in sub-folders be loaded?</param>
    /// <param name="customValidator">Custom file validator to limit return values</param>
    /// <returns>List of all valid files under the base path</returns>
    public static List<string> GetAllFilesAtPath(string basePath, bool loadSubdirectories = true, Func<string, bool> customValidator = null)
    {
        // Setup
        customValidator ??= (x) => true;
        var allPaths = new List<string>();

        // Open the directory
        var directory = DirAccess.Open(basePath);
        directory.ListDirBegin();
        var currentPathName = directory.GetNext();

        // Read all files
        while (!string.IsNullOrEmpty(currentPathName))
        {
            var path = $"{basePath}/{currentPathName}";

            // Has a folder and should load from inside it (recursively)
            if (directory.CurrentIsDir() && loadSubdirectories)
                allPaths.AddRange(GetAllFilesAtPath(path, loadSubdirectories, customValidator));
            // File, load it if valid
            else if (customValidator.Invoke(path))
                allPaths.Add(path);

            // Next file
            currentPathName = directory.GetNext();
        }

        // Close directory and return
        directory.ListDirEnd();
        return allPaths;
    }

    /// <summary>
    /// Tries to load a specific resource as the given type
    /// </summary>
    /// <typeparam name="T">Type to cast to</typeparam>
    /// <param name="path">Where is the file?</param>
    /// <param name="result">The desired file as T</param>
    /// <returns>If the load was successful</returns>
    public static bool TryLoadResource<T>(string path, out T result) where T : Resource
    {
        try
        {
            var res = ResourceLoader.Load(path);

            if (res is null)
            {
                GD.PrintErr($"File utilities failed to load the resource at {path}");
                result = default;
                return false;
            }

            if (res is T)
            {
                result = res as T;
                return true;
            }

            GD.PrintErr($"File utilities loaded a resource but couldn't cast it to the desired type. Resource: {path}");
            result = default;
            return false;
        }
        catch (Exception e)
        {
            GD.PrintErr($"File utilities caught an exception while trying to load a resource. Resource: {path}\nException: {e}");

            result = default;
            return false;
        }
    }
}