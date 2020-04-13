using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameConstants
{
    public const float DropDuration = 0.3f;
    public const float FallDuration = 0.5f;    // Used for falling to empty grid cells.

    public const float MaxDropDuration = 3f;

    public const float DropPosition = 13f;

    public const float RotationDuration = 0.25f;

    public const int ExplosionScore = 20;

    public const float CornerCheckDuration = 0.25f;

    /* UI Constants */
    public const float FloatingTextDistance = 3f;
    public const float FloatingTextDuration = 1f;
    public const float FloatingTextDelay = 0.25f;

#region Enums
    
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
    
#endregion
}
