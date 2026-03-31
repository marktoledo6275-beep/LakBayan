using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    private const string MenuGuideRootName = "MenuGuideRoot";

    [Header("Scene Flow")]
    [SerializeField] private string worldSceneName = "MainWorld";

    [Header("Panels")]
    [SerializeField] private GameObject noSavePanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Guide Character")]
    [SerializeField] private RectTransform menuArtwork;
    [SerializeField] private bool spawnGuideCharacter = true;
    [SerializeField] private bool guideOpensInstructions = true;
    [SerializeField] private string guideSpriteResourcePath = "UI/Menu/MenuGuide";
    [SerializeField] private string guideShadowResourcePath = "UI/Menu/MenuGuideShadow";
    [SerializeField] private Vector2 guideAnchor = new Vector2(0.14f, 0.06f);
    [SerializeField] private Vector2 guideSize = new Vector2(260f, 350f);
    [SerializeField] private Vector2 guideShadowSize = new Vector2(188f, 34f);
    [SerializeField] private Vector2 guideShadowOffset = new Vector2(0f, 18f);

    [Header("Optional Defaults")]
    [SerializeField] private bool hidePanelsOnStart = true;
    [SerializeField] private int targetFrameRate = 60;

    private void Awake()
    {
        Application.targetFrameRate = targetFrameRate;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        if (hidePanelsOnStart)
        {
            CloseAllPanels();
        }
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (IsAnyPanelOpen())
            {
                CloseAllPanels();
            }
            else
            {
                ExitGame();
            }
        }
    }

    public void StartGame()
    {
        StartNewGame();
    }

    public void StartNewGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetProgress();
        }

        LoadWorldScene();
    }

    public void LoadGame()
    {
        if (GameManager.Instance != null && !GameManager.Instance.HasSavedProgress)
        {
            OpenNoSavePanel();
            return;
        }

        LoadWorldScene();
    }

    public void OpenInstructions()
    {
        OpenSettings();
    }

    public void OpenAbout()
    {
        OpenSettings();
    }

    public void OpenNoSavePanel()
    {
        SetPanelState(noSavePanel, true);
        SetPanelState(settingsPanel, false);
    }

    public void OpenSettings()
    {
        SetPanelState(settingsPanel, true);
        SetPanelState(noSavePanel, false);
    }

    private void LoadWorldScene()
    {
        if (SceneController.Instance != null)
        {
            SceneController.Instance.LoadWorldScene(worldSceneName);
            return;
        }

        SceneManager.LoadScene(worldSceneName);
    }

    public void CloseAllPanels()
    {
        SetPanelState(noSavePanel, false);
        SetPanelState(settingsPanel, false);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private bool IsAnyPanelOpen()
    {
        return (noSavePanel != null && noSavePanel.activeSelf) ||
               (settingsPanel != null && settingsPanel.activeSelf);
    }

    private void SetPanelState(GameObject panel, bool isOpen)
    {
        if (panel != null)
        {
            panel.SetActive(isOpen);
        }
    }

    private void ResolveMenuArtwork()
    {
        if (menuArtwork != null)
        {
            return;
        }

        MenuHotspotLayout layout = GetComponentInChildren<MenuHotspotLayout>(true);

        if (layout != null)
        {
            menuArtwork = layout.transform as RectTransform;
        }
    }

    private void EnsureGuideCharacter()
    {
        if (!spawnGuideCharacter || menuArtwork == null)
        {
            return;
        }

        if (menuArtwork.Find(MenuGuideRootName) != null)
        {
            return;
        }

        Sprite guideSprite = Resources.Load<Sprite>(guideSpriteResourcePath);

        if (guideSprite == null)
        {
            return;
        }

        Sprite shadowSprite = Resources.Load<Sprite>(guideShadowResourcePath);

        RectTransform guideRoot = CreateGuideRoot();
        RectTransform shadowRect = CreateGuideShadow(guideRoot, shadowSprite);
        RectTransform spriteRect = CreateGuideSprite(guideRoot, guideSprite);

        MenuCharacterAnimator animator = guideRoot.gameObject.AddComponent<MenuCharacterAnimator>();
        animator.Configure(spriteRect, shadowRect, shadowRect.GetComponent<Image>());
    }

    private RectTransform CreateGuideRoot()
    {
        GameObject rootObject = new GameObject(MenuGuideRootName, typeof(RectTransform), typeof(Image));
        rootObject.transform.SetParent(menuArtwork, false);

        RectTransform rootRect = rootObject.GetComponent<RectTransform>();
        rootRect.anchorMin = guideAnchor;
        rootRect.anchorMax = guideAnchor;
        rootRect.pivot = new Vector2(0.5f, 0f);
        rootRect.sizeDelta = guideSize;
        rootRect.anchoredPosition = Vector2.zero;

        Image rootImage = rootObject.GetComponent<Image>();
        rootImage.color = new Color(1f, 1f, 1f, guideOpensInstructions ? 0.01f : 0f);
        rootImage.raycastTarget = guideOpensInstructions;

        if (guideOpensInstructions)
        {
            Button button = rootObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(1f, 1f, 1f, 0.01f);
            colors.highlightedColor = new Color(1f, 0.95f, 0.8f, 0.18f);
            colors.pressedColor = new Color(1f, 0.87f, 0.55f, 0.25f);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color(1f, 1f, 1f, 0.05f);
            button.colors = colors;

            if (settingsPanel != null)
            {
                button.onClick.AddListener(OpenSettings);
            }
            else
            {
                button.onClick.AddListener(StartNewGame);
            }
        }

        return rootRect;
    }

    private RectTransform CreateGuideShadow(RectTransform parent, Sprite shadowSprite)
    {
        GameObject shadowObject = new GameObject("Shadow", typeof(RectTransform), typeof(Image));
        shadowObject.transform.SetParent(parent, false);

        RectTransform shadowRect = shadowObject.GetComponent<RectTransform>();
        shadowRect.anchorMin = new Vector2(0.5f, 0f);
        shadowRect.anchorMax = new Vector2(0.5f, 0f);
        shadowRect.pivot = new Vector2(0.5f, 0.5f);
        shadowRect.anchoredPosition = guideShadowOffset;
        shadowRect.sizeDelta = guideShadowSize;

        Image shadowImage = shadowObject.GetComponent<Image>();
        shadowImage.sprite = shadowSprite;
        shadowImage.color = new Color(1f, 1f, 1f, 0.78f);
        shadowImage.raycastTarget = false;
        shadowImage.preserveAspect = true;
        return shadowRect;
    }

    private RectTransform CreateGuideSprite(RectTransform parent, Sprite guideSprite)
    {
        GameObject spriteObject = new GameObject("Sprite", typeof(RectTransform), typeof(Image), typeof(Outline));
        spriteObject.transform.SetParent(parent, false);

        RectTransform spriteRect = spriteObject.GetComponent<RectTransform>();
        spriteRect.anchorMin = new Vector2(0.5f, 0f);
        spriteRect.anchorMax = new Vector2(0.5f, 0f);
        spriteRect.pivot = new Vector2(0.5f, 0f);
        spriteRect.anchoredPosition = new Vector2(0f, 28f);
        spriteRect.sizeDelta = guideSize;

        Image spriteImage = spriteObject.GetComponent<Image>();
        spriteImage.sprite = guideSprite;
        spriteImage.preserveAspect = true;
        spriteImage.raycastTarget = false;

        Outline outline = spriteObject.GetComponent<Outline>();
        outline.effectColor = new Color(0.12f, 0.07f, 0.04f, 0.45f);
        outline.effectDistance = new Vector2(3f, -3f);
        return spriteRect;
    }
}

