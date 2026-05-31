using Unity.Netcode;
using UnityEngine;

public class NetworkPlayerEchoController : NetworkBehaviour
{
    [Header("Echo Settings")]
    public GameObject echoRingPrefab;
    public float echoCooldown = 1.2f;
    public float echoRadius = 3.5f;

    private float lastEchoTime;

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
        if (Time.time < lastEchoTime + echoCooldown)
            return;

        lastEchoTime = Time.time;

        SubmitEchoServerRpc(transform.position);
    }

    [ServerRpc]
    private void SubmitEchoServerRpc(Vector3 echoPosition)
    {
        SpawnEchoRingClientRpc(echoPosition);
        RevealNearbyHiddenObjectsClientRpc(echoPosition);
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, echoRadius);
    }
}