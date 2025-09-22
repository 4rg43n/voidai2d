using UnityEngine;

public class PlayerMover : MonoBehaviour
{
    [Header("Debug Settings")]
    [Tooltip("Draw world bounds in the Scene view if enabled.")]
    public bool debugDraw = false;

    [Header("Movement Settings")]
    [Tooltip("Movement speed in units per second.")]
    public float speed = 5f;
    public bool clampToWorld = false;
    public Rect worldBounds = new Rect(-10, -10, 20, 20);

    [Header("Target Settings")]
    [Tooltip("Transform to follow if set.")]
    public Transform target;

    void Update()
    {
        // Get input axes based on W, A, S, D keys
        float moveX = 0f;
        float moveY = 0f;

        if (Input.GetKey(KeyCode.W))
            moveY += 1f;
        if (Input.GetKey(KeyCode.S))
            moveY -= 1f;
        if (Input.GetKey(KeyCode.D))
            moveX += 1f;
        if (Input.GetKey(KeyCode.A))
            moveX -= 1f;

        // If any movement input is detected, clear the target
        if (Mathf.Abs(moveX) > 0f || Mathf.Abs(moveY) > 0f)
        {
            target = null;
        }

        Move(moveX, moveY);
    }

    void Move(float moveX, float moveY)
    {
        if (target != null)
        {
            // Follow the target's position
            Vector3 targetPosition = new Vector3(target.position.x, target.position.y, transform.position.z);
            Vector3 newPosition = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

            if (clampToWorld)
            {
                newPosition.x = Mathf.Clamp(newPosition.x, worldBounds.xMin, worldBounds.xMax);
                newPosition.y = Mathf.Clamp(newPosition.y, worldBounds.yMin, worldBounds.yMax);
            }

            transform.position = newPosition;
        }
        else
        {
            Vector3 move = new Vector3(moveX, moveY, 0f).normalized * speed * Time.deltaTime;
            Vector3 newPosition = transform.position + move;

            if (clampToWorld)
            {
                newPosition.x = Mathf.Clamp(newPosition.x, worldBounds.xMin, worldBounds.xMax);
                newPosition.y = Mathf.Clamp(newPosition.y, worldBounds.yMin, worldBounds.yMax);
            }

            transform.position = newPosition;
        }
    }

    /// <summary>
    /// Sets the position of the GameObject.
    /// </summary>
    /// <param name="newPosition">The new position to set.</param>
    public void SetPosition(Vector3 newPosition)
    {
        transform.position = newPosition;
    }

    /// <summary>
    /// Sets the target Transform to follow.
    /// </summary>
    /// <param name="newTarget">The Transform to follow.</param>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    void OnDrawGizmos()
    {
        if (debugDraw && clampToWorld)
        {
            Gizmos.color = Color.red;
            Vector3 bottomLeft = new Vector3(worldBounds.xMin, worldBounds.yMin, 0f);
            Vector3 topLeft = new Vector3(worldBounds.xMin, worldBounds.yMax, 0f);
            Vector3 topRight = new Vector3(worldBounds.xMax, worldBounds.yMax, 0f);
            Vector3 bottomRight = new Vector3(worldBounds.xMax, worldBounds.yMin, 0f);

            Gizmos.DrawLine(bottomLeft, topLeft);
            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(topRight, bottomRight);
            Gizmos.DrawLine(bottomRight, bottomLeft);
        }
    }
}


