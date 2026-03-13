public struct HeadshotMadeEvent
{
    public float additionalScore { get; private set; }
    public Enemy enemy { get; private set; }

    public HeadshotMadeEvent(Enemy enemy, float additionalScore)
    {
        this.enemy = enemy;
        this.additionalScore = additionalScore;
    }
}
