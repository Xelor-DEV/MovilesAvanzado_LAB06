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
    public UIManager uiManager;

    private int currentIndex = 0;
    private int maxIndex = 0; 
    private LobbyPlayer localPlayer;
    private bool isInitialized = false;

    private void Start()
    {
        nextButton.onClick.AddListener(OnNextClicked);
        previousButton.onClick.AddListener(OnPreviousClicked);

        nextButton.interactable = false;
        previousButton.interactable = false;
    }

    private void Update()
    {
        if (!isInitialized && GameManager.Instance != null && GameManager.Instance.LocalLobbyPlayer != null)
        {
            InitializeWithLocalPlayer();
        }
    }

    private void InitializeWithLocalPlayer()
    {
        isInitialized = true;
        localPlayer = GameManager.Instance.LocalLobbyPlayer;

        Debug.Log($"CosmeticManager ({type}) inicializado.");

        nextButton.interactable = true;
        previousButton.interactable = true;

        switch (type)
        {
            case CosmeticType.Body:
                maxIndex = localPlayer.GetMaxBodyCount();
                localPlayer.BodyIndex.OnValueChanged += (prev, next) => { currentIndex = next; };
                currentIndex = localPlayer.GetCurrentBodyIndex(); // Sincronizar valor inicial
                break;
            case CosmeticType.BodyPart:
                maxIndex = localPlayer.GetMaxBodyPartCount();
                localPlayer.BodyPartIndex.OnValueChanged += (prev, next) => { currentIndex = next; };
                currentIndex = localPlayer.GetCurrentBodyPartIndex();
                break;
            case CosmeticType.Eye:
                maxIndex = localPlayer.GetMaxEyeCount();
                localPlayer.EyeIndex.OnValueChanged += (prev, next) => { currentIndex = next; };
                currentIndex = localPlayer.GetCurrentEyeIndex();
                break;
            case CosmeticType.Glove:
                maxIndex = localPlayer.GetMaxGloveCount();
                localPlayer.GloveIndex.OnValueChanged += (prev, next) => { currentIndex = next; };
                currentIndex = localPlayer.GetCurrentGloveIndex();
                break;
            case CosmeticType.HeadPart:
                maxIndex = localPlayer.GetMaxHeadPartCount();
                localPlayer.HeadPartIndex.OnValueChanged += (prev, next) => { currentIndex = next; };
                currentIndex = localPlayer.GetCurrentHeadPartIndex();
                break;
            case CosmeticType.MouthAndNose:
                maxIndex = localPlayer.GetMaxMouthAndNoseCount();
                localPlayer.MouthAndNoseIndex.OnValueChanged += (prev, next) => { currentIndex = next; };
                currentIndex = localPlayer.GetCurrentMouthAndNoseIndex();
                break;
            case CosmeticType.Tail:
                maxIndex = localPlayer.GetMaxTailCount();
                localPlayer.TailIndex.OnValueChanged += (prev, next) => { currentIndex = next; };
                currentIndex = localPlayer.GetCurrentTailIndex();
                break;
        }
    }

    private void OnNextClicked()
    {
        if (maxIndex == 0) return;

        currentIndex = (currentIndex + 1) % maxIndex;
        RequestCosmeticChange();
    }

    private void OnPreviousClicked()
    {
        if (maxIndex == 0) return;

        currentIndex = (currentIndex - 1 + maxIndex) % maxIndex;
        RequestCosmeticChange();
    }

    private void RequestCosmeticChange()
    {
        if (uiManager != null)
        {
            uiManager.ChangeCosmetic((int)type, currentIndex);
        }
        else
        {
            Debug.LogError("No se encontró el UIManager en la escena para solicitar el cambio de cosmético.");
        }
    }
}