using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    public Button joinButton;
    public Button readyButton;
    public Button skinsButton;
    public Button startGameButton;
    public GameObject skinsMenu;

    [Header("Cosmetic Managers")]
    public CosmeticManager[] cosmeticManagers;

    private LobbyPlayer localPlayer;
    private bool hasJoined = false;

    private void Start()
    {
        // Configurar listeners de botones
        joinButton.onClick.AddListener(OnJoinClicked);
        readyButton.onClick.AddListener(OnReadyClicked);
        skinsButton.onClick.AddListener(OnSkinsClicked);
        startGameButton.onClick.AddListener(OnStartGameClicked);

        // Estado inicial de UI
        readyButton.interactable = false;
        skinsButton.interactable = false;
        startGameButton.interactable = false;
        startGameButton.gameObject.SetActive(false);
        skinsMenu.SetActive(false);
    }

    private void OnJoinClicked()
    {
        if (!hasJoined)
        {
            // El primer jugador que se une es host, los demás son clientes
            GameManager.Instance.JoinAsHostRpc();

            joinButton.interactable = false;
            joinButton.GetComponentInChildren<TextMeshProUGUI>().text = "Connecting...";
            hasJoined = true;
        }
    }

    private void OnReadyClicked()
    {
        if (NetworkManager.Singleton.IsConnectedClient && localPlayer != null)
        {
            GameManager.Instance.ToggleReadyRpc(NetworkManager.Singleton.LocalClientId);

            // Actualizar texto del botón
            UpdateReadyButtonText();
        }
    }

    private void OnSkinsClicked()
    {
        skinsMenu.SetActive(!skinsMenu.activeSelf);
    }

    private void OnStartGameClicked()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            GameManager.Instance.StartGameRpc(NetworkManager.Singleton.LocalClientId);
        }
    }

    private void Update()
    {
        UpdateUIState();
    }

    private void UpdateUIState()
    {
        // Buscar el jugador local usando el diccionario del GameManager
        if (localPlayer == null && NetworkManager.Singleton.IsConnectedClient)
        {
            ulong localClientId = NetworkManager.Singleton.LocalClientId;

            // Usar el diccionario del GameManager para encontrar el LobbyPlayer
            if (GameManager.Instance.playersDictionary.TryGetValue(localClientId, out LobbyPlayer player))
            {
                localPlayer = player;
                OnLocalPlayerFound();
            }
        }

        // Actualizar estado del botón de start game
        if (startGameButton.gameObject.activeSelf)
        {
            startGameButton.interactable = NetworkManager.Singleton.IsHost;

            // También verificar si todos están listos
            if (NetworkManager.Singleton.IsHost)
            {
                UpdateStartGameButtonState();
            }
        }
    }

    private void OnLocalPlayerFound()
    {
        // Habilitar botones cuando se encuentra el jugador local
        readyButton.interactable = true;
        skinsButton.interactable = true;

        // Actualizar textos iniciales
        UpdateReadyButtonText();

        // Mostrar botón de start game solo al host
        if (NetworkManager.Singleton.IsHost)
        {
            startGameButton.gameObject.SetActive(true);
            UpdateStartGameButtonState();
        }

        // Cambiar texto del botón de join
        joinButton.GetComponentInChildren<TextMeshProUGUI>().text = "Joined";
    }

    private void UpdateReadyButtonText()
    {
        if (localPlayer != null)
        {
            readyButton.GetComponentInChildren<TextMeshProUGUI>().text =
                localPlayer.IsReady.Value ? "Not Ready" : "Ready";
        }
    }

    private void UpdateStartGameButtonState()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            // Verificar si todos los jugadores están listos
            bool allReady = true;
            int playerCount = 0;

            foreach (var player in GameManager.Instance.playersDictionary.Values)
            {
                playerCount++;
                if (!player.IsReady.Value)
                {
                    allReady = false;
                    break;
                }
            }

            startGameButton.interactable = allReady && playerCount > 0;

            // Cambiar color del botón según si se puede iniciar
            var colors = startGameButton.colors;
            colors.normalColor = startGameButton.interactable ? Color.green : Color.gray;
            startGameButton.colors = colors;
        }
    }

    // Método para manejar cambios de cosméticos desde los CosmeticManager
    public void ChangeCosmetic(int cosmeticType, int newIndex)
    {
        if (NetworkManager.Singleton.IsConnectedClient)
        {
            GameManager.Instance.ChangeCosmeticRpc(NetworkManager.Singleton.LocalClientId, cosmeticType, newIndex);
        }
    }
}