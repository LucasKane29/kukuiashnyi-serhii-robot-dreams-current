public class UpdatedHeadshotsSignal
{
    public float currentValue { get; private set; }

    public UpdatedHeadshotsSignal(float score)
    {
        this.currentValue = score;
    }
}
