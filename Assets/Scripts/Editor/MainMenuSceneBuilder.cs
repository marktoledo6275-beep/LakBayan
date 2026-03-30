#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
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

    [MenuItem("LAKBAYAN/Build Main Menu Scene")]
    public static void BuildMainMenuScene()
    {
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "MainMenu";

        CreateCamera();
        CreateSystemsRoot();
        CreateEventSystem();
        CreateCanvasAndMenu();

        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Main menu scene created at {ScenePath}");
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

        Button startButton = CreateHotspotButton("StartGameButton", hotspotRoot.transform);
        Button instructionsButton = CreateHotspotButton("InstructionsButton", hotspotRoot.transform);
        Button aboutButton = CreateHotspotButton("AboutButton", hotspotRoot.transform);
        Button exitButton = CreateHotspotButton("ExitButton", hotspotRoot.transform);

        MenuHotspotLayout hotspotLayout = artwork.AddComponent<MenuHotspotLayout>();
        hotspotLayout.AssignTargets(
            startButton.GetComponent<RectTransform>(),
            instructionsButton.GetComponent<RectTransform>(),
            aboutButton.GetComponent<RectTransform>(),
            exitButton.GetComponent<RectTransform>());

        GameObject instructionsPanel = CreateInfoPanel(
            canvasObject.transform,
            "InstructionsPanel",
            "Instructions",
            "Tap START GAME to enter the barangay.\n\nUse the joystick to move.\nTap the action button to talk to NPCs, accept quests, and begin cultural mini-games.\n\nComplete quests and quizzes to learn about pre-colonial Philippine life.");

        GameObject aboutPanel = CreateInfoPanel(
            canvasObject.transform,
            "AboutPanel",
            "About LAKBAYAN",
            "LAKBAYAN is a 2D top-down educational RPG for Grade 4 to Grade 6 learners.\n\nIt teaches pre-colonial Philippine history through dialogue, quests, quizzes, and traditional Filipino mini-games.");

        MainMenuController controller = canvasObject.AddComponent<MainMenuController>();

        SerializedObject serializedController = new SerializedObject(controller);
        serializedController.FindProperty("menuArtwork").objectReferenceValue = artworkRect;
        serializedController.FindProperty("instructionsPanel").objectReferenceValue = instructionsPanel;
        serializedController.FindProperty("aboutPanel").objectReferenceValue = aboutPanel;
        serializedController.ApplyModifiedPropertiesWithoutUndo();

        UnityEventTools.AddPersistentListener(startButton.onClick, controller.StartGame);
        UnityEventTools.AddPersistentListener(instructionsButton.onClick, controller.OpenInstructions);
        UnityEventTools.AddPersistentListener(aboutButton.onClick, controller.OpenAbout);
        UnityEventTools.AddPersistentListener(exitButton.onClick, controller.ExitGame);

        HookCloseButton(instructionsPanel, controller);
        HookCloseButton(aboutPanel, controller);

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
