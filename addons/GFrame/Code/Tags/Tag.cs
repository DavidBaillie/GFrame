using Godot;
using System;

/// <summary>
/// Represents the base Resource type that will be referenced throughout the project
/// </summary>
[GlobalClass, Tool]
public partial class Tag : Resource
{
    /// <summary>
    /// Unique identifier to reference resources by
    /// </summary>
    [Export(PropertyHint.ObjectId)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Determines if the tag should be made available to other systems. 
    /// Disabling this flag is akin to deleting the tag
    /// </summary>
    [Export]
    public bool IsEnabled { get; set; } = true;
}