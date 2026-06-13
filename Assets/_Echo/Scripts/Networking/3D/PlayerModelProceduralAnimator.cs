using UnityEngine;

public class PlayerModelProceduralAnimator : MonoBehaviour
{
    [Header("Model Reference")]
    [SerializeField] private Transform playerModel;
    [SerializeField] private string playerModelChildName = "PlayerModel";

    [Header("Movement Detection")]
    [SerializeField] private float movingThreshold = 0.02f;
    [SerializeField] private float speedSmoothing = 10f;

    [Header("Idle Animation")]
    [SerializeField] private float idleBobAmount = 0.035f;
    [SerializeField] private float idleBobSpeed = 2f;
    [SerializeField] private float idleScaleAmount = 0.025f;

    [Header("Walk Animation")]
    [SerializeField] private float walkBobAmount = 0.10f;
    [SerializeField] private float walkBobSpeed = 8f;
    [SerializeField] private float walkTiltAmount = 5f;
    [SerializeField] private float walkSquashAmount = 0.045f;

    [Header("Smoothing")]
    [SerializeField] private float animationSmoothing = 12f;

    private Vector3 lastWorldPosition;
    private Vector3 baseLocalPosition;
    private Quaternion baseLocalRotation;
    private Vector3 baseLocalScale;

    private float movementAmount;
    private float animationTimer;
    private bool isInitialized;

    private void Awake()
    {
        FindPlayerModelIfNeeded();
        InitializeBaseTransform();
    }

    private void OnEnable()
    {
        FindPlayerModelIfNeeded();
        InitializeBaseTransform();
    }

    private void Update()
    {
        if (!isInitialized || playerModel == null)
            return;

        Vector3 currentWorldPosition = transform.position;

        Vector3 horizontalDelta = currentWorldPosition - lastWorldPosition;
        horizontalDelta.y = 0f;

        float rawSpeed = horizontalDelta.magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
        float targetMovementAmount = rawSpeed > movingThreshold ? 1f : 0f;

        movementAmount = Mathf.Lerp(
            movementAmount,
            targetMovementAmount,
            speedSmoothing * Time.deltaTime
        );

        animationTimer += Time.deltaTime;

        ApplyAnimation();

        lastWorldPosition = currentWorldPosition;
    }

    private void FindPlayerModelIfNeeded()
    {
        if (playerModel != null)
            return;

        Transform foundModel = transform.Find(playerModelChildName);

        if (foundModel != null)
        {
            playerModel = foundModel;
        }
    }

    private void InitializeBaseTransform()
    {
        if (playerModel == null)
        {
            Debug.LogWarning("PlayerModelProceduralAnimator could not find PlayerModel child.");
            isInitialized = false;
            return;
        }

        baseLocalPosition = playerModel.localPosition;
        baseLocalRotation = playerModel.localRotation;
        baseLocalScale = playerModel.localScale;

        lastWorldPosition = transform.position;
        movementAmount = 0f;
        animationTimer = 0f;

        isInitialized = true;
    }

    private void ApplyAnimation()
    {
        float idleWave = Mathf.Sin(animationTimer * idleBobSpeed);
        float walkWave = Mathf.Sin(animationTimer * walkBobSpeed);
        float walkWaveAbs = Mathf.Abs(walkWave);

        float idleWeight = 1f - movementAmount;
        float walkWeight = movementAmount;

        float idleBob = idleWave * idleBobAmount * idleWeight;
        float walkBob = walkWaveAbs * walkBobAmount * walkWeight;

        Vector3 targetLocalPosition = baseLocalPosition;
        targetLocalPosition.y += idleBob + walkBob;

        float tiltZ = walkWave * walkTiltAmount * walkWeight;

        Quaternion targetLocalRotation =
            baseLocalRotation *
            Quaternion.Euler(0f, 0f, tiltZ);

        float idleScale = 1f + idleWave * idleScaleAmount * idleWeight;

        float squashY = 1f - walkWaveAbs * walkSquashAmount * walkWeight;
        float squashXZ = 1f + walkWaveAbs * walkSquashAmount * 0.5f * walkWeight;

        Vector3 targetLocalScale = new Vector3(
            baseLocalScale.x * idleScale * squashXZ,
            baseLocalScale.y * idleScale * squashY,
            baseLocalScale.z * idleScale * squashXZ
        );

        playerModel.localPosition = Vector3.Lerp(
            playerModel.localPosition,
            targetLocalPosition,
            animationSmoothing * Time.deltaTime
        );

        playerModel.localRotation = Quaternion.Slerp(
            playerModel.localRotation,
            targetLocalRotation,
            animationSmoothing * Time.deltaTime
        );

        playerModel.localScale = Vector3.Lerp(
            playerModel.localScale,
            targetLocalScale,
            animationSmoothing * Time.deltaTime
        );
    }

    private void OnDisable()
    {
        if (playerModel == null || !isInitialized)
            return;

        playerModel.localPosition = baseLocalPosition;
        playerModel.localRotation = baseLocalRotation;
        playerModel.localScale = baseLocalScale;
    }
}