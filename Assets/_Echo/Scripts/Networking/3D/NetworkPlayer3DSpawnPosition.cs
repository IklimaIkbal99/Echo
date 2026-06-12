using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class NetworkPlayer3DSpawnPosition : NetworkBehaviour
{
    [Header("Spawn Points")]
    [Tooltip("Only X and Z are used. Y is calculated automatically.")]
    public Vector3 hostSpawnPoint = new Vector3(-7.8f, 0f, 29.18f);

    [Tooltip("Only X and Z are used. Y is calculated automatically.")]
    public Vector3 clientSpawnPoint = new Vector3(-5.8f, 0f, 29.18f);

    [Header("Ground Detection")]
    public float groundOffset = 0.03f;
    public float raycastStartHeight = 100f;
    public float raycastDistance = 200f;
    public LayerMask groundLayerMask = ~0;

    private CharacterController characterController;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        if (characterController == null)
            characterController = GetComponent<CharacterController>();

        Vector3 spawnPosition = GetSpawnPositionForClient(OwnerClientId);

        characterController.enabled = false;
        transform.position = spawnPosition;
        characterController.enabled = true;

        Debug.Log("Player " + OwnerClientId + " spawned at: " + spawnPosition);
    }

    private Vector3 GetSpawnPositionForClient(ulong clientId)
    {
        Vector3 selectedSpawnPoint = clientId == 0 ? hostSpawnPoint : clientSpawnPoint;

        float groundHeight = GetGroundHeight(selectedSpawnPoint);

        float controllerBottomOffset =
            characterController.center.y - characterController.height * 0.5f;

        float rootY = groundHeight - controllerBottomOffset + groundOffset;

        return new Vector3(
            selectedSpawnPoint.x,
            rootY,
            selectedSpawnPoint.z
        );
    }

    private float GetGroundHeight(Vector3 position)
    {
        Terrain activeTerrain = Terrain.activeTerrain;

        if (activeTerrain != null)
        {
            Vector3 terrainPosition = activeTerrain.transform.position;
            TerrainData terrainData = activeTerrain.terrainData;

            bool insideTerrain =
                position.x >= terrainPosition.x &&
                position.x <= terrainPosition.x + terrainData.size.x &&
                position.z >= terrainPosition.z &&
                position.z <= terrainPosition.z + terrainData.size.z;

            if (insideTerrain)
            {
                return activeTerrain.SampleHeight(position) + terrainPosition.y;
            }

            Debug.LogWarning("Spawn point is outside active terrain: " + position);
        }

        Ray ray = new Ray(
            new Vector3(position.x, raycastStartHeight, position.z),
            Vector3.down
        );

        if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, groundLayerMask))
        {
            return hit.point.y;
        }

        Debug.LogWarning("No ground found under spawn point: " + position);
        return 0f;
    }
}