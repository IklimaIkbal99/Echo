using Unity.Netcode;
using UnityEngine;

public class NetworkLocalCameraTarget3D : NetworkBehaviour
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

        CameraFollow3D cameraFollow = mainCamera.GetComponent<CameraFollow3D>();

        if (cameraFollow == null)
        {
            Debug.LogWarning("CameraFollow3D component not found on Main Camera.");
            return;
        }

        cameraFollow.target = transform;
    }
}