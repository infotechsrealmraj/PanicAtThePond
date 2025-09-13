using FishNet.Connection;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Managing.Server;
using FishNet.Object;
using FishNet.Transporting;
using System;
using UnityEngine;
using UnityEngine.UI;

public class NetworkUI : MonoBehaviour
{
    public NetworkManager networkManager;

    public Button hostButton;
    public Button clientButton;

    [Header("UI References")]
    public Text playersListText;
    public Text waitingText;
    public GameObject waitingPanel; // 🆚 Waiting UI panel

    [Header("Spawning (assign in Inspector)")]
    public GameObject playerPrefab;
    public Transform[] spawnPoints;

    public int playersRequired = 2;
    private bool playersSpawned = false;
    private bool playSceneLoaded = false;


    private void OnEnable()
    {
        if (networkManager != null)
        {
            networkManager.ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
            networkManager.SceneManager.OnLoadEnd += OnSceneLoadEnd; // 🆚 Scene load होने का इंतज़ार
        }
    }

    private void OnDisable()
    {
        if (networkManager != null)
        {
            networkManager.ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;
            networkManager.SceneManager.OnLoadEnd -= OnSceneLoadEnd;
        }
    }

    public void StartHost()
    {
        networkManager.ServerManager.StartConnection();
        StartCoroutine(CheckServerStarted());

        if (hostButton != null) hostButton.interactable = false;
        if (clientButton != null) clientButton.interactable = false;

        if (waitingPanel != null) waitingPanel.SetActive(true);
    }

    public void StartClient()
    {
        networkManager.ClientManager.StartConnection();

        if (hostButton != null) hostButton.interactable = false;
        if (clientButton != null) clientButton.interactable = false;

        if (waitingPanel != null) waitingPanel.SetActive(true);
    }

    private System.Collections.IEnumerator CheckServerStarted()
    {
        while (!networkManager.ServerManager.Started)
            yield return null;

        Debug.Log("✅ Server successfully started!");

        networkManager.ClientManager.StartConnection();
        StartCoroutine(CheckHostStarted());
    }

    private System.Collections.IEnumerator CheckHostStarted()
    {
        yield return null;

        if (networkManager.ServerManager.Started)
        {
            Debug.Log("Host successfully started!");

            if (waitingText != null)
                waitingText.text = $"Waiting for players... ({networkManager.ServerManager.Clients.Count}/{playersRequired})";
        }
    }

    private void OnRemoteConnectionState(NetworkConnection connection, RemoteConnectionStateArgs args)
    {
        if (args.ConnectionState == RemoteConnectionState.Started)
        {
            Debug.Log($"Client connected (id={connection.ClientId}). Total connections: {networkManager.ServerManager.Clients.Count}");
            UpdatePlayerListUI();

            if (waitingText != null)
                waitingText.text = $"Waiting for players... ({networkManager.ServerManager.Clients.Count}/{playersRequired})";

            TryLoadPlayScene();
        }
        else if (args.ConnectionState == RemoteConnectionState.Stopped)
        {
            Debug.Log($"Client disconnected (id={connection.ClientId}).");
            UpdatePlayerListUI();

            if (waitingText != null && networkManager.ServerManager.Started)
                waitingText.text = $"Waiting for players... ({networkManager.ServerManager.Clients.Count}/{playersRequired})";
        }
    }

    private void OnSceneLoadEnd(SceneLoadEndEventArgs args)
    {
        if (args.LoadedScenes[0].name == "Play")
        {
            Debug.Log("Play scene loaded completely for all clients");
            TrySpawnPlayers();
        }
    }

    private void TryLoadPlayScene()
    {
        if (playSceneLoaded) return;

        int connectedClients = networkManager.ServerManager.Clients.Count;
        Debug.Log($"Checking scene load readiness: connectedClients={connectedClients}, required={playersRequired}");

        if (networkManager.ServerManager.Started && connectedClients >= playersRequired)
        {
            playSceneLoaded = true;

            SceneLoadData sld = new SceneLoadData("Play")
            {
                ReplaceScenes = ReplaceOption.All // This unloads the current scene(s) first
            };

            networkManager.SceneManager.LoadGlobalScenes(sld);
            if (waitingPanel != null) waitingPanel.SetActive(false);
        }
    }

   
    private void TrySpawnPlayers()
    {
        if (playersSpawned) return;

        Debug.Log("Attempting to spawn players for all connections");

        if (playerPrefab == null)
        {
            Debug.LogError("Player prefab not assigned in NetworkUI.");
            return;
        }

        Debug.Log("✅ All players in Play scene. Spawning players...");

        foreach (var kv in networkManager.ServerManager.Clients)
        {
            NetworkConnection conn = kv.Value;

            // Random position और rotation generate करें
            Vector3 randomPos = GetRandomSpawnPosition();

            GameObject go = Instantiate(playerPrefab, randomPos, Quaternion.Euler(0, 0, 0));
            networkManager.ServerManager.Spawn(go, conn);


            int connectionId = conn.ClientId;

            Debug.Log($"Spawned player for client {conn.ClientId} at random position {randomPos}");
        }

        playersSpawned = true;
        Debug.Log("🎮 All players spawned via ServerManager.Spawn()");
    }

   

  
    // Random spawn position generate करने का method
    private Vector3 GetRandomSpawnPosition()
    {
        float x = UnityEngine.Random.Range(-8f, 8);
        float y = UnityEngine.Random.Range(0, -4);
        return new Vector3(x, y); // Y को 0 मानकर ground पर spawn
    }

    private void UpdatePlayerListUI()
    {
        if (playersListText == null) return;

        string list = "Connected Players:\n";

        foreach (var kv in networkManager.ServerManager.Clients)
        {
            NetworkConnection conn = kv.Value;
            list += $"Player {conn.ClientId}\n";
        }

        playersListText.text = list;
    }


   


}