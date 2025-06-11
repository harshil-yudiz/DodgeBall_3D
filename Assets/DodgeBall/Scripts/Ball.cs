using UnityEngine;

public class Ball : MonoBehaviour
{
    public bool ableToPickUpBall;
    public Rigidbody rb;

    [Header("Throw Settings")]
    public float minThrowForce;
    public float maxThrowForce;

    [Header("Pickup Settings")]
    public float minVelocityToPickup = 0.8f;

    [Header("Physics Settings")]
    public float gravityMultiplier = 1.2f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        EnablePickup(true);
    }

    void FixedUpdate()
    {
        if (!rb.isKinematic)
        {
            rb.AddForce(Physics.gravity * gravityMultiplier, ForceMode.Acceleration);
        }

        if (!ableToPickUpBall && rb.linearVelocity.magnitude < minVelocityToPickup)
        {
            EnablePickup(true);
        }
    }

    public void ThrowBall(Vector3 throwDirection, float powerPercentage)
    {
        ableToPickUpBall = false;
        rb.isKinematic = false;

        float throwForce = Mathf.Lerp(minThrowForce, maxThrowForce, powerPercentage) * 10f;
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);

        // Calculate speed in different units
        float speedMS = throwForce / rb.mass;
        float speedMPH = speedMS * 2.237f;

        Debug.Log($"Ball thrown with Power: {powerPercentage:P0} | Speed: {speedMPH:F1} mph | Direction: {throwDirection}");
    }

    public void EnablePickup(bool value)
    {
        ableToPickUpBall = value;
    }

    public Vector3 CalculateTrajectoryPoint(Vector3 startPosition, Vector3 direction, float power, float time)
    {
        float throwForce = Mathf.Lerp(minThrowForce, maxThrowForce, power) * 10f;
        Vector3 velocity = direction * throwForce;
        return startPosition + velocity * time + 0.5f * (Physics.gravity * gravityMultiplier) * time * time;
    }
}
