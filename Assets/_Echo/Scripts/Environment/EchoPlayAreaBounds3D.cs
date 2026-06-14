using UnityEngine;

public class EchoPlayAreaBounds3D : MonoBehaviour
{
    public static EchoPlayAreaBounds3D Instance { get; private set; }

    [Header("Terrain Reference")]
    public Terrain terrain;

    [Header("Playable Area Offset")]
    public float edgeOffset = 5f;

    [Header("Debug")]
    public bool drawGizmos = true;
    public Color gizmoColor = new Color(1f, 0.8f, 0f, 1f);

    private float minX;
    private float maxX;
    private float minZ;
    private float maxZ;

    private void Awake()
    {
        Instance = this;
        CalculateBounds();
    }

    private void OnValidate()
    {
        CalculateBounds();
    }

    public void CalculateBounds()
    {
        if (terrain == null)
        {
            terrain = Terrain.activeTerrain;
        }

        if (terrain == null || terrain.terrainData == null)
            return;

        Vector3 terrainPosition = terrain.transform.position;
        Vector3 terrainSize = terrain.terrainData.size;

        minX = terrainPosition.x + edgeOffset;
        maxX = terrainPosition.x + terrainSize.x - edgeOffset;

        minZ = terrainPosition.z + edgeOffset;
        maxZ = terrainPosition.z + terrainSize.z - edgeOffset;
    }

    public Vector3 ClampPositionToArea(Vector3 position)
    {
        CalculateBounds();

        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.z = Mathf.Clamp(position.z, minZ, maxZ);

        return position;
    }

    public bool IsInsideArea(Vector3 position)
    {
        CalculateBounds();

        return
            position.x >= minX &&
            position.x <= maxX &&
            position.z >= minZ &&
            position.z <= maxZ;
    }

    public Vector3 GetRandomPointInsideArea()
    {
        CalculateBounds();

        float randomX = Random.Range(minX, maxX);
        float randomZ = Random.Range(minZ, maxZ);

        float y = 0f;

        if (terrain != null)
        {
            Vector3 samplePosition = new Vector3(randomX, 0f, randomZ);
            y = terrain.SampleHeight(samplePosition) + terrain.transform.position.y;
        }

        return new Vector3(randomX, y, randomZ);
    }

    public float GetGroundY(Vector3 position)
    {
        if (terrain != null)
        {
            Vector3 terrainPosition = terrain.transform.position;
            TerrainData terrainData = terrain.terrainData;

            bool insideTerrain =
                position.x >= terrainPosition.x &&
                position.x <= terrainPosition.x + terrainData.size.x &&
                position.z >= terrainPosition.z &&
                position.z <= terrainPosition.z + terrainData.size.z;

            if (insideTerrain)
            {
                return terrain.SampleHeight(position) + terrainPosition.y;
            }
        }

        Ray ray = new Ray(
            new Vector3(position.x, position.y + 100f, position.z),
            Vector3.down
        );

        if (Physics.Raycast(ray, out RaycastHit hit, 200f))
        {
            return hit.point.y;
        }

        return position.y;
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos)
            return;

        CalculateBounds();

        if (terrain == null)
            return;

        Gizmos.color = gizmoColor;

        float centerX = (minX + maxX) * 0.5f;
        float centerZ = (minZ + maxZ) * 0.5f;

        float sizeX = maxX - minX;
        float sizeZ = maxZ - minZ;

        Vector3 center = new Vector3(centerX, terrain.transform.position.y + 0.1f, centerZ);
        Vector3 size = new Vector3(sizeX, 0.1f, sizeZ);

        Gizmos.DrawWireCube(center, size);
    }
}