using Godot;

public static class VectorUtilities
{
    public static Vector2 ToXZVector2(this Vector3 source) => new Vector2(source.X, source.Z);
    public static Vector2 ToXYVector2(this Vector3 source) => new Vector2(source.X, source.Y);

    public static Vector2I ToXZVector2I(this Vector3I source) => new Vector2I(source.X, source.Z);
    public static Vector2I ToXYVector2I(this Vector3I source) => new Vector2I(source.X, source.Y);

    public static Vector2I ToXZVector2I(this Vector3 source) => new Vector2I((int)source.X, (int)source.Z);
    public static Vector2I ToXYVector2I(this Vector3 source) => new Vector2I((int)source.X, (int)source.Y);

    public static Vector3 ToXZVector3(this Vector2 source) => new Vector3(source.X, 0, source.Y);
    public static Vector3 ToXYVector3(this Vector2 source) => new Vector3(source.X, source.Y, 0);

    public static Vector3I ToXZVector3I(this Vector2I source) => new Vector3I(source.X, 0, source.Y);
    public static Vector3I ToXYVector3I(this Vector2I source) => new Vector3I(source.X, source.Y, 0);

    public static Vector3I ToXZVector3I(this Vector2 source) => new Vector3I((int)source.X, 0, (int)source.Y);
    public static Vector3I ToXYVector3I(this Vector2 source) => new Vector3I((int)source.X, (int)source.Y, 0);

    /// <summary>
    /// Generates the distance between each element of a Vector as a positive value.
    /// </summary>
    /// <param name="source">Starting vector</param>
    /// <param name="other">End vector</param>
    /// <returns>Positive difference between each matching element of each vector</returns>
    public static Vector3 ElementDistance(this Vector3 source, Vector3 other)
        => new Vector3(Mathf.Abs(source.X - other.X), Mathf.Abs(source.Y - other.Y), Mathf.Abs(source.Z - other.Z));

    /// <summary>
    /// Generates the distance between each element of a Vector as a positive value.
    /// </summary>
    /// <param name="source">Starting vector</param>
    /// <param name="other">End vector</param>
    /// <returns>Positive difference between each matching element of each vector</returns>
    public static Vector3I ElementDistance(this Vector3I source, Vector3I other)
        => new Vector3I(Mathf.Abs(source.X - other.X), Mathf.Abs(source.Y - other.Y), Mathf.Abs(source.Z - other.Z));

    /// <summary>
    /// Generates the distance between each element of a Vector as a positive value.
    /// </summary>
    /// <param name="source">Starting vector</param>
    /// <param name="other">End vector</param>
    /// <returns>Positive difference between each matching element of each vector</returns>
    public static Vector2 ElementDistance(this Vector2 source, Vector2 other)
        => new Vector2(Mathf.Abs(source.X - other.X), Mathf.Abs(source.Y - other.Y));

    /// <summary>
    /// Generates the distance between each element of a Vector as a positive value.
    /// </summary>
    /// <param name="source">Starting vector</param>
    /// <param name="other">End vector</param>
    /// <returns>Positive difference between each matching element of each vector</returns>
    public static Vector2I ElementDistance(this Vector2I source, Vector2I other)
        => new Vector2I(Mathf.Abs(source.X - other.X), Mathf.Abs(source.Y - other.Y));
}