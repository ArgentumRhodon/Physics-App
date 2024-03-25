using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public struct Interval
{
    public Interval(float start, float end)
    {
        this.start = start;
        this.end = end;
    }

    public float start;
    public float end;
}

public struct Collision
{
    public Collision(OrientedBoundingBox colliderA, OrientedBoundingBox colliderB, Vector3 collisionNormal, float collisionDepth, bool resolved = false)
    {
        this.colliderA = colliderA;
        this.colliderB = colliderB;
        this.collisionNormal = collisionNormal;
        this.collisionDepth = collisionDepth;
        this.resolved = resolved;
    }

    public OrientedBoundingBox colliderA, colliderB;
    public Vector3 collisionNormal;
    public float collisionDepth;
    public bool resolved;
}

public class CollisionWorld
{
    private static CollisionWorld instance = null;
    public static CollisionWorld Instance
    {
        get
        {
            if(instance == null) instance = new CollisionWorld();
            return instance;
        }
    }

    public OrientedBoundingBox[] colliders;
    public List<Collision> collisions;

    public CollisionWorld()
    {
        collisions = new List<Collision>();
    }

    public void UpdateCollisions()
    {
        if(colliders.Length < 2) return;

        collisions = collisions.Where(collision => !collision.resolved).ToList();

        for(int i = 0; i < colliders.Length; i++)
        {
            for(int j = i + 1; j < colliders.Length; j++)
            {
                Vector3 collisionNormal;
                float collisionDepth;

                if (SATCheck(colliders[i], colliders[j], out collisionNormal, out collisionDepth))
                {
                    collisions.Add(new Collision(colliders[i], colliders[j], collisionNormal, collisionDepth));
                }
            }
        }

        Debug.Log(collisions.Count);

        // Simulate resolving collisions
        for(int i = 0; i < collisions.Count; i++)
        {
            Collision collision = collisions[i];
            collision.resolved = true;
            collisions[i] = collision;
        }
    }

    private bool SATCheck(OrientedBoundingBox a, OrientedBoundingBox b, out Vector3 collisionNormal, out float collisionDepth)
    {
        collisionNormal = Vector3.zero;
        collisionDepth = float.PositiveInfinity;

        // Generate axes
        Vector3[] aAxes = a.GetAxes();
        Vector3[] bAxes = b.GetAxes();
        Vector3[] allAxes = new Vector3[15]
        {
            aAxes[0], aAxes[1], aAxes[2],
            bAxes[0], bAxes[1], bAxes[2],
            Vector3.Cross(aAxes[0], bAxes[0]), Vector3.Cross(aAxes[0], bAxes[1]), Vector3.Cross(aAxes[0], bAxes[2]),
            Vector3.Cross(aAxes[1], bAxes[0]), Vector3.Cross(aAxes[1], bAxes[1]), Vector3.Cross(aAxes[1], bAxes[2]),
            Vector3.Cross(aAxes[2], bAxes[0]), Vector3.Cross(aAxes[2], bAxes[1]), Vector3.Cross(aAxes[2], bAxes[2]),
        };

        // Determine vertices
        Vector3[] aVerts = a.GetVertices();
        Vector3[] bVerts = b.GetVertices();

        // Compare overlap against each axis
        foreach (Vector3 axis in allAxes)
        {
            // Quick handling of [0,0,0] cross products
            if (axis == Vector3.zero) return true;

            // Set up max and min comparative values
            Interval aProjectionInterval = GetProjectionInterval(aVerts, axis);
            Interval bProjectionInterval = GetProjectionInterval(bVerts, axis);

            float overlap = GetOverlap(aProjectionInterval, bProjectionInterval);

            if (overlap < collisionDepth)
            {
                collisionDepth = overlap;
                collisionNormal = axis;
            }

            // Separating axis found - return early
            if (overlap <= 0) return false;
        }

        // No separating axes, collision detected
        return true;
    }

    private Interval GetProjectionInterval(Vector3[] vertices, Vector3 axis)
    {
        float projMin = float.MaxValue, projMax = float.MinValue;

        for (int i = 0; i < vertices.Length; i++)
        {
            float scalarProjection = Vector3.Dot(vertices[i], axis);

            if (scalarProjection < projMin)
            {
                projMin = scalarProjection;
            }
            if (scalarProjection > projMax)
            {
                projMax = scalarProjection;
            }
        }

        return new Interval(projMin, projMax);
    }

    // Determine overlap of two ranges on a line
    //         s       e        s           e
    // |-------<   a   >--------<     b     >-------------|
    private float GetOverlap(Interval a, Interval b)
    {
        if(a.start < b.start)
        {
            if (a.end < b.start) return 0f;

            return a.end - b.start;
        }

        if (b.end < a.start) return 0f;

        return b.end - a.start;
    }
}
