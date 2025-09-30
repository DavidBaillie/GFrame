using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;

public static class RaycastUtilities
{
    public const uint DefaultLayerMask = 4294967295;

    /// <summary>
    /// Draws a ray from the 3D camera forwards from the screen position into the game world
    /// </summary>
    /// <param name="viewport">User screen viewport</param>
    /// <param name="spaceState">World space to cast in</param>
    /// <param name="raycastDistance">How far the ray should go</param>
    /// <returns>Raycast hit object containing intersection data</returns>
    public static bool TryRaycastFromCamera(Viewport viewport, PhysicsDirectSpaceState3D spaceState, out RaycastHit hit, Vector2? screenPosition = null,
        uint mask = DefaultLayerMask, float raycastDistance = 1000, bool collideWithAreas = false, bool collideWithBodies = true)
    {
        var camera = viewport.GetCamera3D();
        screenPosition ??= viewport.GetMousePosition();

        var start = camera.ProjectRayOrigin(screenPosition.Value);
        var rayQuery = new PhysicsRayQueryParameters3D()
        {
            From = start,
            To = start + (camera.ProjectRayNormal(screenPosition.Value) * raycastDistance),
            CollisionMask = mask,
            CollideWithAreas = collideWithAreas,
            CollideWithBodies = collideWithBodies
        };

        var result = spaceState.IntersectRay(rayQuery);

        hit = new RaycastHit(result);
        return result.Count > 0;
    }

    /// <summary>
    /// Attempts to cast a ray from the start position in the desired direction
    /// </summary>
    /// <param name="spaceState">World space state to cast in</param>
    /// <param name="hit">Out param to return data</param>
    /// <param name="startPosition">World start point</param>
    /// <param name="direction">Direction of raycast</param>
    /// <param name="mask">Raycast collider mask</param>
    /// <param name="raycastDistance">Distance to raycast</param>
    /// <param name="collideWithAreas">Should the cast interact with an area</param>
    /// <param name="collideWithBodies">Should the cast interact with a body</param>
    /// <param name="debugProfile">Debug properties to view raycast in game</param>
    /// <returns>If a collision with the ray happened</returns>
    public static bool TryRaycast(PhysicsDirectSpaceState3D spaceState, out RaycastHit hit, Vector3 startPosition, Vector3 direction, uint mask = DefaultLayerMask,
        IEnumerable<Rid> excludeRids = null, float raycastDistance = 1000, bool collideWithAreas = false, bool collideWithBodies = true, DebugProfile? debugProfile = null)
    {
        var endPosition = startPosition + (direction.Normalized() * raycastDistance);

        var rayQuery = new PhysicsRayQueryParameters3D()
        {
            From = startPosition,
            To = endPosition,
            CollisionMask = mask,
            CollideWithAreas = collideWithAreas,
            CollideWithBodies = collideWithBodies,
            Exclude = excludeRids is null ? new Array<Rid>() : new Array<Rid>(excludeRids)
        };

        if (debugProfile.HasValue)
            DebugDraw3D.DrawLine(startPosition, endPosition, debugProfile.Value.Color, debugProfile.Value.Duration);

        var result = spaceState.IntersectRay(rayQuery);

        hit = new RaycastHit(result);
        return result.Count > 0;
    }

    /// <summary>
    /// Performs an physics check with an overlap sphere, providing all nodes in the area
    /// </summary>
    /// <param name="spaceState">World space state</param>
    /// <param name="hits">List of physics intersections</param>
    /// <param name="center">Center of the sphere</param>
    /// <param name="radius">Radius of the sphere</param>
    /// <param name="mask">Physics mask</param>
    /// <param name="collideWithAreas">Should the overlap check for areas</param>
    /// <param name="colliderWithBodies">Should the overlap check for bodies</param>
    /// <param name="debugProfile">Debug data to view check</param>
    /// <returns>If any intersections returned</returns>
    public static bool TryOverlapSphere(PhysicsDirectSpaceState3D spaceState, out List<OverlapHit> hits, Vector3 center, float radius, Rid[] ignoreResources = null,
        uint mask = DefaultLayerMask, bool collideWithAreas = false, bool colliderWithBodies = true, DebugProfile? debugProfile = null)
    {
        var shapeRid = PhysicsServer3D.SphereShapeCreate();
        PhysicsServer3D.ShapeSetData(shapeRid, radius);

        var queryParams = new PhysicsShapeQueryParameters3D()
        {
            ShapeRid = shapeRid,
            Transform = new Transform3D(Basis.Identity, center),
            CollideWithAreas = collideWithAreas,
            CollideWithBodies = colliderWithBodies,
            CollisionMask = mask,
            Exclude = new Godot.Collections.Array<Rid>(ignoreResources ?? new Rid[0])
        };

        if (debugProfile.HasValue)
            DebugDraw3D.DrawSphere(center, radius, debugProfile.Value.Color, debugProfile.Value.Duration);

        var intersections = spaceState.IntersectShape(queryParams);
        hits = intersections.Select(x => new OverlapHit(x)).ToList();

        PhysicsServer3D.FreeRid(shapeRid);
        return hits.Count > 0;
    }

