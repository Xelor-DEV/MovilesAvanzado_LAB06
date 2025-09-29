using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class CosmeticManager : MonoBehaviour
{
    public enum CosmeticType
    {
        Body,
        BodyPart,
        Eye,
        Glove,
        HeadPart,
        MouthAndNose,
        Tail
    }

    [Header("Settings")]
    public CosmeticType type;
    public Button nextButton;
    public Button previousButton;

    private int currentIndex = 0;
    private int maxIndex = 0;
    private LobbyPlayer localPlayer;
    private bool isInitialized = false;

    private void Start()
    {
        nextButton.onClick.AddListener(OnNextClicked);
        previousButton.onClick.AddListener(OnPreviousClicked);

        // Inicialmente deshabilitar botones hasta que tengamos el jugador local
        nextButton.interactable = false;
        previousButton.interactable = false;
    }

    private void Update()
    {
        // Buscar el jugador local si no está inicializado
        if (!isInitialized && NetworkManager.Singleton.IsConnectedClient)
        {
            InitializeWithLocalPlayer();
        }
    }

    private void InitializeWithLocalPlayer()
    {
        ulong localClientId = NetworkManager.Singleton.LocalClientId;

        if (GameManager.Instance.playersDictionary.TryGetValue(localClientId, out LobbyPlayer player))
        {
            localPlayer = player;
            SetupCosmeticManager();
        }
    }

    private void SetupCosmeticManager()
    {
        if (localPlayer == null) return;

        // Obtener el índice actual y máximo según el tipo
        switch (type)
        {
            case CosmeticType.Body:
                currentIndex = localPlayer.GetCurrentBodyIndex();
                maxIndex = localPlayer.GetMaxBodyCount() - 1;
                localPlayer.BodyIndex.OnValueChanged += OnBodyIndexChanged;
                break;
            case CosmeticType.BodyPart:
                currentIndex = localPlayer.GetCurrentBodyPartIndex();
                maxIndex = localPlayer.GetMaxBodyPartCount() - 1;
                localPlayer.BodyPartIndex.OnValueChanged += OnBodyPartIndexChanged;
                break;
            case CosmeticType.Eye:
                currentIndex = localPlayer.GetCurrentEyeIndex();
                maxIndex = localPlayer.GetMaxEyeCount() - 1;
                localPlayer.EyeIndex.OnValueChanged += OnEyeIndexChanged;
                break;
            case CosmeticType.Glove:
                currentIndex = localPlayer.GetCurrentGloveIndex();
                maxIndex = localPlayer.GetMaxGloveCount() - 1;
                localPlayer.GloveIndex.OnValueChanged += OnGloveIndexChanged;
                break;
            case CosmeticType.HeadPart:
                currentIndex = localPlayer.GetCurrentHeadPartIndex();
                maxIndex = localPlayer.GetMaxHeadPartCount() - 1;
                localPlayer.HeadPartIndex.OnValueChanged += OnHeadPartIndexChanged;
                break;
            case CosmeticType.MouthAndNose:
                currentIndex = localPlayer.GetCurrentMouthAndNoseIndex();
                maxIndex = localPlayer.GetMaxMouthAndNoseCount() - 1;
                localPlayer.MouthAndNoseIndex.OnValueChanged += OnMouthAndNoseIndexChanged;
                break;
            case CosmeticType.Tail:
                currentIndex = localPlayer.GetCurrentTailIndex();
                maxIndex = localPlayer.GetMaxTailCount() - 1;
                localPlayer.TailIndex.OnValueChanged += OnTailIndexChanged;
                break;
        }

        // Habilitar botones y actualizar UI
        nextButton.interactable = true;
        previousButton.interactable = true;

        isInitialized = true;
    }

    private void OnNextClicked()
    {
        if (!isInitialized || localPlayer == null) return;

        currentIndex = (currentIndex + 1) % (maxIndex + 1);
        UpdateCosmetic();
    }

    private void OnPreviousClicked()
    {
        if (!isInitialized || localPlayer == null) return;

        currentIndex = (currentIndex - 1 + (maxIndex + 1)) % (maxIndex + 1);
        UpdateCosmetic();
    }

    private void UpdateCosmetic()
    {
        if (localPlayer != null && NetworkManager.Singleton.IsConnectedClient)
        {
            GameManager.Instance.ChangeCosmeticRpc(NetworkManager.Singleton.LocalClientId, (int)type, currentIndex);
        }
    }

    // Métodos para manejar cambios en los NetworkVariables
    private void OnBodyIndexChanged(int oldValue, int newValue)
    {
        if (type == CosmeticType.Body)
        {
            currentIndex = newValue;
        }
    }

    private void OnBodyPartIndexChanged(int oldValue, int newValue)
    {
        if (type == CosmeticType.BodyPart)
        {
            currentIndex = newValue;
        }
    }

    private void OnEyeIndexChanged(int oldValue, int newValue)
    {
        if (type == CosmeticType.Eye)
        {
            currentIndex = newValue;
        }
    }

    private void OnGloveIndexChanged(int oldValue, int newValue)
    {
        if (type == CosmeticType.Glove)
        {
            currentIndex = newValue;
        }
    }

    private void OnHeadPartIndexChanged(int oldValue, int newValue)
    {
        if (type == CosmeticType.HeadPart)
        {
            currentIndex = newValue;
        }
    }

    private void OnMouthAndNoseIndexChanged(int oldValue, int newValue)
    {
        if (type == CosmeticType.MouthAndNose)
        {
            currentIndex = newValue;
        }
    }

    private void OnTailIndexChanged(int oldValue, int newValue)
    {
        if (type == CosmeticType.Tail)
        {
            currentIndex = newValue;

        }
    }

    private void OnDestroy()
    {
        // Desuscribirse de los eventos cuando se destruye el objeto
        if (localPlayer != null)
        {
            switch (type)
            {
                case CosmeticType.Body:
                    localPlayer.BodyIndex.OnValueChanged -= OnBodyIndexChanged;
                    break;
                case CosmeticType.BodyPart:
                    localPlayer.BodyPartIndex.OnValueChanged -= OnBodyPartIndexChanged;
                    break;
                case CosmeticType.Eye:
                    localPlayer.EyeIndex.OnValueChanged -= OnEyeIndexChanged;
                    break;
                case CosmeticType.Glove:
                    localPlayer.GloveIndex.OnValueChanged -= OnGloveIndexChanged;
                    break;
                case CosmeticType.HeadPart:
                    localPlayer.HeadPartIndex.OnValueChanged -= OnHeadPartIndexChanged;
                    break;
                case CosmeticType.MouthAndNose:
                    localPlayer.MouthAndNoseIndex.OnValueChanged -= OnMouthAndNoseIndexChanged;
                    break;
                case CosmeticType.Tail:
                    localPlayer.TailIndex.OnValueChanged -= OnTailIndexChanged;
                    break;
            }
        }
    }
}