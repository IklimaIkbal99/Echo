using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class NetworkConnectionUI : MonoBehaviour
{
    private string hostAddress = "127.0.0.1";
    private ushort port = 7777;

    private void OnGUI()
    {
        if (NetworkManager.Singleton == null)
            return;

        GUILayout.BeginArea(new Rect(10, 10, 280, 190), "Network", GUI.skin.window);

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
            string mode = NetworkManager.Singleton.IsHost ? "Host" : "Client";
            GUILayout.Label("Connected as: " + mode);

            if (GUILayout.Button("Shutdown"))
            {
                NetworkManager.Singleton.Shutdown();
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

        NetworkManager.Singleton.StartHost();
    }

    private void StartClient()
    {
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        if (transport != null)
        {
            transport.SetConnectionData(hostAddress, port);
        }

        NetworkManager.Singleton.StartClient();
    }
}