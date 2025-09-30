using Godot;
using Godot.Collections;

public struct RaycastHit
{
    public Vector3 Position { get; set; }
    public Vector3 Normal { get; set; }
    public Variant Collider { get; set; }
    public Rid Rid { get; set; }

    public RaycastHit(Dictionary hitData)
    {
        if (hitData.Count < 1)
        {
            Position = Vector3.Zero;
            Normal = Vector3.Zero;
            Collider = 0;
            Rid = new Rid();
        }
        else
        {
            Position = hitData["position"].AsVector3();
            Normal = hitData["normal"].AsVector3();
            Collider = hitData["collider"];
            Rid = hitData["rid"].AsRid();
        }
    }
}