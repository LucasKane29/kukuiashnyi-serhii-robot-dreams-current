using UnityEngine;

public struct PlayerShotEvent
{
    public Vector3 playerPosition { get; private set; }

    public PlayerShotEvent(Vector3 playerPosition)
    {
        this.playerPosition = playerPosition;
    }
}
