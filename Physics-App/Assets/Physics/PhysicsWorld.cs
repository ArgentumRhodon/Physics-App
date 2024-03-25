using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Profiling.Memory.Experimental;
using UnityEngine;


public class PhysicsWorld : MonoBehaviour
{
    public static float Gravity = -9.81f;
    public static PhysicsWorld Instance { get; private set; }

    private RigidBody[] rigidBodies;

    public OrientedBoundingBox rayTest;

    public Vector3 rayDir = Vector3.zero;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        rigidBodies = FindObjectsOfType<RigidBody>();
        CollisionWorld.Instance.colliders = FindObjectsOfType<OrientedBoundingBox>();
    }

    Vector3 rayHit = Vector3.zero;
    private void Start()
    {
        
    }

    private void Update()
    {
        if (CastRay(rayTest.transform.position, rayTest.transform.position + rayDir, rayTest, out rayHit))
        {
            Debug.DrawRay(rayTest.transform.position, rayDir, Color.red);
            Debug.Log("Ray cast hit at " + rayHit);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(rayHit, 0.05f);
    }

    private void FixedUpdate()
    {
        CollisionWorld.Instance.UpdateCollisions();
        ResolveCollisions();
    }

    private void ResolveCollisions()
    {
        
        for (int i = 0; i < CollisionWorld.Instance.collisions.Count; i++)
        {
            Collision collision = CollisionWorld.Instance.collisions[i];

            RigidBody rb1 = collision.colliderA.RigidBody;
            RigidBody rb2 = collision.colliderB.RigidBody;

            if (rb1 == null && rb2 == null) return;

            Vector3 separation = collision.collisionDepth * collision.collisionNormal / 2f;

            Vector3 relativeVelocity = Vector3.zero;
            if (rb1 != null && rb2 != null)
            {
                relativeVelocity = rb2.LinearVelocity - rb1.LinearVelocity;
            }
            else
            {
                relativeVelocity = rb1 != null ? -rb1.LinearVelocity : rb2.LinearVelocity;
            }

            float orderDotProduct = Vector3.Dot(separation, relativeVelocity);

            if(orderDotProduct > 0)
            {
                if(rb1 != null) rb1.transform.position += separation;
                if(rb2 != null) rb2.transform.position -= separation;
            }
            else
            {
                if(rb1 != null) rb1.transform.position -= separation;
                if(rb2 != null) rb2.transform.position += separation;
            }

            float averageRestitution = 0;
            if (rb1 != null && rb2 != null)
            {
                averageRestitution = (rb1.restitution + rb2.restitution) / 2;
            }
            else
            {
                averageRestitution = rb1 != null ? rb1.restitution : rb2.restitution;
            }

            float v1Dot = 0, v2Dot = 0;
            float summedInverseMasses = 0;

            if(rb1 != null)
            {
                v1Dot = Vector3.Dot(rb1.LinearVelocity, collision.collisionNormal);
                summedInverseMasses += 1 / rb1.mass;
            }
            if(rb2 != null)
            {
                v2Dot = Vector3.Dot(rb2.LinearVelocity, collision.collisionNormal);
                summedInverseMasses += 1 / rb2.mass;
            }

            float pHat = (averageRestitution + 1) * (v2Dot - v1Dot) / summedInverseMasses;

            Vector3 impulse = pHat * collision.collisionNormal;

            if(rb1 != null)
            {
                rb1.AddForce(impulse, ForceMode.Impulse);
            }
            // rb1.gameObject.GetComponent<Renderer>().material.color = Color.red;
            // Debug.DrawLine(rb1.transform.position, impulse, Color.red, .25f);


            if(rb2 != null)
            {
                rb2.AddForce(-impulse, ForceMode.Impulse);
            }
            // Debug.DrawLine(rb2.transform.position, -impulse, Color.green, .25f);
            // rb2.gameObject.GetComponent<Renderer>().material.color = Color.green;

            collision.resolved = true;
            CollisionWorld.Instance.collisions[i] = collision;
        }
    }

    private bool CastRay(Vector3 start, Vector3 end, OrientedBoundingBox collider, out Vector3 intersectionPoint)
    {
        intersectionPoint = Vector3.zero;
        float nearestT = float.MaxValue;
        bool intersects = false;

        Vector3[] verts = collider.GetVertices();

        Vector3[,] faces = new Vector3[6,4] {
            {verts[0], verts[1], verts[2], verts[3] },
            {verts[4], verts[0], verts[6], verts[2] },
            {verts[5], verts[4], verts[7], verts[6] },
            {verts[1], verts[5], verts[3], verts[7] },
            {verts[5], verts[4], verts[1], verts[0] },
            {verts[7], verts[6], verts[3], verts[2] },
        };

        for(int i = 0; i < faces.GetLength(0); i++)
        {
            Vector3 v0 = faces[i,0];
            Vector3 v1 = faces[i,1];
            Vector3 v2 = faces[i,2];
            Vector3 v3 = faces[i,3];

            Vector3 tempIntersectionPoint;
            float tempT;
            if(RayHitsTriangle(start, end, v0, v1, v2, out tempIntersectionPoint, out tempT) || RayHitsTriangle(start, end, v1, v3, v2, out tempIntersectionPoint, out tempT))
            {
                if(tempT < nearestT)
                {
                    nearestT = tempT;
                    intersectionPoint = tempIntersectionPoint;
                    Debug.Log(i != 0 ? i : "");

                    intersects = true;
                }
            }
        }

        return intersects;
    }

    private bool RayHitsTriangle(Vector3 start, Vector3 end, Vector3 v0, Vector3 v1, Vector3 v2, out Vector3 intersectionPoint, out float t)
    {
        float EPSILON = float.Epsilon;

        intersectionPoint = Vector3.zero;
        t = float.MaxValue;

        Vector3 edge1 = v1 - v0;
        Vector3 edge2 = v2 - v0;
        Vector3 h = Vector3.Cross(end - start, edge2);

        float a = Vector3.Dot(edge1, h);
        if (Mathf.Abs(a) < EPSILON) return false;

        float f = 1 / a;
        Vector3 s = start - v0;
        float u = f * Vector3.Dot(s, h);

        if (u < 0.0f || u > 1.0f) return false;

        Vector3 q = Vector3.Cross(s, edge1);
        float v = f * Vector3.Dot(end - start, q);

        if(v < 0.0f || u + v > 1.0f) return false;

        t = f * Vector3.Dot(edge2, q);

        if (t > EPSILON)
        {
            intersectionPoint = start + t * (end - start);
            return true;
        }

        return false;
    }
}
