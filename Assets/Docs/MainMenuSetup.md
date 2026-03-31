# Main Menu Setup

## What Was Added

- `Assets/Art/UI/Menu/Menu.png`
- `Assets/Resources/UI/Menu/MenuGuide.png`
- `Assets/Resources/UI/Menu/MenuGuideShadow.png`
- `Assets/Scripts/UI/Menu/MainMenuController.cs`
- `Assets/Scripts/UI/Menu/MenuCharacterAnimator.cs`
- `Assets/Scripts/UI/Menu/MenuHotspotLayout.cs`
- `Assets/Scripts/UI/Menu/MenuPanelContent.cs`
- `Assets/Scripts/Editor/MenuResourceSpriteImporter.cs`
- `Assets/Scripts/Editor/MainMenuSceneBuilder.cs`

## Fastest Way To Build The Menu

1. Open the project in Unity.
2. Wait for Unity to import the copied menu image from `Assets/Art/UI/Menu/Menu.png`.
3. In the Unity top menu, click `LAKBAYAN > Build Main Menu Scene`.
4. Unity will generate `Assets/Scenes/MainMenu.unity`.
5. Open the generated scene and press Play.

## What The Generated Scene Includes

- full-screen menu art using your provided `Menu.png`
- an animated guide character that matches the in-game pixel-art style
- tapping the guide character opens the instructions panel
- transparent tap zones over:
  - Start Game
  - Instructions
  - About
- instructions panel
- about panel
- mobile-friendly canvas scaling
- `GameManager` and `SceneController` bootstrap objects

## Important Next Step

Set the correct scene names in Build Settings.

1. Add `MainMenu.unity` to Build Settings.
2. Make sure your gameplay scene is named `MainWorld` or update `MainMenuController.worldSceneName`.

## Mobile Testing Notes

- The menu is built for landscape play.
- The tap zones are anchored to the exact wooden buttons in the image.
- Pressing Android back will close an open panel first, then exit the game.

## If You Want To Adjust Button Hit Areas

Open the `MenuArtwork` object in the generated scene and tweak `MenuHotspotLayout` normalized values.
