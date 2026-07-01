using UnityEngine;

public class FireflyCluster3D : MonoBehaviour
{
    [Header("Cluster")]
    public int fireflyCount = 18;
    public Vector3 clusterSize = new Vector3(5f, 2.5f, 5f);

    [Header("Firefly Visual")]
    public Material fireflyMaterial;
    public float minSize = 0.04f;
    public float maxSize = 0.11f;

    [Header("Movement")]
    public float minSpeed = 0.25f;
    public float maxSpeed = 0.65f;
    public float targetReachDistance = 0.15f;
    public float verticalFloatAmount = 0.12f;
    public float verticalFloatSpeed = 1.5f;

    [Header("Twinkle")]
    public float minTwinkleSpeed = 1.5f;
    public float maxTwinkleSpeed = 4f;
    public float minBrightness = 0.35f;
    public float maxBrightness = 1.4f;

    [Header("Trail")]
    public bool addSoftTrails = true;
    public Material trailMaterial;
    public float trailTime = 0.45f;
    public float trailWidth = 0.035f;

    [Header("Optional Cluster Light")]
    public Light clusterPointLight;
    public float clusterLightIntensity = 0.35f;
    public float clusterLightRange = 3f;

    [Header("Debug")]
    public bool drawGizmos = true;

    private FireflyData[] fireflies;

    private class FireflyData
    {
        public Transform transform;
        public Renderer renderer;
        public Vector3 baseLocalPosition;
        public Vector3 targetLocalPosition;
        public float speed;
        public float size;
        public float twinkleSpeed;
        public float randomOffset;
        public Material materialInstance;
    }

    private void Start()
    {
        CreateFireflies();
        SetupClusterLight();
    }

    private void Update()
    {
        if (fireflies == null)
            return;

        for (int i = 0; i < fireflies.Length; i++)
        {
            UpdateFirefly(fireflies[i]);
        }

        UpdateClusterLight();
    }

    private void CreateFireflies()
    {
        fireflies = new FireflyData[fireflyCount];

        for (int i = 0; i < fireflyCount; i++)
        {
            GameObject fireflyObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            fireflyObject.name = "Firefly_" + i.ToString("00");
            fireflyObject.transform.SetParent(transform);

            Collider collider = fireflyObject.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            Renderer renderer = fireflyObject.GetComponent<Renderer>();

            Material materialInstance = null;

            if (fireflyMaterial != null)
            {
                materialInstance = new Material(fireflyMaterial);
                renderer.material = materialInstance;
            }

            float size = Random.Range(minSize, maxSize);

            Vector3 localPosition = GetRandomLocalPosition();

            fireflyObject.transform.localPosition = localPosition;
            fireflyObject.transform.localScale = Vector3.one * size;

            if (addSoftTrails)
            {
                AddTrail(fireflyObject, size);
            }

            FireflyData data = new FireflyData
            {
                transform = fireflyObject.transform,
                renderer = renderer,
                baseLocalPosition = localPosition,
                targetLocalPosition = GetRandomLocalPosition(),
                speed = Random.Range(minSpeed, maxSpeed),
                size = size,
                twinkleSpeed = Random.Range(minTwinkleSpeed, maxTwinkleSpeed),
                randomOffset = Random.Range(0f, 100f),
                materialInstance = materialInstance
            };

            fireflies[i] = data;
        }
    }

