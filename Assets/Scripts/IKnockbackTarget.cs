using UnityEngine;

// Implemented by anything an enemy can shove: the player and its time-loop clones.
public interface IKnockbackTarget
{
    void ApplyKnockback(Vector2 velocity, float lockoutDuration);
}
