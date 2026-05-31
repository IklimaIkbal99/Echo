using Unity.Netcode;
using UnityEngine;

public class NetworkLocalCameraTarget : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            return;

        Camera mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogWarning("Main Camera not found.");
            return;
        }

        CameraFollow cameraFollow = mainCamera.GetComponent<CameraFollow>();

        if (cameraFollow == null)
        {
            Debug.LogWarning("CameraFollow component not found on Main Camera.");
            return;
        }

        cameraFollow.target = transform;
    }
}