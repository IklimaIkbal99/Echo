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

        Vector3 moveDirection = GetCameraRelativeMoveDirection(movementInput);

        SubmitMovementServerRpc(moveDirection);
    }

    private Vector3 GetCameraRelativeMoveDirection(Vector2 input)
    {
        Camera mainCamera = Camera.main;

        if (mainCamera == null)
        {
            return new Vector3(input.x, 0f, input.y);
        }

        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraRight = mainCamera.transform.right;

        cameraForward.y = 0f;
        cameraRight.y = 0f;

        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 moveDirection =
            cameraForward * input.y +
            cameraRight * input.x;

        if (moveDirection.sqrMagnitude > 1f)
        {
            moveDirection.Normalize();
        }

        return moveDirection;
    }

    [ServerRpc]
    private void SubmitMovementServerRpc(Vector3 moveDirection)
    {
        if (moveDirection.sqrMagnitude <= 0.0001f)
            return;

        transform.position += moveDirection * moveSpeed * Time.fixedDeltaTime;

        transform.forward = moveDirection;
    }
}