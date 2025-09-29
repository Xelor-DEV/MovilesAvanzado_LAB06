using Unity.Netcode;
using TMPro;
using UnityEngine;

public class LobbyPlayer : NetworkBehaviour
{
    public NetworkVariable<bool> IsReady = new NetworkVariable<bool>(false);
    public NetworkVariable<int> BodyIndex = new NetworkVariable<int>(0);
    public NetworkVariable<int> BodyPartIndex = new NetworkVariable<int>(0);
    public NetworkVariable<int> EyeIndex = new NetworkVariable<int>(0);
    public NetworkVariable<int> GloveIndex = new NetworkVariable<int>(0);
    public NetworkVariable<int> HeadPartIndex = new NetworkVariable<int>(0);
    public NetworkVariable<int> MouthAndNoseIndex = new NetworkVariable<int>(0);
    public NetworkVariable<int> TailIndex = new NetworkVariable<int>(0);

    [Header("Cosmetic Arrays")]
    public GameObject[] bodies;
    public GameObject[] bodyParts;
    public GameObject[] eyes;
    public GameObject[] gloves;
    public GameObject[] headParts;
    public GameObject[] mouthAndNoses;
    public GameObject[] tails;

    [Header("UI")]
    public TMP_Text playerStateText;

    private GameObject currentBody;
    private GameObject currentBodyPart;
    private GameObject currentEye;
    private GameObject currentGlove;
    private GameObject currentHeadPart;
    private GameObject currentMouthAndNose;
    private GameObject currentTail;

    public override void OnNetworkSpawn()
    {
        // Suscribirse a todos los cambios de NetworkVariables
        IsReady.OnValueChanged += OnReadyStateChanged;
        BodyIndex.OnValueChanged += OnBodyChanged;
        BodyPartIndex.OnValueChanged += OnBodyPartChanged;
        EyeIndex.OnValueChanged += OnEyeChanged;
        GloveIndex.OnValueChanged += OnGloveChanged;
        HeadPartIndex.OnValueChanged += OnHeadPartChanged;
        MouthAndNoseIndex.OnValueChanged += OnMouthAndNoseChanged;
        TailIndex.OnValueChanged += OnTailChanged;

        UpdateAllCosmetics();
        UpdatePlayerStateText();
    }

    private void OnReadyStateChanged(bool oldValue, bool newValue)
    {
        UpdatePlayerStateText();
    }

    private void OnBodyChanged(int oldValue, int newValue)
    {
        UpdateCosmetic(bodies, newValue, ref currentBody);
    }

    private void OnBodyPartChanged(int oldValue, int newValue)
    {
        UpdateCosmetic(bodyParts, newValue, ref currentBodyPart);
    }

    private void OnEyeChanged(int oldValue, int newValue)
    {
        UpdateCosmetic(eyes, newValue, ref currentEye);
    }

    private void OnGloveChanged(int oldValue, int newValue)
    {
        UpdateCosmetic(gloves, newValue, ref currentGlove);
    }

    private void OnHeadPartChanged(int oldValue, int newValue)
    {
        UpdateCosmetic(headParts, newValue, ref currentHeadPart);
    }

    private void OnMouthAndNoseChanged(int oldValue, int newValue)
    {
        UpdateCosmetic(mouthAndNoses, newValue, ref currentMouthAndNose);
    }

    private void OnTailChanged(int oldValue, int newValue)
    {
        UpdateCosmetic(tails, newValue, ref currentTail);
    }

    private void UpdatePlayerStateText()
    {
        if (playerStateText != null)
        {
            playerStateText.text = IsReady.Value ? "Ready" : "Not Ready";
            playerStateText.color = IsReady.Value ? Color.green : Color.red;
        }
    }

    private void UpdateCosmetic(GameObject[] array, int index, ref GameObject current)
    {
        // Desactivar el actual si existe
        if (current != null)
            current.SetActive(false);

        // Activar el nuevo si está en rango
        if (index >= 0 && index < array.Length && array[index] != null)
        {
            current = array[index];
            current.SetActive(true);
        }
    }

    private void UpdateAllCosmetics()
    {
        UpdateCosmetic(bodies, BodyIndex.Value, ref currentBody);
        UpdateCosmetic(bodyParts, BodyPartIndex.Value, ref currentBodyPart);
        UpdateCosmetic(eyes, EyeIndex.Value, ref currentEye);
        UpdateCosmetic(gloves, GloveIndex.Value, ref currentGlove);
        UpdateCosmetic(headParts, HeadPartIndex.Value, ref currentHeadPart);
        UpdateCosmetic(mouthAndNoses, MouthAndNoseIndex.Value, ref currentMouthAndNose);
        UpdateCosmetic(tails, TailIndex.Value, ref currentTail);
    }

    [Rpc(SendTo.Server)]
    public void ChangeCosmeticRpc(int cosmeticType, int newIndex)
    {
        switch (cosmeticType)
        {
            case 0:
                if (newIndex >= 0 && newIndex < bodies.Length)
                    BodyIndex.Value = newIndex;
                break;
            case 1:
                if (newIndex >= 0 && newIndex < bodyParts.Length)
                    BodyPartIndex.Value = newIndex;
                break;
            case 2:
                if (newIndex >= 0 && newIndex < eyes.Length)
                    EyeIndex.Value = newIndex;
                break;
            case 3:
                if (newIndex >= 0 && newIndex < gloves.Length)
                    GloveIndex.Value = newIndex;
                break;
            case 4:
                if (newIndex >= 0 && newIndex < headParts.Length)
                    HeadPartIndex.Value = newIndex;
                break;
            case 5:
                if (newIndex >= 0 && newIndex < mouthAndNoses.Length)
                    MouthAndNoseIndex.Value = newIndex;
                break;
            case 6:
                if (newIndex >= 0 && newIndex < tails.Length)
                    TailIndex.Value = newIndex;
                break;
        }
    }

    // Métodos públicos para obtener índices actuales (útil para UI)
    public int GetCurrentBodyIndex() => BodyIndex.Value;
    public int GetCurrentBodyPartIndex() => BodyPartIndex.Value;
    public int GetCurrentEyeIndex() => EyeIndex.Value;
    public int GetCurrentGloveIndex() => GloveIndex.Value;
    public int GetCurrentHeadPartIndex() => HeadPartIndex.Value;
    public int GetCurrentMouthAndNoseIndex() => MouthAndNoseIndex.Value;
    public int GetCurrentTailIndex() => TailIndex.Value;

    // Métodos públicos para obtener cantidad máxima (útil para navegación)
    public int GetMaxBodyCount() => bodies.Length;
    public int GetMaxBodyPartCount() => bodyParts.Length;
    public int GetMaxEyeCount() => eyes.Length;
    public int GetMaxGloveCount() => gloves.Length;
    public int GetMaxHeadPartCount() => headParts.Length;
    public int GetMaxMouthAndNoseCount() => mouthAndNoses.Length;
    public int GetMaxTailCount() => tails.Length;

    public override void OnNetworkDespawn()
    {
        // Desuscribirse de todos los eventos
        IsReady.OnValueChanged -= OnReadyStateChanged;
        BodyIndex.OnValueChanged -= OnBodyChanged;
        BodyPartIndex.OnValueChanged -= OnBodyPartChanged;
        EyeIndex.OnValueChanged -= OnEyeChanged;
        GloveIndex.OnValueChanged -= OnGloveChanged;
        HeadPartIndex.OnValueChanged -= OnHeadPartChanged;
        MouthAndNoseIndex.OnValueChanged -= OnMouthAndNoseChanged;
        TailIndex.OnValueChanged -= OnTailChanged;
    }
}