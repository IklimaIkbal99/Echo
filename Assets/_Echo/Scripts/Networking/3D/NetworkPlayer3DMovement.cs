using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class NetworkPlayer3DMovement : NetworkBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 12f;
    public float gravity = -25f;
    public float groundedStickForce = -4f;

    [Header("Play Area")]
    public bool keepInsidePlayArea = true;

    private CharacterController characterController;
    private Vector2 movementInput;
    private float verticalVelocity;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

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
            return new Vector3(input.x, 0f, input.y);

        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraRight = mainCamera.transform.right;

        cameraForward.y = 0f;
        cameraRight.y = 0f;

        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 moveDirection = cameraForward * input.y + cameraRight * input.x;

        if (moveDirection.sqrMagnitude > 1f)
            moveDirection.Normalize();

        return moveDirection;
    }

    [ServerRpc]
    private void SubmitMovementServerRpc(Vector3 moveDirection)
    {
        if (characterController == null)
            characterController = GetComponent<CharacterController>();

        if (characterController.isGrounded)
        {
            if (verticalVelocity < 0f)
                verticalVelocity = groundedStickForce;
        }
        else
        {
            verticalVelocity += gravity * Time.fixedDeltaTime;
        }

        Vector3 horizontalMovement = moveDirection * moveSpeed;
        Vector3 verticalMovement = Vector3.up * verticalVelocity;

        Vector3 finalMovement =
            (horizontalMovement + verticalMovement) * Time.fixedDeltaTime;

        characterController.Move(finalMovement);

        KeepPlayerInsidePlayArea();

        if (moveDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime
            );
        }
    }

    private void KeepPlayerInsidePlayArea()
    {
        if (!keepInsidePlayArea)
            return;

        if (EchoPlayAreaBounds3D.Instance == null)
            return;

        Vector3 currentPosition = transform.position;
        Vector3 clampedPosition =
            EchoPlayAreaBounds3D.Instance.ClampPositionToArea(currentPosition);

        float groundY = EchoPlayAreaBounds3D.Instance.GetGroundY(clampedPosition);
        clampedPosition.y = groundY;

        if (Vector3.Distance(currentPosition, clampedPosition) > 0.001f)
        {
            characterController.enabled = false;
            transform.position = clampedPosition;
            characterController.enabled = true;
        }
    }
}