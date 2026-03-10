public class UpdatedScoreSignal
{
    public float currentScore { get; private set; }

    public UpdatedScoreSignal(float score)
    {
        this.currentScore = score;
    }
}
