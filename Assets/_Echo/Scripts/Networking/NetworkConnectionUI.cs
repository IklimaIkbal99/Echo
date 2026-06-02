using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class NetworkConnectionUI : MonoBehaviour
{
    private string hostAddress = "127.0.0.1";
    private ushort port = 7777;

    private string statusMessage = "Not connected";

    private void OnGUI()
    {
        if (NetworkManager.Singleton == null)
            return;

        GUILayout.BeginArea(new Rect(10, 10, 380, 280), "Network", GUI.skin.window);

        GUILayout.Label("Status: " + statusMessage);

        if (NetworkManager.Singleton.IsServer)
        {
            GUILayout.Label("Mode: Host / Server");
            GUILayout.Label("Connected Clients: " + NetworkManager.Singleton.ConnectedClientsIds.Count);
            GUILayout.Label("Host listens on: 0.0.0.0:" + port);
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            GUILayout.Label("Mode: Client");
            GUILayout.Label(NetworkManager.Singleton.IsConnectedClient
                ? "Client Connection: Connected"
                : "Client Connection: Waiting / Failed");
        }

        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            GUILayout.Label("Client: enter HOST IPv4 below");
            hostAddress = GUILayout.TextField(hostAddress);

            if (GUILayout.Button("Start Host"))
                StartHost();

            if (GUILayout.Button("Start Client"))
                StartClient();
        }
        else
        {
            if (GUILayout.Button("Shutdown"))
            {
                NetworkManager.Singleton.Shutdown();
                statusMessage = "Shutdown";
            }
        }

        GUILayout.EndArea();
    }

    private void StartHost()
    {
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        if (transport != null)
        {
            // FIX: Both arguments must be "0.0.0.0" so the socket binds to all
            // network interfaces (Wi-Fi, Ethernet, etc.), not just loopback.
            // Using "127.0.0.1" here restricts the socket to loopback only,
            // which silently blocks all LAN clients even with firewall off.
            transport.SetConnectionData("0.0.0.0", port, "0.0.0.0");
        }

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

        bool started = NetworkManager.Singleton.StartHost();

        if (started)
        {
            statusMessage = "Host started. Waiting for clients...";
            Debug.Log($"[ECHO] Host started. Listening on 0.0.0.0:{port}");
        }
        else
        {
            statusMessage = "Host failed to start";
            Debug.LogWarning("[ECHO] Host failed to start.");
        }
    }

    private void StartClient()
    {
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        if (transport != null)
        {
            // Client connects to host's actual LAN IP. No listen address needed.
            transport.SetConnectionData(hostAddress, port);
        }

        // Subscribe before starting so we catch the result.
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

        bool started = NetworkManager.Singleton.StartClient();

        if (started)
        {
            statusMessage = $"Connecting to {hostAddress}:{port}...";
            Debug.Log($"[ECHO] Client connecting to {hostAddress}:{port}");
        }
        else
        {
            statusMessage = "Client failed to start";
            Debug.LogWarning("[ECHO] StartClient() returned false immediately.");
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            statusMessage = $"Host running. Clients: {NetworkManager.Singleton.ConnectedClientsIds.Count}";
            Debug.Log($"[ECHO] Client connected: id={clientId}");
        }
        else
        {
            statusMessage = "Connected to host!";
            Debug.Log($"[ECHO] Successfully connected. Local client id={clientId}");
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        // DisconnectReason is only populated on the client side.
        string reason = NetworkManager.Singleton.DisconnectReason;

        if (NetworkManager.Singleton.IsServer)
        {
            Debug.Log($"[ECHO] Client disconnected: id={clientId}");
        }
        else
        {
            statusMessage = $"Disconnected. Reason: {(string.IsNullOrEmpty(reason) ? "none" : reason)}";
            Debug.LogWarning($"[ECHO] Client disconnected. Reason: {reason}");
        }
    }

    private void OnDestroy()
    {
        // Clean up callbacks to avoid stale references.
        if (NetworkManager.Singleton == null) return;
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
    }
}