public class HeadshotMadeSignal
{
    public float additionalScore { get; private set; }
    public Enemy enemy { get; private set; }

    public HeadshotMadeSignal(Enemy enemy, float additionalScore)
    {
        this.enemy = enemy;
        this.additionalScore = additionalScore;
    }
}
