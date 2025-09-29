using System.Collections.Generic;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private int maxPlayers = 5;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private GameObject playerPrefab;

    private NetworkVariable<int> playersReady = new NetworkVariable<int>(0);
    private NetworkVariable<bool> gameStarting = new NetworkVariable<bool>(false);
    public Dictionary<ulong, LobbyPlayer> playersDictionary = new Dictionary<ulong, LobbyPlayer>(); // Ahora pública

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            playersReady.OnValueChanged += OnPlayersReadyChanged;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        // Spawnear el jugador cuando se conecta
        SpawnPlayer(clientId);
    }

    private void OnClientDisconnected(ulong clientId)
    {
        // Remover del diccionario cuando se desconecta
        if (playersDictionary.ContainsKey(clientId))
        {
            playersDictionary.Remove(clientId);
            UpdateReadyCountRpc();
        }
    }

    private void OnPlayersReadyChanged(int oldValue, int newValue)
    {
        Debug.Log($"Players ready: {newValue}/{NetworkManager.Singleton.ConnectedClients.Count}");
    }

    [Rpc(SendTo.Server)]
    public void JoinAsHostRpc()
    {
        if (NetworkManager.Singleton.ConnectedClients.Count > 0)
        {
            Debug.Log("Host already exists!");
            return;
        }

        NetworkManager.Singleton.StartHost();
        Debug.Log("Host started");
    }

    [Rpc(SendTo.Server)]
    public void JoinAsClientRpc()
    {
        if (NetworkManager.Singleton.ConnectedClients.Count >= maxPlayers)
        {
            Debug.Log("Lobby is full!");
            return;
        }

        if (NetworkManager.Singleton.ConnectedClients.Count == 0)
        {
            Debug.Log("No host available!");
            return;
        }

        NetworkManager.Singleton.StartClient();
        Debug.Log("Client joined");
    }

    private void SpawnPlayer(ulong clientId)
    {
        if (!IsServer) return;

        int playerIndex = (int)clientId;
        Vector3 spawnPos = GetSpawnPosition(playerIndex);
        GameObject player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);

        // Agregar al diccionario después de spawnear
        StartCoroutine(AddPlayerToDictionaryAfterSpawn(clientId, player));
    }

    private IEnumerator AddPlayerToDictionaryAfterSpawn(ulong clientId, GameObject playerObject)
    {
        // Esperar un frame para asegurar que el componente esté listo
        yield return null;

        LobbyPlayer lobbyPlayer = playerObject.GetComponent<LobbyPlayer>();
        if (lobbyPlayer != null)
        {
            playersDictionary[clientId] = lobbyPlayer;
            Debug.Log($"Player {clientId} added to dictionary");
        }
        else
        {
            Debug.LogError($"LobbyPlayer component not found for client {clientId}");
        }
    }

    [Rpc(SendTo.Server)]
    public void ToggleReadyRpc(ulong clientId)
    {
        if (playersDictionary.TryGetValue(clientId, out LobbyPlayer player))
        {
            player.IsReady.Value = !player.IsReady.Value;
            UpdateReadyCountRpc();
        }
        else
        {
            Debug.LogWarning($"Player {clientId} not found in dictionary");
        }
    }

    [Rpc(SendTo.Server)]
    public void UpdateReadyCountRpc()
    {
        int readyCount = 0;
        foreach (var player in playersDictionary.Values)
        {
            if (player.IsReady.Value)
                readyCount++;
        }

        playersReady.Value = readyCount;
        Debug.Log($"Ready count updated: {readyCount}/{playersDictionary.Count}");
    }

    [Rpc(SendTo.Server)]
    public void StartGameRpc(ulong requesterId)
    {
        if (!IsServer) return;

        // Verificar que el que llama es el host
        if (requesterId != NetworkManager.ServerClientId)
        {
            Debug.Log("Only host can start the game!");
            return;
        }

        // Verificar que todos estén listos
        bool allReady = true;
        foreach (var player in playersDictionary.Values)
        {
            if (!player.IsReady.Value)
            {
                allReady = false;
                break;
            }
        }

        if (allReady && playersDictionary.Count > 0)
        {
            gameStarting.Value = true;
            NetworkManager.Singleton.SceneManager.LoadScene("Game", LoadSceneMode.Single);
        }
        else
        {
            Debug.Log("Not all players are ready!");
        }
    }

    [Rpc(SendTo.Server)]
    public void ChangeCosmeticRpc(ulong clientId, int cosmeticType, int newIndex)
    {
        if (playersDictionary.TryGetValue(clientId, out LobbyPlayer player))
        {
            player.ChangeCosmeticRpc(cosmeticType, newIndex);
        }
    }

    public Vector3 GetSpawnPosition(int playerIndex)
    {
        if (playerIndex < spawnPoints.Length && playerIndex >= 0)
            return spawnPoints[playerIndex].position;

        return Vector3.zero;
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            playersDictionary.Clear();
        }
    }
}