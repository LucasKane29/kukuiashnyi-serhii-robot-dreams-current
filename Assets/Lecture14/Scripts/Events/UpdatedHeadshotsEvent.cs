public struct UpdatedHeadshotsEvent
{
    public float currentValue { get; private set; }

    public UpdatedHeadshotsEvent(float score)
    {
        this.currentValue = score;
    }
}
