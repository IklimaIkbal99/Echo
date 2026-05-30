using UnityEngine;

public class PlayerEchoController : MonoBehaviour
{
    [Header("Echo Settings")]
    public GameObject echoRingPrefab;
    public float echoCooldown = 1.2f;

    private float lastEchoTime;

    private void Update()
    {
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

        if (echoRingPrefab != null)
        {
            Instantiate(echoRingPrefab, transform.position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("Echo Ring Prefab is missing on PlayerEchoController.");
        }
    }
}