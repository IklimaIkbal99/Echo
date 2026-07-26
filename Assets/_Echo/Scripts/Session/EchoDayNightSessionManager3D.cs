using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;

public class EchoDayNightSessionManager3D : NetworkBehaviour
{
    [Header("Cycle")]
    public float cycleDurationSeconds = 600f;
    public bool startCycleOnNetworkSpawn = true;

    [Header("Optional UI")]
    public TMP_Text cyclePhaseText;

    [Header("Sun")]
    public Light sunLight;

    public Vector3 sunriseRotation = new Vector3(12f, -70f, 0f);
    public Vector3 dayRotation = new Vector3(50f, -30f, 0f);
    public Vector3 sunsetRotation = new Vector3(10f, 35f, 0f);
    public Vector3 nightRotation = new Vector3(-25f, 100f, 0f);

    [Header("Sun Colors")]
    public Color sunriseSunColor = new Color(1f, 0.62f, 0.32f);
    public Color daySunColor = new Color(1f, 0.93f, 0.72f);
    public Color sunsetSunColor = new Color(1f, 0.48f, 0.18f);
    public Color eveningSunColor = new Color(0.38f, 0.43f, 0.70f);
    public Color nightSunColor = new Color(0.14f, 0.18f, 0.32f);

    [Header("Sun Intensity")]
    public float sunriseSunIntensity = 0.45f;
    public float daySunIntensity = 1.25f;
    public float sunsetSunIntensity = 0.75f;
    public float eveningSunIntensity = 0.22f;
    public float nightSunIntensity = 0.08f;

    [Header("Ambient Light")]
    public Color sunriseAmbientColor = new Color(0.42f, 0.35f, 0.30f);
    public Color dayAmbientColor = new Color(0.62f, 0.58f, 0.48f);
    public Color sunsetAmbientColor = new Color(0.55f, 0.35f, 0.25f);
    public Color eveningAmbientColor = new Color(0.18f, 0.20f, 0.30f);
    public Color nightAmbientColor = new Color(0.09f, 0.11f, 0.18f);

    [Header("Fog")]
    public bool controlFog = true;
    public Color sunriseFogColor = new Color(0.72f, 0.55f, 0.42f);
    public Color dayFogColor = new Color(0.85f, 0.78f, 0.60f);
    public Color sunsetFogColor = new Color(0.70f, 0.38f, 0.25f);
    public Color eveningFogColor = new Color(0.18f, 0.22f, 0.34f);
    public Color nightFogColor = new Color(0.07f, 0.09f, 0.16f);

    public float dayFogDensity = 0.004f;
    public float nightFogDensity = 0.018f;

    [Header("Lanterns / Night Lights")]
    public Light[] lanternLights;
    public GameObject[] lanternVisualObjects;

    [Range(0f, 1f)] public float lanternFadeInStart = 0.60f;
    [Range(0f, 1f)] public float lanternFullAt = 0.75f;
    [Range(0f, 1f)] public float lanternFadeOutStart = 0.92f;

    public float fallbackLanternIntensity = 2f;

    private readonly NetworkVariable<float> syncedCycleTime = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private readonly NetworkVariable<bool> cycleRunning = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private float localCycleTime;
    private float[] lanternBaseIntensities;

