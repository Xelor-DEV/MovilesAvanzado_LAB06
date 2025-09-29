using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    // --- Singleton Pattern ---
    public static GameManager Instance { get; private set; }

    [Header("Network Prefabs")]
    [SerializeField] private GameObject playerPrefab;

    [Header("Lobby Settings")]
    [SerializeField] private Transform[] spawnPoints;

    public Dictionary<ulong, LobbyPlayer> playersDictionary = new Dictionary<ulong, LobbyPlayer>();

    public LobbyPlayer LocalLobbyPlayer { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // <-- AÑADE ESTA LÍNEA
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
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    public void SetLocalPlayer(LobbyPlayer player)
    {
        if (player.IsOwner)
        {
            LocalLobbyPlayer = player;
            Debug.Log($"Referencia del jugador local (ID: {player.OwnerClientId}) guardada en GameManager.");
        }
        else
        {
            Debug.LogWarning($"Se intentó registrar un jugador no local (ID: {player.OwnerClientId}). Operación ignorada.");
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!IsServer) return;

        Debug.Log($"Cliente {clientId} conectado. Spawneando su LobbyPlayer...");

        int playerIndex = playersDictionary.Count % spawnPoints.Length;
        Vector3 spawnPos = spawnPoints.Length > 0 ? spawnPoints[playerIndex].position : Vector3.zero;

        GameObject playerInstance = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);

        LobbyPlayer lobbyPlayer = playerInstance.GetComponent<LobbyPlayer>();
        if (lobbyPlayer != null)
        {
            playersDictionary[clientId] = lobbyPlayer;
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (!IsServer) return;

        if (playersDictionary.ContainsKey(clientId))
        {
            playersDictionary.Remove(clientId);
            Debug.Log($"Cliente {clientId} desconectado. Jugador eliminado del diccionario.");
        }
    }

    [Rpc(SendTo.Server)]
    public void ToggleReadyServerRpc(ulong clientId)
    {
        if (playersDictionary.TryGetValue(clientId, out LobbyPlayer player))
        {
            player.IsReady.Value = !player.IsReady.Value;
            Debug.Log($"El jugador {clientId} ha cambiado su estado a: {(player.IsReady.Value ? "Listo" : "No Listo")}");
        }
    }

    [Rpc(SendTo.Server)]
    public void ChangeCosmeticServerRpc(ulong clientId, int cosmeticType, int newIndex)
    {
        if (playersDictionary.TryGetValue(clientId, out LobbyPlayer player))
        {
            player.ServerChangeCosmetic(cosmeticType, newIndex);
        }
    }

    [Rpc(SendTo.Server)]
    public void StartGameRpc(ulong requesterId)
    {
        if (!IsServer || requesterId != NetworkManager.ServerClientId) return;

        bool allPlayersReady = true;
        if (playersDictionary.Count == 0)
        {
            allPlayersReady = false;
        }
        else
        {
            foreach (var player in playersDictionary.Values)
            {
                if (!player.IsReady.Value)
                {
                    allPlayersReady = false;
                    break;
                }
            }
        }

        if (allPlayersReady)
        {
            Debug.Log("¡Todos los jugadores están listos! Iniciando partida...");
            foreach (var player in playersDictionary.Values)
            {
                player.GetComponent<NetworkObject>().DestroyWithScene = false;
            }
            NetworkManager.Singleton.SceneManager.LoadScene("Game", LoadSceneMode.Single);
        }
        else
        {
            Debug.LogWarning("Se intentó iniciar la partida pero no todos los jugadores estaban listos.");
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
        base.OnNetworkDespawn();
    }
}