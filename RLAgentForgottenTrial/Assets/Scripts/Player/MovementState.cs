using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum MovementState
{
    idle,
    running,
    airUp,
    airDown,
}
public static class MovementStateExtensions
{
    public static bool IsGrounded(this MovementState s)
    {
        return s == MovementState.idle || s == MovementState.running;
    }
    public static bool IsInAir(this MovementState s)
    {
        return s == MovementState.airUp || s == MovementState.airDown;
    }
}
