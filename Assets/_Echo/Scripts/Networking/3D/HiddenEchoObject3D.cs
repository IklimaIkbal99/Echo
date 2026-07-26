using System.Collections;
using UnityEngine;

public class HiddenEchoObject3D : MonoBehaviour
{
    [Header("Visibility")]
    public bool startHidden = true;
    public Renderer[] renderersToControl;

    [Header("Animal Behaviour")]
    public AnimalWander3D animalWander;
    public Animator animalAnimator;
    public string speedParameterName = "Speed";

    [Header("Revive VFX")]
    public GameObject reviveVfxPrefab;
    public Transform reviveVfxSpawnPoint;

    [Header("Discovery Popup")]
    public bool showFirstDiscoveryPopup = true;

    private bool isRevived;
    private Coroutine revealCoroutine;

    private void Awake()
    {
        if (renderersToControl == null || renderersToControl.Length == 0)
        {
            renderersToControl = GetComponentsInChildren<Renderer>(true);
        }

        if (animalWander == null)
        {
            animalWander = GetComponent<AnimalWander3D>();
        }

        if (animalAnimator == null)
        {
            animalAnimator = GetComponentInChildren<Animator>();
        }
    }

    private void Start()
    {
        SetAnimalAnimationSpeed(0f);

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
        if (isRevived)
            return;

        isRevived = true;

        if (revealCoroutine != null)
        {
            StopCoroutine(revealCoroutine);
            revealCoroutine = null;
        }

        SetVisible(true);
        SpawnReviveVFX();

        if (animalWander != null)
        {
            animalWander.StartWandering();
        }

        ShowDiscoveryPopupIfNeeded();
    }

    private IEnumerator RevealRoutine(float duration)
    {
        SetVisible(true);
        SetAnimalAnimationSpeed(0f);

        yield return new WaitForSeconds(duration);

        if (!isRevived)
        {
            SetVisible(false);
            SetAnimalAnimationSpeed(0f);
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

    private void SetAnimalAnimationSpeed(float speed)
    {
        if (animalAnimator != null)
        {
            animalAnimator.SetFloat(speedParameterName, speed);
        }
    }

    private void SpawnReviveVFX()
    {
        if (reviveVfxPrefab == null)
            return;

        Vector3 spawnPosition = transform.position;

        if (reviveVfxSpawnPoint != null)
        {
            spawnPosition = reviveVfxSpawnPoint.position;
        }

        Instantiate(reviveVfxPrefab, spawnPosition, Quaternion.identity);
    }

    private void ShowDiscoveryPopupIfNeeded()
    {
        if (!showFirstDiscoveryPopup)
            return;

        if (FirstAnimalFoundPopupUI.Instance == null)
            return;

        FirstAnimalFoundPopupUI.Instance.ShowPopupOnce();
    }
}