using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private List<Transform> points;

    [Header("Settings")]
    [SerializeField] private float movementSpeed;
    [SerializeField] private float smoothingSpeed;
    [SerializeField] private float stoppingDistance;
    [SerializeField] private float stoppingTime;

    [Tooltip("Private Variables")]
    private Vector2 movementDir;
    private int pointIndex;
    private float timeStamp;
    private bool timeStampOnce;

    private void Start()
    {
        movementDir = GetMovementDirection(points[0].position);

        pointIndex = 0;
        timeStamp = 0.0f;
        timeStampOnce = true;
    }

    private void Update()
    {
        transform.Translate(movementDir.normalized * movementSpeed * (1 - Mathf.Exp(-smoothingSpeed * Time.deltaTime)));

        NextMovementDirectionHandler();
    }

    private Vector2 GetMovementDirection(Vector2 pointPos)
    {
        return pointPos - (Vector2)transform.position;
    }

    private void NextMovementDirectionHandler()
    {
        if (GetMovementDirection(points[pointIndex].position).magnitude < stoppingDistance)
        {
            movementDir = Vector2.zero;

            if (timeStampOnce)
            {
                timeStamp = Time.time;
                timeStampOnce = false;
            }

            if ((Time.time - timeStamp) > stoppingTime)
            {
                // Get the next point index. 
                if (pointIndex == points.Count - 1)
                    pointIndex = 0;
                else
                    ++pointIndex;

                movementDir = GetMovementDirection(points[pointIndex].position);
                timeStamp = 0.0f;
                timeStampOnce = true;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        foreach(Transform point in points) 
            Gizmos.DrawSphere(point.position, 0.2f);
    }
}
