using Unity.Netcode;
using UnityEngine;

public class NetworkPlayerEchoController3D : NetworkBehaviour
{
    [Header("Input")]
    public KeyCode echoKey = KeyCode.E;

    [Header("Echo VFX")]
    public GameObject echoRingPrefab;

    [Header("Hidden Object Detection")]
    public float revealRadius = 8f;
    public float temporaryRevealDuration = 2f;

    [Header("Co-op Revive")]
    public float coOpTimeWindow = 3f;
    public float coOpEchoDistance = 8f;
    public float reviveRadius = 8f;

    [Header("Debug")]
    public bool showDebugLogs = true;

    private static bool hasLastEcho;
    private static ulong lastEchoClientId;
    private static Vector3 lastEchoPosition;
    private static float lastEchoTime;

    private void Update()
    {
        if (!IsOwner)
            return;

        if (Input.GetKeyDown(echoKey))
        {
            SubmitEchoServerRpc(transform.position);
        }
    }

    [ServerRpc]
    private void SubmitEchoServerRpc(Vector3 echoPosition, ServerRpcParams serverRpcParams = default)
    {
        ulong senderClientId = serverRpcParams.Receive.SenderClientId;

        TriggerEchoClientRpc(echoPosition);

        bool hasValidPreviousEcho =
            hasLastEcho &&
            lastEchoClientId != senderClientId &&
            Time.time - lastEchoTime <= coOpTimeWindow &&
            Vector3.Distance(echoPosition, lastEchoPosition) <= coOpEchoDistance;

        if (hasValidPreviousEcho)
        {
            Vector3 reviveCenter = (echoPosition + lastEchoPosition) * 0.5f;

            ReviveNearbyHiddenObjectsClientRpc(reviveCenter);

            hasLastEcho = false;
            return;
        }

        hasLastEcho = true;
        lastEchoClientId = senderClientId;
        lastEchoPosition = echoPosition;
        lastEchoTime = Time.time;
    }

    [ClientRpc]
    private void TriggerEchoClientRpc(Vector3 echoPosition)
    {
        SpawnEchoRing(echoPosition);
        RevealHiddenObjectsNear(echoPosition, false);
    }

    [ClientRpc]
    private void ReviveNearbyHiddenObjectsClientRpc(Vector3 reviveCenter)
    {
        RevealHiddenObjectsNear(reviveCenter, true);
    }

    private void SpawnEchoRing(Vector3 echoPosition)
    {
        if (echoRingPrefab == null)
        {
            Debug.LogWarning("Echo ring prefab is not assigned.");
            return;
        }

        Vector3 ringPosition = new Vector3(
            echoPosition.x,
            0.05f,
            echoPosition.z
        );

        Instantiate(echoRingPrefab, ringPosition, Quaternion.identity);
    }

    private void RevealHiddenObjectsNear(Vector3 centerPosition, bool permanent)
    {
        float detectionRadius = permanent ? reviveRadius : revealRadius;

        HiddenEchoObject3D[] hiddenObjects = FindObjectsOfType<HiddenEchoObject3D>(true);

        if (showDebugLogs)
        {
            Debug.Log("Hidden objects found in scene: " + hiddenObjects.Length);
        }

        foreach (HiddenEchoObject3D hiddenObject in hiddenObjects)
        {
            if (hiddenObject == null)
                continue;

            Vector3 hiddenPosition = hiddenObject.transform.position;

            float distance = Vector3.Distance(
                new Vector3(centerPosition.x, 0f, centerPosition.z),
                new Vector3(hiddenPosition.x, 0f, hiddenPosition.z)
            );

            if (showDebugLogs)
            {
                Debug.Log(
                    "Distance to hidden object " +
                    hiddenObject.name +
                    ": " +
                    distance
                );
            }

            if (distance > detectionRadius)
                continue;

            if (permanent)
            {
                hiddenObject.RevivePermanently();

                if (showDebugLogs)
                {
                    Debug.Log("Hidden object revived permanently: " + hiddenObject.name);
                }
            }
            else
            {
                hiddenObject.RevealTemporarily(temporaryRevealDuration);

                if (showDebugLogs)
                {
                    Debug.Log("Hidden object revealed temporarily: " + hiddenObject.name);
                }
            }
        }
    }
}