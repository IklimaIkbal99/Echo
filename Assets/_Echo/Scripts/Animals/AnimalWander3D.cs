using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class AnimalWander3D : NetworkBehaviour
{
    [Header("Animator")]
    public Animator animalAnimator;
    public string speedParameterName = "Speed";

    [Header("Wander Movement")]
    public float walkSpeed = 2f;
    public float rotationSpeed = 8f;
    public float arrivalDistance = 0.5f;
    public float minimumTargetDistance = 4f;

    [Header("Pause Behaviour")]
    public float minPauseTime = 1f;
    public float maxPauseTime = 3f;

    [Header("Ground")]
    public float groundOffset = 0.03f;

    [Header("Animation")]
    public float animationSmoothing = 10f;
    public float movingAnimationThreshold = 0.01f;

    private NetworkVariable<bool> isMoving = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private bool isWandering;
    private Vector3 currentTarget;
    private float pauseTimer;

    private Vector3 lastWorldPosition;
    private float currentAnimationSpeed;

    private void Awake()
    {
        if (animalAnimator == null)
        {
            animalAnimator = GetComponentInChildren<Animator>();
        }

        lastWorldPosition = transform.position;
    }

    public override void OnNetworkSpawn()
    {
        isMoving.OnValueChanged += OnMovingStateChanged;

        lastWorldPosition = transform.position;
        SetAnimatorSpeedImmediate(0f);
    }

    public override void OnNetworkDespawn()
    {
        isMoving.OnValueChanged -= OnMovingStateChanged;
    }

    private void Update()
    {
        if (IsServer)
        {
            ServerUpdateWander();
        }

        UpdateAnimatorFromActualMovement();
    }

    public void StartWandering()
    {
        if (!IsServer)
            return;

        isWandering = true;
        pauseTimer = 0f;

        PickNewTarget();

        isMoving.Value = true;
    }

    public void StopWandering()
    {
        if (!IsServer)
            return;

        isWandering = false;
        pauseTimer = 0f;

        isMoving.Value = false;
    }

    private void ServerUpdateWander()
    {
        if (!isWandering)
            return;

        if (EchoPlayAreaBounds3D.Instance == null)
            return;

        if (pauseTimer > 0f)
        {
            pauseTimer -= Time.deltaTime;
            isMoving.Value = false;

            if (pauseTimer <= 0f)
            {
                PickNewTarget();
                isMoving.Value = true;
            }

            return;
        }

        Vector3 currentPosition = transform.position;

        Vector3 flatTarget = currentTarget;
        flatTarget.y = currentPosition.y;

        Vector3 direction = flatTarget - currentPosition;
        direction.y = 0f;

        float distanceToTarget = direction.magnitude;

        if (distanceToTarget <= arrivalDistance)
        {
            pauseTimer = Random.Range(minPauseTime, maxPauseTime);
            isMoving.Value = false;
            return;
        }

        Vector3 moveDirection = direction.normalized;

        Vector3 nextPosition =
            currentPosition + moveDirection * walkSpeed * Time.deltaTime;

        nextPosition =
            EchoPlayAreaBounds3D.Instance.ClampPositionToArea(nextPosition);

        float groundY =
            EchoPlayAreaBounds3D.Instance.GetGroundY(nextPosition);

        nextPosition.y = groundY + groundOffset;

        transform.position = nextPosition;

        if (moveDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(moveDirection, Vector3.up);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        isMoving.Value = true;
    }

    private void PickNewTarget()
    {
        if (EchoPlayAreaBounds3D.Instance == null)
        {
            currentTarget = transform.position;
            return;
        }

        Vector3 newTarget = transform.position;

        for (int i = 0; i < 20; i++)
        {
            newTarget = EchoPlayAreaBounds3D.Instance.GetRandomPointInsideArea();

            Vector3 flatDifference = newTarget - transform.position;
            flatDifference.y = 0f;

            if (flatDifference.magnitude >= minimumTargetDistance)
            {
                break;
            }
        }

        newTarget.y =
            EchoPlayAreaBounds3D.Instance.GetGroundY(newTarget) + groundOffset;

        currentTarget = newTarget;
    }

    private void UpdateAnimatorFromActualMovement()
    {
        if (animalAnimator == null)
            return;

        Vector3 currentPosition = transform.position;

        Vector3 horizontalDelta = currentPosition - lastWorldPosition;
        horizontalDelta.y = 0f;

        float actualMovementAmount = horizontalDelta.magnitude;

        bool actuallyMoving = actualMovementAmount > movingAnimationThreshold;

        float targetSpeed = actuallyMoving || isMoving.Value ? 1f : 0f;

        currentAnimationSpeed = Mathf.Lerp(
            currentAnimationSpeed,
            targetSpeed,
            animationSmoothing * Time.deltaTime
        );

        animalAnimator.SetFloat(speedParameterName, currentAnimationSpeed);

        lastWorldPosition = currentPosition;
    }

    private void SetAnimatorSpeedImmediate(float value)
    {
        currentAnimationSpeed = value;

        if (animalAnimator != null)
        {
            animalAnimator.SetFloat(speedParameterName, value);
        }
    }

    private void OnMovingStateChanged(bool previousValue, bool newValue)
    {
        // Animation is updated every frame from actual movement.
    }
}