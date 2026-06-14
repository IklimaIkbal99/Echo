using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ReviveRingPulse3D : MonoBehaviour
{
    [Header("Ring Shape")]
    public int segments = 128;
    public float startRadius = 0.2f;
    public float endRadius = 2.2f;
    public float duration = 1.0f;
    public float lineWidth = 0.08f;

    [Header("Visual")]
    public Color ringColor = new Color(1f, 0.85f, 0.25f, 1f);

    private LineRenderer lineRenderer;
    private float timer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.useWorldSpace = false;
        lineRenderer.loop = true;
        lineRenderer.positionCount = segments;
        lineRenderer.widthMultiplier = lineWidth;

        lineRenderer.startColor = ringColor;
        lineRenderer.endColor = ringColor;

        if (lineRenderer.sharedMaterial == null)
        {
            Shader shader = Shader.Find("Sprites/Default");
            lineRenderer.material = new Material(shader);
        }

        DrawCircle(startRadius);
    }

    private void Update()
    {
        timer += Time.deltaTime;

        float t = timer / duration;

        if (t >= 1f)
        {
            Destroy(gameObject);
            return;
        }

        float currentRadius = Mathf.Lerp(startRadius, endRadius, t);
        float currentAlpha = Mathf.Lerp(ringColor.a, 0f, t);

        Color currentColor = ringColor;
        currentColor.a = currentAlpha;

        lineRenderer.startColor = currentColor;
        lineRenderer.endColor = currentColor;

        DrawCircle(currentRadius);
    }

    private void DrawCircle(float radius)
    {
        for (int i = 0; i < segments; i++)
        {
            float angle = ((float)i / segments) * Mathf.PI * 2f;

            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;

            lineRenderer.SetPosition(i, new Vector3(x, 0f, z));
        }
    }
}