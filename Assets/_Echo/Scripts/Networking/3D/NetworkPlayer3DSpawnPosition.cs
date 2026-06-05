using Unity.Netcode;
using UnityEngine;

public class NetworkPlayer3DSpawnPosition : NetworkBehaviour
{
    [Header("Spawn Settings")]
    public float spacing = 2f;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        transform.position = new Vector3((float)OwnerClientId * spacing, 0.6f, 0f);
    }
}