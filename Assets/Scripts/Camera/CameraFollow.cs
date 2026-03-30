using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
    [SerializeField] private float followSpeed = 8f;

    /// <summary>
    /// Moves the camera after the player has moved for smoother follow behavior.
    /// </summary>
    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Assigns the target at runtime if you want to set it from another script.
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
