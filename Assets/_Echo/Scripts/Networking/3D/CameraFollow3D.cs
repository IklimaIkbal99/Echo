using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow3D : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Camera Position")]
    public Vector3 offset = new Vector3(-8f, 10f, -10f);

    [Header("Camera Rotation")]
    public Vector3 cameraRotation = new Vector3(45f, 40f, 0f);

    [Header("Camera View")]
    public float orthographicSize = 8f;

    [Header("Follow")]
    public bool useSmoothFollow = false;
    public float smoothSpeed = 10f;

    [Header("Stability")]
    public bool ignoreTargetY = true;
    public float fixedTargetY = 0f;

    private Camera cameraComponent;
    private bool shouldSnap;

    private void Awake()
    {
        cameraComponent = GetComponent<Camera>();
    }

    public void SetTarget(Transform newTarget, bool snapImmediately)
    {
        target = newTarget;
        shouldSnap = snapImmediately;

        if (target != null)
        {
            fixedTargetY = target.position.y;
        }
    }

    private void LateUpdate()
    {
        if (cameraComponent != null)
        {
            cameraComponent.orthographic = true;
            cameraComponent.orthographicSize = orthographicSize;
        }

        transform.rotation = Quaternion.Euler(cameraRotation);

        if (target == null)
            return;

        Vector3 targetPosition = target.position;

        if (ignoreTargetY)
        {
            targetPosition.y = fixedTargetY;
        }

        Vector3 desiredPosition = targetPosition + offset;

        if (shouldSnap || !useSmoothFollow)
        {
            transform.position = desiredPosition;
            shouldSnap = false;
        }
        else
        {
            transform.position = Vector3.Lerp(
                transform.position,
                desiredPosition,
                smoothSpeed * Time.deltaTime
            );
        }
    }
}