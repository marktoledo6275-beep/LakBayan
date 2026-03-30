# Starter Main World Setup

## What This Adds

The starter world builder creates a first playable `MainWorld` scene with:

- a top-down player character
- camera follow
- a village elder NPC
- a simple shrine objective
- dialogue UI
- quest HUD
- mobile joystick
- action button

## How To Generate It

1. Open Unity.
2. Wait for compilation to finish.
3. Click `LAKBAYAN > Build Starter Main World Scene`.
4. Unity will create:
   - `Assets/Scenes/MainWorld.unity`
   - dialogue assets in `Assets/ScriptableObjects/Dialogue`
   - a quest asset in `Assets/ScriptableObjects/Quests`
   - `Assets/Art/Generated/WhiteSquare.png`

## How To Test From The Menu

1. Build or rebuild the menu scene:
   - `LAKBAYAN > Build Main Menu Scene`
2. Build the world scene:
   - `LAKBAYAN > Build Starter Main World Scene`
3. Open `Assets/Scenes/MainMenu.unity`
4. Press Play
5. Tap `START GAME`

## Expected Result

- the menu loads `MainWorld`
- the player appears in a simple village map
- you can move with:
  - `WASD`
  - arrow keys
  - on-screen joystick
- you can interact with the elder using:
  - `E`
  - `Space`
  - the `ACTION` button

## Starter Quest Flow

1. Talk to the `Village Elder`
2. Accept the first quest
3. Walk to the `River Shrine`
4. Return to the elder
5. Finish the intro quest

## If Start Game Still Fails

Open `File > Build Profiles` and confirm both scenes are included:

- `Assets/Scenes/MainMenu.unity`
- `Assets/Scenes/MainWorld.unity`
