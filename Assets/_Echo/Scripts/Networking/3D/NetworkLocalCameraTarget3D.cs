using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class NetworkLocalCameraTarget3D : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            return;

        StartCoroutine(AssignCameraTargetRoutine());
    }

    private IEnumerator AssignCameraTargetRoutine()
    {
        yield return null;

        CameraFollow3D cameraFollow = FindObjectOfType<CameraFollow3D>();

        if (cameraFollow == null)
        {
            Debug.LogWarning("CameraFollow3D not found in scene.");
            yield break;
        }

        Transform cameraTarget = transform.Find("CameraTarget");

        if (cameraTarget == null)
        {
            cameraTarget = transform;
        }

        cameraFollow.SetTarget(cameraTarget, true);

        Debug.Log("Camera assigned to: " + cameraTarget.name);
    }
}