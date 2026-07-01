using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

public class EchoDayNightSessionManager3D : NetworkBehaviour
{
    [Header("Session")]
    public float sessionDurationSeconds = 600f;
    public bool startSessionOnNetworkSpawn = true;

    [Header("Optional UI")]
    public TMP_Text sessionPhaseText;

    [Header("Sun")]
    public Light sunLight;
    public Vector3 sunStartRotation = new Vector3(45f, -30f, 0f);
    public Vector3 sunEndRotation = new Vector3(8f, -80f, 0f);

    [Header("Sun Colors")]
    public Color daySunColor = new Color(1f, 0.93f, 0.72f);
    public Color sunsetSunColor = new Color(1f, 0.55f, 0.22f);
    public Color nightSunColor = new Color(0.25f, 0.35f, 0.65f);

    [Header("Sun Intensity")]
    public float daySunIntensity = 1.25f;
    public float sunsetSunIntensity = 0.75f;
    public float nightSunIntensity = 0.08f;

    [Header("Ambient Light")]
    public Color dayAmbientColor = new Color(0.62f, 0.58f, 0.48f);
    public Color sunsetAmbientColor = new Color(0.55f, 0.35f, 0.25f);
    public Color nightAmbientColor = new Color(0.08f, 0.10f, 0.18f);

    [Header("Fog")]
    public bool controlFog = true;
    public Color dayFogColor = new Color(0.85f, 0.78f, 0.60f);
    public Color nightFogColor = new Color(0.07f, 0.09f, 0.16f);
    public float dayFogDensity = 0.004f;
    public float nightFogDensity = 0.025f;

    [Header("Lanterns / Fireflies / Night Lights")]
    public Light[] lanternLights;
    public GameObject[] lanternVisualObjects;
    public float lanternFadeStart = 0.60f;
    public float lanternFullAt = 0.82f;
    public float fallbackLanternIntensity = 2f;

    [Header("Session End")]
    public UnityEvent onSessionEnded;

    private readonly NetworkVariable<float> syncedSessionTime = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private readonly NetworkVariable<bool> sessionRunning = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private readonly NetworkVariable<bool> sessionEnded = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private float[] lanternBaseIntensities;

    private void Awake()
    {
        CacheLanternIntensities();
        ApplyVisuals(0f);
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer && startSessionOnNetworkSpawn)
        {
            BeginSession();
        }
    }

    private void Update()
    {
        if (IsSpawned && IsServer && sessionRunning.Value)
        {
            syncedSessionTime.Value += Time.deltaTime;

            if (syncedSessionTime.Value >= sessionDurationSeconds)
            {
                syncedSessionTime.Value = sessionDurationSeconds;
                sessionRunning.Value = false;
                sessionEnded.Value = true;

                SessionEndedClientRpc();
            }
        }

        float progress = 0f;

        if (sessionDurationSeconds > 0f)
        {
            progress = Mathf.Clamp01(syncedSessionTime.Value / sessionDurationSeconds);
        }

        ApplyVisuals(progress);
        UpdatePhaseText(progress);
    }

    public void BeginSession()
    {
        if (!IsServer)
            return;

        syncedSessionTime.Value = 0f;
        sessionRunning.Value = true;
        sessionEnded.Value = false;
    }

    public void EndSessionNow()
    {
        if (!IsServer)
            return;

        syncedSessionTime.Value = sessionDurationSeconds;
        sessionRunning.Value = false;
        sessionEnded.Value = true;

        SessionEndedClientRpc();
    }

    [ClientRpc]
    private void SessionEndedClientRpc()
    {
        Debug.Log("Echo session ended. Echo Memory Card can open now.");

        if (onSessionEnded != null)
        {
            onSessionEnded.Invoke();
        }
    }

    private void ApplyVisuals(float progress)
    {
        ApplySun(progress);
        ApplyAmbient(progress);
        ApplyFog(progress);
        ApplyLanterns(progress);
    }

    private void ApplySun(float progress)
    {
        if (sunLight == null)
            return;

        Vector3 sunRotation = Vector3.Lerp(
            sunStartRotation,
            sunEndRotation,
            progress
        );

        sunLight.transform.rotation = Quaternion.Euler(sunRotation);

        if (progress < 0.65f)
        {
            float t = Mathf.InverseLerp(0f, 0.65f, progress);

            sunLight.color = Color.Lerp(daySunColor, sunsetSunColor, t);
            sunLight.intensity = Mathf.Lerp(daySunIntensity, sunsetSunIntensity, t);
        }
        else
        {
            float t = Mathf.InverseLerp(0.65f, 1f, progress);

            sunLight.color = Color.Lerp(sunsetSunColor, nightSunColor, t);
            sunLight.intensity = Mathf.Lerp(sunsetSunIntensity, nightSunIntensity, t);
        }
    }

    private void ApplyAmbient(float progress)
    {
        RenderSettings.ambientMode = AmbientMode.Flat;

        if (progress < 0.65f)
        {
            float t = Mathf.InverseLerp(0f, 0.65f, progress);
            RenderSettings.ambientLight = Color.Lerp(dayAmbientColor, sunsetAmbientColor, t);
        }
        else
        {
            float t = Mathf.InverseLerp(0.65f, 1f, progress);
            RenderSettings.ambientLight = Color.Lerp(sunsetAmbientColor, nightAmbientColor, t);
        }
    }

    private void ApplyFog(float progress)
    {
        if (!controlFog)
            return;

        RenderSettings.fog = true;
        RenderSettings.fogColor = Color.Lerp(dayFogColor, nightFogColor, progress);
        RenderSettings.fogDensity = Mathf.Lerp(dayFogDensity, nightFogDensity, progress);
    }

    private void ApplyLanterns(float progress)
    {
        float lanternT = Mathf.InverseLerp(lanternFadeStart, lanternFullAt, progress);
        lanternT = Mathf.Clamp01(lanternT);
        lanternT = Mathf.SmoothStep(0f, 1f, lanternT);

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

    private void UpdatePhaseText(float progress)
    {
        if (sessionPhaseText == null)
            return;

        if (progress < 0.60f)
        {
            sessionPhaseText.text = "Daylight";
        }
        else if (progress < 0.90f)
        {
            sessionPhaseText.text = "Evening";
        }
        else
        {
            sessionPhaseText.text = "Night";
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

            lanternLights[i].intensity = 0f;
            lanternLights[i].enabled = false;
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
}