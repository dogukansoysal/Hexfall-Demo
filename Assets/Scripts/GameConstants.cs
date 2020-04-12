using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameConstants
{
    public const float DropDuration = 0.3f;
    public const float MaxDropDuration = 3f;

    public const float DropPosition = 13f;

    public const float RotationDuration = 0.25f;
    
    /* Enums */
    public enum RotationDirection
    {
        Clockwise,
        AntiClockwise
    }

    public enum GameState
    {
        Playable,
        NotPlayable
    }
}
