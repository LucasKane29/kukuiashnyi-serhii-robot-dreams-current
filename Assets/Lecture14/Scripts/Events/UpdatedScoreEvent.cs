public struct UpdatedScoreEvent
{
    public float currentScore { get; private set; }

    public UpdatedScoreEvent(float score)
    {
        this.currentScore = score;
    }
}
