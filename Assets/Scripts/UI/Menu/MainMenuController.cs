using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    private const string MenuGuideRootName = "MenuGuideRoot";

    [Header("Scene Flow")]
    [SerializeField] private string worldSceneName = "MainWorld";

    [Header("Panels")]
    [SerializeField] private GameObject instructionsPanel;
    [SerializeField] private GameObject aboutPanel;

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
        if (SceneController.Instance != null)
        {
            SceneController.Instance.LoadWorldScene(worldSceneName);
            return;
        }

        SceneManager.LoadScene(worldSceneName);
    }

    public void OpenInstructions()
    {
        SetPanelState(instructionsPanel, true);
        SetPanelState(aboutPanel, false);
    }

    public void OpenAbout()
    {
        SetPanelState(aboutPanel, true);
        SetPanelState(instructionsPanel, false);
    }

    public void CloseAllPanels()
    {
        SetPanelState(instructionsPanel, false);
        SetPanelState(aboutPanel, false);
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
        return (instructionsPanel != null && instructionsPanel.activeSelf) ||
               (aboutPanel != null && aboutPanel.activeSelf);
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

            if (instructionsPanel != null)
            {
                button.onClick.AddListener(OpenInstructions);
            }
            else
            {
                button.onClick.AddListener(StartGame);
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
