using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private string targetTag = "Player";
    [SerializeField] private bool flipSpriteOnTurn = true;

    private Rigidbody2D rb;
    private Transform target;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void Start() => LocatePlayer();

    private void FixedUpdate()
    {
        if (!target)
        {
            LocatePlayer();
            if (!target) return;
        }

        Vector2 direction = ((Vector2)target.position - rb.position).normalized;
        rb.linearVelocity = direction * moveSpeed;
        HandleFacing(direction.x);
    }

    private void LocatePlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(targetTag);
        target = playerObj ? playerObj.transform : FindAnyObjectByType<PlayerController>()?.transform;
    }

    private void HandleFacing(float dirX)
    {
        if (!flipSpriteOnTurn || Mathf.Abs(dirX) <= 0.05f) return;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (dirX > 0 ? 1 : -1);
        transform.localScale = scale;
    }
}
