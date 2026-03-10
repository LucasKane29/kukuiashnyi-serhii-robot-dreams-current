public class EnemyKilledSignal
{
    public int score { get; private set; }

    public EnemyKilledSignal(float score)
    {
        this.score = (int)score;
    }
}
