using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 4f;

    [Header("Input Keys")]
    public KeyCode moveUpKey = KeyCode.W;
    public KeyCode moveDownKey = KeyCode.S;
    public KeyCode moveLeftKey = KeyCode.A;
    public KeyCode moveRightKey = KeyCode.D;

    private Rigidbody2D rb;
    private Vector2 movementInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    private void Update()
    {
        movementInput = Vector2.zero;

        if (Input.GetKey(moveUpKey))
            movementInput.y += 1f;

        if (Input.GetKey(moveDownKey))
            movementInput.y -= 1f;

        if (Input.GetKey(moveLeftKey))
            movementInput.x -= 1f;

        if (Input.GetKey(moveRightKey))
            movementInput.x += 1f;

        movementInput = movementInput.normalized;
    }

    private void FixedUpdate()
    {
        if (movementInput == Vector2.zero)
            return;

        Vector2 nextPosition = rb.position + movementInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(nextPosition);
    }
}