public class MenuAudioSettingsController : MonoBehaviour
{
    private const string MusicVolumeKey = "lakbayan_music_volume";
    private const string SfxVolumeKey = "lakbayan_sfx_volume";
    private const float MinimumLinearVolume = 0.0001f;
    private const float MinimumDbVolume = -80f;
    private const float MaximumDbVolume = 0f;

    [Header("UI")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider soundEffectsSlider;
    [SerializeField] private Text musicValueText;
    [SerializeField] private Text soundEffectsValueText;
    [SerializeField] private Text statusText;

    [Header("Audio")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string musicVolumeParameter = "MusicVolume";
    [SerializeField] private string soundEffectsVolumeParameter = "SfxVolume";
    [SerializeField] private float defaultMusicVolume = 0.8f;
    [SerializeField] private float defaultSoundEffectsVolume = 0.85f;

    private bool isInitialized;

    private void Awake()
    {
        ConfigureSlider(musicSlider);
        ConfigureSlider(soundEffectsSlider);
    }

    private void OnEnable()
    {
        InitializeIfNeeded();
        RefreshFromSavedValues();
    }

    private void OnDisable()
    {
        UnregisterListeners();
    }

    private void ConfigureSlider(Slider slider)
    {
        if (slider == null)
        {
            return;
        }

        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
    }

    private void InitializeIfNeeded()
    {
        if (isInitialized)
        {
            return;
        }

        RegisterListeners();
        isInitialized = true;
    }

    private void RegisterListeners()
    {
        UnregisterListeners();

        if (musicSlider != null)
        {
            musicSlider.onValueChanged.AddListener(HandleMusicChanged);
        }

        if (soundEffectsSlider != null)
        {
            soundEffectsSlider.onValueChanged.AddListener(HandleSoundEffectsChanged);
        }
    }

    private void UnregisterListeners()
    {
        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveListener(HandleMusicChanged);
        }

        if (soundEffectsSlider != null)
        {
            soundEffectsSlider.onValueChanged.RemoveListener(HandleSoundEffectsChanged);
        }
    }

    private void RefreshFromSavedValues()
    {
        float musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, defaultMusicVolume);
        float sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, defaultSoundEffectsVolume);

