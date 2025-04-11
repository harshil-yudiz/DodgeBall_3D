using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class Ball : MonoBehaviour
{
    public bool ableToPickUpBall;
    public Rigidbody rb;

    [Header("Throw Settings")]
    public float minThrowForce;
    public float maxThrowForce;
    
    [Header("Pickup Settings")]
    public float minVelocityToPickup;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        EnablePickup(true);
    }

    void FixedUpdate()  // Changed from Update to FixedUpdate
    {
        // Check ball's velocity and enable pickup when it's slow enough
        if (!ableToPickUpBall && rb.linearVelocity.magnitude < minVelocityToPickup)
        {
            EnablePickup(true);
        }
    }

    public void ThrowBall(Vector3 throwDirection, float powerPercentage)
    {
        ableToPickUpBall = false;
        rb.isKinematic = false;

        // Calculate and apply force in one frame
        float throwForce = Mathf.Lerp(minThrowForce, maxThrowForce, powerPercentage) * 10f;
        rb.linearVelocity = Vector3.zero; // Reset velocity before applying new force
        rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);
    }

    public void EnablePickup(bool value)
    {
        ableToPickUpBall = value;
    }

    public Vector3 CalculateTrajectoryPoint(Vector3 startPosition, Vector3 direction, float power, float time)
    {
        float throwForce = Mathf.Lerp(minThrowForce, maxThrowForce, power) * 10f;
        Vector3 velocity = direction * throwForce;
        return startPosition + velocity * time + 0.5f * Physics.gravity * time * time;
    }
}
