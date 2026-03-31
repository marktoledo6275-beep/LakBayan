#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class MainMenuSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/MainMenu.unity";
    private const string MenuSpritePath = "Assets/Art/UI/Menu/Menu.png";
    private const string InputActionsAssetPath = "Assets/InputSystem_Actions.inputactions";
    private const string AudioMixerAssetPath = "Assets/Audio/LakbayanAudioMixer.mixer";

    [MenuItem("LAKBAYAN/Build Main Menu Scene")]
    public static void BuildMainMenuScene()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "MainMenu";

        CreateCamera();
        CreateSystemsRoot();
        CreateEventSystem();
        CreateCanvasAndMenu();

        EditorSceneManager.SaveScene(scene, ScenePath);
        AddSceneToBuildSettings(ScenePath);
        AddSceneToBuildSettings("Assets/Scenes/MainWorld.unity");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Main menu scene created at {ScenePath}");
    }

    private static void AddSceneToBuildSettings(string scenePath)
    {
        if (!System.IO.File.Exists(scenePath))
        {
            return;
        }

        EditorBuildSettingsScene[] existingScenes = EditorBuildSettings.scenes;

        foreach (EditorBuildSettingsScene existingScene in existingScenes)
        {
            if (existingScene.path == scenePath)
            {
                return;
            }
        }

        var updatedScenes = new EditorBuildSettingsScene[existingScenes.Length + 1];

        for (int i = 0; i < existingScenes.Length; i++)
        {
            updatedScenes[i] = existingScenes[i];
        }

        updatedScenes[existingScenes.Length] = new EditorBuildSettingsScene(scenePath, true);
        EditorBuildSettings.scenes = updatedScenes;
    }

    private static void CreateCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.06f, 0.08f, 0.12f);
        camera.tag = "MainCamera";

        cameraObject.AddComponent<AudioListener>();
    }

    private static void CreateSystemsRoot()
    {
        GameObject systems = new GameObject("Systems");
        systems.AddComponent<GameManager>();
        systems.AddComponent<SceneController>();
    }

    private static void CreateEventSystem()
    {
        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        InputSystemUIInputModule inputModule = eventSystemObject.AddComponent<InputSystemUIInputModule>();
        InputActionAsset inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsAssetPath);

        if (inputActions != null)
        {
            inputModule.actionsAsset = inputActions;
        }
        else
        {
            Debug.LogWarning($"MainMenuSceneBuilder could not find UI input actions at '{InputActionsAssetPath}'. UI buttons may not receive input.");
        }
    }

    private static void CreateCanvasAndMenu()
    {
        GameObject canvasObject = new GameObject("MainMenuCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
        canvasRect.anchorMin = Vector2.zero;
        canvasRect.anchorMax = Vector2.one;
        canvasRect.offsetMin = Vector2.zero;
        canvasRect.offsetMax = Vector2.zero;

        GameObject dimmer = CreateUIObject("BackgroundDimmer", canvasObject.transform);
        Image dimmerImage = dimmer.AddComponent<Image>();
        dimmerImage.color = new Color(0f, 0f, 0f, 0.2f);
        StretchToParent(dimmer.GetComponent<RectTransform>());

        GameObject artwork = CreateUIObject("MenuArtwork", canvasObject.transform);
        RectTransform artworkRect = artwork.GetComponent<RectTransform>();
        artworkRect.anchorMin = new Vector2(0.5f, 0.5f);
        artworkRect.anchorMax = new Vector2(0.5f, 0.5f);
        artworkRect.pivot = new Vector2(0.5f, 0.5f);
        artworkRect.sizeDelta = new Vector2(1536f, 1024f);

        AspectRatioFitter aspectFitter = artwork.AddComponent<AspectRatioFitter>();
        aspectFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        aspectFitter.aspectRatio = 1.5f;

        Image artworkImage = artwork.AddComponent<Image>();
        artworkImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(MenuSpritePath);
        artworkImage.preserveAspect = true;

        GameObject hotspotRoot = CreateUIObject("Hotspots", artwork.transform);
        StretchToParent(hotspotRoot.GetComponent<RectTransform>());

        Button newGameButton = CreateHotspotButton("NewGameButton", hotspotRoot.transform);
        Button loadGameButton = CreateHotspotButton("LoadGameButton", hotspotRoot.transform);
        Button settingsButton = CreateHotspotButton("SettingsButton", hotspotRoot.transform);
        MenuHotspotLayout hotspotLayout = artwork.AddComponent<MenuHotspotLayout>();
        hotspotLayout.AssignTargets(
            newGameButton.GetComponent<RectTransform>(),
            loadGameButton.GetComponent<RectTransform>(),
            settingsButton.GetComponent<RectTransform>());

        GameObject noSavePanel = CreateInfoPanel(
            canvasObject.transform,
            "NoSavePanel",
            "No Save Data",
            "There is no saved journey yet.\n\nTap NEW GAME to begin your adventure in the barangay, then LOAD GAME will continue from your saved progress.");

        GameObject settingsPanel = CreateSettingsPanel(canvasObject.transform);

        MainMenuController controller = canvasObject.AddComponent<MainMenuController>();

        SerializedObject serializedController = new SerializedObject(controller);
        serializedController.FindProperty("menuArtwork").objectReferenceValue = artworkRect;
        serializedController.FindProperty("noSavePanel").objectReferenceValue = noSavePanel;
        serializedController.FindProperty("settingsPanel").objectReferenceValue = settingsPanel;
        serializedController.FindProperty("spawnGuideCharacter").boolValue = false;
        serializedController.ApplyModifiedPropertiesWithoutUndo();

        UnityEventTools.AddPersistentListener(newGameButton.onClick, controller.StartNewGame);
        UnityEventTools.AddPersistentListener(loadGameButton.onClick, controller.LoadGame);
        UnityEventTools.AddPersistentListener(settingsButton.onClick, controller.OpenSettings);

        HookCloseButton(noSavePanel, controller);
        HookCloseButton(settingsPanel, controller);

        controller.CloseAllPanels();
        EditorUtility.SetDirty(canvasObject);
    }

    private static GameObject CreateInfoPanel(Transform parent, string name, string title, string body)
    {
        GameObject panel = CreateUIObject(name, parent);
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.12f, 0.08f, 0.03f, 0.92f);
        panel.SetActive(false);

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.12f, 0.14f);
        panelRect.anchorMax = new Vector2(0.88f, 0.86f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Outline outline = panel.AddComponent<Outline>();
        outline.effectColor = new Color(0.95f, 0.77f, 0.37f, 0.8f);
        outline.effectDistance = new Vector2(4f, -4f);

        GameObject titleObject = CreateText(panel.transform, "Title", title, 46, TextAnchor.UpperCenter);
        RectTransform titleRect = titleObject.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.08f, 0.78f);
        titleRect.anchorMax = new Vector2(0.92f, 0.94f);

        GameObject bodyObject = CreateText(panel.transform, "Body", body, 28, TextAnchor.UpperLeft);
        RectTransform bodyRect = bodyObject.GetComponent<RectTransform>();
        bodyRect.anchorMin = new Vector2(0.08f, 0.20f);
        bodyRect.anchorMax = new Vector2(0.92f, 0.74f);

        GameObject closeButtonObject = CreateUIObject("CloseButton", panel.transform);
        Image closeImage = closeButtonObject.AddComponent<Image>();
        closeImage.color = new Color(0.45f, 0.24f, 0.08f, 0.95f);
        Button closeButton = closeButtonObject.AddComponent<Button>();
        ConfigureButtonColors(closeButton, false);

        RectTransform closeRect = closeButtonObject.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(0.35f, 0.05f);
        closeRect.anchorMax = new Vector2(0.65f, 0.15f);
        closeRect.offsetMin = Vector2.zero;
        closeRect.offsetMax = Vector2.zero;

        CreateText(closeButtonObject.transform, "Label", "CLOSE", 28, TextAnchor.MiddleCenter);

        MenuPanelContent content = panel.AddComponent<MenuPanelContent>();
        SerializedObject serializedContent = new SerializedObject(content);
        serializedContent.FindProperty("titleText").objectReferenceValue = titleObject.GetComponent<Text>();
        serializedContent.FindProperty("bodyText").objectReferenceValue = bodyObject.GetComponent<Text>();
        serializedContent.ApplyModifiedPropertiesWithoutUndo();
        content.SetContent(title, body);

        return panel;
    }

    private static GameObject CreateSettingsPanel(Transform parent)
    {
        GameObject panel = CreateUIObject("SettingsPanel", parent);
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.12f, 0.08f, 0.03f, 0.94f);
        panel.SetActive(false);

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.12f, 0.12f);
        panelRect.anchorMax = new Vector2(0.88f, 0.88f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Outline outline = panel.AddComponent<Outline>();
        outline.effectColor = new Color(0.95f, 0.77f, 0.37f, 0.8f);
        outline.effectDistance = new Vector2(4f, -4f);

        GameObject titleObject = CreateText(panel.transform, "Title", "SETTINGS", 46, TextAnchor.UpperCenter);
        RectTransform titleRect = titleObject.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.08f, 0.82f);
        titleRect.anchorMax = new Vector2(0.92f, 0.95f);

        GameObject subtitleObject = CreateText(panel.transform, "Subtitle", "Adjust audio for music and sound effects. Changes apply live and save automatically.", 24, TextAnchor.MiddleCenter);
        RectTransform subtitleRect = subtitleObject.GetComponent<RectTransform>();
        subtitleRect.anchorMin = new Vector2(0.08f, 0.70f);
        subtitleRect.anchorMax = new Vector2(0.92f, 0.81f);

        Slider musicSlider = CreateSliderRow(panel.transform, "MusicRow", "MUSIC VOLUME", 0.50f, out Text musicValueText);
        Slider sfxSlider = CreateSliderRow(panel.transform, "SoundEffectsRow", "SFX VOLUME", 0.30f, out Text sfxValueText);

        GameObject statusObject = CreateText(panel.transform, "StatusText", "Assign an AudioMixer in the inspector to route Music and SFX groups.", 22, TextAnchor.MiddleCenter);
        RectTransform statusRect = statusObject.GetComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0.08f, 0.14f);
        statusRect.anchorMax = new Vector2(0.92f, 0.24f);

        GameObject closeButtonObject = CreateUIObject("CloseButton", panel.transform);
        Image closeImage = closeButtonObject.AddComponent<Image>();
        closeImage.color = new Color(0.45f, 0.24f, 0.08f, 0.95f);
        Button closeButton = closeButtonObject.AddComponent<Button>();
        ConfigureButtonColors(closeButton, false);

        RectTransform closeRect = closeButtonObject.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(0.35f, 0.03f);
        closeRect.anchorMax = new Vector2(0.65f, 0.11f);
        closeRect.offsetMin = Vector2.zero;
        closeRect.offsetMax = Vector2.zero;

        CreateText(closeButtonObject.transform, "Label", "CLOSE", 28, TextAnchor.MiddleCenter);

        MenuAudioSettingsController audioSettings = panel.AddComponent<MenuAudioSettingsController>();
        SerializedObject serializedSettings = new SerializedObject(audioSettings);
        serializedSettings.FindProperty("musicSlider").objectReferenceValue = musicSlider;
        serializedSettings.FindProperty("soundEffectsSlider").objectReferenceValue = sfxSlider;
        serializedSettings.FindProperty("musicValueText").objectReferenceValue = musicValueText;
        serializedSettings.FindProperty("soundEffectsValueText").objectReferenceValue = sfxValueText;
        serializedSettings.FindProperty("statusText").objectReferenceValue = statusObject.GetComponent<Text>();

        AudioMixer mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(AudioMixerAssetPath);
        if (mixer != null)
        {
            serializedSettings.FindProperty("audioMixer").objectReferenceValue = mixer;
        }

        serializedSettings.ApplyModifiedPropertiesWithoutUndo();
        return panel;
    }

    private static Slider CreateSliderRow(Transform parent, string name, string label, float anchorY, out Text valueText)
    {
        GameObject row = CreateUIObject(name, parent);
        RectTransform rowRect = row.GetComponent<RectTransform>();
        rowRect.anchorMin = new Vector2(0.08f, anchorY);
        rowRect.anchorMax = new Vector2(0.92f, anchorY + 0.12f);
        rowRect.offsetMin = Vector2.zero;
        rowRect.offsetMax = Vector2.zero;

        GameObject labelObject = CreateText(row.transform, "Label", label, 28, TextAnchor.MiddleLeft);
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 0.10f);
        labelRect.anchorMax = new Vector2(0.28f, 0.90f);

        GameObject sliderObject = CreateUIObject("Slider", row.transform);
        RectTransform sliderRect = sliderObject.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.32f, 0.22f);
        sliderRect.anchorMax = new Vector2(0.82f, 0.78f);
        sliderRect.offsetMin = Vector2.zero;
        sliderRect.offsetMax = Vector2.zero;

        Slider slider = sliderObject.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0.8f;

        GameObject background = CreateUIObject("Background", sliderObject.transform);
        Image backgroundImage = background.AddComponent<Image>();
        backgroundImage.color = new Color(0.20f, 0.14f, 0.08f, 1f);
        RectTransform backgroundRect = background.GetComponent<RectTransform>();
        backgroundRect.anchorMin = new Vector2(0f, 0.35f);
        backgroundRect.anchorMax = new Vector2(1f, 0.65f);
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;

        GameObject fillArea = CreateUIObject("Fill Area", sliderObject.transform);
        RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0f, 0.20f);
        fillAreaRect.anchorMax = new Vector2(1f, 0.80f);
        fillAreaRect.offsetMin = new Vector2(14f, 0f);
        fillAreaRect.offsetMax = new Vector2(-14f, 0f);

        GameObject fill = CreateUIObject("Fill", fillArea.transform);
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = new Color(0.93f, 0.76f, 0.31f, 1f);
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0f, 0.15f);
        fillRect.anchorMax = new Vector2(1f, 0.85f);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        GameObject handleArea = CreateUIObject("Handle Slide Area", sliderObject.transform);
        RectTransform handleAreaRect = handleArea.GetComponent<RectTransform>();
        handleAreaRect.anchorMin = new Vector2(0f, 0f);
        handleAreaRect.anchorMax = new Vector2(1f, 1f);
        handleAreaRect.offsetMin = new Vector2(14f, 0f);
        handleAreaRect.offsetMax = new Vector2(-14f, 0f);

        GameObject handle = CreateUIObject("Handle", handleArea.transform);
        Image handleImage = handle.AddComponent<Image>();
        handleImage.color = new Color(1f, 0.95f, 0.84f, 1f);
        RectTransform handleRect = handle.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(24f, 42f);

        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.targetGraphic = handleImage;
        slider.direction = Slider.Direction.LeftToRight;

        GameObject valueObject = CreateText(row.transform, "Value", "80%", 28, TextAnchor.MiddleRight);
        valueText = valueObject.GetComponent<Text>();
        RectTransform valueRect = valueObject.GetComponent<RectTransform>();
        valueRect.anchorMin = new Vector2(0.84f, 0.10f);
        valueRect.anchorMax = new Vector2(1f, 0.90f);

        return slider;
    }

    private static void HookCloseButton(GameObject panel, MainMenuController controller)
    {
        Button closeButton = panel.GetComponentInChildren<Button>();

        if (closeButton != null)
        {
            UnityEventTools.AddPersistentListener(closeButton.onClick, controller.CloseAllPanels);
        }
    }

    private static Button CreateHotspotButton(string name, Transform parent)
    {
        GameObject buttonObject = CreateUIObject(name, parent);
        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.01f);

        Button button = buttonObject.AddComponent<Button>();
        ConfigureButtonColors(button, true);
        return button;
    }

    private static void ConfigureButtonColors(Button button, bool transparent)
    {
        ColorBlock colors = button.colors;
        colors.normalColor = transparent ? new Color(1f, 1f, 1f, 0.01f) : new Color(0.55f, 0.31f, 0.10f, 0.96f);
        colors.highlightedColor = new Color(1f, 0.88f, 0.50f, transparent ? 0.18f : 1f);
        colors.pressedColor = new Color(0.94f, 0.73f, 0.26f, transparent ? 0.28f : 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.35f, 0.35f, 0.35f, 0.5f);
        button.colors = colors;
        button.transition = Selectable.Transition.ColorTint;
    }

    private static GameObject CreateText(Transform parent, string name, string value, int fontSize, TextAnchor alignment)
    {
        GameObject textObject = CreateUIObject(name, parent);
        Text text = textObject.AddComponent<Text>();
        text.text = value;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = new Color(1f, 0.94f, 0.79f);
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return textObject;
    }

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform));
        gameObject.transform.SetParent(parent, false);
        return gameObject;
    }

    private static void StretchToParent(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }
}
#endif
