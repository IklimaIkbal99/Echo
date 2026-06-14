using System;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;

public class RelayConnectionUI : MonoBehaviour
{
    [Header("Relay Settings")]
    [SerializeField] private int maxConnections = 4;
    [SerializeField] private string connectionType = "dtls";

    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject playMenuPanel;

    [Header("Game UI")]
    [SerializeField] private GameObject gameHudCanvas;

    [Header("Join Room UI")]
    [SerializeField] private TMP_InputField joinCodeInputField;

    [Header("Texts")]
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text roomCodeText;

    [Header("Buttons")]
    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button joinRoomButton;

    private string currentJoinCode = "";
    private bool isBusy;

    private void Start()
    {
        ShowMainMenu();
        SetStatus("");
        SetRoomCode("");
    }

    public void ShowMainMenu()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        if (playMenuPanel != null)
            playMenuPanel.SetActive(false);

        if (gameHudCanvas != null)
            gameHudCanvas.SetActive(false);
    }

    public void ShowPlayMenu()
{
    Debug.Log("ShowPlayMenu called.");

    if (mainMenuPanel != null)
        mainMenuPanel.SetActive(false);

    if (playMenuPanel != null)
        playMenuPanel.SetActive(true);

    if (gameHudCanvas != null)
        gameHudCanvas.SetActive(false);
}

    public void ShowGameHUD()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        if (playMenuPanel != null)
            playMenuPanel.SetActive(false);

        if (gameHudCanvas != null)
            gameHudCanvas.SetActive(true);
    }

    public async void CreateRoom()
    {
        if (isBusy)
            return;

        if (NetworkManager.Singleton == null)
        {
            SetStatus("NetworkManager not found.");
            return;
        }

        isBusy = true;
        SetButtonsInteractable(false);
        SetStatus("Creating room...");

        try
        {
            await EnsureUnityServicesReady();

            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
            currentJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            RelayServerData relayServerData = new RelayServerData(allocation, connectionType);
            transport.SetRelayServerData(relayServerData);

            bool started = NetworkManager.Singleton.StartHost();

            if (started)
            {
                SetStatus("Room created.");
                SetRoomCode(currentJoinCode);
                ShowGameHUD();

                Debug.Log("Relay Host started. Join Code: " + currentJoinCode);
            }
            else
            {
                SetStatus("Failed to create room.");
                Debug.LogWarning("Failed to start Relay Host.");
            }
        }
        catch (Exception exception)
        {
            SetStatus("Create room failed. Check Console.");
            Debug.LogError("Relay Host error: " + exception);
        }

        isBusy = false;
        SetButtonsInteractable(true);
    }

    public async void JoinRoom()
    {
        if (isBusy)
            return;

        if (NetworkManager.Singleton == null)
        {
            SetStatus("NetworkManager not found.");
            return;
        }

        string cleanedJoinCode = "";

        if (joinCodeInputField != null)
        {
            cleanedJoinCode = joinCodeInputField.text.Trim().ToUpperInvariant();
        }

        if (string.IsNullOrEmpty(cleanedJoinCode))
        {
            SetStatus("Enter a room code.");
            return;
        }

        isBusy = true;
        SetButtonsInteractable(false);
        SetStatus("Joining room...");

        try
        {
            await EnsureUnityServicesReady();

            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(cleanedJoinCode);

            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            RelayServerData relayServerData = new RelayServerData(joinAllocation, connectionType);
            transport.SetRelayServerData(relayServerData);

            bool started = NetworkManager.Singleton.StartClient();

            if (started)
            {
                currentJoinCode = cleanedJoinCode;

                SetStatus("Joined room.");
                SetRoomCode(currentJoinCode);
                ShowGameHUD();

                Debug.Log("Relay Client started with join code: " + cleanedJoinCode);
            }
            else
            {
                SetStatus("Failed to join room.");
                Debug.LogWarning("Failed to start Relay Client.");
            }
        }
        catch (Exception exception)
        {
            SetStatus("Join room failed. Check Console.");
            Debug.LogError("Relay Client error: " + exception);
        }

        isBusy = false;
        SetButtonsInteractable(true);
    }

    public void ShutdownRoom()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }

        currentJoinCode = "";
        SetRoomCode("");
        SetStatus("");

        ShowMainMenu();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private async System.Threading.Tasks.Task EnsureUnityServicesReady()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            await UnityServices.InitializeAsync();
        }

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Signed in anonymously. Player ID: " + AuthenticationService.Instance.PlayerId);
        }
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }

    private void SetRoomCode(string joinCode)
    {
        if (roomCodeText == null)
            return;

        if (string.IsNullOrEmpty(joinCode))
        {
            roomCodeText.text = "";
        }
        else
        {
            roomCodeText.text = "Room Code: " + joinCode;
        }
    }

    private void SetButtonsInteractable(bool interactable)
    {
        if (createRoomButton != null)
            createRoomButton.interactable = interactable;

        if (joinRoomButton != null)
            joinRoomButton.interactable = interactable;
    }
}