    private void Awake()
    {
        CacheLanternIntensities();
        ApplyVisuals(0f);
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer && startCycleOnNetworkSpawn)
        {
            BeginCycle();
        }
    }

    private void Update()
    {
        if (IsSpawned)
        {
            if (IsServer && cycleRunning.Value)
            {
                syncedCycleTime.Value += Time.deltaTime;
                syncedCycleTime.Value = Mathf.Repeat(syncedCycleTime.Value, cycleDurationSeconds);
            }

            ApplyVisuals(GetNetworkProgress());
        }
        else
        {
            localCycleTime += Time.deltaTime;
            localCycleTime = Mathf.Repeat(localCycleTime, cycleDurationSeconds);

            ApplyVisuals(GetLocalProgress());
        }
    }

    public void BeginCycle()
    {
        if (!IsServer)
            return;

        syncedCycleTime.Value = 0f;
        cycleRunning.Value = true;
    }

    private float GetNetworkProgress()
    {
        if (cycleDurationSeconds <= 0f)
            return 0f;

        return Mathf.Clamp01(syncedCycleTime.Value / cycleDurationSeconds);
    }

    private float GetLocalProgress()
    {
        if (cycleDurationSeconds <= 0f)
            return 0f;

        return Mathf.Clamp01(localCycleTime / cycleDurationSeconds);
    }

    private void ApplyVisuals(float progress)
    {
        ApplySun(progress);
        ApplyAmbient(progress);
        ApplyFog(progress);
        ApplyLanterns(progress);
        UpdatePhaseText(progress);
    }

    private void ApplySun(float progress)
    {
        if (sunLight == null)
            return;

        sunLight.transform.rotation = Quaternion.Euler(EvaluateRotation(progress));
        sunLight.color = EvaluateSunColor(progress);
        sunLight.intensity = EvaluateSunIntensity(progress);
    }

    private Vector3 EvaluateRotation(float progress)
    {
        if (progress < 0.18f)
        {
            float t = Mathf.InverseLerp(0f, 0.18f, progress);
            return Vector3.Lerp(sunriseRotation, dayRotation, Smooth(t));
        }

        if (progress < 0.55f)
        {
            return dayRotation;
        }

        if (progress < 0.70f)
        {
            float t = Mathf.InverseLerp(0.55f, 0.70f, progress);
            return Vector3.Lerp(dayRotation, sunsetRotation, Smooth(t));
        }

        if (progress < 0.86f)
        {
            float t = Mathf.InverseLerp(0.70f, 0.86f, progress);
            return Vector3.Lerp(sunsetRotation, nightRotation, Smooth(t));
        }

        {
            float t = Mathf.InverseLerp(0.86f, 1f, progress);
            return Vector3.Lerp(nightRotation, sunriseRotation, Smooth(t));
        }
    }

    private Color EvaluateSunColor(float progress)
    {
        if (progress < 0.18f)
        {
            float t = Mathf.InverseLerp(0f, 0.18f, progress);
            return Color.Lerp(sunriseSunColor, daySunColor, Smooth(t));
        }

        if (progress < 0.55f)
        {
            return daySunColor;
        }

        if (progress < 0.68f)
        {
            float t = Mathf.InverseLerp(0.55f, 0.68f, progress);
            return Color.Lerp(daySunColor, sunsetSunColor, Smooth(t));
        }

        if (progress < 0.82f)
        {
            float t = Mathf.InverseLerp(0.68f, 0.82f, progress);
            return Color.Lerp(sunsetSunColor, eveningSunColor, Smooth(t));
        }

        if (progress < 0.92f)
        {
            float t = Mathf.InverseLerp(0.82f, 0.92f, progress);
            return Color.Lerp(eveningSunColor, nightSunColor, Smooth(t));
        }

        {
            float t = Mathf.InverseLerp(0.92f, 1f, progress);
            return Color.Lerp(nightSunColor, sunriseSunColor, Smooth(t));
        }
    }

    private float EvaluateSunIntensity(float progress)
    {
        if (progress < 0.18f)
        {
            float t = Mathf.InverseLerp(0f, 0.18f, progress);
            return Mathf.Lerp(sunriseSunIntensity, daySunIntensity, Smooth(t));
        }

        if (progress < 0.55f)
        {
            return daySunIntensity;
        }

        if (progress < 0.68f)
        {
            float t = Mathf.InverseLerp(0.55f, 0.68f, progress);
            return Mathf.Lerp(daySunIntensity, sunsetSunIntensity, Smooth(t));
        }

        if (progress < 0.82f)
        {
            float t = Mathf.InverseLerp(0.68f, 0.82f, progress);
            return Mathf.Lerp(sunsetSunIntensity, eveningSunIntensity, Smooth(t));
        }

        if (progress < 0.92f)
        {
            float t = Mathf.InverseLerp(0.82f, 0.92f, progress);
            return Mathf.Lerp(eveningSunIntensity, nightSunIntensity, Smooth(t));
        }

        {
            float t = Mathf.InverseLerp(0.92f, 1f, progress);
            return Mathf.Lerp(nightSunIntensity, sunriseSunIntensity, Smooth(t));
        }
    }

    private void ApplyAmbient(float progress)
    {
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = EvaluateAmbientColor(progress);
    }

    private Color EvaluateAmbientColor(float progress)
    {
        if (progress < 0.18f)
        {
            float t = Mathf.InverseLerp(0f, 0.18f, progress);
            return Color.Lerp(sunriseAmbientColor, dayAmbientColor, Smooth(t));
        }

        if (progress < 0.55f)
        {
            return dayAmbientColor;
        }

        if (progress < 0.68f)
        {
            float t = Mathf.InverseLerp(0.55f, 0.68f, progress);
            return Color.Lerp(dayAmbientColor, sunsetAmbientColor, Smooth(t));
        }

        if (progress < 0.82f)
        {
            float t = Mathf.InverseLerp(0.68f, 0.82f, progress);
            return Color.Lerp(sunsetAmbientColor, eveningAmbientColor, Smooth(t));
        }

        if (progress < 0.92f)
        {
            float t = Mathf.InverseLerp(0.82f, 0.92f, progress);
            return Color.Lerp(eveningAmbientColor, nightAmbientColor, Smooth(t));
        }

        {
            float t = Mathf.InverseLerp(0.92f, 1f, progress);
            return Color.Lerp(nightAmbientColor, sunriseAmbientColor, Smooth(t));
        }
    }

    private void ApplyFog(float progress)
    {
        if (!controlFog)
            return;

        RenderSettings.fog = true;
        RenderSettings.fogColor = EvaluateFogColor(progress);

        float nightAmount = EvaluateNightAmount(progress);
        RenderSettings.fogDensity = Mathf.Lerp(dayFogDensity, nightFogDensity, nightAmount);
    }

    private Color EvaluateFogColor(float progress)
    {
        if (progress < 0.18f)
        {
            float t = Mathf.InverseLerp(0f, 0.18f, progress);
            return Color.Lerp(sunriseFogColor, dayFogColor, Smooth(t));
        }

        if (progress < 0.55f)
        {
            return dayFogColor;
        }

        if (progress < 0.68f)
        {
            float t = Mathf.InverseLerp(0.55f, 0.68f, progress);
            return Color.Lerp(dayFogColor, sunsetFogColor, Smooth(t));
        }

        if (progress < 0.82f)
        {
            float t = Mathf.InverseLerp(0.68f, 0.82f, progress);
            return Color.Lerp(sunsetFogColor, eveningFogColor, Smooth(t));
        }

        if (progress < 0.92f)
        {
            float t = Mathf.InverseLerp(0.82f, 0.92f, progress);
            return Color.Lerp(eveningFogColor, nightFogColor, Smooth(t));
        }

        {
            float t = Mathf.InverseLerp(0.92f, 1f, progress);
            return Color.Lerp(nightFogColor, sunriseFogColor, Smooth(t));
        }
    }

    private void ApplyLanterns(float progress)
    {
        float lanternT = EvaluateNightAmount(progress);

        if (lanternLights != null)
        {
            for (int i = 0; i < lanternLights.Length; i++)
            {
                Light lantern = lanternLights[i];

                if (lantern == null)
                    continue;

                float baseIntensity = fallbackLanternIntensity;

                if (lanternBaseIntensities != null && i < lanternBaseIntensities.Length)
                {
                    baseIntensity = lanternBaseIntensities[i];
                }

                lantern.enabled = lanternT > 0.01f;
                lantern.intensity = baseIntensity * lanternT;
            }
        }

        if (lanternVisualObjects != null)
        {
            foreach (GameObject lanternObject in lanternVisualObjects)
            {
                if (lanternObject == null)
                    continue;

                lanternObject.SetActive(lanternT > 0.01f);
            }
        }
    }

    private float EvaluateNightAmount(float progress)
    {
        if (progress < lanternFadeInStart)
        {
            return 0f;
        }

        if (progress < lanternFullAt)
        {
            float t = Mathf.InverseLerp(lanternFadeInStart, lanternFullAt, progress);
            return Smooth(t);
        }

        if (progress < lanternFadeOutStart)
        {
            return 1f;
        }

        {
            float t = Mathf.InverseLerp(lanternFadeOutStart, 1f, progress);
            return 1f - Smooth(t);
        }
    }

    private void UpdatePhaseText(float progress)
    {
        if (cyclePhaseText == null)
            return;

        if (progress < 0.18f)
        {
            cyclePhaseText.text = "Sunrise";
        }
        else if (progress < 0.55f)
        {
            cyclePhaseText.text = "Daylight";
        }
        else if (progress < 0.68f)
        {
            cyclePhaseText.text = "Sunset";
        }
        else if (progress < 0.82f)
        {
            cyclePhaseText.text = "Evening";
        }
        else if (progress < 0.92f)
        {
            cyclePhaseText.text = "Night";
        }
        else
        {
            cyclePhaseText.text = "Dawn";
        }
    }

    private void CacheLanternIntensities()
    {
        if (lanternLights == null)
            return;

        lanternBaseIntensities = new float[lanternLights.Length];

        for (int i = 0; i < lanternLights.Length; i++)
        {
            if (lanternLights[i] == null)
            {
                lanternBaseIntensities[i] = fallbackLanternIntensity;
                continue;
            }

            float intensity = lanternLights[i].intensity;

            if (intensity <= 0.01f)
            {
                intensity = fallbackLanternIntensity;
            }

            lanternBaseIntensities[i] = intensity;

            lanternLights[i].enabled = false;
            lanternLights[i].intensity = 0f;
        }

        if (lanternVisualObjects != null)
        {
            foreach (GameObject lanternObject in lanternVisualObjects)
            {
                if (lanternObject != null)
                {
                    lanternObject.SetActive(false);
                }
            }
        }
    }

    private float Smooth(float value)
    {
        return Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(value));
    }
}