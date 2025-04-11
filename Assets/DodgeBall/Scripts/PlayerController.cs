using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public bool isBallInrange;
    public bool isGizmoWanted;
    public bool isBallInHand;

    [Header("Ball")]
    public GameObject ballObj;
    private Ball ball;

    [Header("Transforms")]
    public GameObject ballHoldingPosition;
    public Transform throwDirection;

    [Header("Script References")]
    public GamePlayScreen gamePlayScreen;

    [Header("Trajectory Settings")]
    public LineRenderer trajectoryLine;

    [Header("Ball Detection Settings")]
    public Vector3 boxSize;
    public Vector3 boxOffset;
    public LayerMask ballLayer;

    void OnEnable()
    {
        ball = ballObj.GetComponent<Ball>();
    }

    void Update()
    {
        CheckBallInRange();
    }

    void CheckBallInRange()
    {
        Vector3 boxCenter = transform.position + transform.TransformDirection(boxOffset);

        Collider[] hits = Physics.OverlapBox(boxCenter, boxSize / 2f, transform.rotation, ballLayer);

        isBallInrange = false;
        foreach (var hit in hits)
        {
            if (ball != null)
            {
                isBallInrange = true;
                Debug.Log("<color=green>Ball detected</color>");
                break;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!isGizmoWanted) return;

        Gizmos.color = Color.cyan;
        Vector3 boxCenter = transform.position + transform.TransformDirection(boxOffset);
        Matrix4x4 cubeTransform = Matrix4x4.TRS(boxCenter, transform.rotation, Vector3.one);
        Gizmos.matrix = cubeTransform;
        Gizmos.DrawWireCube(Vector3.zero, boxSize);
        Gizmos.matrix = Matrix4x4.identity;
    }

    public void PickupBall()
    {
        if (isBallInHand == false)
        {
            if (isBallInrange == true)
            {
                if (ball.ableToPickUpBall == true)
                {
                    isBallInHand = true;
                    Debug.Log("<color=purple>ball picked up Sucessfully</color>");
                    ball.gameObject.transform.position = ballHoldingPosition.transform.position;
                    ball.rb.isKinematic = true;
                    gamePlayScreen.ManageThrowBallButton(true);
                    ball.transform.SetParent(ballHoldingPosition.transform);
                    ball.EnablePickup(true);
                }
                else
                {
                    Debug.Log("<color=red>Ball already picked up</color>");
                }
            }
            else
            {
                Debug.Log("<color=red>ball is not in range</color>");
            }
        }
        else
        {
            Debug.Log("<color=red>ball is Already in hand</color>");
        }
    }

    public void StartAiming()
    {
        if (isBallInHand)
        {
            trajectoryLine.enabled = true;
            DrawTrajectory(gamePlayScreen.slider_HandPower.value);
        }
    }

    public void DrawTrajectory(float power)
    {
        if (!isBallInHand) return;

        Vector3 direction = throwDirection.forward;
        Vector3[] points = new Vector3[30];
        float timeStep = 0.1f;
        Vector3 currentPosition = ballObj.transform.position;

        for (int i = 0; i < points.Length; i++)
        {
            float t = i * timeStep;
            points[i] = ball.CalculateTrajectoryPoint(currentPosition, direction, power, t);
        }

        trajectoryLine.positionCount = points.Length;
        trajectoryLine.SetPositions(points);
    }

    public void ThrowBall(float power)
    {
        if (!isBallInHand) return;

        ball.transform.SetParent(null);
        isBallInHand = false;
        trajectoryLine.enabled = false;
        
        Debug.Log($"<color=white>Power : {power}</color>");
        ball.ThrowBall(throwDirection.forward, power);
    }
}

