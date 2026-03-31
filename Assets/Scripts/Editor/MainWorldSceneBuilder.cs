#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public static class MainWorldSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/MainWorld.unity";
    private const string WhiteSpritePath = "Assets/Art/Generated/WhiteSquare.png";
    private const string QuestPath = "Assets/ScriptableObjects/Quests/RiverShrineQuest.asset";
    private const string DefaultDialoguePath = "Assets/ScriptableObjects/Dialogue/Elder_Default.asset";
    private const string OfferDialoguePath = "Assets/ScriptableObjects/Dialogue/Elder_Offer.asset";
    private const string ProgressDialoguePath = "Assets/ScriptableObjects/Dialogue/Elder_InProgress.asset";
    private const string ReadyDialoguePath = "Assets/ScriptableObjects/Dialogue/Elder_Ready.asset";
    private const string CompletedDialoguePath = "Assets/ScriptableObjects/Dialogue/Elder_Completed.asset";
    private const int MapWidth = 24;
    private const int MapHeight = 18;

    [MenuItem("LAKBAYAN/Build Starter Main World Scene")]
    public static void BuildStarterMainWorldScene()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        EnsureFolders();
        Sprite whiteSprite = EnsureWhiteSprite();

        DialogueData defaultDialogue = CreateDialogue(DefaultDialoguePath, "Village Elder",
            ("Welcome to our barangay. We learn by exploring and listening to the stories of our people.", true),
            ("Talk to me when you are ready for your first task.", false));

        DialogueData offerDialogue = CreateDialogue(OfferDialoguePath, "Village Elder",
            ("Cross the bridge and visit the river shrine to the east.", false),
            ("Many pre-colonial communities settled near rivers because waterways supported fishing, farming, and trade.", true),
            ("Return to me after you reach it.", false));

        DialogueData progressDialogue = CreateDialogue(ProgressDialoguePath, "Village Elder",
            ("Keep following the path across the bridge. The shrine is waiting near the riverbank.", false));

        DialogueData readyDialogue = CreateDialogue(ReadyDialoguePath, "Village Elder",
            ("Excellent. You found the shrine and finished your first lesson.", false),
            ("Rivers connected barangays and helped people travel and exchange goods long before modern roads.", true));

        DialogueData completedDialogue = CreateDialogue(CompletedDialoguePath, "Village Elder",
            ("You are ready for more villagers, more lessons, and traditional mini-games next.", false));

        QuestData quest = CreateQuest();

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "MainWorld";

        Camera camera = CreateCamera();
        CreateSystems();
        CreateEventSystem();

        Transform world = new GameObject("World").transform;
        Transform floor = new GameObject("Floor").transform;
        Transform props = new GameObject("Props").transform;
        Transform walls = new GameObject("Walls").transform;
        floor.SetParent(world, false);
        props.SetParent(world, false);
        walls.SetParent(world, false);

        BuildMap(floor, props, walls, whiteSprite);
        PlayerController player = CreatePlayer(props, whiteSprite);
        CreateElder(props, whiteSprite, defaultDialogue, offerDialogue, progressDialogue, readyDialogue, completedDialogue, quest);
        CreateShrine(props, whiteSprite);

        camera.gameObject.AddComponent<CameraFollow>().SetTarget(player.transform);
        BuildUI(player);

        EditorSceneManager.SaveScene(scene, ScenePath);
        AddSceneToBuildSettings("Assets/Scenes/MainMenu.unity");
        AddSceneToBuildSettings(ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Playable MainWorld scene created.");
    }

    private static void EnsureFolders()
    {
        EnsureFolder("Assets/Art");
        EnsureFolder("Assets/Art/Generated");
        EnsureFolder("Assets/ScriptableObjects");
        EnsureFolder("Assets/ScriptableObjects/Dialogue");
        EnsureFolder("Assets/ScriptableObjects/Quests");
    }

    private static void EnsureFolder(string path)
    {
        string[] parts = path.Split('/');
        string current = parts[0];

        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];

            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }

            current = next;
        }
    }

    private static Sprite EnsureWhiteSprite()
    {
        Sprite existing = AssetDatabase.LoadAssetAtPath<Sprite>(WhiteSpritePath);

        if (existing != null)
        {
            return existing;
        }

        string absolutePath = Path.Combine(Directory.GetCurrentDirectory(), WhiteSpritePath.Replace('/', Path.DirectorySeparatorChar));
        Texture2D texture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
        Color32[] pixels = new Color32[16 * 16];

        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = new Color32(255, 255, 255, 255);
        }

        texture.SetPixels32(pixels);
        texture.filterMode = FilterMode.Point;
        texture.Apply();
        File.WriteAllBytes(absolutePath, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);

        AssetDatabase.ImportAsset(WhiteSpritePath, ImportAssetOptions.ForceUpdate);
        TextureImporter importer = AssetImporter.GetAtPath(WhiteSpritePath) as TextureImporter;

        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(WhiteSpritePath);
    }

    private static DialogueData CreateDialogue(string path, string speaker, params (string text, bool fact)[] lines)
    {
        DialogueData asset = AssetDatabase.LoadAssetAtPath<DialogueData>(path);

        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<DialogueData>();
            AssetDatabase.CreateAsset(asset, path);
        }

        SerializedObject serialized = new SerializedObject(asset);
        serialized.FindProperty("speakerName").stringValue = speaker;
        SerializedProperty lineArray = serialized.FindProperty("lines");
        lineArray.arraySize = lines.Length;

        for (int i = 0; i < lines.Length; i++)
        {
            SerializedProperty line = lineArray.GetArrayElementAtIndex(i);
            line.FindPropertyRelative("text").stringValue = lines[i].text;
            line.FindPropertyRelative("showAsHistoricalFact").boolValue = lines[i].fact;
        }

        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(asset);
        return asset;
    }

    private static QuestData CreateQuest()
    {
        QuestData asset = AssetDatabase.LoadAssetAtPath<QuestData>(QuestPath);

        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<QuestData>();
            AssetDatabase.CreateAsset(asset, QuestPath);
        }

        SerializedObject serialized = new SerializedObject(asset);
        serialized.FindProperty("questId").stringValue = "quest_river_shrine";
        serialized.FindProperty("title").stringValue = "Visit the River Shrine";
        serialized.FindProperty("description").stringValue = "Walk across the bridge, visit the shrine, and return to the Village Elder.";
        serialized.FindProperty("historicalFact").stringValue = "Rivers were important in many pre-colonial communities because they helped with travel, trade, food, and daily life.";

        SerializedProperty objectives = serialized.FindProperty("objectives");
        objectives.arraySize = 1;
        SerializedProperty objective = objectives.GetArrayElementAtIndex(0);
        objective.FindPropertyRelative("objectiveId").stringValue = "reach_river_shrine";
        objective.FindPropertyRelative("description").stringValue = "Reach the shrine on the far side of the bridge.";
        objective.FindPropertyRelative("objectiveType").enumValueIndex = (int)QuestObjectiveType.ReachLocation;
        objective.FindPropertyRelative("targetId").stringValue = "river_shrine";
        objective.FindPropertyRelative("requiredAmount").intValue = 1;

        SerializedProperty rewards = serialized.FindProperty("rewards");
        rewards.arraySize = 1;
        SerializedProperty reward = rewards.GetArrayElementAtIndex(0);
        reward.FindPropertyRelative("rewardItemId").stringValue = "shell_token";
        reward.FindPropertyRelative("rewardAmount").intValue = 1;
        reward.FindPropertyRelative("scoreBonus").intValue = 20;
        reward.FindPropertyRelative("unlockContentId").stringValue = "river_shrine_complete";

        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(asset);
        return asset;
    }

    private static Camera CreateCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 5.6f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.45f, 0.72f, 0.95f);
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
        cameraObject.AddComponent<AudioListener>();
        return camera;
    }

    private static void CreateSystems()
    {
        GameObject systems = new GameObject("Systems");
        systems.AddComponent<GameManager>();
        systems.AddComponent<SceneController>();
        systems.AddComponent<QuestSystem>();
    }

    private static void CreateEventSystem()
    {
        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
    }

    private static void BuildMap(Transform floor, Transform props, Transform walls, Sprite sprite)
    {
        for (int y = 0; y < MapHeight; y++)
        {
            for (int x = 0; x < MapWidth; x++)
            {
                Color color = ((x + y) % 4 == 0) ? new Color(0.36f, 0.68f, 0.28f) : new Color(0.32f, 0.63f, 0.24f);
                CreateTile($"Grass_{x}_{y}", floor, sprite, GridToWorld(x, y), color, 0, new Vector3(1.02f, 1.02f, 1f));
            }
        }

        for (int x = 0; x < MapWidth; x++)
        {
            for (int y = 7; y <= 9; y++)
            {
                CreateTile($"River_{x}_{y}", floor, sprite, GridToWorld(x, y), new Color(0.18f, 0.52f, 0.88f), 1, new Vector3(1.02f, 1.02f, 1f));
            }
        }

        for (int x = 10; x <= 12; x++)
        {
            CreateTile($"Bridge_{x}", floor, sprite, GridToWorld(x, 8), new Color(0.56f, 0.34f, 0.12f), 2, new Vector3(1.02f, 1.02f, 1f));
        }

        for (int y = 0; y <= 12; y++)
        {
            if (y != 8)
            {
                CreateTile($"PathVertical_{y}", floor, sprite, GridToWorld(11, y), new Color(0.83f, 0.71f, 0.42f), 2, new Vector3(0.76f, 1.02f, 1f));
            }
        }

        for (int x = 8; x <= 18; x++)
        {
            CreateTile($"PathHorizontal_{x}", floor, sprite, GridToWorld(x, 12), new Color(0.83f, 0.71f, 0.42f), 2, new Vector3(1.02f, 0.76f, 1f));
        }

        for (int x = 8; x <= 11; x++)
        {
            CreateTile($"PathElder_{x}", floor, sprite, GridToWorld(x, 4), new Color(0.83f, 0.71f, 0.42f), 2, new Vector3(1.02f, 0.76f, 1f));
        }

        CreateTree(props, walls, sprite, 3, 13);
        CreateTree(props, walls, sprite, 6, 13);
        CreateTree(props, walls, sprite, 19, 13);
        CreateTree(props, walls, sprite, 20, 3);
        CreateRock(props, sprite, 8, 13);
        CreateRock(props, sprite, 16, 3);

        CreateBoundary(walls, "Bottom", new Vector3(0f, -9.8f, 0f), new Vector2(30f, 1f));
        CreateBoundary(walls, "Top", new Vector3(0f, 9.8f, 0f), new Vector2(30f, 1f));
        CreateBoundary(walls, "Left", new Vector3(-12.8f, 0f, 0f), new Vector2(1f, 22f));
        CreateBoundary(walls, "Right", new Vector3(12.8f, 0f, 0f), new Vector2(1f, 22f));

        for (int x = 0; x < MapWidth; x++)
        {
            if (x < 10 || x > 12)
            {
                CreateBoundary(walls, $"RiverBlock_{x}", GridToWorld(x, 8), new Vector2(1f, 3f));
            }
        }
    }

    private static PlayerController CreatePlayer(Transform parent, Sprite sprite)
    {
        GameObject player = new GameObject("Player");
        player.transform.SetParent(parent, false);
        player.transform.position = GridToWorld(11, 1);

        CreateTile("Shadow", player.transform, sprite, new Vector3(0f, -0.28f, 0f), new Color(0f, 0f, 0f, 0.22f), 19, new Vector3(0.58f, 0.15f, 1f));
        SpriteRenderer renderer = CreateTile("Visual", player.transform, sprite, Vector3.zero, new Color(0.84f, 0.23f, 0.20f), 20, new Vector3(0.72f, 0.88f, 1f));

        Rigidbody2D body = player.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.freezeRotation = true;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;

        CapsuleCollider2D collider = player.AddComponent<CapsuleCollider2D>();
        collider.size = new Vector2(0.55f, 0.38f);
        collider.offset = new Vector2(0f, -0.14f);

        Transform interactionOrigin = new GameObject("InteractionOrigin").transform;
        interactionOrigin.SetParent(player.transform, false);
        interactionOrigin.localPosition = new Vector3(0f, 0.34f, 0f);

        PlayerController controller = player.AddComponent<PlayerController>();
        SetObject(controller, "spriteRenderer", renderer);
        SetObject(controller, "interactionOrigin", interactionOrigin);
        SetFloat(controller, "moveSpeed", 4.1f);
        return controller;
    }

    private static void CreateElder(Transform parent, Sprite sprite, DialogueData d0, DialogueData d1, DialogueData d2, DialogueData d3, DialogueData d4, QuestData quest)
    {
        GameObject elder = new GameObject("Village Elder");
        elder.transform.SetParent(parent, false);
        elder.transform.position = GridToWorld(8, 4);

        CreateTile("Shadow", elder.transform, sprite, new Vector3(0f, -0.27f, 0f), new Color(0f, 0f, 0f, 0.22f), 17, new Vector3(0.58f, 0.14f, 1f));
        CreateTile("Visual", elder.transform, sprite, Vector3.zero, new Color(0.95f, 0.84f, 0.48f), 18, new Vector3(0.72f, 0.88f, 1f));

        BoxCollider2D collider = elder.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(0.58f, 0.38f);
        collider.offset = new Vector2(0f, -0.12f);

        NPCInteraction npc = elder.AddComponent<NPCInteraction>();
        SetString(npc, "npcId", "elder");
        SetString(npc, "displayName", "Village Elder");
        SetObject(npc, "defaultDialogue", d0);
        SetObject(npc, "questOfferDialogue", d1);
        SetObject(npc, "questInProgressDialogue", d2);
        SetObject(npc, "questReadyToTurnInDialogue", d3);
        SetObject(npc, "questCompletedDialogue", d4);
        SetObject(npc, "questToGive", quest);
    }

    private static void CreateShrine(Transform parent, Sprite sprite)
    {
        GameObject shrine = new GameObject("River Shrine");
        shrine.transform.SetParent(parent, false);
        shrine.transform.position = GridToWorld(18, 12);

        CreateTile("ShrineVisual", shrine.transform, sprite, Vector3.zero, new Color(0.96f, 0.76f, 0.30f), 14, new Vector3(0.88f, 0.88f, 1f));
        BoxCollider2D collider = shrine.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(0.70f, 0.70f);

        QuestObjectiveTrigger trigger = shrine.AddComponent<QuestObjectiveTrigger>();
        SetString(trigger, "targetId", "river_shrine");
        SetEnum(trigger, "objectiveType", QuestObjectiveType.ReachLocation);
        SetInt(trigger, "progressAmount", 1);
    }

    private static void BuildUI(PlayerController player)
    {
        GameObject canvasObject = new GameObject("GameplayCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        GameObject questPanel = CreatePanel(canvas.transform, "QuestPanel", new Color(0.16f, 0.10f, 0.05f, 0.82f), 0.02f, 0.82f, 0.48f, 0.97f);
        TextMeshProUGUI questText = CreateText(questPanel.transform, "QuestText", "Talk to the Village Elder.", 30, TextAlignmentOptions.TopLeft, 0.05f, 0.12f, 0.95f, 0.90f);

        GameObject scorePanel = CreatePanel(canvas.transform, "ScorePanel", new Color(0.16f, 0.10f, 0.05f, 0.82f), 0.82f, 0.88f, 0.97f, 0.97f);
        TextMeshProUGUI scoreText = CreateText(scorePanel.transform, "ScoreText", "0", 34, TextAlignmentOptions.Center, 0f, 0f, 1f, 1f);

        GameObject instructionPanel = CreatePanel(canvas.transform, "InstructionPanel", new Color(0.16f, 0.10f, 0.05f, 0.78f), 0.02f, 0.02f, 0.55f, 0.10f);
        TextMeshProUGUI instructionText = CreateText(instructionPanel.transform, "InstructionText", "Use WASD or the joystick. Tap ACTION to talk.", 24, TextAlignmentOptions.Left, 0.04f, 0.10f, 0.96f, 0.90f);

        HUDController hud = questPanel.AddComponent<HUDController>();
        SetObject(hud, "currentQuestText", questText);
        SetObject(hud, "scoreText", scoreText);
        SetObject(hud, "instructionText", instructionText);

        GameObject dialoguePanel = CreatePanel(canvas.transform, "DialoguePanel", new Color(0.13f, 0.08f, 0.04f, 0.94f), 0.10f, 0.04f, 0.90f, 0.28f);
        dialoguePanel.SetActive(false);
        TextMeshProUGUI speakerText = CreateText(dialoguePanel.transform, "SpeakerText", "Village Elder", 30, TextAlignmentOptions.TopLeft, 0.04f, 0.70f, 0.55f, 0.92f);
        TextMeshProUGUI factText = CreateText(dialoguePanel.transform, "FactText", "Historical Fact", 24, TextAlignmentOptions.TopRight, 0.56f, 0.70f, 0.96f, 0.92f);
        factText.color = new Color(1f, 0.85f, 0.48f);
        TextMeshProUGUI bodyText = CreateText(dialoguePanel.transform, "BodyText", string.Empty, 27, TextAlignmentOptions.TopLeft, 0.04f, 0.18f, 0.96f, 0.66f);
        Button nextButton = CreateButton(dialoguePanel.transform, "NextButton", "NEXT", 0.74f, 0.04f, 0.95f, 0.16f);

        DialogueManager dialogueManager = new GameObject("DialogueManager").AddComponent<DialogueManager>();
        dialogueManager.transform.SetParent(canvas.transform, false);
        SetObject(dialogueManager, "dialoguePanel", dialoguePanel);
        SetObject(dialogueManager, "speakerNameText", speakerText);
        SetObject(dialogueManager, "dialogueText", bodyText);
        SetObject(dialogueManager, "historicalFactLabelText", factText);
        UnityEventTools.AddPersistentListener(nextButton.onClick, dialogueManager.AdvanceDialogueFromButton);

        GameObject joystick = CreatePanel(canvas.transform, "VirtualJoystick", new Color(0.08f, 0.12f, 0.16f, 0.34f), 0.05f, 0.05f, 0.18f, 0.28f);
        GameObject handle = CreatePanel(joystick.transform, "Handle", new Color(0.98f, 0.92f, 0.67f, 0.82f), 0.30f, 0.30f, 0.70f, 0.70f);
        VirtualJoystick virtualJoystick = joystick.AddComponent<VirtualJoystick>();
        SetObject(virtualJoystick, "background", joystick.GetComponent<RectTransform>());
        SetObject(virtualJoystick, "handle", handle.GetComponent<RectTransform>());
        SetObject(virtualJoystick, "player", player);

        Button actionButton = CreateButton(canvas.transform, "ActionButton", "ACTION", 0.80f, 0.08f, 0.95f, 0.20f);
        UnityEventTools.AddPersistentListener(actionButton.onClick, player.TriggerInteractionButton);
    }

    private static SpriteRenderer CreateTile(string name, Transform parent, Sprite sprite, Vector3 localPosition, Color color, int order, Vector3 scale)
    {
        GameObject tile = new GameObject(name);
        tile.transform.SetParent(parent, false);
        tile.transform.localPosition = localPosition;
        tile.transform.localScale = scale;
        SpriteRenderer renderer = tile.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sortingOrder = order;
        return renderer;
    }

    private static void CreateTree(Transform props, Transform walls, Sprite sprite, int x, int y)
    {
        Vector3 pos = GridToWorld(x, y);
        CreateTile($"TreeCanopy_{x}_{y}", props, sprite, pos + new Vector3(0f, 0.35f, 0f), new Color(0.20f, 0.47f, 0.18f), 10, new Vector3(1.7f, 1.5f, 1f));
        CreateTile($"TreeTrunk_{x}_{y}", props, sprite, pos + new Vector3(0f, -0.20f, 0f), new Color(0.48f, 0.28f, 0.10f), 9, new Vector3(0.42f, 0.70f, 1f));
        CreateBoundary(walls, $"TreeWall_{x}_{y}", pos + new Vector3(0f, -0.20f, 0f), new Vector2(0.90f, 0.70f));
    }

    private static void CreateRock(Transform props, Sprite sprite, int x, int y)
    {
        CreateTile($"Rock_{x}_{y}", props, sprite, GridToWorld(x, y), new Color(0.52f, 0.54f, 0.56f), 8, new Vector3(0.72f, 0.58f, 1f));
    }

    private static GameObject CreatePanel(Transform parent, string name, Color color, float xMin, float yMin, float xMax, float yMax)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        panel.GetComponent<Image>().color = color;
        Anchor(panel.GetComponent<RectTransform>(), xMin, yMin, xMax, yMax);
        return panel;
    }

    private static TextMeshProUGUI CreateText(Transform parent, string name, string textValue, float fontSize, TextAlignmentOptions alignment, float xMin, float yMin, float xMax, float yMax)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.text = textValue;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = new Color(1f, 0.95f, 0.84f);
        text.textWrappingMode = TextWrappingModes.Normal;
        Anchor(textObject.GetComponent<RectTransform>(), xMin, yMin, xMax, yMax);
        return text;
    }

    private static Button CreateButton(Transform parent, string name, string label, float xMin, float yMin, float xMax, float yMax)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        buttonObject.GetComponent<Image>().color = new Color(0.54f, 0.30f, 0.10f, 0.96f);
        Anchor(buttonObject.GetComponent<RectTransform>(), xMin, yMin, xMax, yMax);

        TextMeshProUGUI labelText = CreateText(buttonObject.transform, "Label", label, 24, TextAlignmentOptions.Center, 0f, 0f, 1f, 1f);
        labelText.margin = new Vector4(6f, 6f, 6f, 6f);
        return buttonObject.GetComponent<Button>();
    }

    private static void Anchor(RectTransform rect, float xMin, float yMin, float xMax, float yMax)
    {
        rect.anchorMin = new Vector2(xMin, yMin);
        rect.anchorMax = new Vector2(xMax, yMax);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void CreateBoundary(Transform parent, string name, Vector3 position, Vector2 size)
    {
        GameObject wall = new GameObject(name);
        wall.transform.SetParent(parent, false);
        wall.transform.localPosition = position;
        wall.AddComponent<BoxCollider2D>().size = size;
    }

    private static Vector3 GridToWorld(int x, int y)
    {
        return new Vector3(x - MapWidth * 0.5f + 0.5f, y - MapHeight * 0.5f + 0.5f, 0f);
    }

    private static void SetObject(Object target, string propertyName, Object value)
    {
        SerializedObject serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property != null)
        {
            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void SetString(Object target, string propertyName, string value)
    {
        SerializedObject serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property != null)
        {
            property.stringValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void SetFloat(Object target, string propertyName, float value)
    {
        SerializedObject serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property != null)
        {
            property.floatValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void SetInt(Object target, string propertyName, int value)
    {
        SerializedObject serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property != null)
        {
            property.intValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void SetEnum(Object target, string propertyName, System.Enum value)
    {
        SerializedObject serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property != null)
        {
            property.enumValueIndex = System.Convert.ToInt32(value);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void AddSceneToBuildSettings(string scenePath)
    {
        if (!File.Exists(scenePath))
        {
            return;
        }

        EditorBuildSettingsScene[] existing = EditorBuildSettings.scenes;

        foreach (EditorBuildSettingsScene scene in existing)
        {
            if (scene.path == scenePath)
            {
                return;
            }
        }

        var updated = new EditorBuildSettingsScene[existing.Length + 1];
        for (int i = 0; i < existing.Length; i++)
        {
            updated[i] = existing[i];
        }

        updated[existing.Length] = new EditorBuildSettingsScene(scenePath, true);
        EditorBuildSettings.scenes = updated;
    }
}
#endif
