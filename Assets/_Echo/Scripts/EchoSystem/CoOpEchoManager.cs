using UnityEngine;

public class CoOpEchoManager : MonoBehaviour
{
    public static CoOpEchoManager Instance { get; private set; }

    [Header("Co-op Echo Settings")]
    public float coOpTimeWindow = 0.6f;
    public float playerProximityDistance = 2.2f;
    public float reviveRadius = 3.5f;

    private PlayerEchoController lastEchoPlayer;
    private float lastEchoTime;
    private Vector2 lastEchoPosition;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void RegisterEcho(PlayerEchoController echoPlayer, Vector2 echoPosition)
    {
        if (lastEchoPlayer != null && lastEchoPlayer != echoPlayer)
        {
            float timeDifference = Time.time - lastEchoTime;
            float playerDistance = Vector2.Distance(echoPosition, lastEchoPosition);

            if (timeDifference <= coOpTimeWindow && playerDistance <= playerProximityDistance)
            {
                TriggerCoOpEcho(echoPosition);
            }
        }

        lastEchoPlayer = echoPlayer;
        lastEchoTime = Time.time;
        lastEchoPosition = echoPosition;
    }

    private void TriggerCoOpEcho(Vector2 centerPosition)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(centerPosition, reviveRadius);

        foreach (Collider2D hit in hits)
        {
            HiddenEchoObject hiddenObject = hit.GetComponentInParent<HiddenEchoObject>();

            if (hiddenObject != null)
            {
                hiddenObject.RevivePermanently();
            }
        }

        Debug.Log("Co-op Echo triggered!");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, reviveRadius);
    }
}