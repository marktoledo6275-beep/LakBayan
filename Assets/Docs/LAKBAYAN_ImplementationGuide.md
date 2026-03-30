# LAKBAYAN Implementation Guide

## 1. High-Level System Design

LAKBAYAN is structured as one persistent game-state layer plus scene-level gameplay modules.

- `GameManager` stores score, rewards, unlocked content, and the last world scene.
- `SceneController` handles scene loading for the menu, RPG world, quiz, and mini-games.
- `PlayerController` handles four-direction top-down movement using `Rigidbody2D`, keyboard for testing, and joystick input for Android.
- `DialogueManager` + `DialogueData` provide ScriptableObject-based conversations and optional historical fact labels.
- `NPCInteraction` connects villagers, elders, and the datu to dialogue, quest offers, and quest turn-ins.
- `QuestSystem` tracks active objectives, quest completion, and rewards.
- `QuizManager` runs multiple-choice educational quizzes with score tracking and feedback.
- Each mini-game has its own manager script so it can run in a dedicated scene and report its reward back to the RPG loop.

## 2. Recommended Folder Structure

Use this structure inside `Assets`:

```text
Assets
├── Art
│   ├── Characters
│   ├── Environment
│   ├── MiniGames
│   └── UI
├── Audio
│   ├── BGM
│   └── SFX
├── Docs
│   └── LAKBAYAN_ImplementationGuide.md
├── Prefabs
│   ├── Characters
│   ├── NPCs
│   ├── UI
│   └── MiniGames
├── Scenes
│   ├── MainMenu.unity
│   ├── MainWorld.unity
│   ├── ForestPath.unity
│   ├── QuizScene.unity
│   ├── TaguTaguanScene.unity
│   ├── PatinteroScene.unity
│   └── TumbangPresoScene.unity
├── ScriptableObjects
│   ├── Dialogue
│   ├── Quests
│   └── Quiz
└── Scripts
    ├── Core
    ├── Dialogue
    ├── Input
    ├── Interaction
    ├── MiniGames
    ├── NPC
    ├── Player
    ├── Quest
    ├── Quiz
    └── UI
```

## 3. Scene Breakdown

### MainMenu
- Start Game button
- How To Play button
- Exit button

### MainWorld
- Barangay plaza
- Datu house
- NPC villagers
- quest HUD
- dialogue canvas
- touch joystick and action button

### ForestPath
- travel scene
- hidden objective triggers
- portal to mini-games or quiz scene

### QuizScene
- question text
- four answer buttons
- feedback text
- next button

### TaguTaguanScene
- hiding spots
- timer
- result text

### PatinteroScene
- grid
- player token
- moving blockers

### TumbangPresoScene
- can
- throw origin
- aim buttons
- throw button

## 4. Core Scripts in This Project

- `Assets/Scripts/Player/PlayerController.cs`
- `Assets/Scripts/NPC/NPCInteraction.cs`
- `Assets/Scripts/Dialogue/DialogueManager.cs`
- `Assets/Scripts/Quest/QuestSystem.cs`
- `Assets/Scripts/Core/GameManager.cs`
- `Assets/Scripts/Core/SceneController.cs`
- `Assets/Scripts/Quiz/QuizQuestion.cs`
- `Assets/Scripts/Quiz/QuizManager.cs`
- `Assets/Scripts/MiniGames/TaguTaguan/TaguTaguanManager.cs`
- `Assets/Scripts/MiniGames/Patintero/PatinteroManager.cs`
- `Assets/Scripts/MiniGames/TumbangPreso/TumbangPresoManager.cs`

## 5. Unity Setup Instructions

### Project Bootstrap

1. Open the project in Unity.
2. Create the scenes listed above inside `Assets/Scenes`.
3. Add all scenes to `File > Build Profiles > Scene List`.
4. Create one bootstrap object named `Systems` in the first scene and attach:
   - `GameManager`
   - `SceneController`
   - `QuestSystem`
