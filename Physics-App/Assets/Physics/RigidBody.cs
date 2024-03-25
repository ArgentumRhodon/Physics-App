using System;
using UnityEngine;

[Serializable]
[RequireComponent(typeof(OrientedBoundingBox))]
public class RigidBody : MonoBehaviour
{

    public OrientedBoundingBox Collider { get; private set; }

    public float restitution = 1.0f;
    public float mass = 5.0f;
    public float drag = 0.0f;

    public Vector3 LinearVelocity { get; private set; }
    public Vector3 LinearAcceleration { get; private set; }

    private Vector3 force = Vector3.zero;

    public float angularDrag = 0.0f;
    public Vector3 inertiaTensor = new Vector3(1, 1, 1);
    public Vector3 AngularMomentum { get; private set; }
    private Vector3 torque = Vector3.zero;

    public bool useGravity = false;

    private void Awake()
    {
        Collider = GetComponent<OrientedBoundingBox>();
        LinearVelocity = Vector3.zero;
        LinearAcceleration = Vector3.zero;
    }

    private void FixedUpdate()
    {
        NaturalForcesResolve();
        TranslateResolve();
        RotateResolve();
    }

    private void TranslateResolve()
    {
        // Velocity Verlet numerical solution for translation
        transform.position += (LinearVelocity * Time.fixedDeltaTime) + (0.5f * LinearAcceleration * Time.fixedDeltaTime * Time.fixedDeltaTime);
        Vector3 halfStepVelocity = LinearVelocity + (0.5f * LinearAcceleration * Time.fixedDeltaTime);
        LinearAcceleration = (1 / mass) * force;
        LinearVelocity = halfStepVelocity + (0.5f * LinearAcceleration * Time.fixedDeltaTime);

        // Reset forces
        force = Vector3.zero;
    }

    private void RotateResolve()
    {
        // Explicit Euler numerical solution for rotation
        AngularMomentum += torque * Time.fixedDeltaTime;
        Quaternion halfAngularVelocityQuaternionWithTime = new Quaternion(
            (0.5f * Time.fixedDeltaTime) * AngularMomentum.x / inertiaTensor.x,
            (0.5f * Time.fixedDeltaTime) * AngularMomentum.y / inertiaTensor.y,
            (0.5f * Time.fixedDeltaTime) * AngularMomentum.z / inertiaTensor.z,
            1
        );
        transform.rotation = halfAngularVelocityQuaternionWithTime * transform.rotation;

        // Reset torques
        torque = Vector3.zero;
    }

    private void NaturalForcesResolve()
    {
        if (useGravity)
        {
            force += new Vector3(0, mass * PhysicsWorld.Gravity, 0);
        }

        // Opposing drag force - Unity style - https://discussions.unity.com/t/how-is-drag-applied-to-force/15542
        float multiplierT = 1.0f - drag * Time.fixedDeltaTime;
        if (multiplierT < 0.0f) multiplierT = 0.0f;
        LinearVelocity *= multiplierT;

        // Opposing angular drag force - Unity Style - https://discussions.unity.com/t/how-is-drag-applied-to-force/15542
        float multiplierA = 1.0f - angularDrag * Time.fixedDeltaTime;
        if (multiplierA < 0.0f) multiplierA = 0.0f;
        AngularMomentum *= multiplierA;
    }

    public void AddTorque(Vector3 torque, ForceMode forceMode = ForceMode.Force)
    {
        switch(forceMode)
        {
            case ForceMode.Force:
                this.torque += torque; // The torque represents a torque
                break;
            case ForceMode.Impulse: // The torque represents an immediate change in angular momentum considering mass
                torque *= 10;
                AngularMomentum += torque / mass;
                break;
            case ForceMode.VelocityChange: // The torque represents a direct momentum change
                AngularMomentum += torque;
                break;
            default:
                Debug.LogError(forceMode + " force mode not implemented");
                break;
        }
    }

    public void AddForce(Vector3 force, ForceMode forceMode = ForceMode.Force)
    {
        switch (forceMode)
        {
            case ForceMode.Force: // The force represents a force
                this.force += force; 
                break;
            case ForceMode.Impulse: // The force represents an instant change in momentum
                LinearVelocity += force / mass; 
                break;
            case ForceMode.VelocityChange: // The force represents an instant change in velocity
                LinearVelocity += force; 
                break;
            default:
                Debug.LogError(forceMode + " force mode not implemented");
                break;
        }
    }
}
