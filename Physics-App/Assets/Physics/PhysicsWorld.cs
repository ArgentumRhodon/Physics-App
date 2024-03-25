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
            Debug.Log(collision.collisionNormal);

            RigidBody rb1 = collision.colliderA.RigidBody;
            RigidBody rb2 = collision.colliderB.RigidBody;

            Vector3 separation = collision.collisionDepth * collision.collisionNormal / 2;
            Vector3 relativeVelocity = rb2.LinearVelocity - rb1.LinearVelocity;
            float orderDotProduct = Vector3.Dot(separation, relativeVelocity);

            if(orderDotProduct > 0)
            {
                rb1.transform.position += separation;
                rb2.transform.position -= separation;
            }
            else
            {
                rb1.transform.position -= separation;
                rb2.transform.position += separation;
            }

            float averageRestitution = (rb1.restitution + rb2.restitution) / 2;
            float v1Dot = Vector3.Dot(rb1.LinearVelocity, collision.collisionNormal);
            float v2Dot = Vector3.Dot(rb2.LinearVelocity, collision.collisionNormal);
            float pHat = (averageRestitution + 1)*(v2Dot - v1Dot)/((1/rb1.mass) + (1/rb2.mass));

            Vector3 impulse = pHat * collision.collisionNormal;

            rb1.AddForce(impulse, ForceMode.Impulse);
            // rb1.gameObject.GetComponent<Renderer>().material.color = Color.red;
            // Debug.DrawLine(rb1.transform.position, impulse, Color.red, .25f);


            rb2.AddForce(-impulse, ForceMode.Impulse);
            // Debug.DrawLine(rb2.transform.position, -impulse, Color.green, .25f);
            // rb2.gameObject.GetComponent<Renderer>().material.color = Color.green;


            collision.resolved = true;
            CollisionWorld.Instance.collisions[i] = collision;
        }
    }
}