5. Mark `GameManager` and `SceneController` as persistent by keeping their default settings.

### Player Setup

1. Create a `Player` GameObject with:
   - `SpriteRenderer`
   - `Animator`
   - `Rigidbody2D`
   - `CapsuleCollider2D` or `BoxCollider2D`
   - `PlayerController`
2. Set `Rigidbody2D`:
   - `Body Type = Dynamic`
   - `Gravity Scale = 0`
   - `Freeze Rotation Z = true`
   - `Collision Detection = Continuous`
3. Create an empty child named `InteractionOrigin` slightly in front of the sprite and assign it to `PlayerController`.
4. Animator parameters:
   - `MoveX` (float)
   - `MoveY` (float)
   - `IsMoving` (bool)

### Mobile Controls

1. Create a `Canvas`.
2. Add a joystick background image and a joystick handle image.
3. Attach `VirtualJoystick` to the joystick root.
4. Assign:
   - `Background`
   - `Handle`
   - `Player`
5. Add an action button and hook its `OnClick()` to `PlayerController.TriggerInteractionButton`.

### Dialogue Setup

1. Create dialogue assets with `Create > LAKBAYAN > Dialogue Data`.
2. Fill in:
   - `Speaker Name`
   - dialogue lines
   - check `showAsHistoricalFact` for lines that contain history facts
3. Create a dialogue canvas panel with:
   - speaker name text
   - dialogue text
   - historical fact label text
   - next button
4. Attach `DialogueManager` to a scene object and assign the UI references.
5. Hook the next button to `DialogueManager.AdvanceDialogueFromButton`.

### NPC Setup

1. Add `NPCInteraction` to each NPC.
2. Assign the dialogue assets.
3. If the NPC gives a quest, assign:
   - `Quest To Give`
   - offer dialogue
   - in-progress dialogue
   - ready-to-turn-in dialogue
   - completed dialogue
4. Make sure the NPC has a 2D collider.

### Quest Setup

1. Create quest assets with `Create > LAKBAYAN > Quest Data`.
2. Add objectives such as:
   - Talk to the datu
   - Reach the shrine
   - Finish Tagu-taguan
   - Pass the quiz
3. Add rewards such as:
   - score bonus
   - collectible token
   - unlocked content
4. Add your quest assets to `QuestSystem.registeredQuests` if you want them pre-registered.
5. Use `QuestObjectiveTrigger` on trigger zones for location objectives.

### HUD Setup

1. Create text fields for:
   - current quest
   - score
   - instructions
2. Attach `HUDController`.
3. Assign its text references.

### Scene Transition Setup

1. Add `ScenePortal` to doors, boats, or pathway entrances.
2. Set the `targetSceneName`.
3. Use trigger mode for automatic transitions or leave it off for interaction-based travel.

### Quiz Setup

1. Create a `QuizManager` object in `QuizScene`.
2. Add questions in the inspector.
3. Create four answer buttons and connect each one to:
   - `QuizManager.SubmitAnswerFromButton(0)`
   - `QuizManager.SubmitAnswerFromButton(1)`
   - `QuizManager.SubmitAnswerFromButton(2)`
   - `QuizManager.SubmitAnswerFromButton(3)`
4. Add a Next button and hook it to `QuizManager.NextQuestion`.

### Tagu-taguan Setup

1. Create clickable hiding spots in the scene or as UI buttons.
2. Add `TaguTaguanManager`.
3. Assign all hide spots in the inspector.
4. Hook each spot button to `RevealSpot(index)`.

### Patintero Setup

1. Create a visible grid.
2. Place a player token transform and blocker transforms.
3. Add `PatinteroManager`.
4. Assign the token, origin, lanes, and UI.
5. Connect four touch buttons to `MoveUp`, `MoveDown`, `MoveLeft`, and `MoveRight`.

