#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class MainWorldSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/MainWorld.unity";
    private const string SquareTexturePath = "Assets/Art/Generated/WhiteSquare.png";
    private const string InputActionsAssetPath = "Assets/InputSystem_Actions.inputactions";
    private const string BackdropSpritePath = "Assets/Resources/World/Environment/VillageBackdrop.png";
    private const string PlayerSpritePath = "Assets/Resources/World/Characters/PlayerGuide.png";
    private const string ElderSpritePath = "Assets/Resources/World/Characters/VillageElder.png";
    private const string CharacterShadowSpritePath = "Assets/Resources/World/Characters/CharacterShadow.png";
    private const string ShrineSpritePath = "Assets/Resources/World/Environment/RiverShrine.png";
    private const string JoystickBaseSpritePath = "Assets/Resources/UI/Gameplay/JoystickBase.png";
    private const string JoystickKnobSpritePath = "Assets/Resources/UI/Gameplay/JoystickKnob.png";
    private const string ActionButtonSpritePath = "Assets/Resources/UI/Gameplay/ActionButton.png";
    private const string WoodPanelWideSpritePath = "Assets/Resources/UI/Gameplay/WoodPanelWide.png";
    private const string WoodPanelSmallSpritePath = "Assets/Resources/UI/Gameplay/WoodPanelSmall.png";
    private const string WoodPanelDarkSpritePath = "Assets/Resources/UI/Gameplay/WoodPanelDark.png";
    private const string WoodButtonSpritePath = "Assets/Resources/UI/Gameplay/WoodButton.png";
    private const string ElderDefaultDialoguePath = "Assets/ScriptableObjects/Dialogue/Elder_Default.asset";
    private const string ElderOfferDialoguePath = "Assets/ScriptableObjects/Dialogue/Elder_Offer.asset";
    private const string ElderInProgressDialoguePath = "Assets/ScriptableObjects/Dialogue/Elder_InProgress.asset";
    private const string ElderReadyDialoguePath = "Assets/ScriptableObjects/Dialogue/Elder_Ready.asset";
    private const string ElderCompletedDialoguePath = "Assets/ScriptableObjects/Dialogue/Elder_Completed.asset";
    private const string ElderQuestPath = "Assets/ScriptableObjects/Quests/MeetTheElderQuest.asset";

    [MenuItem("LAKBAYAN/Build Starter Main World Scene")]
    public static void BuildStarterMainWorldScene()
    {
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

        Sprite squareSprite = EnsureSquareSprite();
        Sprite backdropSprite = LoadSpriteAsset(BackdropSpritePath, squareSprite);
        Sprite playerSprite = LoadSpriteAsset(PlayerSpritePath, squareSprite);
        Sprite elderSprite = LoadSpriteAsset(ElderSpritePath, squareSprite);
        Sprite characterShadowSprite = LoadSpriteAsset(CharacterShadowSpritePath, squareSprite);
        Sprite shrineSprite = LoadSpriteAsset(ShrineSpritePath, squareSprite);
        Sprite joystickBaseSprite = LoadSpriteAsset(JoystickBaseSpritePath, squareSprite);
        Sprite joystickKnobSprite = LoadSpriteAsset(JoystickKnobSpritePath, squareSprite);
        Sprite actionButtonSprite = LoadSpriteAsset(ActionButtonSpritePath, squareSprite);
        Sprite woodPanelWideSprite = LoadSpriteAsset(WoodPanelWideSpritePath, squareSprite);
        Sprite woodPanelSmallSprite = LoadSpriteAsset(WoodPanelSmallSpritePath, squareSprite);
        Sprite woodPanelDarkSprite = LoadSpriteAsset(WoodPanelDarkSpritePath, squareSprite);
        Sprite woodButtonSprite = LoadSpriteAsset(WoodButtonSpritePath, squareSprite);
        DialogueData defaultDialogue = CreateDialogueAsset(
            ElderDefaultDialoguePath,
            "Village Elder",
            new[]
            {
                ("Welcome, young Lakbayan. Our barangay thrives because everyone helps each other.", true),
                ("If you are ready, I can guide you through your first lesson.", false)
            });

        DialogueData offerDialogue = CreateDialogueAsset(
            ElderOfferDialoguePath,
            "Village Elder",
            new[]
            {
                ("Before long ago, each barangay had places of worship and gathering near rivers and forests.", true),
                ("Walk to the shrine by the river, then return to me so I know you understand our village.", false)
            });

        DialogueData inProgressDialogue = CreateDialogueAsset(
            ElderInProgressDialoguePath,
            "Village Elder",
            new[]
            {
                ("The shrine stands near the riverbank. Observe it carefully, then come back.", false)
            });

        DialogueData readyDialogue = CreateDialogueAsset(
            ElderReadyDialoguePath,
            "Village Elder",
            new[]
            {
                ("Well done. You found the shrine and learned an important part of barangay life.", false),
                ("Our people honor community, nature, and leadership together.", true)
            });

        DialogueData completedDialogue = CreateDialogueAsset(
            ElderCompletedDialoguePath,
            "Village Elder",
            new[]
            {
                ("Continue exploring. More stories and games await in LAKBAYAN.", false)
            });

        QuestData elderQuest = CreateQuestAsset();

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "MainWorld";

        Camera worldCamera = CreateCamera();
        GameObject systems = CreateSystemsRoot();
        EventSystem eventSystem = CreateEventSystem();
        GameObject worldRoot = new GameObject("World");

        CreateBackdrop(backdropSprite, worldRoot.transform);
        CreateForegroundDecorations(squareSprite, worldRoot.transform);

        PlayerController player = CreatePlayer(squareSprite, playerSprite, characterShadowSprite, worldRoot.transform);
        NPCInteraction elder = CreateVillageElder(squareSprite, elderSprite, characterShadowSprite, worldRoot.transform, defaultDialogue, offerDialogue, inProgressDialogue, readyDialogue, completedDialogue, elderQuest);
        CreateShrine(squareSprite, shrineSprite, worldRoot.transform);

        CameraFollow cameraFollow = worldCamera.gameObject.AddComponent<CameraFollow>();
        cameraFollow.SetTarget(player.transform);

        Canvas canvas = CreateCanvas();
        CreateHUD(canvas.transform, woodPanelWideSprite, woodPanelSmallSprite, woodPanelDarkSprite);
        CreateDialogueUI(canvas.transform, player, woodPanelDarkSprite, woodButtonSprite);
        CreateMobileControls(canvas.transform, player, joystickBaseSprite, joystickKnobSprite, actionButtonSprite);

        Selection.activeGameObject = player.gameObject;

        EditorSceneManager.SaveScene(scene, ScenePath);
        EnsureSceneInBuildSettings("Assets/Scenes/MainMenu.unity");
        EnsureSceneInBuildSettings(ScenePath);

        EditorUtility.SetDirty(systems);
        EditorUtility.SetDirty(eventSystem.gameObject);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Starter MainWorld scene created at {ScenePath}");
    }

    private static Camera CreateCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 5.2f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.53f, 0.79f, 0.93f);
        camera.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, -0.35f, -10f);
        cameraObject.AddComponent<AudioListener>();
        return camera;
    }

    private static GameObject CreateSystemsRoot()
    {
        GameObject systems = new GameObject("WorldSystems");
        systems.AddComponent<GameManager>();
        systems.AddComponent<SceneController>();
        systems.AddComponent<QuestSystem>();
        return systems;
    }

    private static EventSystem CreateEventSystem()
    {
        GameObject eventSystemObject = new GameObject("EventSystem");
        EventSystem eventSystem = eventSystemObject.AddComponent<EventSystem>();
        InputSystemUIInputModule inputModule = eventSystemObject.AddComponent<InputSystemUIInputModule>();
        InputActionAsset inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsAssetPath);

        if (inputActions != null)
        {
            inputModule.actionsAsset = inputActions;
        }
        else
        {
            Debug.LogWarning($"MainWorldSceneBuilder could not find UI input actions at '{InputActionsAssetPath}'. UI buttons may not receive input.");
        }

        return eventSystem;
    }

    private static Canvas CreateCanvas()
    {
        GameObject canvasObject = new GameObject("GameplayCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    private static void CreateBackdrop(Sprite backdropSprite, Transform parent)
    {
        GameObject backdrop = new GameObject("VillageBackdrop");
        backdrop.transform.SetParent(parent, false);
        backdrop.transform.position = new Vector3(0f, 0.05f, 0f);
        backdrop.transform.localScale = new Vector3(20.2f, 13.55f, 1f);

        SpriteRenderer renderer = backdrop.AddComponent<SpriteRenderer>();
        renderer.sprite = backdropSprite;
        renderer.sortingOrder = -50;
    }

    private static void CreateForegroundDecorations(Sprite squareSprite, Transform parent)
    {
        CreateWorldSprite("LeftRockCluster", squareSprite, new Color(0.49f, 0.46f, 0.37f), new Vector3(-7.8f, -3.65f, 0f), new Vector3(1.55f, 0.92f, 1f), parent, -3);
        CreateWorldSprite("LeftRockHighlight", squareSprite, new Color(0.72f, 0.67f, 0.56f), new Vector3(-7.95f, -3.58f, 0f), new Vector3(0.72f, 0.36f, 1f), parent, -2);
        CreateWorldSprite("RightRockCluster", squareSprite, new Color(0.49f, 0.46f, 0.37f), new Vector3(7.85f, -3.72f, 0f), new Vector3(1.6f, 0.92f, 1f), parent, -3);
        CreateWorldSprite("RightRockHighlight", squareSprite, new Color(0.72f, 0.67f, 0.56f), new Vector3(8.12f, -3.58f, 0f), new Vector3(0.72f, 0.36f, 1f), parent, -2);

        CreateWorldSprite("GrassPatchLeft", squareSprite, new Color(0.41f, 0.63f, 0.27f), new Vector3(-3.85f, -2.85f, 0f), new Vector3(0.85f, 0.18f, 1f), parent, -1);
        CreateWorldSprite("GrassPatchCenter", squareSprite, new Color(0.41f, 0.63f, 0.27f), new Vector3(0.45f, -1.78f, 0f), new Vector3(0.95f, 0.22f, 1f), parent, -1);
        CreateWorldSprite("GrassPatchRight", squareSprite, new Color(0.41f, 0.63f, 0.27f), new Vector3(4.15f, -2.65f, 0f), new Vector3(0.9f, 0.18f, 1f), parent, -1);
    }

    private static PlayerController CreatePlayer(Sprite fallbackSprite, Sprite playerSprite, Sprite shadowSprite, Transform parent)
    {
        GameObject playerObject = new GameObject("Player");
        playerObject.transform.SetParent(parent, false);
        playerObject.transform.position = new Vector3(0f, -3.05f, 0f);

        SpriteRenderer shadowRenderer = CreateCharacterShadow(playerObject.transform, shadowSprite, 11, new Vector3(0f, -0.58f, 0f), new Vector3(0.9f, 0.32f, 1f));
        SpriteRenderer spriteRenderer = CreateCharacterSprite(playerObject.transform, playerSprite, fallbackSprite, 20, new Vector3(0f, -0.54f, 0f), new Vector3(1.28f, 1.28f, 1f));

        Rigidbody2D rigidbody2D = playerObject.AddComponent<Rigidbody2D>();
        rigidbody2D.gravityScale = 0f;
        rigidbody2D.freezeRotation = true;
        rigidbody2D.interpolation = RigidbodyInterpolation2D.Interpolate;

        CapsuleCollider2D collider2D = playerObject.AddComponent<CapsuleCollider2D>();
        collider2D.size = new Vector2(0.72f, 0.56f);
        collider2D.offset = new Vector2(0f, -0.6f);

        Transform interactionOrigin = new GameObject("InteractionOrigin").transform;
        interactionOrigin.SetParent(playerObject.transform, false);
        interactionOrigin.localPosition = new Vector3(0f, -0.55f, 0f);

        PlayerController controller = playerObject.AddComponent<PlayerController>();
        AssignPrivateField(controller, "spriteRenderer", spriteRenderer);
        AssignPrivateField(controller, "interactionOrigin", interactionOrigin);

        shadowRenderer.color = new Color(1f, 1f, 1f, 0.74f);
        return controller;
    }

    private static NPCInteraction CreateVillageElder(Sprite fallbackSprite, Sprite elderSprite, Sprite shadowSprite, Transform parent, DialogueData defaultDialogue, DialogueData offerDialogue, DialogueData inProgressDialogue, DialogueData readyDialogue, DialogueData completedDialogue, QuestData questData)
    {
        GameObject elderObject = new GameObject("Village Elder");
        elderObject.transform.SetParent(parent, false);
        elderObject.transform.position = new Vector3(-1.75f, -0.85f, 0f);

        CreateCharacterShadow(elderObject.transform, shadowSprite, 10, new Vector3(0f, -0.54f, 0f), new Vector3(0.84f, 0.28f, 1f));
        SpriteRenderer spriteRenderer = CreateCharacterSprite(elderObject.transform, elderSprite, fallbackSprite, 18, new Vector3(0f, -0.5f, 0f), new Vector3(1.18f, 1.18f, 1f));

        BoxCollider2D collider2D = elderObject.AddComponent<BoxCollider2D>();
        collider2D.size = new Vector2(0.7f, 0.52f);
        collider2D.offset = new Vector2(0f, -0.58f);

        NPCInteraction interaction = elderObject.AddComponent<NPCInteraction>();
        AssignPrivateField(interaction, "npcId", "elder");
        AssignPrivateField(interaction, "displayName", "Village Elder");
        AssignPrivateField(interaction, "defaultDialogue", defaultDialogue);
        AssignPrivateField(interaction, "questOfferDialogue", offerDialogue);
        AssignPrivateField(interaction, "questInProgressDialogue", inProgressDialogue);
        AssignPrivateField(interaction, "questReadyToTurnInDialogue", readyDialogue);
        AssignPrivateField(interaction, "questCompletedDialogue", completedDialogue);
        AssignPrivateField(interaction, "questToGive", questData);
        return interaction;
    }

    private static void CreateShrine(Sprite fallbackSprite, Sprite shrineSprite, Transform parent)
    {
        GameObject shrine = new GameObject("River Shrine");
        shrine.transform.SetParent(parent, false);
        shrine.transform.position = new Vector3(5.15f, 0.52f, 0f);

        SpriteRenderer spriteRenderer = shrine.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = shrineSprite != null ? shrineSprite : fallbackSprite;
        spriteRenderer.sortingOrder = 8;

        if (shrineSprite == null || shrineSprite == fallbackSprite)
        {
            spriteRenderer.color = new Color(0.84f, 0.72f, 0.30f);
            shrine.transform.localScale = new Vector3(1.15f, 1.15f, 1f);
        }
        else
        {
            shrine.transform.localScale = new Vector3(1.15f, 1.15f, 1f);
        }

        BoxCollider2D collider2D = shrine.AddComponent<BoxCollider2D>();
        collider2D.isTrigger = true;
        collider2D.size = new Vector2(0.82f, 0.9f);
        collider2D.offset = new Vector2(0f, -0.12f);

        QuestObjectiveTrigger questTrigger = shrine.AddComponent<QuestObjectiveTrigger>();
        AssignPrivateField(questTrigger, "targetId", "river_shrine");
        AssignPrivateField(questTrigger, "objectiveType", QuestObjectiveType.ReachLocation);
        AssignPrivateField(questTrigger, "progressAmount", 1);
    }

    private static void CreateHUD(Transform canvasTransform, Sprite widePanelSprite, Sprite smallPanelSprite, Sprite darkPanelSprite)
    {
        GameObject hudRoot = CreateUIObject("HUD", canvasTransform);
        RectTransform hudRect = hudRoot.GetComponent<RectTransform>();
        StretchToParent(hudRect);

        GameObject questPanel = CreateStyledPanel("QuestPanel", hudRoot.transform, widePanelSprite, new Color(1f, 1f, 1f, 0.97f), new Color(0f, 0f, 0f, 0.22f));
        SetAnchors(questPanel.GetComponent<RectTransform>(), new Vector2(0.025f, 0.84f), new Vector2(0.49f, 0.975f));

        TextMeshProUGUI questLabel = CreateText(questPanel.transform, "QuestLabel", "CURRENT JOURNEY", 20, TextAnchor.UpperLeft, FontStyle.Bold, new Color(1f, 0.87f, 0.47f, 1f));
        questLabel.color = new Color(0.98f, 0.82f, 0.45f, 1f);
        SetAnchors(questLabel.rectTransform, new Vector2(0.04f, 0.62f), new Vector2(0.45f, 0.9f));

        TextMeshProUGUI questText = CreateText(questPanel.transform, "CurrentQuestText", "Talk to the Village Elder to begin your first lesson.", 21, TextAnchor.UpperLeft, FontStyle.Bold, new Color(1f, 0.96f, 0.86f, 1f));
        SetAnchors(questText.rectTransform, new Vector2(0.04f, 0.12f), new Vector2(0.96f, 0.7f));

        GameObject scorePanel = CreateStyledPanel("ScorePanel", hudRoot.transform, smallPanelSprite, new Color(1f, 1f, 1f, 0.97f), new Color(0f, 0f, 0f, 0.22f));
        SetAnchors(scorePanel.GetComponent<RectTransform>(), new Vector2(0.79f, 0.87f), new Vector2(0.975f, 0.975f));

        TextMeshProUGUI scoreLabel = CreateText(scorePanel.transform, "ScoreLabel", "HISTORY SCORE", 17, TextAnchor.UpperCenter, FontStyle.Bold, new Color(1f, 0.87f, 0.47f, 1f));
        scoreLabel.color = new Color(0.98f, 0.82f, 0.45f, 1f);
        SetAnchors(scoreLabel.rectTransform, new Vector2(0.1f, 0.55f), new Vector2(0.9f, 0.88f));

        TextMeshProUGUI scoreText = CreateText(scorePanel.transform, "ScoreText", "0", 32, TextAnchor.MiddleCenter, FontStyle.Bold, new Color(1f, 0.96f, 0.86f, 1f));
        SetAnchors(scoreText.rectTransform, new Vector2(0.12f, 0.1f), new Vector2(0.88f, 0.62f));

        GameObject instructionPanel = CreateStyledPanel("InstructionPanel", hudRoot.transform, darkPanelSprite, new Color(1f, 1f, 1f, 0.93f), new Color(0f, 0f, 0f, 0.18f));
        SetAnchors(instructionPanel.GetComponent<RectTransform>(), new Vector2(0.025f, 0.02f), new Vector2(0.46f, 0.11f));

        TextMeshProUGUI instructionText = CreateText(instructionPanel.transform, "InstructionText", "Move with the joystick or WASD. Stand near a villager or shrine, then tap ACT.", 18, TextAnchor.MiddleLeft, FontStyle.Normal, new Color(0.96f, 0.93f, 0.86f, 1f));
        SetAnchors(instructionText.rectTransform, new Vector2(0.05f, 0.12f), new Vector2(0.95f, 0.88f));

        HUDController hudController = hudRoot.AddComponent<HUDController>();
        AssignPrivateField(hudController, "currentQuestText", questText);
        AssignPrivateField(hudController, "scoreText", scoreText);
        AssignPrivateField(hudController, "instructionText", instructionText);
    }

    private static void CreateDialogueUI(Transform canvasTransform, PlayerController player, Sprite panelSprite, Sprite buttonSprite)
    {
        GameObject panel = CreateStyledPanel("DialoguePanel", canvasTransform, panelSprite, new Color(1f, 1f, 1f, 0.96f), new Color(0f, 0f, 0f, 0.2f));
        SetAnchors(panel.GetComponent<RectTransform>(), new Vector2(0.14f, 0.05f), new Vector2(0.86f, 0.28f));
        panel.SetActive(false);

        TextMeshProUGUI speakerText = CreateText(panel.transform, "SpeakerNameText", "Village Elder", 28, TextAnchor.UpperLeft, FontStyle.Bold, new Color(1f, 0.88f, 0.48f, 1f));
        SetAnchors(speakerText.rectTransform, new Vector2(0.04f, 0.68f), new Vector2(0.46f, 0.92f));

        TextMeshProUGUI historicalFactText = CreateText(panel.transform, "HistoricalFactLabelText", "Historical Fact", 20, TextAnchor.UpperRight, FontStyle.Bold, new Color(0.99f, 0.85f, 0.43f, 1f));
        SetAnchors(historicalFactText.rectTransform, new Vector2(0.56f, 0.70f), new Vector2(0.96f, 0.92f));

        TextMeshProUGUI dialogueText = CreateText(panel.transform, "DialogueText", string.Empty, 25, TextAnchor.UpperLeft, FontStyle.Normal, new Color(0.98f, 0.95f, 0.89f, 1f));
        SetAnchors(dialogueText.rectTransform, new Vector2(0.04f, 0.20f), new Vector2(0.96f, 0.66f));

        Button nextButton = CreateUIButton(panel.transform, "NextButton", "NEXT", buttonSprite);
        SetAnchors(nextButton.GetComponent<RectTransform>(), new Vector2(0.69f, 0.03f), new Vector2(0.95f, 0.17f));

        GameObject dialogueManagerObject = new GameObject("DialogueManager");
        dialogueManagerObject.transform.SetParent(canvasTransform, false);
        DialogueManager dialogueManager = dialogueManagerObject.AddComponent<DialogueManager>();
        AssignPrivateField(dialogueManager, "dialoguePanel", panel);
        AssignPrivateField(dialogueManager, "speakerNameText", speakerText);
        AssignPrivateField(dialogueManager, "dialogueText", dialogueText);
        AssignPrivateField(dialogueManager, "historicalFactLabelText", historicalFactText);

        UnityEventTools.AddPersistentListener(nextButton.onClick, dialogueManager.AdvanceDialogueFromButton);
    }

    private static void CreateMobileControls(Transform canvasTransform, PlayerController player, Sprite joystickBaseSprite, Sprite joystickKnobSprite, Sprite actionButtonSprite)
    {
        GameObject joystickRoot = CreateUIObject("VirtualJoystick", canvasTransform);
        Image joystickBackground = joystickRoot.AddComponent<Image>();
        joystickBackground.sprite = joystickBaseSprite;
        joystickBackground.color = new Color(1f, 1f, 1f, 0.82f);
        joystickBackground.preserveAspect = true;

        RectTransform joystickRect = joystickRoot.GetComponent<RectTransform>();
        SetAnchors(joystickRect, new Vector2(0.035f, 0.055f), new Vector2(0.17f, 0.285f));

        GameObject handleObject = CreateUIObject("Handle", joystickRoot.transform);
        Image handleImage = handleObject.AddComponent<Image>();
        handleImage.sprite = joystickKnobSprite;
        handleImage.color = new Color(1f, 1f, 1f, 0.92f);
        handleImage.preserveAspect = true;

        RectTransform handleRect = handleObject.GetComponent<RectTransform>();
        handleRect.anchorMin = new Vector2(0.28f, 0.28f);
        handleRect.anchorMax = new Vector2(0.72f, 0.72f);
        handleRect.offsetMin = Vector2.zero;
        handleRect.offsetMax = Vector2.zero;

        VirtualJoystick joystick = joystickRoot.AddComponent<VirtualJoystick>();
        AssignPrivateField(joystick, "background", joystickRect);
        AssignPrivateField(joystick, "handle", handleRect);
        AssignPrivateField(joystick, "player", player);
        AssignPrivateField(joystick, "movementRange", 68f);

        Button actionButton = CreateRoundActionButton(canvasTransform, "ActionButton", actionButtonSprite, "ACT");
        SetAnchors(actionButton.GetComponent<RectTransform>(), new Vector2(0.81f, 0.06f), new Vector2(0.95f, 0.29f));
        UnityEventTools.AddPersistentListener(actionButton.onClick, player.TriggerInteractionButton);
    }

    private static Button CreateUIButton(Transform parent, string name, string label, Sprite buttonSprite)
    {
        GameObject buttonObject = CreatePanel(name, parent, buttonSprite, new Color(1f, 1f, 1f, 0.98f));
        Button button = buttonObject.AddComponent<Button>();

        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 0.95f, 0.82f, 1f);
        colors.pressedColor = new Color(0.95f, 0.88f, 0.72f, 1f);
        button.colors = colors;

        TextMeshProUGUI labelText = CreateText(buttonObject.transform, "Label", label, 24, TextAnchor.MiddleCenter, FontStyle.Bold, new Color(0.33f, 0.17f, 0.06f, 1f));
        StretchToParent(labelText.rectTransform);
        return button;
    }

    private static Button CreateRoundActionButton(Transform parent, string name, Sprite buttonSprite, string label)
    {
        GameObject buttonObject = CreateUIObject(name, parent);
        Image image = buttonObject.AddComponent<Image>();
        image.sprite = buttonSprite;
        image.preserveAspect = true;
        image.color = new Color(1f, 1f, 1f, 0.95f);

        Button button = buttonObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 0.95f, 0.82f, 1f);
        colors.pressedColor = new Color(0.96f, 0.88f, 0.68f, 1f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;

        TextMeshProUGUI labelText = CreateText(buttonObject.transform, "Label", label, 34, TextAnchor.MiddleCenter, FontStyle.Bold, new Color(0.36f, 0.18f, 0.05f, 1f));
        SetAnchors(labelText.rectTransform, new Vector2(0.2f, 0.28f), new Vector2(0.8f, 0.72f));
        return button;
    }

    private static GameObject CreateStyledPanel(string name, Transform parent, Sprite sprite, Color fillColor, Color shadowColor)
    {
        GameObject panel = CreatePanel(name, parent, sprite, fillColor);
        Shadow shadow = panel.AddComponent<Shadow>();
        shadow.effectColor = shadowColor;
        shadow.effectDistance = new Vector2(6f, -6f);
        return panel;
    }

    private static GameObject CreatePanel(string name, Transform parent, Sprite sprite, Color color)
    {
        GameObject panel = CreateUIObject(name, parent);
        Image image = panel.AddComponent<Image>();
        image.sprite = sprite;
        image.type = Image.Type.Simple;
        image.color = color;
        return panel;
    }

    private static TextMeshProUGUI CreateText(Transform parent, string name, string value, int fontSize, TextAnchor alignment, FontStyle style, Color color)
    {
        GameObject textObject = CreateUIObject(name, parent);
        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.font = LoadDefaultTmpFont();
        text.fontSize = fontSize;
        text.fontStyle = ConvertFontStyle(style);
        text.alignment = ConvertAlignment(alignment);
        text.color = color;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.outlineWidth = 0.18f;
        text.outlineColor = new Color(0.18f, 0.09f, 0.03f, 1f);
        text.margin = new Vector4(8f, 6f, 8f, 6f);
        return text;
    }

    private static Sprite EnsureSquareSprite()
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SquareTexturePath);

        if (sprite != null)
        {
            return sprite;
        }

        string directory = Path.GetDirectoryName(SquareTexturePath);

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        Texture2D texture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[16 * 16];

        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white;
        }

        texture.SetPixels(pixels);
        texture.Apply();
        File.WriteAllBytes(SquareTexturePath, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);

        AssetDatabase.ImportAsset(SquareTexturePath, ImportAssetOptions.ForceUpdate);

        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(SquareTexturePath);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(SquareTexturePath);
    }

    private static Sprite LoadSpriteAsset(string assetPath, Sprite fallback)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        return sprite != null ? sprite : fallback;
    }

    private static TMP_FontAsset LoadDefaultTmpFont()
    {
        TMP_FontAsset font = TMP_Settings.defaultFontAsset;

        if (font == null)
        {
            font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        }

        return font;
    }

    private static FontStyles ConvertFontStyle(FontStyle style)
    {
        return style switch
        {
            FontStyle.Bold => FontStyles.Bold,
            FontStyle.Italic => FontStyles.Italic,
            FontStyle.BoldAndItalic => FontStyles.Bold | FontStyles.Italic,
            _ => FontStyles.Normal
        };
    }

    private static TextAlignmentOptions ConvertAlignment(TextAnchor alignment)
    {
        return alignment switch
        {
            TextAnchor.UpperLeft => TextAlignmentOptions.TopLeft,
            TextAnchor.UpperCenter => TextAlignmentOptions.Top,
            TextAnchor.UpperRight => TextAlignmentOptions.TopRight,
            TextAnchor.MiddleLeft => TextAlignmentOptions.Left,
            TextAnchor.MiddleCenter => TextAlignmentOptions.Center,
            TextAnchor.MiddleRight => TextAlignmentOptions.Right,
            TextAnchor.LowerLeft => TextAlignmentOptions.BottomLeft,
            TextAnchor.LowerCenter => TextAlignmentOptions.Bottom,
            TextAnchor.LowerRight => TextAlignmentOptions.BottomRight,
            _ => TextAlignmentOptions.Left
        };
    }

    private static DialogueData CreateDialogueAsset(string assetPath, string speakerName, (string text, bool showAsHistoricalFact)[] lines)
    {
        DialogueData asset = AssetDatabase.LoadAssetAtPath<DialogueData>(assetPath);

        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<DialogueData>();
            EnsureAssetFolder(assetPath);
            AssetDatabase.CreateAsset(asset, assetPath);
        }

        SerializedObject serializedObject = new SerializedObject(asset);
        serializedObject.FindProperty("speakerName").stringValue = speakerName;

        SerializedProperty lineArray = serializedObject.FindProperty("lines");
        lineArray.arraySize = lines.Length;

        for (int i = 0; i < lines.Length; i++)
        {
            SerializedProperty element = lineArray.GetArrayElementAtIndex(i);
            element.FindPropertyRelative("text").stringValue = lines[i].text;
            element.FindPropertyRelative("showAsHistoricalFact").boolValue = lines[i].showAsHistoricalFact;
        }

        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(asset);
        return asset;
    }

    private static QuestData CreateQuestAsset()
    {
        QuestData asset = AssetDatabase.LoadAssetAtPath<QuestData>(ElderQuestPath);

        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<QuestData>();
            EnsureAssetFolder(ElderQuestPath);
            AssetDatabase.CreateAsset(asset, ElderQuestPath);
        }

        SerializedObject serializedObject = new SerializedObject(asset);
        serializedObject.FindProperty("questId").stringValue = "meet_the_elder";
        serializedObject.FindProperty("title").stringValue = "Visit the River Shrine";
        serializedObject.FindProperty("description").stringValue = "Learn how places of worship and gathering were important in many pre-colonial communities.";
        serializedObject.FindProperty("historicalFact").stringValue = "Barangays often centered daily life around leadership, cooperation, and shared traditions.";

        SerializedProperty objectives = serializedObject.FindProperty("objectives");
        objectives.arraySize = 1;
        SerializedProperty objective = objectives.GetArrayElementAtIndex(0);
        objective.FindPropertyRelative("objectiveId").stringValue = "visit_shrine";
        objective.FindPropertyRelative("description").stringValue = "Walk to the shrine near the river.";
        objective.FindPropertyRelative("objectiveType").enumValueIndex = (int)QuestObjectiveType.ReachLocation;
        objective.FindPropertyRelative("targetId").stringValue = "river_shrine";
        objective.FindPropertyRelative("requiredAmount").intValue = 1;

        SerializedProperty rewards = serializedObject.FindProperty("rewards");
        rewards.arraySize = 1;
        SerializedProperty reward = rewards.GetArrayElementAtIndex(0);
        reward.FindPropertyRelative("rewardItemId").stringValue = "barangay_token";
        reward.FindPropertyRelative("rewardAmount").intValue = 1;
        reward.FindPropertyRelative("scoreBonus").intValue = 25;
        reward.FindPropertyRelative("unlockContentId").stringValue = "main_world_intro_complete";

        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(asset);
        return asset;
    }

    private static GameObject CreateWorldSprite(string name, Sprite sprite, Color color, Vector3 position, Vector3 scale, Transform parent, int sortingOrder = 0)
    {
        GameObject gameObject = new GameObject(name);
        gameObject.transform.SetParent(parent, false);
        gameObject.transform.position = position;
        gameObject.transform.localScale = scale;

        SpriteRenderer renderer = gameObject.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
        return gameObject;
    }

    private static SpriteRenderer CreateCharacterShadow(Transform parent, Sprite shadowSprite, int sortingOrder, Vector3 localPosition, Vector3 localScale)
    {
        GameObject shadowObject = new GameObject("Shadow");
        shadowObject.transform.SetParent(parent, false);
        shadowObject.transform.localPosition = localPosition;
        shadowObject.transform.localScale = localScale;

        SpriteRenderer renderer = shadowObject.AddComponent<SpriteRenderer>();
        renderer.sprite = shadowSprite;
        renderer.sortingOrder = sortingOrder;
        return renderer;
    }

    private static SpriteRenderer CreateCharacterSprite(Transform parent, Sprite preferredSprite, Sprite fallbackSprite, int sortingOrder, Vector3 localPosition, Vector3 localScale)
    {
        GameObject spriteObject = new GameObject("Visual");
        spriteObject.transform.SetParent(parent, false);
        spriteObject.transform.localPosition = localPosition;
        spriteObject.transform.localScale = localScale;

        SpriteRenderer renderer = spriteObject.AddComponent<SpriteRenderer>();
        renderer.sprite = preferredSprite != null ? preferredSprite : fallbackSprite;
        renderer.sortingOrder = sortingOrder;
        return renderer;
    }

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform));
        gameObject.transform.SetParent(parent, false);
        return gameObject;
    }

    private static void SetAnchors(RectTransform rectTransform, Vector2 min, Vector2 max)
    {
        rectTransform.anchorMin = min;
        rectTransform.anchorMax = max;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

    private static void StretchToParent(RectTransform rectTransform)
    {
        SetAnchors(rectTransform, Vector2.zero, Vector2.one);
    }

    private static void EnsureAssetFolder(string assetPath)
    {
        string folderPath = Path.GetDirectoryName(assetPath)?.Replace("\\", "/");

        if (string.IsNullOrEmpty(folderPath))
        {
            return;
        }

        string[] parts = folderPath.Split('/');
        string currentPath = parts[0];

        for (int i = 1; i < parts.Length; i++)
        {
            string nextPath = $"{currentPath}/{parts[i]}";

            if (!AssetDatabase.IsValidFolder(nextPath))
            {
                AssetDatabase.CreateFolder(currentPath, parts[i]);
            }

            currentPath = nextPath;
        }
    }

    private static void EnsureSceneInBuildSettings(string scenePath)
    {
        if (!File.Exists(scenePath))
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

        EditorBuildSettingsScene[] updatedScenes = new EditorBuildSettingsScene[existingScenes.Length + 1];

        for (int i = 0; i < existingScenes.Length; i++)
        {
            updatedScenes[i] = existingScenes[i];
        }

        updatedScenes[existingScenes.Length] = new EditorBuildSettingsScene(scenePath, true);
        EditorBuildSettings.scenes = updatedScenes;
    }

    private static void AssignPrivateField(Object target, string fieldName, Object value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(fieldName);

        if (property != null)
        {
            property.objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void AssignPrivateField(Object target, string fieldName, string value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(fieldName);

        if (property != null)
        {
            property.stringValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void AssignPrivateField(Object target, string fieldName, int value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(fieldName);

        if (property != null)
        {
            property.intValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void AssignPrivateField(Object target, string fieldName, float value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(fieldName);

        if (property != null)
        {
            property.floatValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void AssignPrivateField(Object target, string fieldName, QuestObjectiveType value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(fieldName);

        if (property != null)
        {
            property.enumValueIndex = (int)value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
