using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class EchoRingVFX : MonoBehaviour
{
    [Header("Ring Settings")]
    public float duration = 0.8f;
    public float maxRadius = 3.5f;
    public float lineWidth = 0.08f;
    public int segments = 96;

    [Header("Color")]
    public Color ringColor = new Color(1f, 0.85f, 0.25f, 1f);

    private LineRenderer lineRenderer;
    private float timer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.useWorldSpace = false;
        lineRenderer.loop = true;
        lineRenderer.positionCount = segments;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.numCapVertices = 8;
        lineRenderer.numCornerVertices = 8;
        lineRenderer.sortingOrder = 50;

        Material material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.material = material;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        float progress = timer / duration;
        float radius = Mathf.Lerp(0.2f, maxRadius, progress);
        float alpha = Mathf.Lerp(1f, 0f, progress);

        DrawCircle(radius, alpha);

        if (timer >= duration)
        {
            Destroy(gameObject);
        }
    }

    private void DrawCircle(float radius, float alpha)
    {
        Color currentColor = ringColor;
        currentColor.a = alpha;

        lineRenderer.startColor = currentColor;
        lineRenderer.endColor = currentColor;

        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;

            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;

            lineRenderer.SetPosition(i, new Vector3(x, y, 0f));
        }
    }
}