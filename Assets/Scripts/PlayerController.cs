using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    private PlayerControls controls;
    private Rigidbody2D rb;
    private Vector2 moveInput;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private bool flipSpriteOnTurn = true;

    public Vector2 MoveInput => moveInput;
    public bool IsMoving => moveInput.sqrMagnitude > 0.01f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        controls = new PlayerControls();
        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += _ => moveInput = Vector2.zero;
        controls.Player.Interact.performed += _ => Debug.Log("Player interacted!");
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    private void Update() => HandleFacing(moveInput.x);

    private void FixedUpdate() => rb.linearVelocity = moveInput * moveSpeed;

    private void HandleFacing(float dirX)
    {
        if (!flipSpriteOnTurn || Mathf.Abs(dirX) <= 0.05f) return;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (dirX > 0 ? 1 : -1);
        transform.localScale = scale;
    }
}
