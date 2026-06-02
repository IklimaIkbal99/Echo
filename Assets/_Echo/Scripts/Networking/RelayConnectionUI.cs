using System;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayConnectionUI : MonoBehaviour
{
    [Header("Relay Settings")]
    [SerializeField] private int maxConnections = 4;
    [SerializeField] private string connectionType = "dtls";

    private string joinCodeInput = "";
    private string currentJoinCode = "";
    private string statusMessage = "Not connected";
    private bool isBusy;

    private void OnGUI()
    {
        if (NetworkManager.Singleton == null)
            return;

        GUILayout.BeginArea(new Rect(10, 10, 380, 300), "ECHO Relay Network", GUI.skin.window);

        GUILayout.Label("Status: " + statusMessage);

        if (!string.IsNullOrEmpty(currentJoinCode))
        {
            GUILayout.Space(8);
            GUILayout.Label("Join Code:");
            GUILayout.TextField(currentJoinCode);
        }

        GUILayout.Space(8);

        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            GUI.enabled = !isBusy;

            if (GUILayout.Button("Start Host with Relay"))
            {
                StartRelayHost();
            }

            GUILayout.Space(10);

            GUILayout.Label("Enter Join Code:");
            joinCodeInput = GUILayout.TextField(joinCodeInput);

            if (GUILayout.Button("Start Client with Relay"))
            {
                StartRelayClient();
            }

            GUI.enabled = true;
        }
        else
        {
            if (NetworkManager.Singleton.IsHost)
            {
                GUILayout.Label("Mode: Host");
                GUILayout.Label("Connected Clients: " + NetworkManager.Singleton.ConnectedClientsIds.Count);
            }
            else if (NetworkManager.Singleton.IsClient)
            {
                GUILayout.Label("Mode: Client");

                if (NetworkManager.Singleton.IsConnectedClient)
                    GUILayout.Label("Client Connection: Connected");
                else
                    GUILayout.Label("Client Connection: Connecting...");
            }

            if (GUILayout.Button("Shutdown"))
            {
                NetworkManager.Singleton.Shutdown();
                statusMessage = "Shutdown";
                currentJoinCode = "";
            }
        }

        GUILayout.EndArea();
    }

    private async void StartRelayHost()
    {
        if (isBusy)
            return;

        isBusy = true;
        statusMessage = "Initializing Unity Services...";

        try
        {
            await EnsureUnityServicesReady();

            statusMessage = "Creating Relay allocation...";

            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
            currentJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            RelayServerData relayServerData = new RelayServerData(allocation, connectionType);
            transport.SetRelayServerData(relayServerData);

            bool started = NetworkManager.Singleton.StartHost();

            if (started)
            {
                statusMessage = "Host started with Relay.";
                Debug.Log("Relay Host started. Join Code: " + currentJoinCode);
            }
            else
            {
                statusMessage = "Failed to start Relay Host.";
                Debug.LogWarning("Failed to start Relay Host.");
            }
        }
        catch (Exception exception)
        {
            statusMessage = "Relay Host failed. Check Console.";
            Debug.LogError("Relay Host error: " + exception);
        }

        isBusy = false;
    }

    private async void StartRelayClient()
    {
        if (isBusy)
            return;

        string cleanedJoinCode = joinCodeInput.Trim();

        if (string.IsNullOrEmpty(cleanedJoinCode))
        {
            statusMessage = "Join code is empty.";
            return;
        }

        isBusy = true;
        statusMessage = "Initializing Unity Services...";

        try
        {
            await EnsureUnityServicesReady();

            statusMessage = "Joining Relay allocation...";

            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(cleanedJoinCode);

            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            RelayServerData relayServerData = new RelayServerData(joinAllocation, connectionType);
            transport.SetRelayServerData(relayServerData);

            bool started = NetworkManager.Singleton.StartClient();

            if (started)
            {
                statusMessage = "Client started with Relay.";
                Debug.Log("Relay Client started with join code: " + cleanedJoinCode);
            }
            else
            {
                statusMessage = "Failed to start Relay Client.";
                Debug.LogWarning("Failed to start Relay Client.");
            }
        }
        catch (Exception exception)
        {
            statusMessage = "Relay Client failed. Check Console.";
            Debug.LogError("Relay Client error: " + exception);
        }

        isBusy = false;
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
}