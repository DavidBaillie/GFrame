using Godot;

public struct DebugProfile
{
    public float Duration { get; set; }
    public Color Color { get; set; }

    public DebugProfile(Color color, float duration)
    {
        Color = color;
        Duration = duration;
    }
}