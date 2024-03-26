using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = System.Random;


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

    public GameObject pauseButton;
    public void ToggleTime()
    {
        if (Time.timeScale > 0) Time.timeScale = 0;
        else Time.timeScale = 1;

        pauseButton.GetComponent<TextMeshPro>().text = Time.timeScale == 0 ? "Start" : "Pause";
    }

    public void Reset()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void GoCrazy()
    {
        float intensity = 50.0f;
        Random random = new Random();
        foreach(RigidBody rigidBody in rigidBodies)
        {
            rigidBody.AddForce(new Vector3(random.Next(-1, 1), random.Next(-1, 1), random.Next(-1, 1)) * intensity, ForceMode.Impulse);
            rigidBody.AddTorque(new Vector3(random.Next(-1, 1), random.Next(-1, 1), random.Next(-1, 1)) * intensity / 10, ForceMode.Impulse);
        }
    }

    public void Quit()
    {
        Application.Quit();
    }

    private void ResolveCollisions()
    {
        
        for (int i = 0; i < CollisionWorld.Instance.collisions.Count; i++)
        {
            // Only handles translation responses
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
}
