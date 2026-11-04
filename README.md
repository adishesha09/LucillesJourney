# Lucille's Journey

An empathy-focused 2D pixel-art narrative game about Lucille, a refugee mother, and the ordinary moments, hard choices and small keepsakes that shape her path from home to a new beginning.

Built with Unity 6 (6000.3.8f1, Universal Render Pipeline 2D) for GADS6222, in collaboration with UXTE7312, for The Refugee Archive.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [The Six Chapters](#the-six-chapters)
- [Controls](#controls)
- [Getting Started](#getting-started)
- [Camera and AR Modes](#camera-and-ar-modes)
- [Project Structure](#project-structure)
- [Editor Tooling and Validation](#editor-tooling-and-validation)
- [Asset Generation Tools](#asset-generation-tools)
- [Save Data](#save-data)
- [Technology](#technology)
- [Content Notice](#content-notice)
- [Licences and Credits](#licences-and-credits)

## Overview

Lucille's Journey follows one mother through six chapters, from an ordinary life at home through danger, displacement and survival, to an uncertain second chance. The player explores small top-down overworlds, interacts with people and objects, uncovers memories, collects keepsakes, and finally faces a decision about how to spend limited money.

The game is designed to build understanding rather than to reward or rank hardship. Progress systems recognise curiosity and engagement. No achievement rewards suffering and no choice is scored as right or wrong.

## Features

- Six playable chapters plus an ending, each with its own overworld, narrative sequences and objectives.
- Narrative interactions driven by dialogue sequences stored as ScriptableObject assets.
- Optional discoveries in every chapter: memory fragments and keepsakes that deepen the story.
- A final decision in Chapter 6 with three competing needs: school fees, electricity and water, or groceries. Each option states what it covers and what it leaves less of.
- A quest journal, memory album, keepsake collection and achievements, presented in a consistent pixel-art UI.
- Ten achievements that recognise curiosity, such as discovering memories, collecting keepsakes and completing optional discoveries.
- Depth-sorted overworlds in which Lucille moves in front of and behind props based on her feet position.
- Procedurally generated pixel art, music, ambience and sound effects, all original to this project.
- Keyboard, mouse, gamepad and touch input, with on-screen controls on mobile.
- Persistent settings and a robust JSON save system with automatic backup.
- Two optional alternative ways to play: tabletop augmented reality on phones and tablets, and a desktop webcam mode.
- Automated Play Mode tests and editor validators for content integrity.

## The Six Chapters

| # | Chapter | Scene | Summary |
|---|---------|-------|---------|
| 1 | Ordinary Life | `Chapter1_OrdinaryLife` | Everyday routines: tending the garden and preparing breakfast. |
| 2 | The Market | `Chapter2_TheMarket` | Ordinary errands, interrupted by a growing sense of danger. |
| 3 | Bombing and Hiding | `Chapter3_BombingAndHiding` | Seeking shelter for the children amid danger. |
| 4 | Displacement | `Chapter4_Displacement` | Leaving home, and deciding what can be carried. |
| 5 | Survival | `Chapter5_Survival` | Temporary shelter, and the daily search for food and water. |
| 6 | A Second Chance | `Chapter6_SecondChance` | A new start, and a decision about school fees, electricity and water, and groceries. |

The game opens at `MainMenu` and concludes in the `Ending` scene.

## Controls

| Action | Keyboard and mouse | Gamepad | Touch |
|--------|--------------------|---------|-------|
| Move | WASD or arrow keys | Left stick | On-screen stick |
| Interact or examine | E, Space, or click a nearby object | A (south button) | USE button or tap an object |
| Continue dialogue | Space, Enter, or Continue button | A | Continue button |
| Quest Journal | J | Y (north button) | LOG button |
| Memory Album | M | - | Journal tabs |
| Keepsakes | K | - | Journal tabs |
| Switch journal section | Tab (Shift+Tab to go back) | Left and right shoulder | Journal tabs |
| Pause or go back | Esc | Start, or B to go back | MENU button |

The in-game Controls screen lists the same bindings.

## Getting Started

### Prerequisites

- Unity Hub with Unity Editor **6000.3.8f1**. The exact version is recorded in `ProjectSettings/ProjectVersion.txt`.
- Optional: the .NET 9 SDK, only if you want to run the asset generation tools in `Tools/`.
- Optional, for AR builds: Android Build Support (ARCore) or iOS Build Support (ARKit) installed through Unity Hub.

### Open and Run

1. Clone the repository.
2. In Unity Hub, choose **Add**, select the project folder, and open it with Unity 6000.3.8f1. The first import takes several minutes.
3. Open `Assets/Scenes/MainMenu.unity` and press Play.
4. To jump straight into a chapter, open any chapter scene from `Assets/Scenes/` and press Play.

All eight scenes (the main menu, six chapters and the ending) are registered in the build settings, in play order.

### Rebuilding Content

Scenes and data assets are generated from the journey definitions. If you change `Assets/Scripts/Gamification/JourneyCatalog.cs`, regenerate them with **Lucille's Journey > Build All** from the Unity menu bar, then run **Lucille's Journey > Validate Journey Catalog**.

## Camera and AR Modes

The standard flat game is the default. Two optional modes present the same 2D game on a board in the real world. In both, the game keeps running unchanged: its camera renders into a small, crisp 384 x 216 texture that is displayed on the board.

### Tabletop AR (phones and tablets)

- Uses AR Foundation, ARCore (Android) and ARKit (iOS).
- Scan for a flat surface, move the translucent board preview to the desired spot, and tap Place.
- Pinch to resize the board and twist to rotate it. Use Move to pick it up and place it again.
- If the device cannot start AR, the game falls back to flat mode and reports the reason.
- The AR scripts are compiled only when the `LJ_AR` scripting define is set, so the project always builds even without AR packages.

To enable AR, run **Lucille's Journey > AR Tabletop > Set Up Everything**. It installs the required packages, defines `LJ_AR`, and applies the Android and iOS player settings AR needs (API level 24 or higher, IL2CPP, ARM64, OpenGL ES 3, and a camera usage description). Afterwards, install the platform build support, switch the build platform, and enable Google ARCore under Project Settings > XR Plug-in Management. Use **Remove AR** from the same menu to switch the AR scripts off again. Then build to a device and choose Play in AR from the main menu.

### Desktop Camera Mode (not real AR)

Available on any non-mobile platform, including the editor, and needs no AR packages. The webcam fills the screen and the game appears on a board you place by clicking. There is no surface detection. Optionally, the board can be anchored to the scene so it follows the picture as the camera moves, using a video-only motion estimate (shift, zoom and twist). This is a 2D approximation: it is not 3D tracking, it can drift, and it stops tracking on blank walls, in darkness or during very fast motion.

| Input | Effect |
|-------|--------|
| Click | Place the board |
| Scroll, or + and - | Resize |
| Z and X | Rotate |
| M | Pick the board up again |
| C | Switch camera |
| F | Mirror the image |
| T | Toggle anchor to scene |
| H | Show or hide help |

Camera mode can be switched on from the main menu or the in-game pause menu. On the ending screen the webcam becomes the backdrop. Only one of AR or camera mode can be active at a time.

## Project Structure

```
Assets/
  Scenes/            MainMenu, six chapter scenes, Ending
  Data/              Chapter definitions and narrative sequences (ScriptableObjects)
  Resources/
    JourneyUI/       Pixel-art UI sprites
    World/           Overworld art and depth-sorted layout JSON
    Menu/            Main menu and ending artwork
    Audio/           Music, ambience and effects
    Fonts/           Pixel fonts and their licences
  Scripts/
    Core/            Game flow, chapters, progress, settings, save system
    Gamification/    Journey catalog, stats, achievements, pixel-art UI (UI/)
    Interaction/     Interactables, choices, prerequisites, detection
    Narrative/       Dialogue lines, sequences and the narrative panel
    Player/          Player movement
    World/           Camera, depth sorting, sprite animation, objective markers
    Audio/           Audio manager
    UI/              Menus, HUD, prompts, mobile controls, transitions
    AR/              Tabletop AR and desktop camera mode
  Editor/            Scene builders, validators, save tools, Play Mode tests
Tools/               Stand-alone .NET asset generators
PDFs/                Reference documents
ProjectSettings/     Unity project configuration
```

Namespaces follow the folder layout under `LucillesJourney.*`.

## Editor Tooling and Validation

All tools are under the **Lucille's Journey** menu in the Unity Editor.

| Menu item | Purpose |
|-----------|---------|
| Build All | Regenerates scenes and data assets from the journey definitions. |
| Validate Journey Catalog | Checks identifiers, sprites and narrative assets. |
| Validate All Scenes | Opens every scene in the build settings and checks for missing scripts and unwired references on key gameplay components. |
| Repair All Scene Layouts | Recomputes UI layout groups in every scene after a batch build. |
| Run Play Mode Smoke Test | Steps through every scene in Play Mode and reports any logged exceptions or errors. |
| Save > Back Up and Reset Progress | Safely resets progress for testing, keeping a backup. |
| Save > Restore Backed-Up Progress | Restores the backed-up save. |
| Save > Open Save Folder | Opens the save location. |
| AR Tabletop > Set Up Everything / Setup Window / Remove AR | Manages AR support. |

Automated Play Mode tests live in `Assets/Editor/JourneyPlayTests.cs` and can be run from the Unity Test Runner.

## Asset Generation Tools

Pixel art and audio are produced by stand-alone .NET 9 console projects in `Tools/`, run from the repository root:

```
dotnet run -c Release --project Tools/<name>
```

| Tool | Produces |
|------|----------|
| `PixelArt` | UI frames, icons, keepsakes, memories, achievements and choice art |
| `CharacterArt` | Lucille, the children, the cat and portraits |
| `MenuArt` | Main menu and ending backdrops |
| `WorldArt` | Chapter 1 overworld: baked ground, props and layout JSON |
| `WorldArtCh2` to `WorldArtCh6` | Overworlds for Chapters 2 to 6 |
| `AudioGen` | Music, ambience and sound effects |

Layout files reference sprites by name, so artist-made sprites can replace any generated sprite without changing the layouts. Preview images from each tool are written to its `out_preview` folder.

## Save Data

Progress is stored as a single JSON file, `lucillesjourney_save.json`, in the Unity persistent data path, with a `.bak` backup alongside it. Saved identifiers that no longer exist in the catalog are ignored. Use **Lucille's Journey > Save > Open Save Folder** to find the file and **Back Up and Reset Progress** for a safe test reset. Settings (volume, text scale, reduced motion) and the AR or camera preference are stored separately in PlayerPrefs.

## Technology

- Unity 6 (6000.3.8f1) with the Universal Render Pipeline (2D)
- Input System 1.18 for keyboard, mouse, gamepad and touch
- AR Foundation, ARCore and ARKit 6.3 for tabletop AR
- TextMesh Pro and uGUI for the user interface
- Unity Test Framework for automated tests
- .NET 9 for the asset generation tools

## Content Notice

All narrative text in this repository is placeholder, written only from the chapter structure. It is not Lucille's testimony and must be reviewed against the source documents, with appropriate care and consent, before any public release or submission.

## Licences and Credits

- Developed for GADS6222 in collaboration with UXTE7312, for The Refugee Archive.
- Fonts: Press Start 2P and VT323, both under the SIL Open Font License 1.1. Licence files are in `Assets/Resources/Fonts`.
- Pixel art and audio are generated by the tools in `Tools/` and are original to this project.
- No project-wide software licence has been declared. Contact the project owners before reusing code or assets.