### Tumbang Preso Setup

1. Create the can with `Rigidbody2D`.
2. Create a slipper prefab with `Rigidbody2D` and `Collider2D`.
3. Add `TumbangPresoManager`.
4. Assign:
   - can rigidbody
   - slipper prefab
   - throw origin
5. Connect buttons to:
   - `AimUp`
   - `AimDown`
   - `AimLeft`
   - `AimRight`
   - `Throw`

## 6. Suggested Sample Quest Flow

1. Talk to the village elder to learn what a barangay is.
2. Visit the datu and hear a historical fact about leadership.
3. Travel to the forest path and inspect the shrine.
4. Complete Tagu-taguan to earn trust from village children.
5. Take the quiz about barangay life and pre-colonial culture.
6. Finish Patintero and Tumbang Preso as cultural challenges.
7. Return to the datu for the final quest reward.

## 7. Testing Features Included

Use `LakbayanDebugTools` in any scene for quick checks.

- `Test Movement Setup`
- `Test Quiz Validation`
- `Test Mini-Game Managers`

The mini-game managers and `QuizManager` also include context menu debug helpers:

- `Debug Force Win`
- `Debug Force Lose`
- `Debug Finish With Pass`
- `Debug Finish With Fail`

## 8. Android APK Build Steps

1. Open `File > Build Profiles`.
2. Install Android Build Support if Unity asks for it.
3. Switch platform to Android.
4. In `Player Settings`:
   - set package name, for example `com.yourschool.lakbayan`
   - set orientation to `Portrait` or `Landscape` depending on your UI design
   - set minimum API level based on your device target
5. In `Resolution and Presentation`:
   - enable `Render Outside Safe Area` if needed
   - test on a common phone aspect ratio
6. In `Other Settings`:
   - `Scripting Backend = IL2CPP`
   - `Target Architectures = ARM64`
7. Create or assign a keystore for release builds.
8. Add all required scenes to the build.
9. Click `Build` to create an APK or `Build And Run` for a connected Android device.

## 9. Mobile Optimization Tips

- Use sprite atlases for UI and characters.
- Keep physics objects simple and avoid too many colliders in mini-games.
- Use one main camera in each scene.
- Reuse prefabs instead of duplicating objects.
- Keep particle effects and transparency light for lower-end phones.
- Prefer small audio files in compressed formats.

## 10. Free Asset Suggestions

- Kenney: top-down characters, UI packs, icons
- CraftPix free 2D packs
- OpenGameArt: tiles, nature sprites, ambient sounds
- Google Fonts for UI mockups and title concepts
- Freesound for non-commercial prototype SFX

## 11. Sound Suggestions

- Kulintang-inspired menu music
- bamboo percussion for quest completion
- soft wooden click for UI buttons
- crowd chatter and birds for barangay ambience
- whoosh and impact sounds for Tumbang Preso

## 12. Engagement Improvements

- Add a collectible codex of historical facts unlocked after each quest.
- Reward mini-games with story tokens, not just points.
- Use illustrated NPC portraits during important dialogue.
- Add a village festival ending after all cultural challenges are complete.
- Include stars or badges per mini-game for replay value.

## 13. ISO 25010 Evaluation Guide

Evaluate the capstone using these quality characteristics:

- Functional Suitability
  - Do quests, dialogue, quiz, and mini-games work as intended?
- Performance Efficiency
  - Does the game run smoothly on Android with stable FPS?
- Compatibility
  - Does it behave correctly across different Android screen sizes?
- Usability
  - Can Grade 4 to Grade 6 learners understand controls, goals, and feedback?
- Reliability
  - Does scene switching, saving, and replaying mini-games remain stable?
- Security
  - Keep save data local and avoid unnecessary permissions.
- Maintainability
  - Are scripts modular and easy to update for new lessons or quests?
- Portability
  - Can the project be built on other Android devices or adapted for PC testing?

