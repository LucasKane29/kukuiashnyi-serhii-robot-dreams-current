public struct GamePausedEvent
{
    public bool isGamePaused { get; private set; }
    public GamePausedEvent(bool isGamePaused)
    {
        this.isGamePaused = isGamePaused;
    }
}