        if (musicSlider != null)
        {
            musicSlider.SetValueWithoutNotify(musicVolume);
        }

        if (soundEffectsSlider != null)
        {
            soundEffectsSlider.SetValueWithoutNotify(sfxVolume);
        }

        ApplyMusicVolume(musicVolume, saveValue: false);
        ApplySoundEffectsVolume(sfxVolume, saveValue: false);
        UpdateStatusText();
    }

    private void HandleMusicChanged(float value)
    {
        ApplyMusicVolume(value, saveValue: true);
    }

    private void HandleSoundEffectsChanged(float value)
    {
        ApplySoundEffectsVolume(value, saveValue: true);
    }

    private void ApplyMusicVolume(float value, bool saveValue)
    {
        ApplyVolume(musicVolumeParameter, value);
        UpdateValueText(musicValueText, value);

        if (saveValue)
        {
            PlayerPrefs.SetFloat(MusicVolumeKey, value);
            PlayerPrefs.Save();
        }
    }

    private void ApplySoundEffectsVolume(float value, bool saveValue)
    {
        ApplyVolume(soundEffectsVolumeParameter, value);
        UpdateValueText(soundEffectsValueText, value);

        if (saveValue)
        {
            PlayerPrefs.SetFloat(SfxVolumeKey, value);
            PlayerPrefs.Save();
        }
    }

    private void ApplyVolume(string parameterName, float linearVolume)
    {
        if (audioMixer == null || string.IsNullOrWhiteSpace(parameterName))
        {
            return;
        }

        float safeLinearVolume = Mathf.Clamp(linearVolume, 0f, 1f);
        float dbVolume = safeLinearVolume <= MinimumLinearVolume
            ? MinimumDbVolume
            : Mathf.Clamp(Mathf.Log10(safeLinearVolume) * 20f, MinimumDbVolume, MaximumDbVolume);

        audioMixer.SetFloat(parameterName, dbVolume);
    }

    private void UpdateValueText(Text target, float value)
    {
        if (target == null)
        {
            return;
        }

        target.text = $"{Mathf.RoundToInt(Mathf.Clamp01(value) * 100f)}%";
    }

    private void UpdateStatusText()
    {
        if (statusText == null)
        {
            return;
        }

        statusText.text = audioMixer == null
            ? "Assign an AudioMixer in the inspector to route Music and SFX groups."
            : "Audio updates live and is saved automatically.";
    }
}
