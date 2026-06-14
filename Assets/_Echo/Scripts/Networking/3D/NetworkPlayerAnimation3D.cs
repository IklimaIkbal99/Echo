using Unity.Netcode;
using UnityEngine;

public class NetworkPlayerAnimation3D : NetworkBehaviour
{
    [Header("Animator")]
    public Animator playerAnimator;

    [Header("Animation Parameters")]
    public string speedParameterName = "Speed";

    [Header("Settings")]
    public float movingThreshold = 0.03f;
    public float speedSmoothing = 10f;

    private Vector3 lastPosition;
    private float currentAnimationSpeed;

    private void Start()
    {
        if (playerAnimator == null)
        {
            playerAnimator = GetComponentInChildren<Animator>();
        }

        lastPosition = transform.position;
    }

    private void Update()
    {
        if (playerAnimator == null)
            return;

        Vector3 currentPosition = transform.position;

        Vector3 horizontalDelta = currentPosition - lastPosition;
        horizontalDelta.y = 0f;

        float rawSpeed = horizontalDelta.magnitude / Mathf.Max(Time.deltaTime, 0.0001f);

        float targetAnimationSpeed = rawSpeed > movingThreshold ? 1f : 0f;

        currentAnimationSpeed = Mathf.Lerp(
            currentAnimationSpeed,
            targetAnimationSpeed,
            speedSmoothing * Time.deltaTime
        );

        playerAnimator.SetFloat(speedParameterName, currentAnimationSpeed);

        lastPosition = currentPosition;
    }
}