    /// <summary>
    /// Performs an physics check with an overlap sphere, providing all nodes in the area
    /// </summary>
    /// <param name="spaceState">World space state</param>
    /// <param name="hits">List of physics intersections</param>
    /// <param name="center">Center of the sphere</param>
    /// <param name="radius">Radius of the sphere</param>
    /// <param name="mask">Physics mask</param>
    /// <param name="collideWithAreas">Should the overlap check for areas</param>
    /// <param name="colliderWithBodies">Should the overlap check for bodies</param>
    /// <param name="debugProfile">Debug data to view check</param>
    /// <returns>If any intersections returned</returns>
    public static bool TryCollisionSphere(PhysicsDirectSpaceState3D spaceState, out List<Vector3> hits, Vector3 center, float radius, Rid[] ignoreResources = null,
        uint mask = DefaultLayerMask, bool collideWithAreas = false, bool colliderWithBodies = true, DebugProfile? debugProfile = null)
    {
        var shapeRid = PhysicsServer3D.SphereShapeCreate();
        PhysicsServer3D.ShapeSetData(shapeRid, radius);

        var queryParams = new PhysicsShapeQueryParameters3D()
        {
            ShapeRid = shapeRid,
            Transform = new Transform3D(Basis.Identity, center),
            CollideWithAreas = collideWithAreas,
            CollideWithBodies = colliderWithBodies,
            CollisionMask = mask,
            Exclude = new Godot.Collections.Array<Rid>(ignoreResources ?? new Rid[0])
        };

        if (debugProfile.HasValue)
            DebugDraw3D.DrawSphere(center, radius, debugProfile.Value.Color, debugProfile.Value.Duration);

        var intersections = spaceState.CollideShape(queryParams);
        hits = intersections.ToList();

        PhysicsServer3D.FreeRid(shapeRid);
        return hits.Count > 0;
    }

    /// <summary>
    /// Takes the desired RID to find the closest collision point to and determines if the point exists
    /// </summary>
    /// <param name="spaceState">World space</param>
    /// <param name="points">Out param for the closest collision</param>
    /// <param name="center">Center of check</param>
    /// <param name="radius">Radius of check</param>
    /// <param name="targetRid">Desired target to check for</param>
    /// <returns>If a collision point exists</returns>
    public static bool TryGetOverlapPointsOfTarget(PhysicsDirectSpaceState3D spaceState, out List<Vector3> points, Vector3 center, float radius, Rid targetRid)
    {
        points = new List<Vector3>();

        if (!TryOverlapSphere(spaceState, out List<OverlapHit> hits, center, radius))
            return false;

        var toIgnore = hits.Where(x => x.Rid != targetRid).Select(x => x.Rid).ToArray();
        if (toIgnore.Length == hits.Count)
            return false;

        return TryCollisionSphere(spaceState, out points, center, radius, toIgnore);
    }

    public static bool TryGetClosestPointOnTarget(PhysicsDirectSpaceState3D spaceState, out Vector3 closestPoint, Vector3 center, float radius, Rid targetRid)
    {
        closestPoint = Vector3.Zero;

        if (TryGetOverlapPointsOfTarget(spaceState, out List<Vector3> overlaps, center, radius, targetRid))
        {
            closestPoint = overlaps.MinBy(x => x.DistanceTo(center));
            return true;
        }

        return false;
    }
}
