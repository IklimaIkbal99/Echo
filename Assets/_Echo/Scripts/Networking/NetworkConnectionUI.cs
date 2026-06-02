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

        GUILayout.BeginArea(new Rect(10, 10, 320, 230), "Network", GUI.skin.window);

        GUILayout.Label("Status: " + statusMessage);

        if (NetworkManager.Singleton.IsServer)
        {
            GUILayout.Label("Mode: Host / Server");
            GUILayout.Label("Connected Clients: " + NetworkManager.Singleton.ConnectedClientsIds.Count);
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            GUILayout.Label("Mode: Client");

            if (NetworkManager.Singleton.IsConnectedClient)
                GUILayout.Label("Client Connection: Connected");
            else
                GUILayout.Label("Client Connection: Connecting / Not fully connected");
        }

        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            GUILayout.Label("Host IP Address:");
            hostAddress = GUILayout.TextField(hostAddress);

            if (GUILayout.Button("Start Host"))
            {
                StartHost();
            }

            if (GUILayout.Button("Start Client"))
            {
                StartClient();
            }
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
            transport.SetConnectionData("127.0.0.1", port, "0.0.0.0");
        }

        bool started = NetworkManager.Singleton.StartHost();

        if (started)
        {
            statusMessage = "Host started";
            Debug.Log("Host started.");
        }
        else
        {
            statusMessage = "Host failed to start";
            Debug.LogWarning("Host failed to start.");
        }
    }

    private void StartClient()
    {
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        if (transport != null)
        {
            transport.SetConnectionData(hostAddress, port);
        }

        bool started = NetworkManager.Singleton.StartClient();

        if (started)
        {
            statusMessage = "Client started. Waiting for connection...";
            Debug.Log("Client started. Waiting for connection...");
        }
        else
        {
            statusMessage = "Client failed to start";
            Debug.LogWarning("Client failed to start.");
        }
    }
}