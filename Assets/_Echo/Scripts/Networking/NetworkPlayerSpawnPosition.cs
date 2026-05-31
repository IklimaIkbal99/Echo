using Unity.Netcode;
using UnityEngine;

public class NetworkPlayerSpawnPosition : NetworkBehaviour
{
    [Header("Spawn Settings")]
    public float spacing = 1.5f;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        transform.position = new Vector3((float)OwnerClientId * spacing, 0f, 0f);
    }
}