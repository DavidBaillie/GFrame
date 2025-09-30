using Godot;
using Godot.Collections;

public struct OverlapHit
{
    public Rid Rid { get; set; }
    public int ColliderId { get; set; }
    public Node3D Node3D { get; set; }
    public int Shape { get; set; }

    public OverlapHit(Rid rid, int colliderId, Node3D collider, int shape)
    {
        Rid = rid;
        ColliderId = colliderId;
        Node3D = collider;
        Shape = shape;
    }

    public OverlapHit(Dictionary dictionary)
    {
        Rid = dictionary["rid"].As<Rid>();
        ColliderId = dictionary["collider_id"].As<int>();
        Node3D = dictionary["collider"].As<Node3D>();
        Shape = dictionary["shape"].As<int>();
    }
}