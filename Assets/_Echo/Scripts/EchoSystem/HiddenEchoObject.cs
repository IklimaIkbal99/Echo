using System.Collections;
using UnityEngine;

public class HiddenEchoObject : MonoBehaviour
{
    [Header("Visibility Settings")]
    public float hiddenAlpha = 0f;
    public float revealedAlpha = 0.75f;
    public float revivedAlpha = 1f;
    public float revealDuration = 2.5f;
    public float fadeSpeed = 8f;

    private SpriteRenderer[] spriteRenderers;
    private Coroutine revealRoutine;
    private bool isRevived;

    private void Awake()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        SetAlpha(hiddenAlpha);
    }

    public void RevealTemporarily()
    {
        if (isRevived)
            return;

        if (revealRoutine != null)
        {
            StopCoroutine(revealRoutine);
        }

        revealRoutine = StartCoroutine(RevealRoutine());
    }

    public void RevivePermanently()
    {
        isRevived = true;

        if (revealRoutine != null)
        {
            StopCoroutine(revealRoutine);
            revealRoutine = null;
        }

        StartCoroutine(FadeTo(revivedAlpha));
    }

    private IEnumerator RevealRoutine()
    {
        yield return FadeTo(revealedAlpha);

        yield return new WaitForSeconds(revealDuration);

        if (!isRevived)
        {
            yield return FadeTo(hiddenAlpha);
        }

        revealRoutine = null;
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        if (spriteRenderers.Length == 0)
            yield break;

        float startAlpha = spriteRenderers[0].color.a;
        float timer = 0f;

        while (timer < 1f)
        {
            timer += Time.deltaTime * fadeSpeed;

            float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, timer);
            SetAlpha(currentAlpha);

            yield return null;
        }

        SetAlpha(targetAlpha);
    }

    private void SetAlpha(float alpha)
    {
        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }
    }
}