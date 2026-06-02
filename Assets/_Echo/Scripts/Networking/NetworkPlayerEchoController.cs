using Unity.Netcode;
using UnityEngine;

public class NetworkPlayerEchoController : NetworkBehaviour
{
    [Header("Echo Settings")]
    public GameObject echoRingPrefab;
    public float echoCooldown = 1.2f;
    public float echoRadius = 3.5f;

    [Header("Co-op Echo Settings")]
    public float coOpTimeWindow = 1.0f;
    public float playerProximityDistance = 3.5f;
    public float reviveRadius = 4f;

    private float lastLocalEchoTime;

    private static bool hasLastServerEcho;
    private static ulong lastEchoClientId;
    private static float lastServerEchoTime;
    private static Vector3 lastServerEchoPosition;

    private void Update()
    {
        if (!IsOwner)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryEcho();
        }
    }

    private void TryEcho()
    {
        if (Time.time < lastLocalEchoTime + echoCooldown)
            return;

        lastLocalEchoTime = Time.time;

        SubmitEchoServerRpc(transform.position);
    }

    [ServerRpc]
    private void SubmitEchoServerRpc(Vector3 echoPosition, ServerRpcParams serverRpcParams = default)
    {
        ulong senderClientId = serverRpcParams.Receive.SenderClientId;

        SpawnEchoRingClientRpc(echoPosition);
        RevealNearbyHiddenObjectsClientRpc(echoPosition);

        TryTriggerCoOpEcho(senderClientId, echoPosition);
    }

    private void TryTriggerCoOpEcho(ulong senderClientId, Vector3 echoPosition)
    {
        if (hasLastServerEcho && lastEchoClientId != senderClientId)
        {
            float timeDifference = Time.time - lastServerEchoTime;
            float playerDistance = Vector3.Distance(echoPosition, lastServerEchoPosition);

            if (timeDifference <= coOpTimeWindow && playerDistance <= playerProximityDistance)
            {
                Vector3 coOpCenter = (echoPosition + lastServerEchoPosition) * 0.5f;

                ReviveNearbyHiddenObjectsClientRpc(coOpCenter);

                Debug.Log("Network co-op echo triggered!");

                hasLastServerEcho = false;
                return;
            }
        }

        hasLastServerEcho = true;
        lastEchoClientId = senderClientId;
        lastServerEchoTime = Time.time;
        lastServerEchoPosition = echoPosition;
    }

    [ClientRpc]
    private void SpawnEchoRingClientRpc(Vector3 echoPosition)
    {
        if (echoRingPrefab == null)
        {
            Debug.LogWarning("Echo Ring Prefab is missing on NetworkPlayerEchoController.");
            return;
        }

        Instantiate(echoRingPrefab, echoPosition, Quaternion.identity);
    }

    [ClientRpc]
    private void RevealNearbyHiddenObjectsClientRpc(Vector3 echoPosition)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(echoPosition, echoRadius);

        foreach (Collider2D hit in hits)
        {
            HiddenEchoObject hiddenObject = hit.GetComponentInParent<HiddenEchoObject>();

            if (hiddenObject != null)
            {
                hiddenObject.RevealTemporarily();
            }
        }
    }

    [ClientRpc]
    private void ReviveNearbyHiddenObjectsClientRpc(Vector3 centerPosition)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(centerPosition, reviveRadius);

        foreach (Collider2D hit in hits)
        {
            HiddenEchoObject hiddenObject = hit.GetComponentInParent<HiddenEchoObject>();

            if (hiddenObject != null)
            {
                hiddenObject.RevivePermanently();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, echoRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, reviveRadius);
    }
}