using UnityEngine;

public class PlayerEchoController : MonoBehaviour
{
    [Header("Player Echo Settings")]
    public int playerIndex = 1;
    public KeyCode echoKey = KeyCode.E;

    [Header("Echo Settings")]
    public GameObject echoRingPrefab;
    public float echoCooldown = 1.2f;
    public float echoRadius = 3.5f;

    private float lastEchoTime;

    private void Update()
    {
        if (Input.GetKeyDown(echoKey))
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

        RevealNearbyHiddenObjects();

        if (CoOpEchoManager.Instance != null)
        {
            CoOpEchoManager.Instance.RegisterEcho(this, transform.position);
        }
    }

    private void RevealNearbyHiddenObjects()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, echoRadius);

        foreach (Collider2D hit in hits)
        {
            HiddenEchoObject hiddenObject = hit.GetComponentInParent<HiddenEchoObject>();

            if (hiddenObject != null)
            {
                hiddenObject.RevealTemporarily();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, echoRadius);
    }
}