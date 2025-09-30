using Godot;

public static class RotationUtility
{
    /// <summary>
    /// Rotates a node from it's current forward (-Z) by the rotation amount provided
    /// </summary>
    /// <param name="source">Node to rotate</param>
    /// <param name="target">Target world location to look towards</param>
    /// <param name="rotationRadians">Radians to rotate the Node by towards the target</param>
    public static void RotateTowards(this Node3D source, Vector3 target, double rotationRadians)
        => RotateTowards(source, target, rotationRadians, 1);

    /// <summary>
    /// Rotates a Node from it's current forward (-Z) by the rotation amount provided multiplied by the current frame time. 
    /// This in effect allows you to rotate a Nodes forward by X radians per second towards the target
    /// </summary>
    /// <param name="source">Node to rotate</param>
    /// <param name="target">Target to rotate towards</param>
    /// <param name="rotationRadians">Desired radians to rotate the Node each second of game time</param>
    /// <param name="deltaThisFrame">Delta from the current frame (to bind the speed to)</param>
    public static void RotateTowards(this Node3D source, Vector3 target, double rotationRadians, double deltaThisFrame)
    {
        // Vector to target
        var targetDirection = (target - source.GlobalPosition).Normalized();

        // Current node "forwards"
        var currentDirection = -source.GlobalTransform.Basis.Z;

        // Angle between target and forwards vectors
        var angleInRadians = currentDirection.AngleTo(targetDirection);

        // If the angle between them is larger than "0"
        if (angleInRadians > 0.001f)
        {
            // Rotate by (X radians * frame time)
            var rotationStepThisFrame = rotationRadians * deltaThisFrame;

            // If the angle would overstep, bind to max angle
            if (rotationStepThisFrame > angleInRadians)
                rotationStepThisFrame = angleInRadians;

            // Apply the rotation
            source.RotateObjectLocal(currentDirection.Cross(targetDirection).Normalized(), (float)rotationStepThisFrame);
        }
    }
}