    private void UpdateFirefly(FireflyData firefly)
    {
        if (firefly == null || firefly.transform == null)
            return;

        Vector3 currentPosition = firefly.transform.localPosition;

        Vector3 direction = firefly.targetLocalPosition - currentPosition;

        if (direction.magnitude <= targetReachDistance)
        {
            firefly.targetLocalPosition = GetRandomLocalPosition();
            direction = firefly.targetLocalPosition - currentPosition;
        }

        Vector3 move = direction.normalized * firefly.speed * Time.deltaTime;
        Vector3 nextPosition = currentPosition + move;

        float floatWave = Mathf.Sin(
            Time.time * verticalFloatSpeed + firefly.randomOffset
        ) * verticalFloatAmount;

        nextPosition.y += floatWave * Time.deltaTime;

        nextPosition = ClampLocalPosition(nextPosition);

        firefly.transform.localPosition = nextPosition;

        float twinkle = Mathf.Sin(Time.time * firefly.twinkleSpeed + firefly.randomOffset);
        float normalizedTwinkle = Mathf.InverseLerp(-1f, 1f, twinkle);
        float brightness = Mathf.Lerp(minBrightness, maxBrightness, normalizedTwinkle);

        float scalePulse = Mathf.Lerp(0.75f, 1.25f, normalizedTwinkle);
        firefly.transform.localScale = Vector3.one * firefly.size * scalePulse;

        ApplyBrightness(firefly, brightness);
    }

    private void ApplyBrightness(FireflyData firefly, float brightness)
    {
        if (firefly.materialInstance == null)
            return;

        Color baseColor = new Color(1f, 0.85f, 0.25f, 1f);
        Color brightColor = baseColor * brightness;
        brightColor.a = Mathf.Clamp01(brightness);

        if (firefly.materialInstance.HasProperty("_BaseColor"))
        {
            firefly.materialInstance.SetColor("_BaseColor", brightColor);
        }
        else if (firefly.materialInstance.HasProperty("_Color"))
        {
            firefly.materialInstance.SetColor("_Color", brightColor);
        }
    }

    private void AddTrail(GameObject fireflyObject, float size)
    {
        TrailRenderer trail = fireflyObject.AddComponent<TrailRenderer>();

        trail.time = trailTime;
        trail.startWidth = trailWidth;
        trail.endWidth = 0f;
        trail.autodestruct = false;
        trail.emitting = true;

        if (trailMaterial != null)
        {
            trail.material = trailMaterial;
        }
        else if (fireflyMaterial != null)
        {
            trail.material = fireflyMaterial;
        }

        Gradient gradient = new Gradient();

        gradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(new Color(1f, 0.85f, 0.25f), 0f),
                new GradientColorKey(new Color(0.5f, 1f, 0.6f), 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(0.55f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );

        trail.colorGradient = gradient;
    }

    private Vector3 GetRandomLocalPosition()
    {
        return new Vector3(
            Random.Range(-clusterSize.x * 0.5f, clusterSize.x * 0.5f),
            Random.Range(0f, clusterSize.y),
            Random.Range(-clusterSize.z * 0.5f, clusterSize.z * 0.5f)
        );
    }

    private Vector3 ClampLocalPosition(Vector3 localPosition)
    {
        localPosition.x = Mathf.Clamp(
            localPosition.x,
            -clusterSize.x * 0.5f,
            clusterSize.x * 0.5f
        );

        localPosition.y = Mathf.Clamp(
            localPosition.y,
            0f,
            clusterSize.y
        );

        localPosition.z = Mathf.Clamp(
            localPosition.z,
            -clusterSize.z * 0.5f,
            clusterSize.z * 0.5f
        );

        return localPosition;
    }

    private void SetupClusterLight()
    {
        if (clusterPointLight == null)
            return;

        clusterPointLight.type = LightType.Point;
        clusterPointLight.color = new Color(1f, 0.75f, 0.25f);
        clusterPointLight.range = clusterLightRange;
        clusterPointLight.intensity = clusterLightIntensity;
        clusterPointLight.shadows = LightShadows.None;
    }

    private void UpdateClusterLight()
    {
        if (clusterPointLight == null)
            return;

        float pulse = Mathf.Sin(Time.time * 1.5f) * 0.5f + 0.5f;
        clusterPointLight.intensity = clusterLightIntensity * Mathf.Lerp(0.6f, 1.2f, pulse);
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos)
            return;

        Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.4f);
        Gizmos.DrawWireCube(
            transform.position + Vector3.up * clusterSize.y * 0.5f,
            clusterSize
        );
    }
}