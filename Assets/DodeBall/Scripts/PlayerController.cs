using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public bool isBallInrange;

    [Header("Ball Script")]
    public Ball ball;

    [Header("Game objects")]
    public GameObject ballHoldingPosition;

    [Header("Ball Detection Settings")]
    public float raycastDistance = 2.0f;
    public LayerMask ballLayer;

    void Update()
    {
        // Check if ball is in range
        CheckBallInRange();
    }

    void CheckBallInRange()
    {
        // Create a ray from the center of the player in the forward direction
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        // Debug ray to visualize in scene view
        Debug.DrawRay(ray.origin, ray.direction * raycastDistance, Color.red);

        // Cast the ray and check if it hits something with the Ball component
        if (Physics.Raycast(ray, out hit, raycastDistance, ballLayer))
        {
            // Try to get the Ball component from the hit object
            Ball detectedBall = hit.collider.GetComponent<Ball>();

            // If we found a ball and it can be picked up
            if (detectedBall != null && detectedBall.ableToPickUpBall)
            {
                isBallInrange = true;
                ball = detectedBall; // Store reference to the detected ball
            }
            else
            {
                isBallInrange = false;
            }
        }
        else
        {
            // No ball was detected
            isBallInrange = false;
        }
    }
}