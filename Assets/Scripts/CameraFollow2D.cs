using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Transform target;
    [Range(0.01f, 0.5f)] [SerializeField] private float smoothTime = 0.15f;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

    private Vector3 currentVelocity;

    private void Awake()
    {
        if (!target) target = FindAnyObjectByType<PlayerController>()?.transform;
    }

    private void LateUpdate()
    {
        if (!target) return;
        Vector3 targetPos = new Vector3(target.position.x + offset.x, target.position.y + offset.y, offset.z);
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref currentVelocity, smoothTime);
    }
}
