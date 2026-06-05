using Unity.Netcode;
using UnityEngine;

public class NetworkPlayer3DMovement : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    private Vector2 movementInput;

    private void Update()
    {
        if (!IsOwner)
            return;

        movementInput.x = Input.GetAxisRaw("Horizontal");
        movementInput.y = Input.GetAxisRaw("Vertical");
        movementInput = movementInput.normalized;
    }

    private void FixedUpdate()
    {
        if (!IsOwner)
            return;

        SubmitMovementServerRpc(movementInput);
    }

    [ServerRpc]
    private void SubmitMovementServerRpc(Vector2 input)
    {
        if (input == Vector2.zero)
            return;

        Vector3 movement = new Vector3(input.x, 0f, input.y) * moveSpeed * Time.fixedDeltaTime;
        transform.position += movement;

        if (movement.sqrMagnitude > 0.001f)
        {
            transform.forward = movement.normalized;
        }
    }
}