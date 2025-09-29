using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    public Button hostButton;
    public Button clientButton;
    public Button readyButton;
    public Button skinsButton;
    public Button startGameButton;
    public GameObject skinsMenu;

    [Header("Cosmetic Managers")]
    public CosmeticManager[] cosmeticManagers;

    private LobbyPlayer localPlayer;
    private bool hasJoined = false;
    private bool isUiInitialized = false;

    private void Start()
    {
        hostButton.onClick.AddListener(OnHostClicked);
        clientButton.onClick.AddListener(OnClientClicked);
        readyButton.onClick.AddListener(OnReadyClicked);
        skinsButton.onClick.AddListener(OnSkinsClicked);
        startGameButton.onClick.AddListener(OnStartGameClicked);

        readyButton.interactable = false;
        skinsButton.interactable = false;
        startGameButton.interactable = false;
        startGameButton.gameObject.SetActive(false);
        skinsMenu.SetActive(false);
    }

    private void Update()
    {
        if (!isUiInitialized && GameManager.Instance != null && GameManager.Instance.LocalLobbyPlayer != null)
        {
            InitializeUI();
        }

        if (NetworkManager.Singleton.IsHost && startGameButton.gameObject.activeSelf)
        {
            UpdateStartGameButtonState();
        }
    }

    private void InitializeUI()
    {
        isUiInitialized = true; 
        localPlayer = GameManager.Instance.LocalLobbyPlayer;

        Debug.Log("Jugador local encontrado! Inicializando UI...");

        readyButton.interactable = true;
        skinsButton.interactable = true;

        localPlayer.IsReady.OnValueChanged += OnReadyStateChanged;
        UpdateReadyButtonText();

        if (NetworkManager.Singleton.IsHost)
        {
            startGameButton.gameObject.SetActive(true);
        }

        string role = NetworkManager.Singleton.IsHost ? "Host" : "Client";
        hostButton.GetComponentInChildren<TextMeshProUGUI>().text = $"{role} - Connected";
        clientButton.GetComponentInChildren<TextMeshProUGUI>().text = $"{role} - Connected";
    }

    private void OnHostClicked()
    {
        if (!hasJoined)
        {
            if (NetworkManager.Singleton.StartHost())
            {
                OnJoined("Host");
            }
        }
    }

    private void OnClientClicked()
    {
        if (!hasJoined)
        {
            if (NetworkManager.Singleton.StartClient())
            {
                OnJoined("Client");
            }
        }
    }

    private void OnReadyClicked()
    {
        if (localPlayer != null)
        {
            GameManager.Instance.ToggleReadyServerRpc(NetworkManager.Singleton.LocalClientId);
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

    private void OnJoined(string role)
    {
        hasJoined = true;
        hostButton.interactable = false;
        clientButton.interactable = false;
        hostButton.GetComponentInChildren<TextMeshProUGUI>().text = "Connecting...";
        clientButton.GetComponentInChildren<TextMeshProUGUI>().text = "Connecting...";
    }

    private void OnReadyStateChanged(bool previousValue, bool newValue)
    {
        UpdateReadyButtonText();
    }

    private void UpdateReadyButtonText()
    {
        if (localPlayer != null)
        {
            readyButton.GetComponentInChildren<TextMeshProUGUI>().text = localPlayer.IsReady.Value ? "Not Ready" : "Ready";
        }
    }

    private void UpdateStartGameButtonState()
    {
        if (!NetworkManager.Singleton.IsServer) return;

        bool allReady = true;
        if (GameManager.Instance.playersDictionary.Count == 0)
        {
            allReady = false;
        }
        else
        {
            foreach (var player in GameManager.Instance.playersDictionary.Values)
            {
                if (!player.IsReady.Value)
                {
                    allReady = false;
                    break;
                }
            }
        }

        startGameButton.interactable = allReady;
    }

    public void ChangeCosmetic(int cosmeticType, int newIndex)
    {
        if (localPlayer != null)
        {
            GameManager.Instance.ChangeCosmeticServerRpc(NetworkManager.Singleton.LocalClientId, cosmeticType, newIndex);
        }
    }

    private void OnDestroy()
    {
        if (localPlayer != null)
        {
            localPlayer.IsReady.OnValueChanged -= OnReadyStateChanged;
        }
    }
}