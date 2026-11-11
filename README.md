# Hexfall — Hex-Grid Puzzle Prototype

A mobile puzzle prototype built in Unity: rotate groups of three hexagons to line up matching colours, and watch out for bombs.

> Built in April 2020 (commit history: 3 days). Archived as a finished prototype.

## Gameplay

- Tap a corner between three hexagons to select the group.
- Drag around it to rotate the group 120° clockwise or anti-clockwise.
- Three or more same-coloured hexagons that touch are cleared, and new ones fall in from above.
- Every 1,000 points a **bomb** appears with a move counter. Each move lowers it, and if it reaches zero you lose.

Inspired by the mechanic of the mobile game *Hexfall*. All code is my own.

## Technical highlights

- **Offset hex grid with corner selection:** `GridManager` maps every grid corner to its three surrounding hexagons in clockwise order and handles the odd/even row offset.
- **Rotation from a free drag:** the signed angle between the first and last touch decides the rotation direction, so players do not need an exact swipe.
- **Match and refill:** after each rotation the grid checks neighbours, clears matches, drops hexagons down and spawns new colours.
- **Data-driven levels:** grid width/height, colour count and gap live in a `ScriptableObject` (`Assets/Levels/Level_0.asset`), so changing them needs no code change.
- **Motion and screen fit:** tweened motion uses DOTween, and the camera adapts to any screen ratio (`CameraAspectRatioFix`).

## Project layout

```
Assets/Scripts/
  GameManager.cs       game loop, rotation, score, bombs
  GridManager.cs       hex grid, corners, matching, refilling
  Hexagon.cs           tile state and animations
  Bomb.cs              countdown bomb
  TouchManager.cs      touch input
  ScriptableObject/Level.cs
```

## Run it

- **Android:** install [`APK/HexfallDogukan.apk`](APK/HexfallDogukan.apk).
- **Editor:** open the project in Unity **2019.3.3f1** and play the scene in `Assets/Scenes`. Portrait orientation, 1080×2160 recommended.

## Tech

Unity 2019.3 · C# · DOTween · TextMesh Pro
