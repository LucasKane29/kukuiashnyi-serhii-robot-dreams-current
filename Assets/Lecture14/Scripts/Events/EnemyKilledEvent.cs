public struct EnemyKilledEvent
{
    public int score { get; private set; }

    public EnemyKilledEvent(float score)
    {
        this.score = (int)score;
    }
}
