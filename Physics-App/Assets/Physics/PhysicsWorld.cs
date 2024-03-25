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
        // ResolveCollisions()
    }
}
