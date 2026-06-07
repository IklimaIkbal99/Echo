using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow3D : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Camera Position")]
    public Vector3 offset = new Vector3(-8f, 10f, -10f);
    public float smoothSpeed = 6f;

    [Header("Camera Rotation")]
    public Vector3 cameraRotation = new Vector3(45f, 40f, 0f);

    [Header("Camera View")]
    public float orthographicSize = 8f;

    private Camera cameraComponent;

    private void Awake()
    {
        cameraComponent = GetComponent<Camera>();
        cameraComponent.orthographic = true;
        cameraComponent.orthographicSize = orthographicSize;
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

        Vector3 desiredPosition = target.position + offset;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime
        );
    }
}