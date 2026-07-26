using UnityEngine;
using UnityEngine.UI;

public class FirstAnimalFoundPopupUI : MonoBehaviour
{
    public static FirstAnimalFoundPopupUI Instance { get; private set; }

    [Header("Popup Panel")]
    [SerializeField] private GameObject popupPanel;

    [Header("Close Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button closeButton;

    private bool hasShownPopup;
    private bool isPopupOpen;

    public bool IsPopupOpen => isPopupOpen;

    private void Awake()
    {
        Instance = this;

        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }

        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(ClosePopup);
            continueButton.onClick.AddListener(ClosePopup);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(ClosePopup);
            closeButton.onClick.AddListener(ClosePopup);
        }
    }

    public void ShowPopupOnce()
    {
        if (hasShownPopup)
            return;

        hasShownPopup = true;
        isPopupOpen = true;

        if (popupPanel != null)
        {
            popupPanel.SetActive(true);
        }
    }

    public void ClosePopup()
    {
        isPopupOpen = false;

        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }
    }

    public void ResetPopupForNewSession()
    {
        hasShownPopup = false;
        ClosePopup();
    }
}