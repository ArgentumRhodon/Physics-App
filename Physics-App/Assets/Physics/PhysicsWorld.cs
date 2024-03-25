using System.Collections.Generic;
using UnityEngine;


public class PhysicsWorld : MonoBehaviour
{
    public static float Gravity = -9.81f;
    public static PhysicsWorld Instance { get; private set; }

    private RigidBody[] rigidBodies;

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

    private void FixedUpdate()
    {
        CollisionWorld.Instance.UpdateCollisions();
        ResolveCollisions();
    }

    private void ResolveCollisions()
    {
        for(int i = 0; i < CollisionWorld.Instance.collisions.Count; i++)
        {
            Collision collision = CollisionWorld.Instance.collisions[i];

            RigidBody rb1 = collision.colliderA.RigidBody;
            RigidBody rb2 = collision.colliderB.RigidBody;

            rb1.transform.position += collision.collisionDepth * collision.collisionNormal / 2;
            rb2.transform.position -= collision.collisionDepth * collision.collisionNormal / 2;

            float averageRestitution = (rb1.restitution + rb2.restitution) / 2;
            float v1Dot = Vector3.Dot(rb1.LinearVelocity, collision.collisionNormal);
            float v2Dot = Vector3.Dot(rb2.LinearVelocity, collision.collisionNormal);
            float pHat = (averageRestitution + 1)*(v2Dot - v1Dot)/((1/rb1.mass) + (1/rb2.mass));

            Vector3 impulse = pHat * collision.collisionNormal;

            rb1.AddForce(impulse, ForceMode.Impulse);
            rb2.AddForce(-impulse, ForceMode.Impulse);

            collision.resolved = true;
            CollisionWorld.Instance.collisions[i] = collision;
        }
    }
}
