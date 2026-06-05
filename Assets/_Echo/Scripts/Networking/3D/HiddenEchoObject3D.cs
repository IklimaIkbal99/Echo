using System.Collections;
using UnityEngine;

public class HiddenEchoObject3D : MonoBehaviour
{
    [Header("Visibility")]
    public bool startHidden = true;
    public Renderer[] renderersToControl;

    private bool isRevived;
    private Coroutine revealCoroutine;

    private void Awake()
    {
        if (renderersToControl == null || renderersToControl.Length == 0)
        {
            renderersToControl = GetComponentsInChildren<Renderer>(true);
        }
    }

    private void Start()
    {
        if (startHidden && !isRevived)
        {
            SetVisible(false);
        }
    }

    public void RevealTemporarily(float duration)
    {
        if (isRevived)
            return;

        if (revealCoroutine != null)
        {
            StopCoroutine(revealCoroutine);
        }

        revealCoroutine = StartCoroutine(RevealRoutine(duration));
    }

    public void RevivePermanently()
    {
        isRevived = true;

        if (revealCoroutine != null)
        {
            StopCoroutine(revealCoroutine);
        }

        SetVisible(true);
    }

    private IEnumerator RevealRoutine(float duration)
    {
        SetVisible(true);

        yield return new WaitForSeconds(duration);

        if (!isRevived)
        {
            SetVisible(false);
        }

        revealCoroutine = null;
    }

    private void SetVisible(bool visible)
    {
        foreach (Renderer targetRenderer in renderersToControl)
        {
            if (targetRenderer != null)
            {
                targetRenderer.enabled = visible;
            }
        }
    }
}